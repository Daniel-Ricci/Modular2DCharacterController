# Modular2DCharacterController

A modular 2D character controller for Unity built around reusable gameplay features, runtime profile overrides, and support for both Unity Input Systems.

The goal of this project is to provide a flexible foundation for 2D platformers while keeping gameplay features decoupled from input implementations and gameplay tuning data.

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

### Profile System

Gameplay tuning is driven by ScriptableObject profiles.

All profiles inherit from a common base class:

```text
FeatureProfile
├── HorizontalMovementProfile
└── JumpProfile
```

Each profile contains a Priority value.

Profiles are managed by generic `ProfileProvider<T>` instances stored inside `CharacterController2D`.

When multiple profiles are registered simultaneously, the provider automatically selects the highest-priority profile.

This enables runtime gameplay overrides without modifying feature logic.

Examples:

* Air movement while airborne
* Sprint movement while running
* Crouch movement while crouching
* Environmental modifiers
* Temporary powerups

Features consume only the currently active profile and remain unaware of where it originated.

---

### Horizontal Movement

`HorizontalMovementFeature` includes:

* Configurable movement profiles
* Acceleration
* Deceleration
* Turn acceleration
* Character facing support
* Multiple flipping modes

Supported facing modes:

* None
* Transform Scale
* SpriteRenderer Flip

Movement values are driven entirely by the currently active `HorizontalMovementProfile`.

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
* Air movement profile overrides

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

* Jump Height
* Time To Apex

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

## Profile Architecture

### Base Profile

All profiles inherit from:

```csharp
FeatureProfile
```

which contains:

```csharp
public int priority;
```

The priority determines which profile becomes active when multiple profiles are registered.

Higher priority wins.

---

### Profile Providers

Profile providers manage active profiles at runtime.

Current providers:

```csharp
ProfileProvider<HorizontalMovementProfile>
ProfileProvider<JumpProfile>
```

Providers support:

```csharp
RegisterProfile(...)
UnregisterProfile(...)
GetCurrentProfile()
```

The currently active profile is always the highest-priority registered profile.

---

### Example

Default movement:

```text
Walk Profile
Priority = 0
```

Air movement:

```text
Air Profile
Priority = 10
```

When airborne:

```text
Current Profile = Air Profile
```

When grounded:

```text
Current Profile = Walk Profile
```

No changes are required inside the movement feature itself.

---

## Creating Profiles

### Creating a Horizontal Movement Profile

Create:

```text
Create
└── Modular 2D Character Controller
    └── Horizontal Movement Profile
```

Configure:

* Priority
* Max Speed
* Acceleration
* Deceleration
* Turn Acceleration

Example:

```text
Priority = 0

Max Speed = 8
Acceleration = 80
Deceleration = 100
Turn Acceleration = 150
```

Assign the profile to:

```text
HorizontalMovementFeature
└── Default Movement Profile
```

---

### Creating a Jump Profile

Create:

```text
Create
└── Modular 2D Character Controller
    └── Jump Profile
```

Configure:

* Priority
* Jump Height
* Time To Apex
* Fall Gravity Multiplier

Example:

```text
Priority = 0

Jump Height = 4
Time To Apex = 0.4
Fall Gravity Multiplier = 2
```

Assign the profile to:

```text
JumpFeature
└── Default Jump Profile
```

---

## Runtime Profile Overrides

Features can temporarily override gameplay behavior by registering profiles.

Register a profile:

```csharp
provider.RegisterProfile(profile);
```

Remove a profile:

```csharp
provider.UnregisterProfile(profile);
```

Retrieve the active profile:

```csharp
provider.GetCurrentProfile();
```

The provider automatically selects the highest-priority registered profile.

---

### Example: Air Movement

`JumpFeature` can register an air movement profile when the character becomes airborne.

Grounded:

```text
Walk Profile
Priority = 0
```

Airborne:

```text
Air Profile
Priority = 10
```

Because the air profile has a higher priority, it becomes the active movement profile until the character lands.

---

## Project Structure

```text
Scripts/
├── Core/
│   ├── CharacterController2D
│   ├── CharacterMotor
│   └── GroundDetector
│
├── Features/
│   ├── ICharacterFeature
│   ├── HorizontalMovementFeature
│   └── JumpFeature
│
├── Data/
│   ├── FeatureProfile
│   ├── HorizontalMovementProfile
│   ├── JumpProfile
│   └── ProfileProvider
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
└── Jump
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

on the `NewInputSystemProvider`.

---

### 3. Create Ground

Create a GameObject with:

* BoxCollider2D

Assign it to a Ground layer.

Configure the same layer inside `GroundDetector`.

---

### 4. Create Movement Profiles

Create a `HorizontalMovementProfile`.

Assign it to:

```text
HorizontalMovementFeature
└── Default Movement Profile
```

---

### 5. Create Jump Profiles

Create a `JumpProfile`.

Assign it to:

```text
JumpFeature
└── Default Jump Profile
```

---

### 6. Press Play

Your character should now:

* Move
* Jump
* Support coyote time
* Support jump buffering
* Support multiple jumps
* Use profile-driven gameplay tuning

---

## Design Goals

* Modular architecture
* Profile-driven gameplay tuning
* Reusable gameplay features
* Support for both Unity input systems
* Easy to extend
* Minimal setup
* Runtime gameplay overrides
* Future support for advanced platformer mechanics
