## Metroidvania Vertical Slice Spec (v0.1)

### Goal
Deliver a small, complete metroidvania loop that proves the core pillars:
- traversal feels good
- ability gating works
- combat is readable
- backtracking unlocks new routes

### Slice scope (content)
- **Rooms**: 10–15 total rooms in one mini-region (“FileSystem_Alpha”)
  - 1 hub/save room
  - 6–8 traversal rooms
  - 2–3 combat-focused rooms
  - 1 mini-boss arena
  - 1 boss arena
- **Abilities (3 unlocks)**:
  1. **PacketHook** (grapple to anchors)
  2. **PrivilegeEscalation** (wall hang/climb on special surfaces)
  3. **BurstTransfer** (dash upgrade)
- **Enemy set (3)**: reuse existing archetypes where possible
  - **ScannerDrone** (patrol + ranged/scan pressure)
  - **Charger** (commitment attack)
  - **Crawler** (ground pressure)
- **Boss (1)**:
  - **FirewallDaemon**: 2 phases, 3–4 attacks, clear telegraphs, reward unlock (ability 2 or 3)
- **Pickups (files)**:
  - “Keyfile” items (open locked gates)
  - “DataChunk” currency (optional, for later meta progression)

### Slice scope (systems)
- **Room system**: additive load/unload rooms by id
- **World graph**: rooms connected by exits; exits can be gated by required ability/flag
- **Player progress**: unlocked abilities + collected keyfiles + discovered rooms
- **Save/load**: save point writes progress; respawn uses last save point
- **UI minimum**:
  - health
  - equipped tools display (2 slots, can be dummy icons initially)
  - simple map screen (room nodes + current room highlight; no fancy art)

### Success criteria (“Definition of done”)
- Start from hub, reach boss via an initial route, earn an ability, then **backtrack** and open a previously blocked path.
- At least **one hard ability gate** (cannot pass without the new unlock).
- No major breakage when swapping rooms (player persists, managers persist).
- Debug hotkeys exist to accelerate iteration (teleport/unlock) even if UI is rough.

### Test plan (manual)
- Enter each room and confirm:
  - exits connect to the correct destination
  - gates block/unblock correctly based on abilities
  - room load/unload doesn’t duplicate managers or UI
- Boss defeat grants the intended unlock and enables a new route immediately.

