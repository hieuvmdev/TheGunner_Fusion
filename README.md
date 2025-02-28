# TheGunner_Fusion

**TheGunner_Fusion** is a Unity-based multiplayer shooter game leveraging Photon Fusion for real-time networking. This project provides a foundation for developing fast-paced, networked games with Unity and Photon Fusion.

## Table of Contents

- [Features](#features)
- [Setup Instructions](#setup-instructions)
- [Testing the Project](#testing-the-project)
- [Tools and Libraries Used](#tools-and-libraries-used)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Real-time Multiplayer:** Seamless networking using Photon Fusion.
- **Third-Person Shooter Mechanics:** Core TPS gameplay elements.
- **Scalable Architecture:** Designed to support multiple players with minimal latency.
- **Completed Feature: **: Movement, Shooting, Leaderboard, System Message, Basic Game Flow.

## Setup Instructions

Follow these steps to set up the project locally:

### 1. Clone the Repository

```bash
git clone https://github.com/hieuvmdev/TheGunner_Fusion.git
cd TheGunner_Fusion
```

### 2. Install Unity

Ensure you have [Unity](https://unity.com/) installed. This project was developed using Unity version 2021.3.10f1. It's recommended to use this version or later.

### 3. Open the Project in Unity

1. Launch Unity Hub.
2. Click on the **"Open"** button.
3. Navigate to the cloned `TheGunner_Fusion` directory and select it.
4. Unity will load the project; this may take a few moments.

### 4. Import Photon Fusion

Photon Fusion is the networking library used in this project. To import it:

1. Open the Unity Asset Store or the [Photon Fusion website](https://doc.photonengine.com/fusion).
2. Download and import Photon Fusion into your project.
	
### 5. Configure Photon Fusion

1. 	Create a Photon account.
2. In the Photon Dashboard, create a new Fusion application to obtain your App ID.
3. In Unity, navigate to Photon Fusion > Fusion Hub > Setup.
4. Paste your App ID into the appropriate field. (You can use my Test App Id: ```57be1194-0b6b-4133-ad6f-959f41a97aa5```)

# Unity GameEditor Tutorial

## **Overview**
The `GameEditor` script is a custom **Unity Editor Window** that provides various tools to **manage scenes, start the game, and take screenshots**. This is useful for developers who want to quickly switch between scenes, start the game from a specific point, or capture in-game screenshots during development.

---

## **How to Use GameEditor**
### **1️. Open the GameEditor Window**
1. In Unity, navigate to the top menu bar.
2. Click on **"Tools" > "Scene Manager"**.
3. A custom Editor window will pop up with scene management tools.

---

### **2️. Start the Game**
- Click the **"Start Game"** button to:
  - Save the current scene.
  - Load the **Loading Scene** (`Assets/Scenes/Loading.unity`).
  - Enter Play Mode.

---

### **3️. Switch Between Scenes**
- The tool **automatically detects all enabled scenes** from `EditorBuildSettings`.
- Click on any scene name from the list to open it in the Editor.

---

### **4️. Open Player Settings**
- Click the **"Open Players Setting"** button to open **Build Settings**.

---

### **5️. Capture Screenshots**
- Enter the desired file path (default: `/Screenshots`).
- Click **"Capture Screenshot"** to save an image in the selected directory.

---

## Testing the Project With Game Editor

Once the project is set up, follow these steps to test the multiplayer functionality:

1. **Run the Server Instance:** Open `MainScene`, press Play in Unity Editor.
2. **Connect a Client:** Run another instance of the game (either in a new Editor window or a standalone build).
3. **Simulate Multiplayer:** Move characters, shoot, and test latency in real-time.

## Testing the Project 

1. **Window** Open `TheGunner_Fusion.exe` in folder Build/PC
1. **MacOS** Open `TheGunner_Fusion.exe` in folder Build/MacOS

## Tools and Libraries Used

- **Unity 2021.3.10f1** - Game engine.
- **Photon Fusion** - Networking library for real-time multiplayer.
- **C#** - Primary programming language.
- **Cinemachine** - Camera management system.
- **TextMeshPro** - Advanced text rendering.
- **Unity Input System** - Modern input handling.
- **DoTween** -  Efficient tweening engine.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
