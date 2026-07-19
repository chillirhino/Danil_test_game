#!/usr/bin/env node
'use strict';
/*
 * Telegram <-> Claude Code relay (STREAMING).
 * Message the bot from your phone; it runs Claude Code in the project folder on
 * this PC and streams EVERYTHING back live: assistant text, bash commands,
 * file edits (with code), tool results. Access is locked to one chat id.
 *
 * Zero dependencies: built-in fetch (Node 18+) + child_process. Setup: README.md.
 */
const { spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

// ---------- config ----------
const CFG_PATH = path.join(__dirname, 'config.json');
let cfg;
try { cfg = JSON.parse(fs.readFileSync(CFG_PATH, 'utf8')); }
catch (e) { console.error('[!] Missing/invalid config.json. Copy config.example.json to config.json and fill it in.'); process.exit(1); }
const TOKEN = String(cfg.botToken || '').trim();
const ALLOWED = String(cfg.allowedChatId || '').trim();
const PROJECT_DIR = cfg.projectDir || process.cwd();
const SKIP_PERMS = cfg.skipPermissions !== false;        // default true
const CLAUDE_BIN = cfg.claudeBin || 'claude';
const TIMEOUT_MS = (cfg.timeoutMinutes || 15) * 60 * 1000;
const SHOW_RESULTS = cfg.showToolResults !== false;      // default true: show tool outputs (truncated)
const RESULT_TRUNC = cfg.resultTruncChars || 600;
if (!TOKEN) { console.error('[!] config.json: botToken is required'); process.exit(1); }

const API = `https://api.telegram.org/bot${TOKEN}`;
const sleep = ms => new Promise(r => setTimeout(r, ms));
let sessionId = null;   // for conversation continuity (--resume)
let busy = false;

// ---------- telegram + outgoing queue (ordered, rate-limit friendly) ----------
async function tg(method, body) {
  const r = await fetch(`${API}/${method}`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
  return r.json();
}
async function sendOne(chatId, text) {
  const r = await tg('sendMessage', { chat_id: chatId, text });
  if (r && !r.ok && r.error_code === 429 && r.parameters) { await sleep((r.parameters.retry_after || 2) * 1000 + 200); return sendOne(chatId, text); }
  return r;
}
const outQ = [];
let draining = false;
function emitTo(chatId, text) { if (text && text.trim()) { outQ.push({ chatId, text: text.slice(0, 4000) }); drain(); } }
async function drain() {
  if (draining) return; draining = true;
  while (outQ.length) { const m = outQ.shift(); await sendOne(m.chatId, m.text).catch(() => {}); await sleep(280); }
  draining = false;
}
async function flushQueue() { while (outQ.length || draining) await sleep(120); }

const trunc = (s, n) => { s = String(s == null ? '' : s); return s.length > n ? s.slice(0, n) + ' …[+' + (s.length - n) + ']' : s; };

// ---------- format one stream event into chat text ----------
function fmtToolUse(tu) {
  const n = tu.name, i = tu.input || {};
  switch (n) {
    case 'Bash': return '🖥 $ ' + (i.command || '') + (i.description ? '\n# ' + i.description : '');
    case 'Read': return '📖 Read ' + (i.file_path || '') + (i.offset ? ' @' + i.offset : '');
    case 'Edit': return '✏️ Edit ' + (i.file_path || '') + '\n➖ ' + trunc(i.old_string, 350) + '\n➕ ' + trunc(i.new_string, 1000);
    case 'Write': return '📝 Write ' + (i.file_path || '') + '\n' + trunc(i.content, 1400);
    case 'Grep': return '🔍 Grep "' + (i.pattern || '') + '"' + (i.path ? ' in ' + i.path : '');
    case 'Glob': return '🔍 Glob "' + (i.pattern || '') + '"';
    case 'TodoWrite': return '🗒 (план обновлён)';
    default: return '🔧 ' + n + ' ' + trunc(JSON.stringify(i), 900);
  }
}
function handleEvent(ev, emit) {
  if (!ev || !ev.type) return;
  if (ev.type === 'system') { if (ev.session_id) sessionId = ev.session_id; }
  else if (ev.type === 'assistant' && ev.message) {
    for (const b of ev.message.content || []) {
      if (b.type === 'text' && b.text && b.text.trim()) emit('💬 ' + b.text.trim());
      else if (b.type === 'tool_use') emit(fmtToolUse(b));
    }
  }
  else if (ev.type === 'user' && ev.message && SHOW_RESULTS) {
    for (const b of ev.message.content || []) {
      if (b.type === 'tool_result') {
        let c = b.content;
        if (Array.isArray(c)) c = c.map(x => (x && x.text) ? x.text : '').join('');
        c = String(c || '').trim();
        if (c) emit('↳ ' + trunc(c, RESULT_TRUNC));
      }
    }
  }
  else if (ev.type === 'result') {
    if (ev.session_id) sessionId = ev.session_id;
    emit('✅ Готово' + (typeof ev.total_cost_usd === 'number' ? '  ($' + ev.total_cost_usd.toFixed(3) + ')' : ''));
  }
}

// ---------- run Claude Code headless, streaming events ----------
function runClaudeStream(prompt, emit) {
  return new Promise((resolve) => {
    const args = ['-p', '--output-format', 'stream-json', '--verbose'];
    if (sessionId) args.push('--resume', sessionId);
    if (SKIP_PERMS) args.push('--dangerously-skip-permissions');
    args.push('--append-system-prompt',
      'You are driven remotely via a Telegram relay from the user\'s phone. The user watches your full activity ' +
      '(messages, commands, code, results) live in the chat. Keep prose concise but do the work normally. ' +
      'You have full access to this Unity project, its files, tools and memory.');

    const child = spawn(CLAUDE_BIN, args, { cwd: PROJECT_DIR, shell: true });
    let buf = '';
    child.stdin.on('error', () => {});
    child.stdin.write(prompt); child.stdin.end();
    child.stdout.on('data', chunk => {
      buf += chunk;
      let idx;
      while ((idx = buf.indexOf('\n')) >= 0) {
        const line = buf.slice(0, idx).trim();
        buf = buf.slice(idx + 1);
        if (!line) continue;
        let ev; try { ev = JSON.parse(line); } catch { continue; }
        handleEvent(ev, emit);
      }
    });
    let stderr = '';
    child.stderr.on('data', d => stderr += d);
    const to = setTimeout(() => { try { child.kill(); } catch {} emit('⏱ Прервано по таймауту — задача слишком долгая, разбей на шаги.'); resolve(); }, TIMEOUT_MS);
    child.on('close', () => { clearTimeout(to); if (stderr.trim() && !/Warning|deprecat/i.test(stderr)) emit('stderr: ' + trunc(stderr, 500)); resolve(); });
    child.on('error', (e) => { clearTimeout(to); emit('❌ Не удалось запустить claude: ' + e.message + '\nПроверь claudeBin в config.json.'); resolve(); });
  });
}

// ---------- message handling ----------
async function handle(msg) {
  const chatId = msg.chat.id;
  const text = (msg.text || '').trim();
  if (!ALLOWED || ALLOWED === 'GET_MY_ID') { await sendOne(chatId, 'Твой chat id: ' + chatId + '\nВпиши его в config.json → allowedChatId и перезапусти бота.'); return; }
  if (String(chatId) !== ALLOWED) { console.log('[x] rejected chat', chatId); await sendOne(chatId, '⛔ Not authorized.'); return; }
  if (!text) { await sendOne(chatId, 'Пришли текст.'); return; }
  if (text === '/start' || text === '/help') {
    await sendOne(chatId, '🦫 Пульт к твоему ПК (стрим-режим).\nПиши задачу — я покажу весь ход работы: сообщения, команды, код, результаты.\n\n/new — новый разговор (сбросить контекст)\n/id — твой chat id\n/help — справка'); return;
  }
  if (text === '/id') { await sendOne(chatId, 'chat id: ' + chatId); return; }
  if (text === '/new') { sessionId = null; await sendOne(chatId, '🆕 Новый разговор.'); return; }
  if (busy) { await sendOne(chatId, '⏳ Ещё выполняю прошлую задачу — подожди.'); return; }

  busy = true;
  console.log('>> ' + text);
  emitTo(chatId, '▶️ ' + text);
  await runClaudeStream(text, (t) => emitTo(chatId, t));
  await flushQueue();
  busy = false;
  console.log('<< done');
}

// ---------- long-poll loop ----------
async function main() {
  const me = await tg('getMe', {}).catch(() => null);
  if (!me || !me.ok) { console.error('[!] Bad bot token — getMe failed.'); process.exit(1); }
  console.log(`[ok] Bot @${me.result.username} running (STREAM mode).`);
  console.log(`     project: ${PROJECT_DIR}`);
  console.log(`     allowed chat: ${ALLOWED || '(setup mode)'} | skip-permissions: ${SKIP_PERMS} | show tool results: ${SHOW_RESULTS}`);
  let offset = 0;
  for (;;) {
    try {
      const r = await tg('getUpdates', { offset, timeout: 30 });
      if (r && r.ok) for (const u of r.result) { offset = u.update_id + 1; if (u.message) await handle(u.message); }
    } catch (e) { console.error('[poll] ' + e.message); await sleep(3000); }
  }
}
main();
