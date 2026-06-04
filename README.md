# Modular2DCharacterController

A modular 2D character controller for Unity built around reusable features, interchangeable movement implementations, and support for both Unity Input Systems.

The goal of this project is to provide a flexible foundation for 2D platformers while keeping gameplay features decoupled from input and movement implementations.

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

Features implement the `ICharacterFeature` interface and are automatically discovered by `CharacterController2D`.

This allows new features to be added without modifying the controller itself.

Examples of planned features:

* Jump
* Roll
* Dash
* Wall Jump
* Wall Slide
* Ledge Grab
* Sprint

---

### Movement Motor System

Movement execution is separated from gameplay features.

Features express movement intent while motors determine how that movement is applied.

Current motor implementations:

* `VelocityMotor`
* `ForceMotor`

All motors implement:

* `ICharacterMotor`

This allows the same gameplay features to work with different movement implementations.

---

### Ground Detection

`GroundDetector` provides:

* `IsGrounded`
* `GroundNormal`
* `GroundAngle`

Ground detection uses collider casting instead of manually placed ground check points, making it easier to support different collider shapes and future slope handling.

---

## Project Structure

```text
Scripts/
├── Core/
│   ├── CharacterController2D
│   ├── GroundDetector
│   ├── ICharacterMotor
│   ├── VelocityMotor
│   └── ForceMotor
│
├── Features/
│   ├── ICharacterFeature
│   └── HorizontalMovementFeature
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
* VelocityMotor or ForceMotor
* GroundDetector
* HorizontalMovementFeature

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

### 4. Test Movement

Press Play.

The character should:

* Fall using Rigidbody2D physics
* Move left and right using the configured input provider

---

## Design Goals

* Modular architecture
* Reusable gameplay features
* Support for multiple movement implementations
* Support for both Unity input systems
* Easy to extend
* Minimal setup
* Future support for advanced platformer mechanics