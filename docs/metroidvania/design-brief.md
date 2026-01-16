## Code Runner: Metroidvania Design Brief (v0.1)

### Player fantasy (the “hacker inside a computer”)
- **You are a hacker avatar** navigating a stylized computer world.
- **Progress = capability**: you gain new “tools” (abilities) that let you access deeper system layers.
- **Rewards are files**: keys, exploits, credentials, modules—represented as collectible “files” and “packages.”

### Core pillars (what we optimize for)
- **Traversal-first**: movement is expressive and precise; abilities double as traversal gates.
- **Readable combat**: enemies telegraph clearly; bosses test mastery of a small set of mechanics.
- **Exploration loop**: discover → collect files → unlock tools → open new routes → backtrack meaningfully.
- **Solo-dev iteration**: content is modular (rooms), tuning is data-driven, debugging is built-in.

### Core loop (moment-to-moment)
1. Enter a **room** (system area) → evaluate hazards/enemies.
2. Fight / platform to reach **file pickups** (loot, keys, upgrades).
3. Hit an **ability gate** (permission wall, locked port, encrypted door).
4. Find the required **tool/exploit** elsewhere → return and unlock the new route.

### World structure (example regions)
- **BootSector (Hub/Tutorial)**: safe hub + basic enemies; teaches map, save points, first tool.
- **FileSystem (Traversal Intro)**: folders/partitions; verticality; introduces first hard gate.
- **NetworkStack (Hazards/Tools)**: routing lasers, moving packets, timed “port” grapples.
- **KernelSpace (Endgame)**: heavy security; boss rush segments; high-risk/high-reward shortcuts.

### Progression: tools/abilities (initial set of 5)
1. **PacketHook (Grapple)**: latch to “ports” (anchor points) and swing/pull across gaps.
2. **PrivilegeEscalation (Wall Climb / Wall Hang)**: climb special “permission” surfaces.
3. **BurstTransfer (Dash Upgrade)**: longer dash + optional cancel; used for gap clears and combat evasion.
4. **ScriptInjection (Remote Hack)**: trigger terminals/switches at range; disable turrets; open doors.
5. **Keyfile (Firewall Bypass)**: consumable/permanent keys that open encrypted barriers (locks).

### Combat model (keep it tight, keep it small)
- **Baseline actions**: move, jump, dash, attack (existing controls).
- **Tools as abilities**: up to 2 equipped “tools” at a time (cooldowns/energy later if needed).
- **Hit feedback**: clear hitstop, knockback, invuln frames, readable hurtboxes.

### Enemy themes (security processes)
- **ScannerDrone**: patrol + line-of-sight “scan”; forces positioning.
- **Charger**: commits to charges; teaches bait-and-punish.
- **Crawler**: ground pressure; encourages aerial movement.

### Boss theme (security suite)
- **FirewallDaemon** (example): phase-based “exam” that mixes traversal checks with attack patterns.
- **Boss reward**: tool unlock that immediately enables 2–3 new routes in prior regions.

### Art/UI framing (computer-world language)
- Rooms are “directories”; doors are “ports”; locks are “permissions/encryption.”
- Map is a “system topology”; save points are “checkpoints/restore points.”

### Success criteria for the overhaul
- A player can **get lost in a good way**: curiosity-driven exploration with consistent gating.
- Unlocks feel like **new verbs**, not just bigger numbers.
- A vertical slice can be shipped as a demo without “placeholder glue” systems.

