# Castle Defense
Castle Defense is a top-down strategy game where the goal is to destroy all enemy castles and be the last castle standing. The main defining game feature is that you can draw the path your troops will follow. If you use this correctly, you can evade enemy troops or towers. You can freely use this code, but please mention me. Thank You!   

# Features
- Singleplayer against AI   
- Multiplayer locally   
- Level Creator   

# Controls
Move Camera: Arrow Keys or WASD   
Settings: ESC   
Spawn Troops: 1,2,3   
Spawn Towers: 4,5,6   

# Features I'd like to add in the future
- More troops and towers   
- Spells like a falling comet or a temporary wall   
- A technology tree where you can unlock new troops, towers and spells for gold   
- Maybe expand the Level Creator more   

# Things I'm proud of / Things I learned
- The Multiplayer:
This is my first multiplayer game, so it was really hard to implement it. I used Mirror Networking for this (website: https://mirror-networking.com/).
Implementing multiplayer was really frustrating because, in my opinion, there was too little documentation and tutorials for everything. This made it really hard to understand certain concepts (ex. Client Server interactions with [Command], [ClientRpc], etc.). Debugging was also really hard. To debug the multiplayer, I had to run 2 different Unity Editors to test it. My pc isn't fast, so sometimes it would take ages before I found a bug. Now that I understand the concepts more, I can safely say that implementing my next multiplayer game will go much smoother. It was just really hard to learn the basics by myself.   

- The Level Creator:
The Level Creator was not easy to implement. I used a couple design patterns to make it as expandable and understandable possible.
I used the state pattern to handle the different kinds of tools you can select (Normal drawing tool, Draw Line tool, Selecting tool, etc.)
I used the memento pattern to handle the undo and redo functionality. Each time a change is made, that change will be saved. This way it can later be undone or redone if necessary.   

- The Pathfinding:
For the Pathfinding, I implemented the A* algorithm. This was harder than expected because I couldn't find a good implementation of a balanced tree. Because of this, I used an AVL tree from a website (https://simpledevcode.wordpress.com/2014/09/16/avl-tree-in-c/) and had to rewrite everything except the balancing. I did however learn a lot about balanced trees thanks to this.
I also implemented a method for creating semi-random paths. This is currently used by the Ai, so they don't use the same path each time. I implemented this by creating additional random virtual obstacles and then searching the shortest path. The virtual obstacles are random, so each Troop will have a different path.
Lastly, I made it possible to visualize the pathfinding for debugging and satisfying reasons.   

- The full system: I'm proud of the system I implemented. It is not perfect, but I think it's expandable and understandable. I'm planning on improving it and adding better/more documentation.   

- The graphical design:
I have no background in drawing or graphical design, but I still think the game looks pretty nice. I drew every sprite and animation myself.
I used a free drawing application called Krita (website: https://krita.org/en/).   
