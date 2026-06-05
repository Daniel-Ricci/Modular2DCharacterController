# Modular2DCharacterController

A modular 2D character controller for Unity built around reusable gameplay features and support for both Unity Input Systems.

The goal of this project is to provide a flexible foundation for 2D platformers while keeping gameplay features decoupled from input implementations.

---

## Current Features

### Input System Support

Supports both Unity input systems:

* Legacy Input Manager
* New Input System

Input providers implement a common interface, allowing gameplay features to remain independent of the chosen input solution.

Available providers:

* `LegacyInputProvider`
* `NewInputSystemProvider`

---

### Modular Feature Architecture

Character behavior is implemented through independent features.

Current implemented features:

* `HorizontalMovementFeature`
* `JumpFeature`

Features implement the `ICharacterFeature` interface and are automatically discovered by `CharacterController2D`.

This allows new features to be added without modifying the controller itself.

---

### Jump System

`JumpFeature` includes a modern platformer-style jump system with built-in quality-of-life mechanics.

Features:

* Multiple jumps
* Coyote time
* Jump buffering
* Variable jump height
* Fixed jump height option
* Jump hang time
* Custom gravity
* Faster fall gravity

#### Coyote Time

Allows jumps to occur shortly after leaving a platform.

This improves responsiveness and makes jumps feel more forgiving.

#### Jump Buffering

Allows jump input to be pressed shortly before landing.

The jump will automatically execute on landing if the buffer window is still active.

#### Variable Jump Height

When enabled:

* Tap jump for a short jump
* Hold jump for a full jump

#### Fixed Jump Height

When enabled:

* Every jump reaches the same height regardless of button hold duration

#### Jump Hang Time

Gravity is reduced near the apex of a jump.

This provides:

* Better aerial control
* Improved platforming precision
* More responsive jump feel

#### Custom Gravity

Jumping uses manually calculated gravity based on:

* Desired jump height
* Desired time to apex

This allows jump behavior to be tuned through gameplay values rather than trial-and-error physics settings.

---

### Ground Detection

`GroundDetector` provides:

* `IsGrounded`
* `GroundNormal`
* `GroundAngle`

Ground detection uses collider casting instead of manually placed ground check points, making it easier to support different collider shapes and future slope handling.

Features include:

* Layer filtering
* Surface normal detection
* Ground angle calculation
* Slope validation

---

## Project Structure

```text
Scripts/
├── Core/
│   ├── CharacterController2D
│   └── CharacterMotor
│   └── GroundDetector
│
├── Features/
│   ├── ICharacterFeature
│   ├── HorizontalMovementFeature
│   └── JumpFeature
│
├── Data/
│   └── JumpSettings
│
└── Input/
    ├── ICharacterInput
    ├── LegacyInputProvider
    └── NewInputSystemProvider
```

---

## Quick Start

### 1. Create a Player

Create a GameObject with:

* Rigidbody2D
* CapsuleCollider2D

Add the following components:

* CharacterController2D
* GroundDetector
* HorizontalMovementFeature
* JumpFeature

Choose one input provider:

* LegacyInputProvider
* NewInputSystemProvider

---

### 2. Configure Input

#### Legacy Input Manager

Uses:

* Horizontal
* Jump
* Roll

Ensure these entries exist in:

```text
Edit > Project Settings > Input Manager
```

---

#### New Input System

Create an Input Actions asset.

Example action map:

```text
Player
├── Move
├── Jump
└── Roll
```

Recommended setup:

```text
Move
└── 2D Vector Composite
    ├── W
    ├── A
    ├── S
    └── D
```

Assign the actions to:

* Move Action
* Jump Action
* Roll Action

on the `NewInputSystemProvider`.

---

### 3. Create Ground

Create a GameObject with:

* BoxCollider2D

Assign it to a Ground layer.

Configure the same layer inside `GroundDetector`.

---

### 4. Configure Jump Settings

Create a `JumpSettings` scriptable object asset and configure:

* Jump Height
* Time To Apex
* Fall Gravity Multiplier

Sample JumpSettings are provided, but a custom one can be made in Create > Modular 2D Character Controller > Jump Settings.

Optional gameplay tuning:

* Maximum Jump Count
* Coyote Time
* Jump Buffer Time
* Variable Jump Height
* Jump Hang Time

---

## Design Goals

* Modular architecture
* Reusable gameplay features
* Support for both Unity input systems
* Easy to extend
* Minimal setup
* Gameplay-driven jump tuning
* Future support for advanced platformer mechanics