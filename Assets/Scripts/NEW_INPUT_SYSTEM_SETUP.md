# New Input System Setup Guide

## Overview
All player input scripts have been converted from the legacy `Input` class to the new **Input System** package. This provides better performance, flexibility, and support for multiple input devices.

## Converted Scripts
1. **PlayerController.cs** - Movement, jumping, sprinting, looking, and cursor unlock
2. **PlayerGrabInteraction.cs** - Item grabbing (E) and throwing (Q)
3. **PlayerHiding.cs** - Hiding (H) and unhiding (L)

## Required Input Actions

The following Input Actions must be defined in your `InputSystem_Actions.inputactions` file:

### Player Input Actions Required:

1. **Move** (Value Type: Value, Control Type: Vector2)
   - Keyboard: W, A, S, D (or arrow keys)
   - Gamepad: Left Stick

2. **Look** (Value Type: Value, Control Type: Vector2)
   - Keyboard: Mouse X and Y axes
   - Gamepad: Right Stick

3. **Jump** (Value Type: Digital/Button)
   - Keyboard: Space
   - Gamepad: South Button (A on Xbox)

4. **Sprint** (Value Type: Digital/Button)
   - Keyboard: Left Shift
   - Gamepad: Left Stick Click

5. **Unlock** (Value Type: Digital/Button)
   - Keyboard: Left Alt or Right Alt
   - Gamepad: (Optional - can be unmapped)

6. **Grab** (Value Type: Digital/Button)
   - Keyboard: E
   - Gamepad: (Optional - can be unmapped)

7. **Throw** (Value Type: Digital/Button)
   - Keyboard: Q
   - Gamepad: (Optional - can be unmapped)

8. **Hide** (Value Type: Digital/Button)
   - Keyboard: H
   - Gamepad: (Optional - can be unmapped)

9. **Unhide** (Value Type: Digital/Button)
   - Keyboard: L
   - Gamepad: (Optional - can be unmapped)

## How to Set Up in Unity

### 1. Open Input Actions Editor
- In Project window, find and double-click `InputSystem_Actions.inputactions`
- This opens the Input Actions Editor

### 2. Create Action Map (if not already present)
- Click "Create new Action Map"
- Name it "Player" (or similar)

### 3. Add Actions
For each action listed above:
- Click "Create new Action"
- Name it exactly as specified
- Set the Action Type (Value or Button)
- For Vector2 actions (Move, Look): Set Control Type to Vector2

### 4. Bind Keys to Actions
- Under each action, click "Create new Binding"
- Select the control (keyboard key, mouse axis, gamepad button)
- Repeat for alternative controls

### Example Bindings:

**Move Action:**
- Binding 1: W key → Up in Vector2
- Binding 2: A key → Left in Vector2
- Binding 3: S key → Down in Vector2
- Binding 4: D key → Right in Vector2

**Look Action:**
- Binding 1: Mouse X
- Binding 2: Mouse Y (Negative: Yes)

**Jump Action:**
- Binding 1: Space

**Sprint Action:**
- Binding 1: Left Shift

**Grab Action:**
- Binding 1: E

**Throw Action:**
- Binding 1: Q

**Hide Action:**
- Binding 1: H

**Unhide Action:**
- Binding 1: L

### 5. Configure Player Component
- Select your Player GameObject in the scene
- Add a **PlayerInput** component (if not present)
- In the Inspector, set:
  - **Actions**: Drag the `InputSystem_Actions` asset
  - **Default Control Scheme**: (leave as is, or set to your preferred scheme)
  - **Behavior**: "Send Messages" or "Invoke Unity Events"

### 6. Verify Setup
- In the Game view, test all input:
  - WASD/Arrows for movement
  - Mouse for looking
  - Space for jumping
  - Shift for sprinting
  - Alt for cursor unlock
  - E for grabbing items
  - Q for throwing items
  - H for hiding
  - L for unhiding

## Troubleshooting

### Input Not Working?
- ✅ Verify PlayerInput component is on the Player GameObject
- ✅ Check that action names match exactly (case-sensitive)
- ✅ Ensure InputSystem_Actions is assigned in PlayerInput component
- ✅ Save and reimport the InputSystem_Actions file

### Action Not Found Error?
- The action name might be missing from InputSystem_Actions
- Add the missing action and rebind keys
- Restart the scene

## Benefits of New Input System
- ✅ Better performance
- ✅ Easy gamepad/controller support
- ✅ Flexible keybinding system
- ✅ Rebindable controls
- ✅ Multiple input device support
- ✅ Better event-driven architecture

## Next Steps (Optional)
- Create a settings UI to allow players to rebind keys
- Add gamepad rumble feedback
- Add support for additional control schemes
- Save/load keybindings with PlayerPrefs
