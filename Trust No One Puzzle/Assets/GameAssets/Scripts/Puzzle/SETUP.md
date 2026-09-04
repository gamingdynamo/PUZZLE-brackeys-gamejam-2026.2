# Puzzle systems — wire-up

Copy `Assets/GameAssets/Scripts/Puzzle` into the Unity project (same path).

These scripts talk to existing `IInteractable`, `PlayerInteraction`, `OpenableInteractable`, and `DialogueManager`.

## 1. Player carry

On the **Player** (same object as `PlayerInteraction`):

- Add `PlayerCarry`
- Assign `Hold Point` (empty child in front of the camera)
- Optional: bind `Drop Action` (e.g. G / gamepad B)
- Optional: bind `Spatial Mode Action` to mouse middle button (or skip and use HUD)
- Assign `Player Body` (character root) and `View Camera`

No other branch in the repo (Interactions / toolbox / Mobile_Screen) has box-shoving — only pick/drop furniture. This carry layer is the storage-room mover.

## 2. Placeable objects

On each pickup:

- Collider + Rigidbody
- `PlaceableItem`
  - `Item Id` unique string (`key_red`, `photo`, `wrench`…)
  - `Display Name` for prompts
  - **Carry Style**
    - `Handheld` — keys/tools; parented, rotate with the player/camera
    - `WorldStable` — follows hold position, **does not rotate** with look
    - `Spatial` — crates/boxes in the tight storage room (see below)

Layer must be on the interaction mask used by `PlayerInteraction`. Keep colliders enabled for Spatial/WorldStable so boxes stop on walls.

### Tight storage (Spatial)

While carrying a `Spatial` item:

| Input | Effect |
| --- | --- |
| Mouse wheel **up** | Push object **away from** camera |
| Mouse wheel **down** | Pull object **toward** camera |
| **MMB hold** or mobile HUD button | Shove in player space |
| Screen **X** while shoving | Player **local Y** (up) |
| Screen **Y** while shoving | Player **local Z** (forward) |

Yaw stays locked to the **player body** (not camera pitch), so looking up/down does not tip the crate. Movement is swept against `Obstruction Mask` so boxes do not tunnel through shelves.

**Mobile:** add a UI button with `SpatialMoveHudButton` (hold = MMB). Pointer down starts shove, pointer up stops. Enable `Toggle Instead Of Hold` if you want a sticky mode.

Set `Default / Min / Max Hold Distance` per crate so large boxes cannot clip into the camera.

Layer must be on the interaction mask used by `PlayerInteraction`.

## 3. Placement validation

Empty snap targets (table outline, drawer interior, pedestal):

- Collider (can be trigger)
- `PlacementSlot`
  - `Slot Id`
  - `Required Item Id` must match a `PlaceableItem.itemId`
  - `Snap Point` child transform
  - `Allow Wrong Items` — if true, player can seat the wrong object (fires wrong-placement)
  - `Lock When Correct` — prevents removing a solved piece

Look at the slot and press Interact while carrying to place.

## 4. Drawer unlock triggers

On a manager object (or on the furniture root):

- `DrawerUnlockTrigger`
  - `Drawer Id` (used by hints)
  - `Condition`
    - **All Slots Correct** — every listed slot has the right item
    - **Any Slot Correct**
    - **Specific Item In Any Slot** — uses `Required Item Id`
  - `Required Slots` — drag `PlacementSlot`s
  - `Drawers` — drag `OpenableInteractable`s that **Start Locked**
  - `Unlock Once` — stay unlocked after first solve

Those drawers must have `OpenableInteractable.startsLocked = true`. Unlock is `OpenableInteractable.Unlock()`.

## 5. Wrong hint system

On the same object as `DialogueManager` / Mobile UI:

- `WrongHintSystem`
- Fill **Hints** list:

| Field | Use |
| --- | --- |
| `Id` | unique, used to never-repeat |
| `Text` | message shown in the phone chat |
| `Is Misleading` | prefixes `[???]` vs `[hint]` |
| `Trigger On Wrong Slot Id` | send when that slot gets the wrong item |
| `Trigger On Correct Slot Id` | send when that slot is solved |
| `Trigger On Drawer Id` | send when that drawer unlocks |

Wrong placements also roll a random misleading hint (`Wrong Hint Chance`).

`DialogueManager.SendChatMessage` is used automatically if a manager exists in the scene.

## Example flow

1. Toolbox key (`itemId = key`) is a `PlaceableItem`.
2. Desk slot requires `key`.
3. Putting a photo in that slot → wrong hint on the phone.
4. Putting the key in → `DrawerUnlockTrigger` (AllSlotsCorrect) unlocks the nightstand drawer.
5. Drawer unlock hint: “Something just clicked in the nightstand.”
