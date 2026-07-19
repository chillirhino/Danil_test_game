# Sirvival — Core Loop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the playable Vampire-Survivors core loop for the kitchen game *Sirvival* — the chef survives ramping waves of "customers", auto-fires a weapon, collects XP, levels up and picks upgrades, and can die.

**Architecture:** A single `RunManager` singleton owns run state (a small state machine: `Playing / LevelUp / GameOver`), the player's live `PlayerStats`, XP/level, and the elapsed timer; it raises C# events that the UI and systems subscribe to. Gameplay behaviours (enemy chase, spawner, auto-attack, projectiles, XP gems) are thin MonoBehaviours that read stats from `RunManager` and never talk to each other directly. All numeric/decision logic that can be pure (wave ramp, XP curve, upgrade rolling, damage) lives in plain static classes so it is unit-testable without Play mode. Everything uses placeholder sprites (`_px.png`, `_circle.png`) — real art comes later.

**Tech Stack:** Unity 6000.5.3f1, URP 2D, new Input System, C#. Driven through the AI Game Developer MCP (`npx unity-mcp-cli run-tool ...`). Verification uses (a) compile checks via `assets-refresh` + `console-get-logs`, (b) **edit-mode deterministic assertions** run through `script-execute` (Play mode over MCP is flaky — see `[[sirvival-project]]` memory), and (c) `screenshot-camera` for visual checks.

---

## Assumptions / defaults (tell me to change any)

- **Win/lose:** endless — timer counts up, difficulty ramps forever, run ends when chef HP hits 0. (A "survive 10:00 = win" gate can be added later.)
- **Upgrades:** 3 random choices offered per level-up; game pauses (`Time.timeScale = 0`) during the choice.
- **First weapon:** *Spicy Sauce* — auto-targets the nearest enemy and throws a projectile on an interval driven by `fireRate`.
- **Starting stats:** maxHP 100, moveSpeed 5, damage 20, fireRate 1.5/s, projectileSpeed 9, pickupRange 1.5, xpToFirstLevel 5.
- **Enemies:** one "customer" type for the MVP (chase + contact damage); more types after the loop is fun.
- **Art:** placeholders (tinted squares/circles) throughout this plan.
- **Perf:** MVP uses `Instantiate`/`Destroy`. Object pooling is a follow-up task (flagged) once counts get high.

---

## Verification approach (used by every task)

Because there is no test runner wired up and Play mode is flaky over MCP, "tests" take two concrete forms:

1. **Pure-logic assertions (preferred for math):** a throwaway C# class run via `script-execute` that calls the pure static method, `Debug.Assert`s the result, and writes `PASS`/`FAIL` lines to `C:/Users/cyber/AppData/Local/Temp/sirv_test.txt`. The runner reads that file. Example harness is given in Task 4.
2. **Edit-mode behaviour sims (for MonoBehaviours):** invoke private `Awake`/`Update`/`FixedUpdate` by reflection, step `Physics2D.Simulate(dt)` with `Physics2D.simulationMode = Script`, read transforms — exactly the pattern proven for `ChefMovement`.

Every code-touching task ends with: `assets-refresh` → assert **no `error CS`** in `console-get-logs` → run the task's assertion/sim → **commit**.

**Commit discipline:** commit after each task on a branch `feature/sirvival-core-loop`. Only stage `Assets/Scripts/Sirvival/**`, `Assets/Scenes/Sirvival.unity`, `Assets/Art/Sprites/Sirvival/**`, `docs/superpowers/**` (the repo working tree has unrelated churn — never `git add -A`).

---

## File structure

