# Sirvival Phase 2 — Weapons & Passives (plan)

**Goal:** turn the single auto-attack into a Vampire-Survivors weapon roster.

**Increment 1 (this step):** give the chef 3 simultaneous auto-weapons so it *feels* like VS.
- Spicy Sauce — existing `ChefAutoAttack` (nearest-target projectile).
- **Rolling Pin** — `RollingPinWeapon`: N kinematic pins orbit the chef, damage on trigger (per-enemy cooldown). Scales with `Stats.damage`.
- **Garlic Aura** — `GarlicAuraWeapon`: every ~0.6s `Physics2D.OverlapCircleAll` damages all enemies in radius; translucent ring visual.
- Added to Chef by `Editor/SirvivalWeaponsBuilder.cs` (menu **Sirvival ▸ Build Weapons**). Verify orbit + aura + faster clears in play mode.

**Increment 2 (next):** weapon framework proper — `WeaponBase` (level, cooldown), `WeaponManager`, and integrate weapon acquire/level-up into the level-up draft (draft offers new weapons / +level as well as stat upgrades). Inventory cap 6+6.

**Increment 3:** more weapons (Knife Fan, Boiling Soup AoE, Frying Pan arc, Pizza Boomerang) + passives that modify weapons (Extra Plate = +1 projectile, Cookbook = +area).

**Increment 4:** evolutions (maxed weapon + required passive + chest → evolved super-weapon).

Verification: pure/state parts in edit mode; anything collision/trigger-driven in play mode (edit-mode `Physics2D.Simulate` does NOT fire trigger callbacks).
