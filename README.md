# Modular2DCharacterController

The Modular 2D Character Controller is a flexible framework for creating 2D platformer characters in Unity. Instead of relying on a single monolithic controller, it is built around a modular architecture where each gameplay mechanic is implemented as an independent feature.

Movement, jumping, dashing, wall interactions, gliding, ground pounding, and other mechanics are separated into self-contained modules that can be configured, extended, or replaced without affecting the rest of the controller. Gameplay parameters are stored in ScriptableObject profiles, allowing characters to be easily tuned or modified both in the Editor and at runtime.

The framework supports both Unity's Legacy Input Manager and the New Input System through a common input abstraction, making it easy to switch input solutions or implement custom input providers.

Whether you're creating a simple platformer or a more complex character system, the controller is designed to provide a clean, scalable foundation while remaining easy to customize and extend.

The following sections explain the project's architecture, demonstrate how to set up a character, describe each included feature, and provide an overview of the sample scenes and configuration profiles.

---

## Project Architecture

The Modular 2D Character Controller is built around a simple principle: each system should have a single responsibility. Rather than concentrating all movement logic in one large script, the controller is divided into small, specialized components that work together to produce the final character behaviour.

At the center of the framework is the CharacterController2D component. Its role is not to implement gameplay mechanics, but to coordinate them. Every component that implements the ICharacterFeature interface is automatically discovered when the character is initialized, and the controller calls each feature every frame (Tick) and every physics update (FixedTick). This means features operate independently while sharing the same update loop.

### CharacterController2D

The CharacterController2D component acts as the central coordinator for the character.

Its responsibilities are intentionally minimal:

Discover all character features attached to the GameObject.
Execute every feature each frame.
Execute every feature during each physics update.
Expose the profile providers used by the different movement systems.

Because the controller only coordinates features instead of implementing them directly, new mechanics can be added without modifying the controller itself. Any component implementing ICharacterFeature automatically becomes part of the controller's update loop.

### Character Features

Every movement mechanic is implemented as an independent feature.

Examples include:

* Horizontal Movement
* Jump
* Dash
* Roll
* Wall Jump
* Wall Slide
* Glide
* Ground Pound
* Run
* Crouch
* Platform Motion Transfer

Each feature is responsible only for its own behaviour. For example, the Jump Feature manages jumping, while the Dash Feature manages dashing. Neither feature needs to understand how the other works, which keeps the codebase modular and easy to maintain.

### CharacterMotor

The CharacterMotor is responsible for interacting with the Rigidbody2D.

Instead of features manipulating the Rigidbody2D directly, they perform movement through the motor. This creates a single location responsible for applying velocity and movement-related operations, ensuring all features interact with the physics system consistently.

By centralizing physics operations, the framework avoids conflicting movement code spread across multiple gameplay scripts.

### Detectors

The framework separates environment detection from gameplay logic.

Instead of every feature performing its own raycasts or overlap checks, specialized detector components continuously gather information about the character's surroundings.

The included detectors are:

* Ground Detector
* Wall Detector
* Ceiling Detector
* Ledge Detector

Features simply query these detectors whenever they need environmental information. For example, the Jump Feature checks whether the character is grounded, while the Wall Jump and Wall Slide features use the Wall Detector to determine whether a wall is available.

This approach avoids duplicated physics queries and keeps gameplay code focused on behaviour instead of collision detection.

### Character Status

The CharacterStatusProvider stores the current state of the character.

Rather than repeatedly recalculating information such as whether the character is grounded, jumping, rolling, gliding, or dashing, features can expose and consume shared state through the status provider.

This allows different systems to react to the character's current state without becoming tightly coupled to one another.

### Event Dispatcher

The framework includes an event dispatcher that broadcasts important character events.

Examples include landing, jumping, hitting a ceiling, beginning a dash, finishing a ground pound, and many other gameplay events.

Instead of external systems, including gameplay logic, animations, audio, particle effects, or custom scripts, having to reference every feature from the character, they can subscribe to these events without having to know all of the components included in the character.

This event-driven approach makes the controller significantly easier to extend.

### Profile Providers

Some configurable features have an associated profile provider.