```
Assets/Scripts/Sirvival/
  Core/
    RunManager.cs        # singleton: state machine, stats, xp/level, timer, events
    PlayerStats.cs       # serializable base stats + live copy + ApplyUpgrade
    RunState.cs          # enum { Playing, LevelUp, GameOver }
  Player/
    ChefMovement.cs      # EXISTS
    ChefHealth.cs        # hp, TakeDamage, i-frames, death -> RunManager.EndRun
    ChefAutoAttack.cs    # nearest-enemy targeting, fires Projectile on fireRate
  Enemies/
    Enemy.cs             # chase chef + contact damage + hp + death(drop xp)
    EnemySpawner.cs      # spawns around camera edges using WaveMath
    WaveMath.cs          # PURE: interval & hp/speed scaling by elapsed time
  Combat/
    Projectile.cs        # travels, hits first enemy, applies damage, expires
  Progression/
    XpGem.cs             # magnet toward chef within pickupRange, grants xp
    Leveling.cs          # PURE: XpForLevel(level), pending level-ups
    Upgrade.cs           # data: id, title, apply(ref PlayerStats)
    UpgradePool.cs       # PURE: RollChoices(count, rngSeed) -> Upgrade[]
  UI/
    Hud.cs               # hp bar, xp bar, level, timer, kills (reads RunManager)
    LevelUpPanel.cs      # 3 buttons, pause, apply chosen upgrade, resume
    GameOverPanel.cs     # final time/kills, Restart button (reload scene)
  Editor/
    SirvivalSceneBuilder.cs  # EXISTS — extend: build enemy/gem/projectile prefabs,
                             # spawner, RunManager, HUD/panels, and wire refs
tests handled via script-execute assertion harness (no asmdef test assembly for MVP)
```

---

## Task 1: Run state + PlayerStats + RunManager skeleton

**Files:**
- Create: `Assets/Scripts/Sirvival/Core/RunState.cs`
- Create: `Assets/Scripts/Sirvival/Core/PlayerStats.cs`
- Create: `Assets/Scripts/Sirvival/Core/RunManager.cs`

- [ ] **Step 1: `RunState.cs`**
```csharp
namespace Sirvival { public enum RunState { Playing, LevelUp, GameOver } }
```

- [ ] **Step 2: `PlayerStats.cs`** — serializable base + runtime copy.
```csharp
using UnityEngine;
namespace Sirvival
{
    [System.Serializable]
    public class PlayerStats
    {
        public float maxHP = 100f;
        public float moveSpeed = 5f;
        public float damage = 20f;
        public float fireRate = 1.5f;      // shots per second
        public float projectileSpeed = 9f;
        public float pickupRange = 1.5f;

        public PlayerStats Clone() => (PlayerStats)MemberwiseClone();
    }
}
```

- [ ] **Step 3: `RunManager.cs`** — singleton, state, xp/level/timer, events.
```csharp
using System;
using UnityEngine;
namespace Sirvival
{
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [SerializeField] private PlayerStats baseStats = new PlayerStats();
        public PlayerStats Stats { get; private set; }

        public RunState State { get; private set; } = RunState.Playing;
        public float Elapsed { get; private set; }
        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }
        public int XpToNext { get; private set; }
        public int Kills { get; private set; }
        public Transform Player { get; private set; }

        public event Action OnStatsChanged;     // xp/level/kills/hp changed
        public event Action<Upgrade[]> OnLevelUp; // choices to present
        public event Action OnGameOver;

        private void Awake()
        {
            Instance = this;
            Stats = baseStats.Clone();
            XpToNext = Leveling.XpForLevel(Level);
            var chef = GameObject.Find("Chef");
            if (chef != null) Player = chef.transform;
        }

        private void Update()
        {
            if (State == RunState.Playing) Elapsed += Time.deltaTime;
        }

        public void AddKill() { Kills++; OnStatsChanged?.Invoke(); }

        public void AddXp(int amount)
        {
            if (State != RunState.Playing) return;
            Xp += amount;
            while (Xp >= XpToNext)
            {
                Xp -= XpToNext;
                Level++;
                XpToNext = Leveling.XpForLevel(Level);
                EnterLevelUp();
            }
            OnStatsChanged?.Invoke();
        }

        private void EnterLevelUp()
        {
            State = RunState.LevelUp;
            Time.timeScale = 0f;
            OnLevelUp?.Invoke(UpgradePool.RollChoices(3, Level * 7919 + Kills));
        }

        public void ChooseUpgrade(Upgrade u)
        {
            u.Apply(Stats);
            State = RunState.Playing;
            Time.timeScale = 1f;
            OnStatsChanged?.Invoke();
        }

        public void EndRun()
        {
            if (State == RunState.GameOver) return;
            State = RunState.GameOver;
            Time.timeScale = 0f;
            OnGameOver?.Invoke();
        }
    }
}
```
> NOTE: `Leveling`, `Upgrade`, `UpgradePool` are created in Tasks 4/6/7. Until then this won't compile — implement Tasks in the order below, or stub those three first. To keep each task independently compilable, **do Task 4 (Leveling), Task 7's `Upgrade`+`UpgradePool` before finishing Task 1's compile check.** (Grouped so the plan stays DRY; if executing strictly serially, create the three pure files as empty-but-valid stubs first, listed in Step 4.)

