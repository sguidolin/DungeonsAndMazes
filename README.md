# Dungeons & Mazes

Little game about exploring a procedurally generated maze.

Move with `WASD` and shoot projectiles with `IJKL`, in a `NWSE` pattern (North, West, South, East).
The player will be given 5 projectiles to find and shoot the target.

### Menus

: `Play` -> Start the game.

: `Algorithm` -> View how the maze gets generated in a step-by-step generation (actual generation will not be slowed down).

: `Customize` -> Open the customization menu.

* `Seed` = A string to generate the random instance that will control every random generation for the session. Use the same seed to challenge the same dungeon;
* `Depth` & `Width` = The size of the maze;
* `Ratio` = % of how many tiles will be filled (size is `Depth x Width`);
* `Tunnels` = It's a toggle, by default it's on (`w/ = ON`, `w/o = OFF`);
* `Multi` = The number of players in the game, increase the slider for a local multiplayer session;

**Customize window *must* be open when pressing `Play` in order to start a customized game.**

To freely customize the ratio of events and such, refer to the script inside the `Dungeon` GameObject in the `DungeonPlay` scene.

### Rules

1) A player can only move in a direciton that isn't blocked;
2) A player can only shoot in a direction that isn't blocked;
3) A player can only shoot 5 projectiles. Once they're out, they're done;
4) Once a projectile has been shot, the game will wait until it has stopped traveling. If a player is hit by a projectile, they will be out, and with tunnels it's possible that a player hit themselves (though with the default settings it's highly unlikely to find a looping path);
5) Events close-by will be notified on top of the player, but only if it's in the close-by cells that you can reach. Tunnel paths will not be considered for this and lead to surprises;
6) In a multiplayer session, two players meeting up will trigger a death. The advantage is on the moving player, so if a player moves in a cell with another one they will kick them off the game;
7) Fog of war is shared between all players;
8) If a projectile hits a wall, the quest target will move somewhere still in the fog of war. If the entire map is revealed, it will move in a random position;
9) The game ends when a player hits the quest target, *or* all players are out.