Rather than referencing a single configuration asset, features obtain their settings from their corresponding provider. A provider can manage multiple profiles simultaneously and automatically returns the one with the highest priority.

This makes it possible to modify gameplay dynamically without changing the feature implementation. For example, different movement profiles can be activated while sprinting, crouching, swimming, or under the effects of a power-up.

The profile system is covered in detail later in this documentation.

### Input Providers

The controller does not communicate directly with Unity's input APIs.

Instead, input is supplied through an ICharacterInput implementation. The package includes providers for both Unity's Legacy Input Manager and the New Input System, while additional providers can be created for AI, networking, replay systems, or any custom input source.

Because every feature reads input through the same interface, the gameplay code remains completely independent of the underlying input system.

### Putting It All Together

During gameplay, the systems interact in the following order:

The input provider reads player input.
Detector components gather information about the environment.
CharacterController2D updates every registered feature.
Each feature evaluates its own conditions and performs its behaviour using the CharacterMotor.
Features retrieve their gameplay settings from the active profile providers.
Character events are raised as gameplay actions occur, allowing other systems to respond independently.

This separation of responsibilities is the foundation of the framework. Each component has a clearly defined role, making the controller easy to understand, extend, and maintain as additional gameplay mechanics are added.

---

## Setting Up a Character

This section provides a quick overview of the components required to create a character using the Modular 2D Character Controller. While each system will be explained in detail later in this documentation, following the steps below is enough to get a fully functional character up and running.

Character Hierarchy

A typical character hierarchy looks like this:

Player
├── Sprite
├── Rigidbody2D
├── Collider2D
├── CharacterController2D
├── CharacterMotor
├── CharacterStatusProvider
├── CharacterEventDispatcher
├── GroundDetector
├── WallDetector
├── CeilingDetector
├── LedgeDetector
├── Input Provider
├── Horizontal Movement Feature
├── Jump Feature
├── Dash Feature
├── ...

Depending on your game, your character may include more or fewer features. The controller automatically detects all attached features during initialization, so there is no additional setup required when adding or removing them.

### Rigidbody2D

The character requires a Rigidbody2D component to interact with Unity's physics system.

For most platformers, the recommended configuration is:

Body Type: Dynamic
Gravity Scale: 1 (or as required by your project)
Collision Detection: Continuous
Interpolation: Interpolate

The controller does not require any special Rigidbody settings beyond those typically used for 2D platformers.

### Collider

Attach the collider that best fits your character.

The framework works with any Collider2D type, although CapsuleCollider2D or BoxCollider2D are generally the most appropriate choices for player characters.

The collider dimensions should match the visible character sprite as closely as possible.

### Core Components

Add the following components to the character:

* CharacterController2D
* CharacterMotor
* The input provider of your choice

These components form the foundation of the controller and are required regardless of which gameplay features you intend to use.

### Detectors

Next, add the detector components required by the features you plan to use.

The package includes:

* Ground Detector
* Wall Detector
* Ceiling Detector
* Ledge Detector

Not every project requires every detector. For example, a simple platformer without wall mechanics may not need a Wall Detector or Ledge Detector.

Each detector should be configured according to the dimensions of your character and the collision layers used by your level geometry.

### Input Provider

Choose the input provider that matches your project.

The package includes implementations for both:

* Unity Legacy Input Manager
* Unity New Input System

Only one input provider should be attached to a character at a time.

Because all gameplay features communicate through a common input interface, switching input systems does not require any changes to the features themselves.

### Adding Features

Gameplay mechanics are added by attaching feature components to the character.

For example, a basic platformer character might include:

* Horizontal Movement
* Jump

A more advanced character could additionally include:

* Run
* Dash
* Wall Jump
* Wall Slide
* Glide
* Roll
* Ground Pound
* Platform Motion Transfer

The controller automatically discovers every attached feature during initialization. Simply adding or removing a feature component enables or disables that mechanic.

### Assigning Profiles

Most features require a configuration profile containing their gameplay parameters.

Create or select the appropriate ScriptableObject profile for each feature and assign it through the corresponding Profile Provider component.