- [ ] **Step 4 (unblock compile): minimal stubs** for cross-referenced pure types so Task 1 compiles standalone. Replace fully in their own tasks.
```csharp
// Progression/Leveling.cs
namespace Sirvival { public static class Leveling { public static int XpForLevel(int l) => 5 + (l-1)*3; } }
// Progression/Upgrade.cs
namespace Sirvival { public class Upgrade { public string id, title; public System.Action<PlayerStats> apply;
    public void Apply(PlayerStats s){ apply?.Invoke(s); } } }
// Progression/UpgradePool.cs
namespace Sirvival { public static class UpgradePool { public static Upgrade[] RollChoices(int n,int seed) => new Upgrade[0]; } }
```

- [ ] **Step 5: compile** — `assets-refresh`; assert no `error CS`.
- [ ] **Step 6: commit** — `git commit -m "feat(sirvival): run state, PlayerStats, RunManager core"`.

---

## Task 2: Chef health + death

**Files:**
- Create: `Assets/Scripts/Sirvival/Player/ChefHealth.cs`

- [ ] **Step 1: `ChefHealth.cs`**
```csharp
using UnityEngine;
namespace Sirvival
{
    public class ChefHealth : MonoBehaviour
    {
        public float Current { get; private set; }
        public float Max => RunManager.Instance != null ? RunManager.Instance.Stats.maxHP : 100f;
        private float _invulnUntil;

        private void Start() { Current = Max; }

        public void TakeDamage(float amount)
        {
            if (Time.time < _invulnUntil || Current <= 0f) return;
            Current = Mathf.Max(0f, Current - amount);
            _invulnUntil = Time.time + 0.5f; // i-frames
            RunManager.Instance?.RaiseStatsChanged();
            if (Current <= 0f) RunManager.Instance?.EndRun();
        }
    }
}
```
- [ ] **Step 2:** add `public void RaiseStatsChanged() => OnStatsChanged?.Invoke();` to `RunManager` (events are private-invoke otherwise).
- [ ] **Step 3: compile check** (`assets-refresh`, no `error CS`).
- [ ] **Step 4: edit-mode assertion** — via `script-execute`: add `ChefHealth` to a temp GO with a stubbed RunManager, call `TakeDamage(30)` twice within i-frame window, assert `Current == 70` (second call blocked); advance `_invulnUntil` by reflection, call again, assert `40`. Write PASS/FAIL to `sirv_test.txt`.
- [ ] **Step 5: commit** — `git commit -m "feat(sirvival): chef health, i-frames, death"`.

---

## Task 3: Enemy — chase + contact damage + HP + death

**Files:**
- Create: `Assets/Scripts/Sirvival/Enemies/Enemy.cs`

- [ ] **Step 1: `Enemy.cs`**
```csharp
using UnityEngine;
namespace Sirvival
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private float maxHp = 30f;
        [SerializeField] private float contactDamage = 8f;
        [SerializeField] private int xpValue = 1;

        private float _hp;
        private Rigidbody2D _rb;

        public void Configure(float hp, float spd) { maxHp = hp; speed = spd; _hp = hp; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f; _rb.freezeRotation = true;
            _hp = maxHp;
        }

        private void FixedUpdate()
        {
            var player = RunManager.Instance?.Player;
            if (player == null) return;
            Vector2 dir = ((Vector2)player.position - _rb.position).normalized;
            _rb.linearVelocity = dir * speed;
        }

        public void TakeDamage(float amount)
        {
            _hp -= amount;
            if (_hp <= 0f) Die();
        }

        private void Die()
        {
            RunManager.Instance?.AddKill();
            XpGem.Spawn(transform.position, xpValue);   // Task 6
            Destroy(gameObject);
        }

        private void OnCollisionStay2D(Collision2D c)
        {
            var hp = c.collider.GetComponent<ChefHealth>();
            if (hp != null) hp.TakeDamage(contactDamage);
        }
    }
}
```
> `XpGem.Spawn` is Task 6 — add a temporary no-op `XpGem` stub (`public static void Spawn(Vector3 p,int v){}`) to compile now; replace in Task 6.

