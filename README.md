# Stickman Tank Rush 🎮

🎥 **Gameplay Video**  

https://github.com/user-attachments/assets/f9800ffc-7451-4004-b6ea-9de460d1e674

**Stickman Tank Rush** is a simple yet fun hyper-casual game featuring colorful tanks and stickmen. In this game, players aim to match tanks and stickmen based on their colors, creating an engaging puzzle mechanic. The color match between the stickmen and tanks is a key factor in progressing through the game.

## 🎮 Features

- **Tank and Stickman Matching**: Stickmen can board tanks if their color matches. Tanks and stickmen are the core of the game, with color matching mechanics driving gameplay.
- **Grid-Based Movement System**: Stickmen move through empty grid cells to reach tanks. The system calculates the most optimal path based on adjacency relations.
- **Level and Grid Structure**: Each level is dynamically created using **ScriptableObject** and **Array2DGrid** for grid layout and tank placement.
- **Tank and Holder Areas**: Tanks can be placed in empty grid cells, but filled holder areas will cause you to lose.
- **Fun Graphics**: Simple, colorful graphics enhance the player experience visually.

📸 **Additional Screenshots**  

<p align="center">
  <img src="https://github.com/SERAP-KEREM/StickmanTankRush/blob/main/Assets/GameImages/Ekran%20g%C3%B6r%C3%BCnt%C3%BCs%C3%BC%202025-01-28%20005354.png?raw=true" alt="Game Screenshot 1" width="200">
  <img src="https://github.com/SERAP-KEREM/StickmanTankRush/blob/main/Assets/GameImages/Ekran%20g%C3%B6r%C3%BCnt%C3%BCs%C3%BC%202025-01-28%20005409.png?raw=true" alt="Game Screenshot 2" width="200">
</p>
<p align="center">
  <img src="https://github.com/SERAP-KEREM/StickmanTankRush/blob/main/Assets/GameImages/Ekran%20g%C3%B6r%C3%BCnt%C3%BCs%C3%BC%202025-01-28%20005417.png?raw=true" alt="Game Screenshot 3" width="200">
  <img src="https://github.com/SERAP-KEREM/StickmanTankRush/blob/main/Assets/GameImages/Ekran%20g%C3%B6r%C3%BCnt%C3%BCs%C3%BC%202025-01-28%20005511.png?raw=true" alt="Game Screenshot 4" width="200">
</p>



## 🛠 Technologies Used

- **DOTween**: Used for animations and transitions.
- **Array2D**: Utilized for efficient grid management and game layout.
- **ColorType**: Handles color matching between tanks and stickmen.
- **TriInspector**: Used to simplify the Unity interface for easier use.
- **SerapkeremGameTools**: Provides helper tools like Singleton structures and player input management.
- **GridPathfinder**: Used for pathfinding algorithms and adjacency checks.

## 🔧 How to Play

1. **Clone the repository**  
2. **Open the project in Unity**  
3. **Run the game**: When the game starts, stickmen and tanks will be placed on the grid. Stickmen must match with the correct tanks or move to the holder area.
4. **Game Goal**: The objective is to successfully match all stickmen and tanks to win the game, while avoiding filled holder areas.

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](https://github.com/SERAP-KEREM/SERAP-KEREM/blob/main/MIT%20License.txt) file for details.

