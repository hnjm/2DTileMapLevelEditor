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

_DemoLevels_

- Examples of platformer and Sokoban levels, saved using the Index and Name tile identification method

_Materials_

- GridMaterial: material used to visualize the grid. Color can be changed in the LevelEditor script 

_Prefabs_

- LevelBuilder: GameObject used to build the saved levels independent of the level editor. Allows the users to load and build their saved levels in their game
- LevelEditor: GameObject used to handle user input and edit the level
- TileSet: GameObject that stores a collection of building block tiles used to build levels, required for the level editor.
- TestTiles: tiles and tile sets used to develop the demo scenes
- UI
    - ButtonPrefab: GameObject used to represent the building block tiles in the level editor
    - LevelEditorUI: the user interface for the level editor

_Scenes_

- LevelBuilderDemo: a level builder demo environment to demonstrate the level builder.
- PlatformerDemo: a platformer game demo environment to demonstrate the tool
- SokobanDemo: a Sokoban game demo environment to demonstrate the tool

_Scripts_

- GridOverlay: the grid used to visualize the tiles
- MoveCamera: allows the user to move the camera while using the level editor
- LevelEditor: the logic that keeps track of input and updates the level 
- TileSet: collection of tiles used to build the level
- LevelBuilder
    - LevelBuilder: loads and builds a level given a path
    - LevelBuilderDemoCaller: example level builder called to illustrate the usage of the LevelBuilder script
- Functionalities
    - The functionalities of the level editor divided per script 
- UI
    - UserInterface: the logic that sets up and updates the user interface based on information from the LevelEditor.

_Sprites_

- Sprites for testing and the user interface