Profiles contain all configurable values, such as movement speed, acceleration, jump force, cooldowns, and timing values, allowing gameplay to be adjusted without modifying code.

The profile system is explained in detail in a later section of this documentation.

### Testing the Character

Once all required components have been added:

Place the character in a scene.
Ensure the ground uses the collision layers configured by the detector components.
Press Play.
Verify that the attached features behave as expected.

If a mechanic does not function correctly, first confirm that its required detector and profile have been assigned. Most setup issues are caused by missing references or incorrect collision layer configuration.

At this point, your character is fully configured and ready to use. If necessary, the debug overlay can be added to provide useful information that may be necessary to adjust the features properties to your liking.

### Debug Overlay

The package includes a Debug Overlay that displays useful runtime information while testing your character. It is intended as a development tool to help verify that the controller is behaving as expected and to simplify troubleshooting during implementation.

To use it, simply add the Debug Overlay component to your character and configure which information should be displayed. Gizmos, vectors, a layout and even a key to show and hide the debug information can be configured. When the game is running, the overlay will automatically display live information about the active character.

This information can be invaluable when diagnosing unexpected behaviour, verifying detector configuration, or confirming that profile changes and gameplay states are being applied correctly.

The Debug Overlay is intended for development and testing only, and should be removed or disabled in production builds.

---

## Features Overview

Gameplay mechanics in the Modular 2D Character Controller are implemented as **features**. A feature is a self-contained component responsible for a single aspect of the character's behaviour, such as movement, jumping, dashing, or wall interaction.

Each feature operates independently and focuses on one specific responsibility. This modular approach keeps the controller easy to understand, simplifies maintenance, and allows new mechanics to be added without modifying existing systems.

Features are attached directly to the character as MonoBehaviour components. During initialization, the `CharacterController2D` automatically discovers every attached feature and updates them as part of the character's execution loop. No additional registration or configuration is required.

Most features expose their gameplay settings through ScriptableObject profiles, allowing their behaviour to be adjusted without modifying code. Multiple profiles can also be swapped at runtime using the profile provider system, making it easy to temporarily change the character's abilities during gameplay.

Each feature can be enabled or disabled independently, allowing you to build anything from a simple platformer character with basic movement and jumping to a more advanced controller with multiple traversal mechanics.

The following sections describe each feature in detail, including its purpose, configuration options, and any additional components or profiles required for it to function.

### Horizontal Movement

The Horizontal Movement feature is responsible for moving the character horizontally based on player input. It provides smooth acceleration, deceleration, turn acceleration, configurable maximum speed, and optional visual flipping.

Movement settings are defined through a **Horizontal Movement Profile**, making it easy to adjust the character's feel without modifying code. An optional **Air Movement Profile** can also be assigned to automatically use different movement settings while the character is airborne.

The feature supports preserving momentum above the configured maximum speed, allowing mechanics such as dashing or moving platforms to carry additional velocity naturally before gradually slowing the character down.

Character flipping can be performed either by changing a transform's local scale or by using a SpriteRenderer's **Flip X** property. A minimum input threshold can also be configured to ignore small analog stick movements.

This feature requires a **Horizontal Movement Profile** and an **Input Provider** to function.

### Run

The Run feature allows the character to temporarily switch to a different **Horizontal Movement Profile** while the run input is held. Instead of modifying movement values directly, it registers the assigned profile with the Horizontal Movement Profile Provider, allowing the Horizontal Movement feature to automatically use the new settings.

Running is only enabled while all of the following conditions are met:

* The run input is held.
* The horizontal movement input is greater than the configured minimum value.
* The character is grounded.

When any of these conditions is no longer true, the run profile is automatically removed and the previous movement profile becomes active again.

The feature exposes `StartedRun` and `StoppedRun` events, allowing other systems such as animations, audio, or visual effects to respond whenever the character starts or stops running.

This feature requires a **Horizontal Movement Profile**, an **Input Provider**, and a **Ground Detector**.

### Jump

The Jump feature handles all jump-related mechanics, including grounded jumps, air jumps, coyote time, jump buffering, variable jump height, and jump hang time.

