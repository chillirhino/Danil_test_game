# Sirvival — Master Game Plan (Vampire-Survivors, chef theme)

**Vision:** A Vampire-Survivors-like where you play a **chef** surviving endless waves of hungry customers in a kitchen. Move with a screen joystick, weapons (food) auto-fire, kill crowds, vacuum XP, level up and draft food-themed upgrades, evolve weapons, beat bosses, bank tips, and spend them between runs on permanent upgrades, new chefs and new kitchens. Portrait, mobile/WebGL, currently silent (audio comes later).

**Platform:** Unity 6 URP 2D · new Input System · portrait · target WebGL (itch.io) + optional Android. Driven via AI Game Developer MCP.

**Method:** phased. Each phase produces working, testable software and gets its **own detailed plan** when we reach it (Phase 1's detailed plan already exists: `2026-07-19-sirvival-core-loop.md`). Numeric logic stays in pure, unit-testable classes; verification via edit-mode assertions + screenshots (Play mode over MCP is flaky).

---

## Phase 0 — Foundation ✅ DONE
- Scene `Sirvival.unity`, portrait 1080×1920, audio muted, top-down ortho camera.
- Chef movement via on-screen **joystick** (bottom-center) + camera follow.
- **Infinite auto-generated level:** tiled floor that scrolls seamlessly + chunked deterministic decor around the player.

## Phase 1 — Core survivors loop  *(detailed plan written)*
> Goal: a minimal but complete VS loop you can lose.
- Enemy "customer" (chases chef, contact damage, HP, death → drops XP).
- Wave **spawner** ramping over time (pure `WaveMath`).
- Chef **HP**, i-frames, death → game over + restart.
- First auto-weapon: **Spicy Sauce** (nearest-target projectile).
- **XP gems** + magnet + **leveling curve** (pure).
- **Level-up**: pause + draft 1 of 3 upgrades (pure `UpgradePool`).
- **HUD** (HP/XP/level/timer/kills) + Game Over screen.

## Phase 2 — Combat depth: weapons & passives
> Goal: build variety — the heart of the genre.
- **Weapon framework:** many weapons fire simultaneously, each with its own pattern, level, cooldown; inventory cap (6 weapons + 6 passives).
- **Food weapons roster (auto-fire):**
  - Spicy Sauce — homing/nearest projectile (from P1)
  - Rolling Pin — orbits the chef
  - Knife Fan — spread of 3–5 thrown knives
  - Boiling Soup — lobbed AoE puddle (damage-over-time)
  - Frying Pan — melee arc swing (knockback)
  - Pizza Boomerang — returns, pierces
  - Garlic/Steam Aura — constant close-range damage ring
  - Meat Cleaver — piercing line shot
- **Passive items:** Chef Hat (+XP), Apron (+armor), Energy Drink (+move speed), Whetstone (+damage), Kitchen Timer (+fire rate), Big Tray (+pickup range), Extra Plate (+1 projectile), Cookbook (+area), Lucky Coin (+luck).
- **Weapon leveling:** level-up drafts offer *new* weapon/passive OR *level up* an owned one.
- **Evolutions:** maxed weapon + required passive + a chest → **evolved super-weapon** (e.g., Spicy Sauce + Whetstone → *Ghost-Pepper Volley*; Rolling Pin + Kitchen Timer → *Dough Tornado*).

## Phase 3 — Enemy variety & bosses
- **Enemy types:** slow tank (fat customer), fast waiter, ranged food-critic (throws bad reviews), swarming flies, exploding hot-dog, shielded manager.
- **Elites/champions:** bigger, more HP, glow, drop a **chest**.
- **Bosses on a timer** (~every 5 min): Angry Manager, Giant Burger, Health Inspector — HP bar + simple attack patterns; killing one drops a big reward.
- **Spawn director:** timed wave patterns, formations, "swarm" events, rising danger curve, on-screen enemy cap with culling.

## Phase 4 — Pickups, economy & rewards
- **Pickups:** Roast Chicken (heal), Tips/Coins (currency), Magnet (collect all gems), Bomb (screen-clear), XP Boost.
- **Chests** (from elites/bosses) → reward screen (slot-machine): weapon level-ups / evolutions.
- **Run currency:** tips collected during a run bank into meta gold at run end.

## Phase 5 — Meta-progression & content
- **Save system** (persistent gold, unlocks, settings).
- **Power-Ups shop** (permanent, gold-bought): +Max HP, +Damage, +Move Speed, +Fire Rate, +Luck, +Greed (more tips), +Growth (more XP), Revive, +Reroll/Skip/Banish for drafts.
- **Characters (chefs)** with unique start weapon/passive/stat: Sushi Master (Knife Fan), Pizzaiolo (Pizza Boomerang), Grill Cook (fire aura), Barista (scalding coffee), Baker (Rolling Pin). Unlockable.
- **Stages (kitchens):** Fast-Food Alley, Fine Dining, Street Market, Nightmare Kitchen — each with its own enemies, decor, boss, hazards and modifiers. Unlockable.
- **Unlock system:** weapons/characters/stages unlocked via achievements & gold.

## Phase 6 — UX, menus & flow
- Main Menu → Character Select → Stage Select → Run → Results → back.
- Pause menu, results/summary (time, kills, gold, level, DPS), Collection/Codex of unlocked items.
- Settings (audio placeholder), RU/EN text.

## Phase 7 — Juice & feel
- Damage numbers, hit-flash, enemy death poof (particles), screen shake, level-up flash, weapon VFX, gem vacuum sweep, crits.
- **Object pooling** for enemies/projectiles/gems (needed once hundreds are on screen).

## Phase 8 — Art & audio
- Real sprites + animation: chef (idle/run), enemy types, weapons/projectiles, pickups, kitchen tilesets & props (replace placeholders).
- Audio (deferred per "no sound"): SFX + music with a mute toggle — revisit here.

## Phase 9 — Balance, polish & ship
- Tuning pass: DPS/HP/spawn curves, drop rates, economy; optional central config (mirror the existing web config-editor pattern).
- Performance pass (pooling, GC, draw calls).
- **WebGL portrait build → itch.io** (existing pipeline); optional Android APK.

---

## Cross-cutting systems (built as needed across phases)
- **RunManager** (state machine, stats, xp/level/timer, events) — Phase 1.
- **Weapon/Item framework** + data (ScriptableObjects) — Phase 2.
- **Save/persistence** + **meta economy** — Phase 4/5.
- **Object pooling** + **spawn director** — Phase 3/7.
- **UI framework** (panels, draft, HUD) — Phase 1/6.
- Pure, tested logic for every curve (waves, XP, damage, economy, luck).

## Definition of "done" for a first shippable release (MVP+)
Phases 1–4 + minimal 5 (one shop, 2 characters, 1 stage) + Phase 7 pooling + a WebGL build. Everything past that is content expansion.

## Suggested build order
0 ✅ → **1 (now)** → 2 → 3 → 4 → 7(pooling early if perf hurts) → 5 → 6 → 8 → 9. Reassess after each phase; each gets its own detailed plan before we build it.