- [ ] **Step 2: enemy prefab** — via `SirvivalSceneBuilder` (extend in Task 9) or a one-off `script-execute`: GO "Enemy" with SpriteRenderer (`_px.png`, red tint), `CircleCollider2D` r=0.4, `Rigidbody2D`, `Enemy`; save as `Assets/Art/Sprites/Sirvival/Enemy.prefab` (use `assets-prefab-create`).
- [ ] **Step 3: compile check.**
- [ ] **Step 4: edit-mode sim** — place one Enemy at (5,0), chef at (0,0), init `RunManager.Player`, run `Enemy.FixedUpdate` + `Physics2D.Simulate(0.02f)` ~50×, assert enemy X decreased toward 0 (it chases). Write PASS/FAIL.
- [ ] **Step 5: commit** — `"feat(sirvival): chasing enemy with contact damage"`.

---

## Task 4: Wave spawner + WaveMath (PURE, tested)

**Files:**
- Create: `Assets/Scripts/Sirvival/Enemies/WaveMath.cs`
- Create: `Assets/Scripts/Sirvival/Enemies/EnemySpawner.cs`

- [ ] **Step 1: `WaveMath.cs` (pure)**
```csharp
using UnityEngine;
namespace Sirvival
{
    public static class WaveMath
    {
        // seconds between spawns: starts 1.2s, floors at 0.15s, tighter over time
        public static float SpawnInterval(float t) => Mathf.Max(0.15f, 1.2f - t * 0.01f);
        // enemy hp/speed scale gently with minutes survived
        public static float EnemyHp(float t)    => 30f + t * 1.5f;
        public static float EnemySpeed(float t)  => Mathf.Min(4.5f, 2f + t * 0.02f);
    }
}
```

- [ ] **Step 2: assertion harness** — `script-execute`:
```csharp
using System.IO; using UnityEngine; using Sirvival;
public class T { public static void Main() {
  var sb = new System.Text.StringBuilder();
  void Chk(string n, bool ok){ sb.AppendLine((ok?"PASS ":"FAIL ")+n); }
  Chk("interval@0=1.2", Mathf.Abs(WaveMath.SpawnInterval(0)-1.2f)<1e-4f);
  Chk("interval floors", WaveMath.SpawnInterval(1000f)==0.15f);
  Chk("hp grows", WaveMath.EnemyHp(60)>WaveMath.EnemyHp(0));
  Chk("speed caps", WaveMath.EnemySpeed(10000f)==4.5f);
  File.WriteAllText(@"C:/Users/cyber/AppData/Local/Temp/sirv_test.txt", sb.ToString());
}}
```
Run it, `cat` the file, expect 4× `PASS`.

- [ ] **Step 3: `EnemySpawner.cs`** — spawns Enemy prefab just outside the camera each interval.
```csharp
using UnityEngine;
namespace Sirvival
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Enemy enemyPrefab;
        private float _next;

        private void Update()
        {
            var rm = RunManager.Instance;
            if (rm == null || rm.State != RunState.Playing || enemyPrefab == null) return;
            if (Time.time < _next) return;
            _next = Time.time + WaveMath.SpawnInterval(rm.Elapsed);
            Spawn(rm);
        }

        private void Spawn(RunManager rm)
        {
            var cam = Camera.main;
            Vector3 c = rm.Player != null ? rm.Player.position : Vector3.zero;
            float h = cam.orthographicSize + 1f, w = h * cam.aspect;
            // pick a random point on the ring just outside the view
            float a = Random.value * Mathf.PI * 2f;
            Vector3 pos = c + new Vector3(Mathf.Cos(a) * w, Mathf.Sin(a) * h, 0f);
            var e = Instantiate(enemyPrefab, pos, Quaternion.identity);
            e.Configure(WaveMath.EnemyHp(rm.Elapsed), WaveMath.EnemySpeed(rm.Elapsed));
        }
    }
}
```
- [ ] **Step 4: compile check.**
- [ ] **Step 5: commit** — `"feat(sirvival): wave spawner + tested WaveMath"`.

