# STICKMAN TANK RUSH 🚗💥

🎥 **Gameplay Video**  

https://github.com/user-attachments/assets/f9800ffc-7451-4004-b6ea-9de460d1e674

**Stickman Tank Rush** is a simple yet fun hyper-casual game featuring colorful tanks and stickmen. In this game, players aim to match tanks and stickmen based on their colors, creating an engaging puzzle mechanic. The color match between the stickmen and tanks is a key factor in progressing through the game.

## 🎮 Features

- **Tank and Stickman Matching** 🟩🔴🟦: Stickmen with matching colors can board tanks. The color matching mechanic adds puzzle-like gameplay.
- **Grid-Based Movement System** 🔲: Stickmen navigate the grid, finding tanks or holder areas. The grid layout is dynamically generated and managed.
- **Dynamic Level Creation** 🎮✨: Each level uses **ScriptableObject** and **Array2DGrid** to define grid layout, tank positions, and more.
- **Tank & Holder Areas** 🟩🔲: Tanks are placed in grid cells, and filled holder areas end the game.
- **Colorful, Simple Graphics** 🌈: Visually appealing with easy-to-understand gameplay mechanics.


📸 **Additional Screenshots**  

<p align="center">
  <img src="https://github.com/SERAP-KEREM/StickmanTankRush/blob/main/Assets/GameImages/Ekran%20g%C3%B6r%C3%BCnt%C3%BCs%C3%BC%202025-01-28%20005354.png?raw=true" alt="Game Screenshot 1" width="200">
  <img src="https://github.com/SERAP-KEREM/StickmanTankRush/blob/main/Assets/GameImages/Ekran%20g%C3%B6r%C3%BCnt%C3%BCs%C3%BC%202025-01-28%20005409.png?raw=true" alt="Game Screenshot 2" width="200">
</p>
<p align="center">
  <img src="https://github.com/SERAP-KEREM/StickmanTankRush/blob/main/Assets/GameImages/Ekran%20g%C3%B6r%C3%BCnt%C3%BCs%C3%BC%202025-01-28%20005417.png?raw=true" alt="Game Screenshot 3" width="200">
  <img src="https://github.com/SERAP-KEREM/StickmanTankRush/blob/main/Assets/GameImages/Ekran%20g%C3%B6r%C3%BCnt%C3%BCs%C3%BC%202025-01-28%20005511.png?raw=true" alt="Game Screenshot 4" width="200">
</p>

## 🌟 How to Create a New Level

### 1. **Create a New ScriptableObject for Level Data** 📝
In Unity, go to the **Create** menu and select `Create > StickmanTankRush > LevelData`. This creates a **LevelDataSO** ScriptableObject file that holds all the information for your level.

### 2. **Define Grid Layout** 🌐
- Open the **LevelDataSO** file in the Unity Inspector.
- Edit the **Array2DGrid** field to specify the grid size and layout. This defines where tanks and stickmen will be placed.

### 3. **Add Tanks** 🚙
- In the **LevelDataSO** file, locate the **TankDataList** section.
- Add the tanks you want to appear in your level. Each tank can be assigned a unique color (handled by **ColorType**) to match with stickmen.

### 4. **Configure Stickmen Placement** 👨‍🚀
- Define where stickmen are placed on the grid. They will move to tanks of matching colors or holder areas if tanks are unavailable.

### 5. **Integrate New Level in Game** 🕹️
- In the **GameManager**, use the new **LevelDataSO** to load the grid and tank placements.
- Players can now play the newly created level!



## 🛠 Technologies Used

- **DOTween** 🎉: For smooth animations and transitions.
- **Array2D** 🌐: Manages 2D grid layout for tank and stickman placement.
- **ColorType** 🎨: Handles color matching between tanks and stickmen.
- **TriInspector** 🖥️: A tool to improve Unity’s interface for easy setup and editing.
- **SerapkeremGameTools** 🧰: Provides essential utilities, including Singleton patterns and input management.
- **GridPathfinder** 🛤️: For pathfinding and grid-based movement logic.

## 🔧 How to Play

1. **Clone the repository** ⬇️  
2. **Open the project in Unity** 🖥️  
3. **Run the game** 🎮: When you start the game, stickmen and tanks will be placed on a grid. Stickmen must find a matching tank or go to the holder area.
4. **Goal** 🏆: Match all stickmen to their corresponding tanks or move them to a holder area to win the level. Avoid filling all holder areas to avoid losing.

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](https://github.com/SERAP-KEREM/SERAP-KEREM/blob/main/MIT%20License.txt) file for details.