Jump settings are defined through a **Jump Profile**, allowing jump height and gravity settings to be adjusted independently from the feature itself. Like all profile-based features, the active profile can be overridden at runtime through the profile provider system.

The feature supports **coyote time**, allowing the character to jump for a short period after leaving the ground, and **jump buffering**, which stores a jump input pressed shortly before landing and executes it automatically when possible. These mechanics make jumping feel more responsive and forgiving.

Multiple air jumps can be enabled by configuring the maximum number of air jumps. The feature also supports optional jumps immediately after an air dash or an edge-continuing air roll without consuming an available air jump.

Jump height can be either **variable**, where releasing the jump button early results in a shorter jump, or **fixed**, where every jump reaches the same height regardless of how long the button is held. An optional **Time to Apex** mode automatically calculates the required ascent gravity to reach the configured jump height in a specific amount of time.

To create a smoother jump arc, the feature can also reduce gravity near the top of the jump, producing a configurable "hang time" effect before the character begins to fall.

This feature requires a **Jump Profile**, an **Input Provider**, and a **Ground Detector**.

### Crouch

The Crouch feature allows the character to enter a crouching state, optionally reducing movement speed by activating a dedicated **Horizontal Movement Profile** while crouched.

Crouching can operate in either **Hold** or **Toggle** mode and can be configured to only activate while the character is grounded. It can also optionally require a minimum amount of movement input before crouching begins.

When enabled, the feature resizes the character's **BoxCollider2D** or **CapsuleCollider2D** while crouching. The crouched height is configurable, and the bottom of the collider can optionally remain fixed to prevent the character from sinking into the ground during the transition.

Before returning to the standing state, the feature checks whether there is enough room above the character. If an obstacle blocks the character from standing, the crouch state is maintained until there is sufficient clearance.

The feature exposes events for entering and exiting the crouch state, detecting when standing is blocked, and notifying when the collider shape changes. These events can be used to trigger animations, sound effects, or other gameplay logic.

This feature requires a **Horizontal Movement Profile**, an **Input Provider**, a **Collider2D** (BoxCollider2D or CapsuleCollider2D), and a **Ceiling Detector**. If **Grounded Only** is enabled, a **Ground Detector** is also required.

### Dash

The Dash feature allows the character to perform a burst of movement in a configurable direction for a fixed duration.

Dash behaviour is defined through a **Dash Profile**, which controls properties such as dash speed, duration, cooldown, available dash count, and other movement settings. Like other profile-based features, the active profile can be changed at runtime through the profile provider system.

The dash direction can be determined from the player's movement input or, if no input is provided, from the character's current facing direction. The feature also supports restricting dashes to the ground, the air, or allowing them in both.

The number of available dashes is configurable and is automatically restored based on the conditions defined in the dash profile, such as landing on the ground.

The feature exposes events when a dash starts and ends, making it easy to trigger animations, visual effects, sound effects, or other gameplay systems.

This feature requires a **Dash Profile**, an **Input Provider**, and a **Horizontal Movement** feature. Some dash reset options also require a **Ground Detector**.

### Roll

The Roll feature allows the character to perform a fast rolling movement along the ground. During a roll, the feature temporarily takes control of horizontal movement until the roll ends or is interrupted.

Roll behaviour is configured through a **Roll Profile**, which defines properties such as roll speed, duration, cooldown, and other movement settings. The active profile can be overridden at runtime through the profile provider system.

The roll direction can be based on either the current movement input or the character's facing direction. The feature can also be configured to prevent the character from rolling off ledges, automatically ending the roll if no ground is detected ahead.

If enabled, a roll can be interrupted by jumping, allowing the character to transition smoothly into a jump before the roll animation has completed.

The feature exposes events when a roll starts and ends, making it easy to synchronize animations, sound effects, particle effects, or other gameplay systems.

This feature requires a **Roll Profile**, an **Input Provider**, and a **Ground Detector**. If **Prevent Rolling Off Ledges** is enabled, a **Ledge Detector** is also required.

### Wall Slide

The Wall Slide feature slows the character's descent while they are in contact with a wall, allowing for more controlled movement and creating opportunities for wall jumps or other traversal mechanics.

