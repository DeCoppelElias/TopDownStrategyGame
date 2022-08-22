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
      It was not easy to implement. I used a couple design patterns to make it as expandable and understandable possible.
      I used the state pattern to handle the different kinds of tools you can select (Normal drawing tool, Draw Line tool, Selecting tool, etc.)
      I used the memento pattern to handle the undo and redo functionality. Each time a change is made, that change will be saved. This way it can later be undone or  redone if neccesary.
