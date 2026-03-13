---
name: ""
overview: ""
todos: []
isProject: false
---

# Pixel Outline Hover Highlight (updated)

## Current state

- **PixelOutline shader** exposes `_OutlineThickness` and `_OutlineColour`. Thickness 0 = no outline; 1 = 1px outline.
- **OutlineSprite.mat** is already assigned on Employee, Station, and WalkableFloor. Default thickness is 0.
- **SetHoverHighlight(bool)** exists on Employee, Station, WalkableFloor, FloorMovementTarget but is empty.
- **EmployeeSelectionManager** does not track hover or call SetHoverHighlight.

---

## 1. Shared HoverHighlightHelper (no repeated logic)

Add a single place that applies or clears the outline so all hoverables use it.

- **New script**: `HoverHighlightHelper` (e.g. under `Assets/Scripts/` or `Assets/Scripts/UI/`).
- **Static API**, e.g.:
  - `ApplyHighlight(SpriteRenderer[] renderers, bool on, Color? highlightColour = null)`  
  - Or two methods: `SetHighlight(SpriteRenderer[] renderers, Color colour)` and `ClearHighlight(SpriteRenderer[] renderers)`.
- **Internals**:
  - Cache `Shader.PropertyToID("_OutlineThickness")` and `Shader.PropertyToID("_OutlineColour")` (static).
  - Use one **MaterialPropertyBlock** per call (or per component if you make it non-static). Set `_OutlineThickness` to 0 or 1 and `_OutlineColour` to the desired colour (e.g. white when on).
  - For each renderer: call `renderer.SetPropertyBlock(block)` when `on == true`, or `renderer.SetPropertyBlock(null)` when `on == false` so the material’s default (0 thickness) is used.
- **Callers** (no duplicate outline logic):
  - **Employee**: `GetComponent<SpriteRenderer>()` (or single renderer), pass to helper.
  - **Station**: `GetComponentsInChildren<SpriteRenderer>(true)` (optionally exclude progress bar), pass array to helper.
  - **Floor tiles**: use overlay quad (see section 3); do not use the helper on the floor sprite.

So each hoverable’s `SetHoverHighlight(bool on)` either calls `HoverHighlightHelper.ApplyHighlight(...)` (employees, stations) or shows/hides the overlay quad (floor tiles).

---

## 2. Hover tracking

- In **EmployeeSelectionManager**: each frame (after UI check), get OverlapPointAll, resolve **Employee first** then **IMovementTarget**. Keep a single “last hovered” (e.g. `IHoverable` or two refs). When hovered object changes, call `SetHoverHighlight(false)` on previous, `SetHoverHighlight(true)` on current.
- Optional: **IHoverable** with `SetHoverHighlight(bool)`; IMovementTarget extends it, Employee implements it; manager stores `IHoverable _lastHovered`.

---

## 3. Floor tiles: 32x32 full square + overlay quad

- **Floor tiles**: Full **32x32** tiles, entirely filled (no transparent border). No gaps between tiles.
- **Highlight**: Use an **overlay quad** per tile. Same shape for every tile (full square with border). When the tile is hovered, show the overlay; when not, hide it. The overlay can be:
  - A child GameObject with a **SpriteRenderer** (OutlineSprite material, 1px outline, e.g. simple full-tile sprite or white quad so only the outline is visible), same size as the tile, disabled by default and enabled in `SetHoverHighlight(true)`.
  - Or a pooled overlay instance per tile; either way, one overlay per tile, same shape and border.
- **WalkableFloor** / **FloorMovementTarget**: In `SetHoverHighlight(bool on)` only enable/disable (or show/hide) the overlay; no HoverHighlightHelper on the main floor sprite. Keeps floor art and logic simple.

**Workers / stations (non-tiled):**  
Use **30x30 content centered in 32x32** (1px transparent border) so the pixel outline has room. No gap issue because these sprites are not edge-to-edge.

---

## 4. Cleanup (do as part of implementation)

- **Station HoverOutline child**: Disable or remove the existing **HoverOutline** child GameObject on the Station prefab so only the shader-driven outline (via HoverHighlightHelper) is used. Prevents duplicate/legacy highlight.

---

## Files to touch

- **New:** `Assets/Scripts/HoverHighlightHelper.cs` – static API that applies/clears outline via MaterialPropertyBlock on given SpriteRenderer(s). Used by Employee and Station only.
- **IMovementTarget.cs** – optional IHoverable for cleaner hover tracking.
- **EmployeeSelectionManager.cs** – hover tracking, call SetHoverHighlight on current/last hovered.
- **Employee.cs** – SetHoverHighlight: get SpriteRenderer, call HoverHighlightHelper.
- **Station.cs** – SetHoverHighlight: get SpriteRenderers (all or filtered), call HoverHighlightHelper. Do not use the old HoverOutline child.
- **WalkableFloor.cs** – SetHoverHighlight: enable/disable overlay quad child (same shape + border for all tiles). No HoverHighlightHelper on main sprite.
- **FloorMovementTarget.cs** – same as WalkableFloor (overlay quad for highlight).
- **Prefab – Station**: Disable or remove the **HoverOutline** child so only the shader outline is used (cleanup).
- **Prefab – WalkableFloor** (and FloorMovementTarget if separate): Add or wire a child overlay quad (SpriteRenderer, OutlineSprite material, outline-on, same size as tile) and reference it for show/hide in SetHoverHighlight. Same shape for all tiles.

No changes to PixelOutline shader or OutlineSprite.mat; employees/stations use runtime property blocks via the helper; floor uses overlay quads.