Wall slide behaviour is configured through a **Wall Slide Profile**, which defines properties such as slide speed and the conditions required to enter the wall slide state.

The feature only activates when the configured conditions are met, such as being airborne, touching a wall, and providing movement input toward the wall. While wall sliding, the character's falling speed is limited according to the active profile.

The feature exposes events when wall sliding begins and ends, allowing animations, sound effects, and visual effects to react to the character entering or leaving the wall slide state.

This feature requires a **Wall Slide Profile**, an **Input Provider**, a **Wall Detector**, and a **Ground Detector**.

### Wall Jump

The Wall Jump feature allows the character to jump away from a wall while airborne, providing additional vertical and horizontal momentum to push the character away from the surface.

Wall jump behaviour is configured through a **Wall Jump Profile**, which defines properties such as jump force, jump direction, horizontal control lock duration, and other movement settings.

The feature only activates while the character is in contact with a wall and meets the configured jump conditions. It may or may not require the character to be wall sliding to perform a wall jump. After performing a wall jump, horizontal movement can be temporarily restricted, preventing the player from immediately steering back toward the wall.

The feature exposes an event when a wall jump is performed, allowing animations, sound effects, particle effects, or other gameplay systems to react accordingly.

This feature requires a **Wall Jump Profile**, an **Input Provider**, a **Wall Detector**, and the **Jump** feature. It also requires the **Wall Slide** feature, if it is a condition to perform the jump.

### Glide

The Glide feature allows the character to alter its vertical movement while airborne by limiting its fall speed to a configurable value defined in a **Glide Profile**.

In a traditional platformer, this is commonly used to slow the character's descent, giving the player greater control in the air and increasing the distance that can be traveled before landing.

However, the feature is not limited to reducing fall speed. By configuring a **negative fall speed**, the character will gradually move upward while gliding instead. This makes the feature suitable for a wider variety of movement mechanics, such as the spaceship demonstrated in the included sample scene, where holding the glide input causes the ship to ascend instead of descend.

The feature activates while the glide input is held and the configured conditions are met. Releasing the input or landing immediately ends the glide and restores normal gravity.

The feature exposes events when gliding starts and ends, making it easy to trigger animations, sound effects, particle effects, or other gameplay systems.

This feature requires a **Glide Profile**, an **Input Provider**, and a **Ground Detector**.

### Ground Pound

The Ground Pound feature allows the character to rapidly descend toward the ground while airborne. Once activated, the feature overrides the character's vertical movement until the character lands or the ground pound is otherwise cancelled.

Ground pound behaviour is configured through a **Ground Pound Profile**, which defines properties such as fall speed, activation conditions, and landing behaviour. The active profile can also be overridden at runtime through the profile provider system.

When the character lands, the feature can optionally apply a configurable bounce, allowing the player to immediately rebound into the air. It also exposes separate events for starting the ground pound, landing, and finishing the complete ground pound sequence, making it easy to synchronize animations, camera shake, sound effects, particle effects, or gameplay interactions.

This feature requires a **Ground Pound Profile**, an **Input Provider**, and a **Ground Detector**.

### Platform Motion Transfer

The Platform Motion Transfer feature allows the character to inherit the velocity of moving platforms while remaining fully responsive to player input.

When the character is standing on a moving platform, the feature retrieves the platform's velocity from the **Ground Detector** and applies it as a separate external velocity layer through the **Character Motor**. This allows the character to walk, jump, and perform other movement actions normally while still being carried by the platform.

The feature requires no additional configuration or profile. It simply transfers the velocity reported by the current ground object whenever the character is grounded.

This feature requires a **Ground Detector** and a **Character Motor**.

---

## Profile System

The Modular 2D Character Controller uses a profile-based architecture to configure the behaviour of its gameplay features. Instead of hardcoding movement values directly into each component, configurable data is stored in **ScriptableObject profiles**, allowing values to be shared across multiple characters, edited independently from code, and replaced dynamically at runtime.

Each configurable feature has its own profile type. For example, horizontal movement uses a **Horizontal Movement Profile**, jumping uses a **Jump Profile**, dashing uses a **Dash Profile**, and so on. Every profile inherits from the common **FeatureProfile** base class, which provides a single shared property: **Priority**.

