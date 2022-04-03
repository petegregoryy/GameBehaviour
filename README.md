# GameBehaviour
Project for Game Behaviour modue at University of Derby.

## Controls
- **R** - Start and Restart
- **Space** - Fly forward
- **Left/Right** - Turn left and right
- **D** - Open/Close Debug menu
- **Q** - Quit game

Fly around the screen collecting pink cirlces for points. Avoid asteroids as you will lose points. Avoid the enemy as he will kill you.

Game is similar to asteroids. Asteroids have collisions using SAT collision algorithm and will move around using Newtonion Physics.
There is 1 enemy AI, a blue diamond which moves around the level using Djikstras pathfinding algorithm. The pathfinder uses rudimentary prediction for moving objects, attempting to avoid asteroids when moving between locations.
The enemy will move into an alerted mode if the player gets too close. If the player is too close for 2 seconds the enemy will fly straight at the player. If the player gets hit the game is over.
The pathfinding is based on an occupancy grid, which lets the pathfinder know which cells can be used.
