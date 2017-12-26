---
Layout: page
title: Setup
permalink: /setup/

---

# Setup

***

**Version 1.3**

Version 1.3 has __breaking changes__, so please read this before updating or risk losing your saved levels.

Instead of having the public Tiles list variable in the LevelEditor, the new TileSet class was created. This class allows the tiles list to be extracted and transformed into a prefab, allowing the user to reuse the tiles and keep them persistent more easily. 

After the update, the version 1.2 level editor script and prefab lose their Tiles variable and therefore the order of the tiles is lost. Since the levels are currently saved using the index of the tiles, the order is crucial for rebuilding the level using the correct tiles.

To transition smoothly, make sure you know the order of the tiles, e.g. take a screenshot of the LevelEditor prefab. Like this:

![TilesOrder]({{ "/assets/images/Setup/Tiles Order.png" | absolute_url }})

Then create a TileSet using the same order, transform it into a prefab and attach it to the LevelEditor TileSet variable. Like this:

![TileSet]({{ "/assets/images/Setup/TileSet.png" | absolute_url }})

This should allow you to load your levels and build them using the correct tiles order!

To prevent future order problems, version 1.3 also allows the user to save the level using the name of the tiles. 

***

**Create Tile Set**

1. Create an empty GameObject in the Hierarchy and attach the TileSet script

![CreateTileSetObject]({{ "/assets/images/Setup/Create TileSet Object.gif" | absolute_url }})

2. Add your tiles to the Tiles variable and create a prefab of the GameObject

![AddTilesToTileSet]({{ "/assets/images/Setup/Add Tiles to TileSet.gif" | absolute_url }})

***

**Clean Setup**

1. Create an instance of Canvas using the Unity create in the Hierarchy (Create -> UI -> Canvas)  
    My settings:
	- UI Scale Mode: Scale With Screen Size
	- Reference resolution: 1920 x 1080
	- Rest as default

![CanvasSettings]({{ "/assets/images/Setup/Canvas Settings.png" | absolute_url }})

2. Attach the GridOverlay and MoveCamera script to the main camera

3. Choose the GridMaterial as the Line Material in the GridOverlay script component of the main camera

![CameraScripts]({{ "/assets/images/Setup/Camera Scripts.png" | absolute_url }})

4. Create an instance of the LevelEditor prefab in the Hierarchy (drag and drop)

5. Setup of the LevelEditor prefab settings:
	- Set the desired height, width and amount of layer in the LevelEditor prefab
	- Add the created TileSet GameObject to the LevelEditor
	- Rest as default
	
![CreateLevelEditor]({{ "/assets/images/Setup/Create LevelEditor.gif" | absolute_url }})

6. Set the Game Mode aspect ratio to 16:9 or resolution to 1920 x 1080

7. Hit the play button

***

**UI Options**

The UI has a few options for scaling panels and buttons. 

These are self-explanatory and can be setup easily by the public variables in the LevelEditorUI prefab.

***