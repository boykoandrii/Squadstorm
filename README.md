# Squadstorm 🌪️

**Working Title:** Low-poly-Fodder

A modern mobile spiritual successor to the legendary *Cannon Fodder (1993)*, reimagined as a Roguelike Twin-Stick Shooter with vibrant, low-poly 3D visuals. 

## 🎮 About the Game

"War has never been so much fun!" – **Squadstorm** brings back the dark irony and tactical gameplay of classic squad shooters but introduces modern roguelike progression and intuitive mobile controls.

### Core Features:
- **Squad Management:** Control a small squad of soldiers that move as a single unit but take damage individually.
- **Twin-Stick Action:** Use the left stick for tactical movement (flanking, dodging) and the right stick for aiming and auto-firing.
- **Roguelike Progression:** 5-minute procedurally generated missions where you can pick up randomized perks (e.g., Heavy Ordnance, Medic Pack).
- **Permadeath & Recruits:** Fallen soldiers are gone for good and leave a tombstone. They are replaced by fresh "recruits" after the run.
- **Meta-Progression:** Upgrade your "Boot Camp" between runs to unlock permanent upgrades and airstrikes.

---

## 🛠 Technical Stack

This project is built from the ground up for mobile platforms with performance and modularity in mind.

- **Game Engine:** Unity 3D
- **Programming Language:** C#
- **Input Handling:** Unity Input System / Custom Mobile Joysticks
- **Physics:** Built-in Unity 3D Physics (Rigidbodies, Colliders)
- **Architecture:** Component-based design (separate controllers for Player Movement, Squad Members, and Projectiles)

---

## 🚀 Getting Started

Follow these steps to get the game running on your local machine:

### Prerequisites
- [Unity Hub](https://unity.com/download)
- **Unity Editor:** Recommended version `2022.3 LTS` (or newer)
- Git

### Installation
1. **Clone the repository:**
   ```bash
   git clone git@github.com:boykoandrii/Squadstorm.git
   ```
2. **Open the project:**
   - Launch **Unity Hub**.
   - Click on **Add** -> **Add project from disk** (or **Open**).
   - Select the `Squadstorm` folder.
3. **Open the Scene:**
   - Once the project is loaded, navigate to the `Assets/Scenes` folder in the Project window.
   - Double-click the main game scene to open it.
4. **Play:**
   - Press the **Play** button (▶️) at the top of the Unity Editor to test the game.

### Building for Mobile
- Go to `File > Build Settings`.
- Select your target platform (**iOS** or **Android**) and click **Switch Platform**.
- Once compiled, click **Build and Run** while your device is connected via USB.

---

## 🤝 Contributing
Currently under active development by [Andrii Boyko](https://github.com/boykoandrii).
