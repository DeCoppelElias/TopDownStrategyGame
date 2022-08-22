# Castle Defense
Castle Defense is a topdown strategy game where the goal is to destroy all enemy castles and be the last castle standing.
The main defining game feature is that you can draw the path your troops will follow. If you use this correctly you can evade enemy troops or towers.
You can freely use this code but please mention me. Thank You!

# Features
- Singleplayer against AI
- Multiplayer locally
- Level Creator

# Controls
- Move Camera: Arrow Keys or WASD  
- Settings: ESC  
- Spawn Troops: 1,2,3  
- Spawn Towers: 4,5,6  

# Features I'd like to add in the future
- More troops and towers
- Spells like a falling comet or a temporary wall
- A technology tree where you can unlock new troops, towers and spells for gold
- Maybe expand the Level Creator more

# Things I'm proud of / Things I learned
- The graphical design:   
      I have no background in drawing or graphical design but I still think the game looks pretty nice. I drew every sprite and animation myself.   
      I used a free drawing application call Krita (here's their website: https://krita.org/en/).   


- The Level Creator:  
      The Level Creator was not easy to implement. I used a couple design patterns to make it as expandable and understandable possible.  
      I used the state pattern to handle the different kinds of tools you can select (Normal drawing tool, Draw Line tool, Selecting tool, etc.)  
      I used the memento pattern to handle the undo and redo functionality. Each time a change is made, that change will be saved. This way it can later be undone or  redone if neccesary.  

- The Pathfinding:  
      For the Pathfinding I implemented the A* algorithm. This was harder than expected because I couldn't find a good implementation of a balanced tree. Because of this I used an AVL tree from a website and had to rewrite everything except the balancing. I did however learn a lot about balanced trees thanks to this.   
      I also impelented a method for creating semi-random paths. This is currently used by the Ai so they don't use the same path each time. I implemented this by creating additional random virtual obstacles and then searching the shortest path. The virtual obstacles are random so each Troop will have a different path.   
      Lastly I made it possible to visualize the pathfinding for debugging and satisfying reasons.  
