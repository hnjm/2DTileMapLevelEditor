---
Layout: page
title: Content
permalink: /content/
---

# Content

***

**Common**

- FiniteStack: stack used to store the path for the backward/forward feature (see credits)
- Utilities: class defining some project-independent methods

***

**[Simple File Browser](https://GracesGames.GitHub.io/SimpleFileBrowser)**

Used to save and load files without dependency on UnityEditor library

***

**2D Tile Map Level Editor**

_Materials_

- GridMaterial: material used to visualize the grid. Color can be changed in the LevelEditor script 

_Prefabs_

- TestTiles: tiles used to develop the demo scenes
- LevelEditor: GameObject used to handle user input and edit the level
- UI
    - LevelEditorUI: the user interface for the level editor

_Scenes_

- PlatformerDemo: a platformer game demo environment to demonstrate the tool
- SokobanDemo: a Sokoban game demo environment to demonstrate the tool

_Scripts_

- GridOverlay: the grid used to visualize the tiles
- LevelEditor: the logic that keeps track of input and updates the level 
- MoveCamera: allows the user to move the camera while using the level editor
- UI
    - UserInterface: the logic that sets up and updates the user interface based on information from the LevelEditor.

_Sprites_

- Sprites for testing and the user interface