### Profile Providers

Every supported profile type has a dedicated **Profile Provider** inside the `CharacterController2D`. These providers are responsible for determining which profile is currently active.

Rather than storing only a single profile reference, each provider maintains a collection of registered profiles. Whenever a profile is registered or removed, the provider automatically determines which profile has the highest priority and exposes it as the current active profile.

This makes profile selection completely automatic. Features simply request the current profile from their corresponding provider instead of worrying about which profile should be active.

### Runtime Profile Switching

One of the main advantages of the profile system is that features can temporarily override another feature's configuration without permanently modifying it.

For example, the **Run** feature does not directly increase the player's speed. Instead, when the player begins running, it registers its own **Horizontal Movement Profile** with the Horizontal Movement Profile Provider. If that profile has a higher priority than the default movement profile, it automatically becomes the active profile. When the player stops running, the Run feature unregisters its profile, causing the provider to immediately fall back to the next highest-priority profile.

This approach keeps features independent from one another while allowing them to cooperate naturally.

### Priority

Every feature profile contains a **Priority** value.

When multiple profiles of the same type are registered simultaneously, the Profile Provider always selects the profile with the highest priority. If only one profile is registered, it becomes the active profile automatically. If the active profile is removed, the provider immediately switches to the remaining profile with the next highest priority.

Because profile selection is entirely priority-driven, multiple systems can temporarily influence the same feature without requiring any feature-specific logic to resolve conflicts.

### Benefits

Using profiles instead of hardcoded values provides several advantages:

* Configuration is completely separated from implementation.
* Multiple characters can share the same movement settings.
* Gameplay values can be tuned without modifying code.
* Features can temporarily override another feature's behaviour by registering a higher-priority profile.
* Profile transitions are handled automatically by the Profile Provider, allowing features to remain simple and independent.

---

## Detectors

The detector components are responsible for sensing the character's surroundings and exposing that information to the rest of the controller. Rather than having each feature perform its own physics queries, all environment detection is centralized into dedicated detector components that update every physics frame.

This approach avoids duplicated physics checks, keeps features independent from one another, and ensures every system works from the same environmental data. For example, both the Jump and Crouch features rely on the same Ground Detector, while Wall Slide, Wall Jump, Roll, and other features share the same Wall and Ledge detectors.

### Ground Detector

The **Ground Detector** determines whether the character is standing on valid ground. In addition to exposing the grounded state, it provides detailed information about the surface beneath the character, including the ground normal, slope angle, contact point, collider, transform, movement velocity, and frame-to-frame movement delta.

Ground surfaces are filtered using configurable layer masks and a maximum slope angle, allowing steep surfaces to be rejected as ground. An ascending velocity threshold also prevents the character from becoming grounded while moving upward.

When standing on moving platforms, the detector can automatically retrieve the platform's velocity from its `Rigidbody2D`, allowing other systems—such as Platform Motion Transfer—to use accurate platform movement.

The Ground Detector also exposes **Landed** and **LeftGround** events that are used throughout the framework.

### Wall Detector

The **Wall Detector** detects walls on either side of the character using configurable layer masks and cast distances. It exposes whether the character is currently touching a wall, along with the wall's surface normal.

In addition to continuous wall detection, it provides methods that allow other systems to query for walls in arbitrary directions and offsets. This functionality is used by systems such as the Ledge Detector to perform additional environmental checks.

### Ceiling Detector

The **Ceiling Detector** detects obstacles above the character and provides information about the current ceiling, including its normal, angle, contact point, collider, and transform.

Besides determining whether the character is touching a ceiling, it also provides collision queries used to determine whether there is enough space for the character to stand after changing collider size, such as when exiting a crouch.

Support for one-way `PlatformEffector2D` colliders is built in, allowing ceilings to be ignored when approached from their pass-through side if desired.

The detector exposes a **CeilingHit** event that is raised when the character impacts a ceiling with sufficient upward velocity.

### Ledge Detector

The **Ledge Detector** builds upon the Ground Detector and Wall Detector to provide higher-level environmental information related to ledges.

