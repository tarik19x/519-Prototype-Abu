# Abu Simbel VR

A virtual reality travel application built for older adults, developed through a six-month co-design study. The starting point was not the application — it was the people. The temple of Abu Simbel, the teleportation-based movement, and the narrated tour were all decisions that came out of the co-design sessions, not assumptions made before them.

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

- [About the Project](#about-the-project)
- [How the Application Was Decided](#how-the-application-was-decided)
- [What the Application Does](#what-the-application-does)
- [Tech Stack](#tech-stack)
- [System Architecture](#system-architecture)
- [Script Reference](#script-reference)
- [Controls](#controls)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Build and Deployment (Meta Quest)](#build-and-deployment-meta-quest)
- [Roadmap and Known Limitations](#roadmap-and-known-limitations)
- [Credits](#credits)

---

## About the Project

Most VR applications are built without older adults in mind. Older users often have accessibility needs and interaction preferences that standard VR ignores — comfort with the controllers, a preference for teleportation over continuous locomotion, pacing of audio and narration, and so on. When applications are designed, these requirements are rarely considered.

This project was set up to do the opposite. The goal was to run a co-design process with older adults, gather their requirements first, and let the actual application take shape from what they wanted. The result is **Abu Simbel VR**, a virtual reality travel application for the Meta Quest, but the application is the output of the process rather than its premise.

The work ran for roughly six months. The first three months were spent on the co-design itself — establishing what to build and how to fit it to the participants' requirements. Development came afterward, with weekly check-ins so participants could react to each implementation as it was built.

---

## How the Application Was Decided

The application was not chosen up front. It was filtered out of the co-design sessions step by step.

**Familiarization.** Before any design work, participants were introduced to how VR and AR work. They tried existing applications from both industry and research, including consumer titles such as *Wander*, so that their feedback came from direct use rather than description.

**Finding the direction.** With that experience in place, the sessions moved into open discussion — what participants liked and disliked about VR, what felt comfortable, and what they would actually want to use. A virtual travel application came out of this. Further sessions narrowed down where they wanted to go; the destination could have been somewhere in Europe or elsewhere, and Abu Simbel was the place that emerged.

**Designing the interactions.** Specific interaction methods were worked out the same way. Three to four co-design weeks were spent discussing teleportation and locomotion methods alone, and the voice-narration approach was decided through its own set of sessions. The teleport movement, the single-button narrator switch, and the proximity-triggered audio in this codebase are all direct results of those discussions.

**Building it.** Once the direction and the interactions were settled, the application was developed and shown back to the participants week by week, so the implementation stayed close to what they had asked for.

A cohort of **10 older adults** took part throughout.

---

## What the Application Does

The user is placed inside a 3D model of the Abu Simbel temple complex (South Egypt) and can:

- **Teleport** between vantage points instead of walking, to reduce fatigue and discomfort.
- **Trigger narration** by approaching points of interest in the scene.
- **Switch between voice narrators** with a single controller button, each giving a different guided-tour voice.
- **Hear spatialized audio** rendered through the Meta XR Audio SDK.

A central audio service handles global volume and separate mute control for the music and effects channels, so the soundscape can be kept comfortable.

---

## Tech Stack

| Layer | Technology | Version |
| --- | --- | --- |
| Engine | Unity LTS | `2022.3.24f1` |
| Language | C# (Mono / .NET) | — |
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

The runtime audio layer carries most of the application's logic. It is kept small on purpose, so that it stayed easy to explain to participants during the co-design weeks.

```
                    +------------------------------+
                    |   SingletonMonoBehavior<T>   |   scene-persistent
                    |   (global access pattern)    |   service base class
                    +--------------+---------------+
                                   | inherits
                                   v
                    +------------------------------+
                    |         SoundManager         |   single audio authority
                    |  - musicSource (ambient)     |   - PlaySound(clip)
                    |  - effectsSource (SFX / VO)  |   - ChangeMasterVolume()
                    |  - SoundManager.Instance     |   - ToggleMusic / ToggleEffects
                    +--------------^---------------+
                                   | PlaySound(clip)
            +----------------------+----------------------+
            |                      |                      |
 +----------+---------+  +---------+--------+  +----------+---------+
 | ColliderController |  |    SfxHandler    |  |   VoiceManager     |
 | proximity trigger  |  | narrator-aware   |  | controller input   |
 | plays clip on      |  | playback, reads  |  | toggles the active |
 | object contact     |  | the counter      |  | narrator index     |
 +--------------------+  +---------+--------+  +----------+---------+
                                   |  reads counter       |
                                   +----------------------+
```

The ideas behind this layout:

- **One job per script** — input, narrator selection, playback, and triggering are each handled separately.
- **One audio authority** — every clip goes through `SoundManager`, so volume, muting, and channel rules live in a single place.
- **Global access through a singleton** — `SoundManager.Instance` is reachable from any trigger without wiring references across the scene.
- **A single shared index for narrators** — `VoiceManager` and `SfxHandler` share one integer (`counter`), which keeps the narrator toggle simple and predictable for the user.

---

## Script Reference

The first-party scripts are in `Assets/_Scripts/`, plus `Assets/AudioTrigger.cs`. Everything under `Assets/Samples/`, `Assets/Oculus/`, `Assets/UI Icons - 3D Low Poly Style/`, and `Assets/TutorialInfo/` is third-party SDK or asset-pack code.

### `SingletonMonoBehavior<T>`
A generic Singleton base class for `MonoBehaviour` services. On `Awake()` it caches the instance and destroys any duplicate in the scene, so there is always one global access point through the static `Instance` property.

```csharp
public abstract class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    public static T Instance { get; }
    // Destroys duplicates, caches the live instance on Awake.
}
```

### `SoundManager`
The single audio authority, derived from `SingletonMonoBehavior<SoundManager>`. It holds two `AudioSource` channels — `musicSource` for ambient sound and `effectsSource` for narration and effects — and exposes:

| Method | Purpose |
| --- | --- |
| `PlaySound(AudioClip clip)` | Plays a one-shot clip on the effects channel, guarding against overlap. |
| `ChangeMasterVolume(float val)` | Sets the global `AudioListener.volume`. |
| `ToggleMusic()` | Mutes and unmutes the ambient music channel. |
| `ToggleEffects()` | Mutes and unmutes the effects channel. |

### `VoiceManager`
Handles narrator switching from the controller. It reads `OVRInput.Button.Two` (the **B** button) and acts on the release of a press, advancing an index. The `counter` cycles modulo 2 across the two narrators and plays the matching `AudioSource`.

```csharp
isBPressed = OVRInput.Get(OVRInput.Button.Two);
// on release: counter = (counter + 1) % 2  ->  play GR or RM
```

### `SfxHandler`
Reads the current narrator index from `VoiceManager.counter` and sends the matching narrator clip (`GR` or `RB`) to `SoundManager.PlaySound()`. Selection stays in `VoiceManager`; playback stays in `SoundManager`.

### `ColliderController`
A proximity trigger. When a target object (for example `SoundIcon_FourStatus`) enters its collider through `OnTriggerEnter`, it plays the assigned `AudioClip` through `SoundManager`. This is how approaching an object in the scene starts its narration.

### `AudioTrigger`
A trigger scaffold tied to the `Player` tag, with `OnTriggerEnter` and `OnTriggerExit` hooks. It is the place to add region-based audio events such as entering or leaving a chamber.

---

## Controls

| Action | Input |
| --- | --- |
| Switch narrator | **B** button (`OVRInput.Button.Two`) |
| Teleport / movement | Meta XR Interaction SDK teleport interactor (controller ray + thumbstick) |
| Start object narration | Move into a point-of-interest trigger volume |

---

## Project Structure

```
519-Prototype-Abu/
|-- Assets/
|   |-- _Scripts/                  # First-party logic
|   |   |-- SingletonMonoBehaviour.cs
|   |   |-- SoundManager.cs
|   |   |-- VoiceManager.cs
|   |   |-- SfxHandler.cs
|   |   |-- ColliderController.cs
|   |-- AudioTrigger.cs            # Region trigger scaffold
|   |-- _Scenes/                   # Application scenes
|   |-- Audios/                    # Narrator voice tracks
|   |-- Models/                    # Abu Simbel temple (OBJ) and sound-icon models
|   |-- 360 Photos/                # 360-degree environment materials
|   |-- Materials/ . Images/       # Materials and textures
|   |-- UI Icons - 3D Low Poly Style/   # Third-party UI asset pack
|   |-- Samples/                   # Meta XR SDK sample scenes
|   |-- Oculus/ . XR/ . Plugins/Android/   # XR runtime and platform glue
|   |-- TextMesh Pro/ . Resources/ . Settings/
|-- Packages/                      # Unity Package Manager manifest
|-- ProjectSettings/               # Unity 2022.3.24f1 configuration
```

---

## Getting Started

### Prerequisites

- **Unity `2022.3.24f1`**, installed through Unity Hub. The version is pinned in `ProjectSettings/ProjectVersion.txt`.
- **Android Build Support** (SDK, NDK, OpenJDK) for Meta Quest deployment.
- A **Meta Quest** headset (Quest 2, 3, or Pro) with Developer Mode on.
- Meta XR SDK dependencies resolve through the Package Manager manifest.

### Clone and open

```bash
git clone https://github.com/tarik19x/519-Prototype-Abu.git
```

1. Open Unity Hub, choose **Add**, and select the cloned folder.
2. Let the Package Manager restore the dependencies (Meta XR SDK `63.0.0`, URP, GLTFUtility, and the rest).
3. Open the scene in `Assets/_Scenes/`.
4. Press **Play** with Quest Link or Air Link connected to preview in the editor, or build to the device as below.

---

## Build and Deployment (Meta Quest)

1. **File > Build Settings > Android**, then **Switch Platform**.
2. Check the XR setup under **Project Settings > XR Plug-in Management > Android > Oculus**.
3. Set **Texture Compression** to `ASTC`.
4. Connect the headset over USB, enable USB debugging, and accept the prompt inside the headset.
5. Pick the device under **Run Device** and choose **Build And Run** to deploy the APK.

---

## Roadmap and Known Limitations

This is a research prototype, and there are clear next steps:

- **`AudioTrigger`** is a scaffold; its `OnTriggerEnter` and `OnTriggerExit` bodies are placeholders for region-audio logic.
- **`SoundManager.PlaySound`** stops the effects channel when a clip is already playing rather than queuing — an anti-overlap guard that could grow into a proper playback queue.
- **Narrators are fixed to two voices** through a modulo-2 counter. Moving to an array of narrators would make the system data-driven.
- **`ColliderController`** matches objects by `gameObject.name`. Tags, layers, or a `ScriptableObject` registry would be more reliable.
- Add automated tests (the Unity Test Framework is already in the project) and an in-VR options menu for accessibility settings such as subtitles, narration speed, and comfort vignetting.

---

## Credits

- **Heritage subject:** Abu Simbel temple complex, South Egypt.
- **Voice narration:** the narrator tracks in `Assets/Audios/`.
- **Third-party packages:** Meta XR SDK (Audio, Interaction OVR, Platform), Unity Universal Render Pipeline, GLTFUtility by siccity, TextMeshPro, and the *UI Icons - 3D Low Poly Style* asset pack.
- **The 10 older adults** who took part in the co-design sessions and shaped what this application became.
