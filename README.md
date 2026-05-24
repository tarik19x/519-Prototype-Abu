# Abu Simbel VR — An Accessible Virtual Heritage Experience for Older Adults

> A room-scale virtual reality application that lets users explore a 3D reconstruction of the **Abu Simbel Temple** (South Egypt) through teleport-based locomotion, diegetic audio narration, and low-effort controller interactions — designed *with* older adults through a multi-month co-design process.

<p align="left">
  <img alt="Unity" src="https://img.shields.io/badge/Unity-2022.3.24f1_LTS-000000?logo=unity">
  <img alt="Platform" src="https://img.shields.io/badge/Platform-Meta_Quest_(Android)-1C1E20?logo=oculus">
  <img alt="Render Pipeline" src="https://img.shields.io/badge/Render-URP_14.0.10-2096ED">
  <img alt="Meta XR SDK" src="https://img.shields.io/badge/Meta_XR_SDK-63.0.0-0467DF">
  <img alt="Language" src="https://img.shields.io/badge/Language-C%23-239120?logo=csharp">
  <img alt="Status" src="https://img.shields.io/badge/Status-Research_Prototype-orange">
</p>

---

## Table of Contents

- [Overview](#overview)
- [Research Motivation](#research-motivation)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [System Architecture](#system-architecture)
- [Script Reference](#script-reference)
- [Controls](#controls)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Build & Deployment (Meta Quest)](#build--deployment-meta-quest)
- [The Co-Design Process](#the-co-design-process)
- [Roadmap & Known Limitations](#roadmap--known-limitations)
- [Credits & Acknowledgements](#credits--acknowledgements)

---

## Overview

**Abu Simbel VR** is an immersive heritage-tourism prototype built in Unity for the Meta Quest family of head-mounted displays. The user is placed inside a 3D virtual model of the Abu Simbel temple complex and can:

- **Teleport** between curated vantage points instead of free-walking, reducing fatigue and motion sickness.
- **Trigger contextual audio** by approaching or interacting with points of interest within the scene (the "museum objects").
- **Switch between multiple voice narrators**, each offering a different guided-tour personality, with a single controller button.
- **Listen to spatialized audio** rendered through the Meta XR Audio SDK for a believable sense of place.

The application was not built as a generic VR demo. Every interaction pattern — locomotion, button mapping, audio pacing, and UI affordances — was **tailored to an older-adult audience** and refined iteratively through direct user feedback.

---

## Research Motivation

Mainstream VR experiences assume dexterity, spatial confidence, and prior gaming literacy that many older adults do not have. They also tend to rely on physical movement that is uncomfortable or inaccessible for users with limited mobility.

This project asks a focused question:

> *What does a virtual heritage experience look like when older adults shape it from the very first design decision?*

The answer drove concrete engineering choices throughout the codebase:

| Design goal | Engineering decision |
| --- | --- |
| Minimize physical strain | Teleport-based locomotion rather than continuous movement |
| Reduce cognitive & motor load | A single, repeatable button toggle for narrator switching (`OVRInput.Button.Two`) |
| Keep the user oriented and calm | Proximity-triggered audio that plays *to* the user rather than requiring precise aiming |
| Provide a warm, human guide | Pre-recorded human voice narratives instead of synthetic text-to-speech |
| Avoid sensory overload | Centralized audio management with global volume and per-channel mute controls |

---

## Features

- 🏛️ **Explorable 3D Abu Simbel temple** imported as an OBJ/glTF model with authored materials and textures.
- 🚶 **Teleport locomotion** powered by the Meta XR Interaction SDK (OVR), chosen for comfort and accessibility.
- 🎧 **Spatialized 3D audio** via the Meta XR Audio SDK for immersive, positional sound.
- 🗣️ **Multi-narrator system** — switch between distinct guided-tour voices on the fly with one button.
- 🎯 **Proximity / collision-triggered narration** at points of interest inside the scene.
- 🔊 **Centralized audio service** with master-volume control and independent music / SFX mute toggles.
- 🖼️ **360° photo materials** and 3D iconography to anchor the heritage context.
- 👵 **Accessibility-first interaction design** validated through iterative co-design sessions.

---

## Tech Stack

| Layer | Technology | Version |
| --- | --- | --- |
| Engine | Unity LTS | `2022.3.24f1` |
| Language | C# (.NET / Mono) | — |
| Render pipeline | Universal Render Pipeline (URP) | `14.0.10` |
| XR runtime | Unity XR Management + Oculus XR Plugin | `4.4.1` / `4.2.0` |
| Interaction | Meta XR Interaction SDK (OVR) | `63.0.0` |
| Spatial audio | Meta XR Audio SDK | `63.0.0` |
| Platform | Meta XR Platform SDK | `62.0.0` |
| Model loading | GLTFUtility (siccity) | git package |
| UI / Text | TextMeshPro | `3.0.6` |
| Visual logic | Unity Visual Scripting | `1.9.2` |
| Target hardware | Meta Quest (Android build target) | — |

---

## System Architecture

The runtime audio layer is the heart of the application. It is organized around a small, deliberately simple set of responsibilities so that non-engineering collaborators could reason about it during co-design sessions.

```
                    ┌─────────────────────────────┐
                    │   SingletonMonoBehavior<T>   │   generic, scene-persistent
                    │   (global access pattern)    │   service base class
                    └──────────────┬──────────────┘
                                   │ inherits
                                   ▼
                    ┌─────────────────────────────┐
                    │        SoundManager         │   one audio authority
                    │  • musicSource (ambient)    │   • PlaySound(clip)
                    │  • effectsSource (SFX/VO)   │   • ChangeMasterVolume()
                    │  • SoundManager.Instance    │   • ToggleMusic / ToggleEffects
                    └──────────────▲──────────────┘
                                   │ PlaySound(clip)
              ┌────────────────────┼────────────────────┐
              │                    │                    │
   ┌──────────┴─────────┐ ┌────────┴────────┐ ┌─────────┴──────────┐
   │  ColliderController│ │   SfxHandler    │ │   VoiceManager     │
   │  proximity trigger │ │  narrator-aware │ │  controller input  │
   │  → plays clip on   │ │  playback,      │ │  → toggles active  │
   │    object hit      │ │  reads counter  │ │    narrator index  │
   └────────────────────┘ └────────┬────────┘ └─────────┬──────────┘
                                    │   reads counter    │
                                    └────────────────────┘
```

**Design principles applied**

- **Single Responsibility** — each script does exactly one thing (input, selection, playback, triggering).
- **Centralized audio authority** — all playback routes through `SoundManager` so volume, muting, and channel policy live in one place.
- **Global access via Singleton** — `SoundManager.Instance` is reachable from any trigger or interaction without scene-wide references.
- **State-driven narrator selection** — narrator choice is a single integer (`counter`) shared between `VoiceManager` and `SfxHandler`, keeping the toggle logic trivial and predictable for users.

---

## Script Reference

All first-party gameplay scripts live in `Assets/_Scripts/` (plus `Assets/AudioTrigger.cs`). Everything under `Assets/Samples/`, `Assets/Oculus/`, `Assets/UI Icons - 3D Low Poly Style/`, and `Assets/TutorialInfo/` is third-party SDK or asset-pack code.

### `SingletonMonoBehavior<T>`
A reusable generic base class implementing the Singleton pattern for `MonoBehaviour` services. On `Awake()` it caches the instance and destroys any duplicate that appears in the scene, guaranteeing a single global access point via the static `Instance` property.

```csharp
public abstract class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    public static T Instance { get; }
    // Destroys duplicates, caches the live instance on Awake.
}
```

### `SoundManager`
The single audio authority for the application, derived from `SingletonMonoBehavior<SoundManager>`. It owns two `AudioSource` channels — `musicSource` (ambient bed) and `effectsSource` (narration / SFX) — and exposes:

| Method | Purpose |
| --- | --- |
| `PlaySound(AudioClip clip)` | Plays a one-shot clip on the effects channel, guarding against overlapping playback. |
| `ChangeMasterVolume(float val)` | Sets the global `AudioListener.volume`. |
| `ToggleMusic()` | Mutes / unmutes the ambient music channel. |
| `ToggleEffects()` | Mutes / unmutes the effects channel. |

### `VoiceManager`
Handles **narrator switching** from the VR controller. It polls `OVRInput.Button.Two` (the **B** button) and detects a *release* event (debounced press-then-release) to advance an index. The `counter` cycles modulo 2 across the two available narrators and triggers the corresponding `AudioSource` directly.

```csharp
isBPressed = OVRInput.Get(OVRInput.Button.Two);
// on release: counter = (counter + 1) % 2  →  play GR or RM
```

### `SfxHandler`
A lightweight bridge that reads the current narrator index from `VoiceManager.counter` and routes the matching narrator clip (`GR` or `RB`) through `SoundManager.PlaySound()`. This keeps narrator selection and playback decoupled — selection lives in `VoiceManager`, playback policy lives in `SoundManager`.

### `ColliderController`
A **proximity / collision trigger**. When a designated object (e.g. `SoundIcon_FourStatus`) enters its collider via `OnTriggerEnter`, it plays the assigned `AudioClip` through the `SoundManager`. This is the mechanism behind "walk up to a museum object and hear about it."

### `AudioTrigger`
A trigger scaffold wired to the `Player` tag with `OnTriggerEnter` / `OnTriggerExit` hooks — the extension point for region-based audio events (entering/leaving a chamber or zone).

> **Narrators.** The two voice tracks shipped in `Assets/Audios/` correspond to the `GR` and `RB`/`RM` references in code, giving users a choice of guided-tour personality.

---

## Controls

| Action | Input |
| --- | --- |
| Switch narrator | **B** button (`OVRInput.Button.Two`) |
| Teleport / locomotion | Meta XR Interaction SDK teleport interactor (controller ray + thumbstick) |
| Trigger object narration | Move into a point-of-interest trigger volume |

---

## Project Structure

```
519-Prototype-Abu/
├── Assets/
│   ├── _Scripts/                  # First-party gameplay logic
│   │   ├── SingletonMonoBehaviour.cs
│   │   ├── SoundManager.cs
│   │   ├── VoiceManager.cs
│   │   ├── SfxHandler.cs
│   │   └── ColliderController.cs
│   ├── AudioTrigger.cs            # Region trigger scaffold
│   ├── _Scenes/                   # Application scenes
│   ├── Audios/                    # Narrator voice tracks (multiple narrators)
│   ├── Models/                    # Abu Simbel temple (OBJ) + sound-icon models
│   ├── 360 Photos/                # 360° environment materials
│   ├── Materials/ · Images/       # Authored materials & textures
│   ├── UI Icons - 3D Low Poly Style/   # Third-party UI asset pack
│   ├── Samples/                   # Meta XR SDK sample scenes (Audio / Core / Interaction)
│   ├── Oculus/ · XR/ · Plugins/Android/   # XR runtime & platform glue
│   └── TextMesh Pro/ · Resources/ · Settings/
├── Packages/                      # Unity Package Manager manifest
└── ProjectSettings/               # Unity 2022.3.24f1 project configuration
```

---

## Getting Started

### Prerequisites

- **Unity `2022.3.24f1`** (install via Unity Hub — the version is pinned in `ProjectSettings/ProjectVersion.txt`).
- **Android Build Support** module (SDK, NDK, OpenJDK) for Meta Quest deployment.
- A **Meta Quest** headset (Quest 2 / 3 / Pro) with Developer Mode enabled.
- Meta XR SDK dependencies resolve automatically through the Package Manager manifest.

### Clone & open

```bash
git clone https://github.com/tarik19x/519-Prototype-Abu.git
```

1. Open **Unity Hub → Add → Open Project** and select the cloned folder.
2. Let the Package Manager restore all dependencies (Meta XR SDK `63.0.0`, URP, GLTFUtility, etc.).
3. Open the scene under `Assets/_Scenes/`.
4. Press **Play** with Quest Link / Air Link connected to preview in-editor, or build to device (below).

---

## Build & Deployment (Meta Quest)

1. **File → Build Settings → Android**, then **Switch Platform**.
2. Confirm XR setup under **Project Settings → XR Plug-in Management → Android → Oculus**.
3. Set **Texture Compression** to `ASTC`.
4. Connect the headset via USB, enable USB debugging, and allow the connection prompt inside the headset.
5. Select your device under **Run Device** and click **Build And Run** to deploy the `.apk`.

---

## The Co-Design Process

This prototype is the output of a **6–7 month** participatory development cycle that placed older adults at the center of every decision.

**Participants.** A cohort of **10 older adults** contributed throughout the project.

**Phase 1 — Familiarization.** Participants were introduced to the fundamentals of virtual and augmented reality. They tried established VR products from both industry and research (including consumer applications such as *Wander*), so that feedback came from lived experience rather than abstraction.

**Phase 2 — Co-design (≈3 months).** In structured sessions the team gathered feedback on:
- what participants enjoyed and disliked about VR,
- preferred locomotion and interaction styles,
- comfort with the controllers, and
- their requirements and expectations for a heritage experience.

**Phase 3 — Iterative build.** The application was developed *on the fly* alongside these sessions. Each week the team demonstrated the latest implementation, and controller interaction patterns were customized directly from participant input — producing the teleport locomotion, single-button narrator toggle, and proximity-triggered narration seen in the codebase.

The result is a VR pipeline deliberately re-tailored from defaults to fit the needs, comfort, and preferences of an older-adult audience.

---

## Roadmap & Known Limitations

This is a **research prototype** (as reflected in the repository name), and there are clear opportunities to harden it for production:

- **`AudioTrigger`** is currently a scaffold; its `OnTriggerEnter` / `OnTriggerExit` bodies are placeholders awaiting region-audio logic.
- **`SoundManager.PlaySound`** stops the effects channel when a clip is already playing rather than queuing or layering — a deliberate anti-overlap guard that could be extended into a proper playback queue.
- **Narrator references are hard-coded to two voices** via a modulo-2 counter. Generalizing to an array of narrators would make the system data-driven and scalable.
- **`ColliderController`** matches points of interest by `gameObject.name` string comparison; migrating to tags, layers, or a `ScriptableObject` registry would be more robust.
- Add automated tests (the project already includes the Unity Test Framework) and accessibility settings exposed through an in-VR options menu (subtitles, narration speed, comfort vignetting).

---

## Credits & Acknowledgements

- **Heritage subject:** Abu Simbel Temple complex, South Egypt.
- **Voice narration:** multiple pre-recorded narrator tracks (see `Assets/Audios/`).
- **Third-party packages:** Meta XR SDK (Audio, Interaction OVR, Platform), Unity Universal Render Pipeline, GLTFUtility by *siccity*, TextMeshPro, and the *UI Icons – 3D Low Poly Style* asset pack.
- **With deepest thanks to the 10 older-adult co-designers** whose feedback shaped every interaction in this application.

---

*Built with Unity and the Meta XR SDK as an accessibility-focused virtual heritage prototype.*