---

## Task 5: Projectile + auto-attack (Spicy Sauce)

**Files:**
- Create: `Assets/Scripts/Sirvival/Combat/Projectile.cs`
- Create: `Assets/Scripts/Sirvival/Player/ChefAutoAttack.cs`

- [ ] **Step 1: `Projectile.cs`**
```csharp
using UnityEngine;
namespace Sirvival
{
    public class Projectile : MonoBehaviour
    {
        private Vector2 _vel; private float _damage; private float _dieAt;
        public void Launch(Vector2 dir, float speed, float damage, float life)
        { _vel = dir.normalized * speed; _damage = damage; _dieAt = Time.time + life; }

        private void Update()
        {
            transform.position += (Vector3)(_vel * Time.deltaTime);
            if (Time.time >= _dieAt) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var e = other.GetComponent<Enemy>();
            if (e != null) { e.TakeDamage(_damage); Destroy(gameObject); }
        }
    }
}
```

- [ ] **Step 2: `ChefAutoAttack.cs`** — every `1/fireRate`s, find nearest enemy, fire.
```csharp
using UnityEngine;
namespace Sirvival
{
    public class ChefAutoAttack : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        private float _next;

        private void Update()
        {
            var rm = RunManager.Instance;
            if (rm == null || rm.State != RunState.Playing || projectilePrefab == null) return;
            if (Time.time < _next) return;
            var target = Nearest();
            if (target == null) return;
            _next = Time.time + 1f / Mathf.Max(0.01f, rm.Stats.fireRate);
            Vector2 dir = (Vector2)(target.position - transform.position);
            var p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            p.Launch(dir, rm.Stats.projectileSpeed, rm.Stats.damage, 3f);
        }

        private Transform Nearest()
        {
            Transform best = null; float bestSqr = float.MaxValue;
            foreach (var e in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                float d = ((Vector2)(e.transform.position - transform.position)).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = e.transform; }
            }
            return best;
        }
    }
}
```
> Projectile prefab: SpriteRenderer `_circle.png` (light tint), `CircleCollider2D` r=0.15 **isTrigger=true**, `Projectile`. `Enemy`'s collider is non-trigger; trigger/collider overlap fires `OnTriggerEnter2D`. Saved as `Assets/Art/Sprites/Sirvival/Projectile.prefab`.

- [ ] **Step 3: compile check.**
- [ ] **Step 4: edit-mode sim** — spawn 3 enemies at varied distances, call `Nearest()` by reflection, assert it returns the closest; launch a projectile toward an enemy, step physics, assert enemy `_hp` dropped / destroyed. PASS/FAIL to file.
- [ ] **Step 5: commit** — `"feat(sirvival): projectile + nearest-target auto-attack"`.

---

## Task 6: XP gems + Leveling (PURE, tested)

**Files:**
- Replace stub: `Assets/Scripts/Sirvival/Progression/Leveling.cs`
- Create: `Assets/Scripts/Sirvival/Progression/XpGem.cs`

- [ ] **Step 1: `Leveling.cs` (pure)** — rising XP curve.
```csharp
using UnityEngine;
namespace Sirvival
{
    public static class Leveling
    {
        // XP needed to go from `level` to `level+1`
        public static int XpForLevel(int level) => Mathf.RoundToInt(5f + (level - 1) * 4f + (level - 1) * (level - 1) * 0.6f);
    }
}
```

- [ ] **Step 2: assertion** — assert `XpForLevel(1)==5`, and monotonic increase for levels 1..10. PASS/FAIL to file.

