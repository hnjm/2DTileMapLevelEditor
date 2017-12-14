---
Layout: page
title: Setup
permalink: /setup/

---

# Setup

**Usually you only have to drag your tile prefabs into the LevelEditor Tiles variable of the scene and press play**

***

**Basic and Clean Setup**

1. Create an instance of the LevelEditor prefab in the Hierarchy (drag and drop)

2. Create an instance of Canvas using the Unity create in the Hierarchy (Create -> UI -> Canvas)  
    My settings:
	- UI Scale Mode: Scale With Screen Size
	- Reference resolution: 1280 x 720
	- Rest as default

3. Attach the GridOverlay and MoveCamera script to the main camera

4. Choose the GridMaterial as the Line Material in the GridOverlay script component of the main camera

5. Setup of the LevelEditor prefab settings:
	- Set the desired height, width and amount of layer in the LevelEditor prefab
	- Add the prefab test tiles or your own files to the Tiles array
	- Rest as default
	
6. Set the Game Mode aspect ratio to 16:9 or resolution to 1280 x 720

7. Hit the play button

***

**UI Options**

The UI has a few options for scaling panels and buttons. 

These are self-explanatory and can be setup easily by the public variables in the UI prefab.

***