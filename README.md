# Unity AI Capture the Flag Game
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/christopher-boustros/Unity-AI-Capture-the-Flag-Game)

A 3D first-person capture the flag game made with Unity in which the player must capture a treasure chest guarded by a Hierarchical Task Network (HTN) based AI. The game also contains mice controlled by steering behaviors to obstruct the player's motion. This was made as part of a course assignment for COMP 521 Modern Computer Games in fall 2020 at McGill University.

No third-party assets are used in this project.

You can run the game on GitHub Pages [**HERE**](https://christopher-boustros.github.io/Unity-AI-Capture-the-Flag-Game/)!

![Alt text](/Game_Screenshot_1.png?raw=true "Game Screenshot 1")

![Alt text](/Game_Screenshot_2.png?raw=true "Game Screenshot 2")

![Alt text](/Game_Screenshot_3.png?raw=true "Game Screenshot 3")
<br></br>

## How to run the game

### Requirements

You must have Unity version 2019.4.9f1 installed on your computer. Other versions of Unity may have compatibility issues.

### Running the game in Unity

Clone the master branch of this repository with `git clone --single-branch https://github.com/christopher-boustros/Unity-AI-Capture-the-Flag-Game.git`, or alternatively, download and extract the ZIP archive of the master branch. 

Open the Unity Hub, click on the Projects tab, click on the ADD button, and select the root directory of this repository.

Click on the project to open it in Unity.

In the Project window, double click on the `MainScene.unity` file from the `Assets/Scenes` folder to replace the sample scene.

Click on the play button to play the game.
<br></br>

## How to play

### Controls
W = move forward

A = move left

S = move backward

D = move right

Space bar = toggle shield

Mouse = move camera

### Gameplay
![Alt text](/Game_Screenshot_4.png?raw=true "Game Screenshot 4")

The game area consists of an entrance, an obstacle field, and a cave. The obstacle field contains numerous rocks, crates, and mice. The cave contains a treasure chest, which is guarded by a cave monster. 

Your goal is to capture the treasure chest and return to the entrance. Once you enter the obstacle field, the cave monster emerges from the cave and attempts to stop you from capturing the treasure chest by picking up and throwing rocks and crates towards you. Every time you are hit by a rock or a crate, your health drops by 1. To avoid being hit, you can take cover behind obstacles or activate your shield. The shield can be active for up to 10 seconds. You win the game once you return to the entrance with the treasure chest. You lose the game if your health drops to 0.
<br></br>

## Game features

### Hierarchical Task Network (HTN) based AI
The cave monster is controlled by an HTN-based AI. This is a type of AI that searches an HTN tree to generate a **plan**, which is an ordered set of primitive tasks that the cave monster will perform. The algorithm used to generate the plan is known as a **planner**. A **Hierarchical Task Network (HTN) tree** is a behavior tree that consists of primitive task, compound task, and method nodes. A **primitive task** is a single action that can be performed. A **compound task** is a task that can be performed by at least one set of subtasks (subtasks can be compound tasks or primitive tasks), so there can be different ways of performing one compound task. A **method** is one set of subtasks that can be performed to accomplish a compound task. Each primitive task node has a set of preconditions that must be satisfied to for the task to be performed and a set of postconditions that will be satisfied after the task is performed. These conditions allow the AI to generate a plan that is appropriate to the current state of the game.

This is the HTN tree used to control the cave monster:

![Alt text](/HTN_Tree.png?raw=true "HTN tree")

The circular nodes represent compound tasks, the diamond nodes represent methods, and the rectangular nodes represent primitive tasks. The curve around the edges of the method nodes indicates that the child nodes of the method must be performed in the order that they are drawn, from left to right. This game uses a **simple forward planner** that performs a depth-first search of the HTN tree to generate a plan for the cave monster to perform. The planner only generates a plan that satisfies the preconditions of each primitive task.

The behavior of the cave monster is as follows:
The monster will be inactive until the player enters the obstacle field for the first time. Then, the monster will alternate between Method 1, Method 2, and Method 3. So, the monster will walk around randomly for some time. Then, the monster may perform either a quick or slow rock or crate attack whenever it is close to a rock or a crate. The monster may only perform a quick attack if it is not near the player, and the monster may only perform a slow attack if it is near the player and the player is not near the cave. 

### Steering behaviors
The game starts with 100 mice moving around the obstacle field, each mouse pausing from time to time. The mice avoid obstacles with **steering behaviors**, meaning once they encounter an obstacle, they either steer towards the left or towards the right to avoid it. A **steering force** is a force that is applied to a mouse to change the direction of its velocity. The closer a mouse is to an obstacle, the greater the magnitude of its steering force. Steering behaviors allow the mice to avoid bumping into each other, the player, cave monster, walls, rocks, and crates in a realistic-looking manner. 

Whenever a mouse is stepped over by the cave monster or hit by a rock or crate, the mouse is removed from the game. The mice make the game slightly more challenging by obstructing the player's motion since the player cannot walk over them.

### Dynamically generated obstacles
Around 25 rocks and crates are generated at random positions and orientations along the obstacle field at the start of the game. Crates that are thrown by the cave monster are destroyed upon impact. Rocks thrown by the cave monster are only destroyed upon impact if they hit the player; otherwise, they remain in place. So, the number and position of obstacles changes over the course of the game.
<br></br>

## References

The implementation of character movement with the W/A/S/D keys in the `PlayerMovement.cs` script and the implementation of camera movement with the cursor in the `PlayerLook.cs` script is based on [this script](https://github.com/Brackeys/MultiplayerFPS-Tutorial/blob/702e31cb1f9a7f480b4f9673551804db2436488d/MultiplayerFPS/Assets/Scripts/PlayerController.cs) and [this script](https://github.com/Brackeys/MultiplayerFPS-Tutorial/blob/702e31cb1f9a7f480b4f9673551804db2436488d/MultiplayerFPS/Assets/Scripts/PlayerMotor.cs), which were released under the [Unlicense License](https://unlicense.org/).

## License

This repository is released under the [MIT License](https://opensource.org/licenses/MIT) (see LICENSE).