- [ ] **Step 3: `XpGem.cs`** — magnet toward chef within `pickupRange`, grant xp on reach.
```csharp
using UnityEngine;
namespace Sirvival
{
    public class XpGem : MonoBehaviour
    {
        private int _value;
        private static Projectile _unused; // keep namespace tidy

        public static void Spawn(Vector3 pos, int value)
        {
            var go = new GameObject("XpGem");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SirvivalAssets.Circle();   // helper added in Task 9 (loads _circle.png)
            sr.color = new Color(0.3f, 0.9f, 0.4f); sr.transform.localScale = Vector3.one * 0.3f;
            go.AddComponent<XpGem>()._value = value;
        }

        private void Update()
        {
            var rm = RunManager.Instance; if (rm?.Player == null) return;
            float range = rm.Stats.pickupRange;
            Vector2 to = (Vector2)(rm.Player.position - transform.position);
            if (to.magnitude <= 0.35f) { rm.AddXp(_value); Destroy(gameObject); return; }
            if (to.magnitude <= range)
                transform.position += (Vector3)(to.normalized * 8f * Time.deltaTime); // pulled in
        }
    }
}
```
> `SirvivalAssets.Circle()` — a tiny helper (Task 9) that caches `Resources`/`AssetDatabase` loads of `_circle.png`. Add its stub now: `namespace Sirvival { public static class SirvivalAssets { public static Sprite Circle() => null; } }` and finish in Task 9.

- [ ] **Step 4: compile check + assertion** (Leveling test PASS).
- [ ] **Step 5: commit** — `"feat(sirvival): xp gems + tested leveling curve"`.

---

## Task 7: Upgrades — data, pool (PURE, tested), and choice UI

**Files:**
- Replace stub: `Assets/Scripts/Sirvival/Progression/Upgrade.cs`
- Replace stub: `Assets/Scripts/Sirvival/Progression/UpgradePool.cs`
- Create: `Assets/Scripts/Sirvival/UI/LevelUpPanel.cs`

- [ ] **Step 1: `Upgrade.cs`**
```csharp
using System;
namespace Sirvival
{
    public class Upgrade
    {
        public string Id; public string Title; public string Desc;
        private readonly Action<PlayerStats> _apply;
        public Upgrade(string id, string title, string desc, Action<PlayerStats> apply)
        { Id = id; Title = title; Desc = desc; _apply = apply; }
        public void Apply(PlayerStats s) => _apply(s);
    }
}
```

- [ ] **Step 2: `UpgradePool.cs` (pure, deterministic by seed)**
```csharp
using System.Collections.Generic;
namespace Sirvival
{
    public static class UpgradePool
    {
        private static readonly Upgrade[] All =
        {
            new Upgrade("dmg",  "Острее соус",   "+25% урон",        s => s.damage *= 1.25f),
            new Upgrade("rate", "Быстрее руки",  "+20% скорострел.", s => s.fireRate *= 1.20f),
            new Upgrade("spd",  "Лёгкие ноги",   "+12% скорость",    s => s.moveSpeed *= 1.12f),
            new Upgrade("hp",   "Плотный ужин",  "+25 макс. HP",     s => s.maxHP += 25f),
            new Upgrade("mag",  "Большой поднос","+30% радиус сбора",s => s.pickupRange *= 1.30f),
            new Upgrade("proj", "Дальнобойность","+20% скорость снаряда", s => s.projectileSpeed *= 1.20f),
        };

        // deterministic Fisher-Yates by seed; returns first `count` distinct
        public static Upgrade[] RollChoices(int count, int seed)
        {
            var list = new List<Upgrade>(All);
            var rng = new System.Random(seed);
            for (int i = list.Count - 1; i > 0; i--)
            { int j = rng.Next(i + 1); (list[i], list[j]) = (list[j], list[i]); }
            return list.GetRange(0, System.Math.Min(count, list.Count)).ToArray();
        }
    }
}
```

- [ ] **Step 3: assertion** — `RollChoices(3, seed)` returns 3 distinct ids; same seed → same ids; different seed → (usually) different order. Also assert applying "dmg" makes `damage` 25% higher. PASS/FAIL to file.

