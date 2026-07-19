#!/usr/bin/env node
'use strict';
/*
 * Web console: a terminal-style page served from this PC that streams Claude Code
 * live (text, commands, code edits, results). Open it from your phone browser and
 * "Add to Home Screen" to use it like an app.
 *
 * Access:
 *   - Same Wi-Fi: http://<PC-LAN-IP>:<port>/   (printed at startup)
 *   - Remote:     via Tailscale (http://<tailscale-ip>:<port>/) or a tunnel.
 *
 * Auth: a shared password (config.json -> webPassword). Zero npm dependencies.
 */
const http = require('http');
const os = require('os');
const { spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

const cfg = JSON.parse(fs.readFileSync(path.join(__dirname, '..', 'config.json'), 'utf8'));
const PORT = cfg.webPort || 8765;
const PASS = String(cfg.webPassword || '');
const PROJECT_DIR = cfg.projectDir || process.cwd();
const SKIP = cfg.skipPermissions !== false;
const CLAUDE = cfg.claudeBin || 'claude';
const TIMEOUT = (cfg.timeoutMinutes || 15) * 60 * 1000;
const SHOW_RESULTS = cfg.showToolResults !== false;
const RTRUNC = cfg.resultTruncChars || 800;

if (!PASS) { console.error('[!] Set "webPassword" in config.json before starting the web console.'); process.exit(1); }

let sessionId = null, busy = false;
const clients = new Set();
const trunc = (s, n) => { s = String(s == null ? '' : s); return s.length > n ? s.slice(0, n) + ' …[+' + (s.length - n) + ']' : s; };
function push(ev) { const d = 'data: ' + JSON.stringify(ev) + '\n\n'; for (const r of clients) { try { r.write(d); } catch {} } }

function fmtTool(tu) {
  const n = tu.name, i = tu.input || {};
  switch (n) {
    case 'Bash': return { kind: 'bash', text: '$ ' + (i.command || '') + (i.description ? '\n# ' + i.description : '') };
    case 'Read': return { kind: 'tool', text: 'Read ' + (i.file_path || '') };
    case 'Edit': return { kind: 'edit', text: 'Edit ' + (i.file_path || '') + '\n- ' + trunc(i.old_string, 400) + '\n+ ' + trunc(i.new_string, 1400) };
    case 'Write': return { kind: 'edit', text: 'Write ' + (i.file_path || '') + '\n' + trunc(i.content, 1800) };
    case 'Grep': return { kind: 'tool', text: 'Grep "' + (i.pattern || '') + '"' + (i.path ? ' in ' + i.path : '') };
    case 'Glob': return { kind: 'tool', text: 'Glob "' + (i.pattern || '') + '"' };
    case 'TodoWrite': return { kind: 'tool', text: '(план обновлён)' };
    default: return { kind: 'tool', text: n + ' ' + trunc(JSON.stringify(i), 1000) };
  }
}
function onEvent(ev) {
  if (!ev || !ev.type) return;
  if (ev.type === 'system') { if (ev.session_id) sessionId = ev.session_id; }
  else if (ev.type === 'assistant' && ev.message) {
    for (const b of ev.message.content || []) {
      if (b.type === 'text' && b.text && b.text.trim()) push({ kind: 'text', text: b.text.trim() });
      else if (b.type === 'tool_use') push(fmtTool(b));
    }
  } else if (ev.type === 'user' && ev.message && SHOW_RESULTS) {
    for (const b of ev.message.content || []) {
      if (b.type === 'tool_result') {
        let c = b.content;
        if (Array.isArray(c)) c = c.map(x => (x && x.text) ? x.text : '').join('');
        c = String(c || '').trim();
        if (c) push({ kind: 'result', text: trunc(c, RTRUNC) });
      }
    }
  } else if (ev.type === 'result') {
    if (ev.session_id) sessionId = ev.session_id;
    push({ kind: 'done', text: 'Готово' + (typeof ev.total_cost_usd === 'number' ? '  ($' + ev.total_cost_usd.toFixed(3) + ')' : '') });
  }
}

function run(prompt) {
  if (busy) { push({ kind: 'error', text: 'Занят другой задачей — подожди.' }); return; }
  if (prompt === '/new') { sessionId = null; push({ kind: 'info', text: 'Новый разговор.' }); return; }
  busy = true;
  push({ kind: 'you', text: prompt });
  const args = ['-p', '--output-format', 'stream-json', '--verbose'];
  if (sessionId) args.push('--resume', sessionId);
  if (SKIP) args.push('--dangerously-skip-permissions');
  args.push('--append-system-prompt',
    'You are driven remotely through a web console on the user\'s phone. They watch your full activity live. ' +
    'Keep prose concise but do the work normally. You have full access to this Unity project, its files, tools and memory.');
  const child = spawn(CLAUDE, args, { cwd: PROJECT_DIR, shell: true });
  child.stdin.on('error', () => {}); child.stdin.write(prompt); child.stdin.end();
  let buf = '';
  child.stdout.on('data', c => { buf += c; let i; while ((i = buf.indexOf('\n')) >= 0) { const line = buf.slice(0, i).trim(); buf = buf.slice(i + 1); if (!line) continue; let ev; try { ev = JSON.parse(line); } catch { continue; } onEvent(ev); } });
  const to = setTimeout(() => { try { child.kill(); } catch {} push({ kind: 'error', text: 'Таймаут — задача слишком долгая.' }); busy = false; }, TIMEOUT);
  child.on('close', () => { clearTimeout(to); busy = false; });
  child.on('error', e => { clearTimeout(to); push({ kind: 'error', text: 'Не запустился claude: ' + e.message }); busy = false; });
}

// ---------- http ----------
const INDEX = fs.readFileSync(path.join(__dirname, 'index.html'));
const MANIFEST = JSON.stringify({
  name: 'Capybara Console', short_name: 'Console', start_url: '.', display: 'standalone',
  background_color: '#0b0f14', theme_color: '#0b0f14',
  icons: [{ src: 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><rect width="64" height="64" fill="%230b0f14"/><text y="46" x="10" font-size="40">🦫</text></svg>', sizes: '512x512', type: 'image/svg+xml' }],
});

function readBody(req) { return new Promise(res => { let b = ''; req.on('data', d => b += d); req.on('end', () => res(b)); }); }

const server = http.createServer(async (req, res) => {
  const u = new URL(req.url, 'http://x');
  if (req.method === 'GET' && u.pathname === '/') { res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' }); return res.end(INDEX); }
  if (req.method === 'GET' && u.pathname === '/app.webmanifest') { res.writeHead(200, { 'Content-Type': 'application/manifest+json' }); return res.end(MANIFEST); }
  if (req.method === 'GET' && u.pathname === '/stream') {
    if (u.searchParams.get('pass') !== PASS) { res.writeHead(401); return res.end('bad pass'); }
    res.writeHead(200, { 'Content-Type': 'text/event-stream', 'Cache-Control': 'no-cache', Connection: 'keep-alive' });
    res.write('retry: 3000\n\n'); res.write('data: ' + JSON.stringify({ kind: 'info', text: busy ? 'Подключено (идёт задача…)' : 'Подключено. Пиши задачу.' }) + '\n\n');
    clients.add(res); req.on('close', () => clients.delete(res)); return;
  }
  if (req.method === 'POST' && u.pathname === '/send') {
    const body = await readBody(req); let j = {};
    try { j = JSON.parse(body); } catch {}
    if (j.pass !== PASS) { res.writeHead(401); return res.end('bad pass'); }
    const text = String(j.text || '').trim();
    if (text) run(text);
    res.writeHead(200, { 'Content-Type': 'application/json' }); return res.end('{"ok":true}');
  }
  res.writeHead(404); res.end('not found');
});

function lanIPs() {
  const out = [];
  for (const list of Object.values(os.networkInterfaces())) for (const ni of list || []) if (ni.family === 'IPv4' && !ni.internal) out.push(ni.address);
  return out;
}
server.listen(PORT, '0.0.0.0', () => {
  console.log('[ok] Web console running (STREAM). project: ' + PROJECT_DIR);
  console.log('     open on this PC:  http://localhost:' + PORT + '/');
  for (const ip of lanIPs()) console.log('     open on phone (same Wi-Fi):  http://' + ip + ':' + PORT + '/');
  console.log('     password: (as set in config.json -> webPassword)');
  console.log('     skip-permissions: ' + SKIP + ' | show tool results: ' + SHOW_RESULTS);
});