It continuously checks whether there is ground ahead of the character, allowing it to determine when the character is standing at the edge of a platform. It also detects walls directly ahead and determines whether the space above those walls is clear, allowing features to identify climbable or traversable high ledges.

The detector exposes properties describing the ground and wall ahead, including their contact points, surface normals, colliders, and convenience states such as **IsOnGroundEdge** and **HasHighLedge**.

Unlike the other detectors, the Ledge Detector determines its queries based on the character's current facing direction, automatically following the direction reported by the Horizontal Movement feature when available.

---

## Character Status Provider

The **Character Status Provider** serves as a centralized, read-only access point for the character's current state. Instead of requiring external systems to know which detector or feature owns a particular piece of information, they can simply query the Status Provider.

The component does not contain any gameplay logic of its own. During initialization, it caches references to the character's detectors, motor, and gameplay features, then exposes their most commonly used runtime values through a single, unified API.

The information exposed by the Status Provider includes:

* **Movement**

  * Current Rigidbody velocity
  * Self velocity (produced by the Character Motor)
  * External velocity (moving platforms, external forces, etc.)
  * Current facing direction

* **Ground Information**

  * Whether the character is grounded
  * Ground normal
  * Ground angle
  * Current ground transform

* **Wall Information**

  * Whether the character is touching a wall
  * Wall normal

* **Ceiling Information**

  * Whether the character is touching a ceiling
  * Current ceiling transform

* **Ledge Information**

  * Whether the character is standing on a ground edge
  * Whether ground exists ahead
  * Whether a wall exists ahead
  * Whether the space above the wall is clear
  * Whether a high ledge has been detected

* **Jump State**

  * Whether a jump is currently active
  * Whether the character is ascending during a jump
  * Remaining air jumps

* **Feature States**

  * Running
  * Dashing
  * Dash direction
  * Remaining dashes
  * Rolling
  * Roll direction
  * Crouching
  * Stand blocked
  * Wall sliding
  * Wall jump control influence
  * Gliding
  * Ground pounding
  * Ground pound recovery

By aggregating these values into a single component, the Status Provider greatly simplifies integrations with systems such as UI, animation controllers, AI, camera systems, debugging tools, save systems, or custom gameplay scripts. External code can depend on a single component instead of referencing numerous detectors and gameplay features throughout the character.

---

## Character Event Dispatcher

The **Character Event Dispatcher** provides a single event hub for the character. Rather than requiring external systems to subscribe to events from every detector and gameplay feature individually, it listens to those events internally and re-exposes them through one centralized component.

During initialization, the dispatcher automatically locates all supported detectors and features attached to the character. When enabled, it subscribes to their events, forwards them through its own API, and automatically unsubscribes again when disabled.

The dispatcher exposes events for every major gameplay action implemented by the controller, including:

* **Ground Events**

  * Landed
  * LeftGround

* **Ceiling Events**

  * CeilingHit

* **Jump Events**

  * Jumped

* **Run Events**

  * StartedRun
  * StoppedRun

* **Dash Events**

  * Dashed
  * DashHit
  * DashEnded

* **Roll Events**

  * Rolled
  * RollHit
  * RollEnded

* **Crouch Events**

  * CrouchStarted
  * CrouchEnded
  * CrouchStandBlocked
  * CrouchColliderChanged

* **Glide Events**

  * GlideStarted
  * GlideEnded

* **Ground Pound Events**

  * GroundPoundStarted
  * GroundPoundInterrupted
  * GroundPoundFinished

* **Wall Slide Events**

  * WallSlideStarted
  * WallSlideEnded

* **Wall Jump Events**

  * WallJumped

Using the Event Dispatcher allows external systems to react to gameplay without depending on the internal architecture of the controller. For example, animation controllers, sound effects, particle systems, UI elements, achievements, analytics, and custom gameplay scripts can subscribe to a single component instead of maintaining references to every individual feature that produces events.

As new features are added to the framework, the dispatcher can be extended to expose their events as well, preserving a consistent integration point for external systems.

---

## Sample Scenes

The package includes several sample scenes demonstrating different ways the Modular 2D Character Controller can be configured. Each scene focuses on a different use case, ranging from a traditional platformer to an auto-runner and a physics-driven spaceship.