- [ ] **Step 4: `LevelUpPanel.cs`** — subscribes to `RunManager.OnLevelUp`, builds 3 buttons, on click calls `ChooseUpgrade`. (UI built by Task 9's builder; this script populates it.)
```csharp
using UnityEngine; using UnityEngine.UI;
namespace Sirvival
{
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;      // panel container
        [SerializeField] private Button[] buttons;     // 3 buttons
        [SerializeField] private Text[] labels;        // title+desc per button

        private void Awake()
        {
            RunManager.Instance.OnLevelUp += Show;
            root.SetActive(false);
        }
        private void Show(Upgrade[] choices)
        {
            root.SetActive(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var u = i < choices.Length ? choices[i] : null;
                buttons[i].gameObject.SetActive(u != null);
                if (u == null) continue;
                labels[i].text = u.Title + "\n" + u.Desc;
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() => { RunManager.Instance.ChooseUpgrade(u); root.SetActive(false); });
            }
        }
    }
}
```
> Buttons must work while paused — `GraphicRaycaster` + `InputSystemUIInputModule` process at `Time.timeScale = 0` fine. Button visuals use `_circle.png`/`_px.png` placeholders; text via legacy `UI.Text` for zero setup.

- [ ] **Step 5: compile check + assertion PASS.**
- [ ] **Step 6: commit** — `"feat(sirvival): upgrade pool + level-up choice panel"`.

---

## Task 8: HUD + Game Over / restart

**Files:**
- Create: `Assets/Scripts/Sirvival/UI/Hud.cs`
- Create: `Assets/Scripts/Sirvival/UI/GameOverPanel.cs`

- [ ] **Step 1: `Hud.cs`** — reads RunManager each frame / on `OnStatsChanged`.
```csharp
using UnityEngine; using UnityEngine.UI;
namespace Sirvival
{
    public class Hud : MonoBehaviour
    {
        [SerializeField] private Image hpFill;    // filled image, fillAmount 0..1
        [SerializeField] private Image xpFill;
        [SerializeField] private Text levelText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text killsText;
        [SerializeField] private ChefHealth health;

        private void Update()
        {
            var rm = RunManager.Instance; if (rm == null) return;
            if (health != null && hpFill != null) hpFill.fillAmount = health.Current / Mathf.Max(1f, health.Max);
            if (xpFill != null) xpFill.fillAmount = rm.XpToNext > 0 ? (float)rm.Xp / rm.XpToNext : 0f;
            if (levelText != null) levelText.text = "LV " + rm.Level;
            if (killsText != null) killsText.text = rm.Kills.ToString();
            if (timerText != null) { int s=(int)rm.Elapsed; timerText.text = (s/60)+":"+(s%60).ToString("00"); }
        }
    }
}
```

- [ ] **Step 2: `GameOverPanel.cs`**
```csharp
using UnityEngine; using UnityEngine.UI; using UnityEngine.SceneManagement;
namespace Sirvival
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text summary;
        [SerializeField] private Button restart;

        private void Awake()
        {
            RunManager.Instance.OnGameOver += Show;
            root.SetActive(false);
            restart.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); });
        }
        private void Show()
        {
            var rm = RunManager.Instance;
            int s = (int)rm.Elapsed;
            summary.text = "Продержался " + (s/60) + ":" + (s%60).ToString("00") + "\nУбито: " + rm.Kills;
            root.SetActive(true);
        }
    }
}
```
> Scene must be in Build Settings for `LoadScene` by name — Task 9 adds `Assets/Scenes/Sirvival.unity` to build settings.

- [ ] **Step 3: compile check.**
- [ ] **Step 4: commit** — `"feat(sirvival): HUD + game over/restart"`.

---

## Task 9: Scene assembly (extend SirvivalSceneBuilder) + full-loop verification

**Files:**
- Modify: `Assets/Scripts/Sirvival/Editor/SirvivalSceneBuilder.cs`
- Create: `Assets/Scripts/Sirvival/Core/SirvivalAssets.cs` (replace stub — cached sprite loader)

- [ ] **Step 1: `SirvivalAssets.cs`**
```csharp
using UnityEngine;
namespace Sirvival
{
    public static class SirvivalAssets
    {
        private static Sprite _circle, _px;
        public static Sprite Circle() => _circle != null ? _circle : (_circle = Load("_circle"));
        public static Sprite Px()     => _px != null ? _px : (_px = Load("_px"));
        private static Sprite Load(string n)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Sirvival/" + n + ".png");
#else
            return null; // runtime build: gems/projectiles come from prefabs, not this path
#endif
        }
    }
}
```
> For runtime builds, gems are spawned from a **prefab** reference instead of `SirvivalAssets` (revisit `XpGem.Spawn` to use a prefab injected by the spawner). MVP editor testing uses the AssetDatabase path.

- [ ] **Step 2: extend builder** with a `[MenuItem("Sirvival/Build Gameplay")]` that:
  - adds `RunManager`, `EnemySpawner`, `ChefAutoAttack` (on Chef), `ChefHealth` (on Chef, + Chef collider non-trigger, tag/layer), `Hud`, `LevelUpPanel`, `GameOverPanel`;
  - creates Enemy/Projectile prefabs if missing (`assets-prefab-create`) and assigns them to spawner/auto-attack via `SerializedObject`;
  - builds HUD UI (top bar: HP fill, XP fill, level/timer/kills texts) + LevelUp panel (dimmer + 3 buttons + labels) + GameOver panel (summary + restart), all under `UICanvas`, wiring every `[SerializeField]` ref via `SerializedObject`;
  - adds `Assets/Scenes/Sirvival.unity` to `EditorBuildSettings.scenes`;
  - `EditorSceneManager.SaveScene`.
  (Full method written at implementation time — mirrors the existing control-layer builder's patterns: find-or-create, `SerializedObject.FindProperty(...).objectReferenceValue`, Unity-aware `== null`.)

- [ ] **Step 3: compile check** (no `error CS`).
- [ ] **Step 4: full-loop edit-mode verification** (deterministic, no Play mode):
  - init `RunManager` (reflection `Awake`), set `Player`;
  - simulate 60s of ticks: loop stepping `EnemySpawner.Update`(reflection)+`Physics2D.Simulate`, `ChefAutoAttack.Update`, `Enemy.FixedUpdate`s, feeding `Time`? — since `Time.time` can't be advanced by reflection cheaply, instead **assert each system in isolation** (already done Tasks 3–7) and here assert integration points: `RunManager.AddXp(100)` triggers `OnLevelUp` with 3 choices; `ChooseUpgrade` raises damage and sets `State=Playing`, `Time.timeScale=1`; `EndRun` sets `GameOver` + fires event. Write PASS/FAIL for each.
  - visual: `screenshot-camera` (portrait) after placing a few enemies + gems to confirm they render.
- [ ] **Step 5: brief Play-mode smoke (best-effort)** — enter Play, `screenshot-game-view`, confirm enemies stream in and chef fires; if Play mode drops (known flaky), rely on Step 4. Screenshot saved for the user.
- [ ] **Step 6: commit** — `"feat(sirvival): assemble core loop scene + build settings"`.

---

## Follow-ups (out of scope for this plan, note only)

- Object pooling for enemies/projectiles/gems (perf once counts climb).
- Real art (chef/customer/food sprites, kitchen tileset) replacing placeholders.
- More enemy types + a boss; more weapons/upgrades; weapon evolutions.
- Sound is intentionally **off** (`SirvivalAudio`) — revisit when adding SFX/music.
- Meta-progression between runs; results screen; WebGL/itch portrait build.

---

## Self-review notes

- **Coverage:** movement (done, prior work) + enemies (T3) + waves (T4) + combat (T5) + HP/death (T2) + XP/level (T6) + upgrades (T7) + HUD/game-over (T8) + assembly (T9) = the full core loop. ✔
- **Cross-task type consistency:** `RunManager` API used everywhere — `Instance`, `Stats`, `State`, `Player`, `Elapsed`, `Level/Xp/XpToNext/Kills`, `AddXp`, `AddKill`, `ChooseUpgrade`, `EndRun`, `RaiseStatsChanged`, events `OnStatsChanged/OnLevelUp/OnGameOver`. `Enemy.TakeDamage`, `Enemy.Configure`, `Projectile.Launch`, `Leveling.XpForLevel`, `WaveMath.*`, `UpgradePool.RollChoices`, `Upgrade.Apply`, `XpGem.Spawn` — names match across tasks. ✔
- **Stub ordering:** Task 1 notes the three pure-type stubs so it compiles before Tasks 4/6/7 replace them; `XpGem`/`SirvivalAssets` stubs likewise. ✔
- **Placeholders:** none of the forbidden kind — every code step has real code; the only deferred bodies are the two builder methods (T9) which are explicitly "mirrors existing builder patterns" with the exact operations listed. Acceptable given they are large mechanical scene-assembly methods.
```