All sample scenes can be found under:

```text
Assets/Modular2DCharacterController/SampleScenes
```

### Simple Platformer Demo

The **Simple Platformer Demo** showcases the controller in a traditional platforming environment and demonstrates how multiple features work together during normal gameplay.

This scene includes examples of:

* Walking and running
* Variable-height jumping
* Dashing
* Ground pound
* Character animations and events

The level also contains interactive gameplay elements, including colored platform switches and question blocks, providing practical examples of how gameplay systems can interact with the controller.

**How to play**

* Move using the configured horizontal movement controls.
* Use the jump button to navigate platforms.
* Experiment with the different movement abilities to explore the level.
* Reach the end of the stage by interacting with the blocks by jumping, dashing or ground pounding on them.

---

### Fast Paced Demo

The **Fast Paced Demo** demonstrates how the controller can be used for high-speed platforming.

Instead of manually controlling horizontal movement, the player continuously moves forward using the included **AutoMoveFeature**, allowing the demo to focus on reaction time and movement chaining.

The scene is designed to showcase how features such as jumping, wall movement and dashing, can be configured for a different game feel from the simple platformer demo. It also demonstrates how simple it is to create a custom feature and integrate it with the controller.

**How to play**

* The character automatically moves forward.
* Use the available movement abilities to avoid obstacles and traverse the course.
* Reach the end of the level.

---

### Spaceship Demo

The **Spaceship Demo** demonstrates that the framework is not limited to traditional platformers.

In this scene, the Glide feature is configured with a **negative fall speed**, causing the character to accelerate upward while the glide input is held, creating a simple spaceship controller.

The demo illustrates how existing features can be repurposed to create entirely different gameplay styles without modifying the controller itself.

**How to play**

* Move left and right using the configured horizontal movement controls.
* Hold the glide input to make the ship ascend.
* Release the glide input to descend.
* Fly through the obstacles without colliding with them.

---

## Folder Structure

The package is organized into a small number of top-level folders that separate the runtime framework, sample content, and supporting assets.

```text
Modular2DCharacterController/
├── PhysicsMaterial/
├── Runtime/
│   ├── Camera/
│   ├── Core/
│   ├── Data/
│   │   └── FeatureProfiles/
│   ├── Debug/
│   ├── Features/
│   ├── Input/
│   │   └── NewInputSystem/
│   └── Prefabs/
└── SampleScenes/
    ├── FastPaced/
    ├── SimplePlatformer/
    └── Spaceship/
```

### PhysicsMaterial

Contains the physics materials used by the sample content. The included zero-friction material is used by the player controller to prevent unwanted friction against colliders.

### Runtime

Contains the entire runtime framework used by the controller.

#### Camera

Contains the included camera controller used by the sample scenes.

#### Core

Contains the core framework responsible for coordinating the character controller.

This folder includes:

* Character Controller
* Character Motor
* Ground, Wall, Ceiling, and Ledge Detectors
* Character Status Provider
* Character Event Dispatcher
* Character Hit Events and Receivers

These systems form the foundation upon which all gameplay features are built.

#### Data

Contains all ScriptableObject data used by the framework.

##### FeatureProfiles

Contains every feature profile and its corresponding sample assets. These ScriptableObjects define the configurable behaviour for features such as Horizontal Movement, Jump, Dash, Roll, Glide, Ground Pound, and Wall Jump.

This folder also contains the generic `ProfileProvider` and `FeatureProfile` base classes used by the profile system.

#### Debug

Contains debugging utilities, including the runtime Character Debug Overlay used by the sample scenes.

#### Features

Contains all modular gameplay features included with the package.

Each feature is implemented as an independent component that can be added or removed as needed, allowing you to build only the controller functionality required by your project.

#### Input

Contains the framework's input abstraction layer.

The package includes support for both Unity's legacy Input Manager and the new Input System, allowing either input backend to drive the same gameplay features.

#### Prefabs

Contains the ready-to-use Player prefab used throughout the sample scenes and as a starting point for your own projects.

### SampleScenes

Contains the example projects demonstrating different ways the framework can be configured.

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
