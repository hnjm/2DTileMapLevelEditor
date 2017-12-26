---
Layout: page
title: File Format
permalink: /file_format/
---

# File Format

***

**Tile Identification Method**

The 2D Tile Map Level Editor currently allows for two methods of identifying the tile building blocks, namely index and name.

The index method saves the index of the tile in the TileSet list, while the name method saves the name of the tile.

The index method allows renaming of the tiles without affecting saved levels, while the name method allows reordering of the tiles without affecting saved levels.  

**Save Method**

The 2D Tile Map Level Editor loops trough the level and creates a string representation of the level.

The following hierarchy is used to save the levels:

1. Elements in one row
2. Rows in one layer
3. Layers in one level

The index or name of the prefab for each position is saved and separated using a character per hierarchy element.

1. Each element in one row is followed by a `,`
2. Each row in one layer is followed by a `\n (new line)`
3. Each layer in one level is followed by a `\t (tab)`

To create the level the right side up (Unity Y-axis goes down), the rows are reversed.

After that the level is serialized using a BinaryFormatter and saved to a file. 

***

**Save Code**

```csharp
// Save the level to a variable and file using FileBrowser and SaveLevelUsingPath
private void SaveLevel() {
    int[,,] levelToSave = _levelEditor.GetLevel();
    int width = _levelEditor.Width;
    int height = _levelEditor.Height;
    int layers = _levelEditor.Layers;
    List<string> newLevel = new List<string>();
    // Loop through the layers
    for (int layer = 0; layer < layers; layer++) {
        // If the layer is not empty, add it and add \t at the end"
        if (!EmptyLayer(levelToSave, width, height, layer, LevelEditor.GetEmpty())) {
            // Loop through the rows and add \n at the end"
            for (int y = 0; y < height; y++) {
                string newRow = "";
                for (int x = 0; x < width; x++) {
                    newRow += TileSaveRepresentationToString(levelToSave, x, y, layer) + ",";
                }
                if (y != 0) {
                    newRow += "\n";
                }
                newLevel.Add(newRow);
            }
            newLevel.Add("\t" + layer);
        }
    }

    // Reverse the rows to make the final version rightside up
    newLevel.Reverse();
    string levelComplete = "";
    foreach (string level in newLevel) {
        levelComplete += level;
    }
    // Temporarily save the level to save it using SaveLevelUsingPath
    _levelToSave = levelComplete;
    // Open file browser to get the path and file name
    OpenFileBrowser();
}
```

The variable `levelToSave` is then used in the `SaveLevelUsingPath` method shown below:

```csharp
// Save to a file using a path
public void SaveLevelUsingPath(string path) {
    // Enable the LevelEditor when the fileBrowser is done
    _levelEditor.ToggleLevelEditor(_preFileBrowserState);
    if (path.Length != 0) {
        // Save the level to file
        BinaryFormatter bFormatter = new BinaryFormatter();
        FileStream file = File.Create(path);
        bFormatter.Serialize(file, _levelToSave);
        file.Close();
        // Reset the temporary variable
        _levelToSave = null;
    } else {
        Debug.Log("Invalid path given");
    }
}
```

***

**Example Index String Representation (PlatformIndexExample.lvl)**

`\t` and `\n` added for visual clarification. 

String starts with `\t0` to indicate layer 0.

```csharp
\t0-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,\n
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,\n
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,\n
-1,1,11,1,-1,-1,-1,-1,-1,-1,4,-1,-1,-1,-1,-1,\n
-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,-1,\n
-1,2,-1,-1,-1,0,-1,-1,-1,-1,-1,3,-1,-1,-1,-1,\n
5,5,8,-1,-1,-1,-1,-1,-1,-1,3,-1,-1,2,-1,-1,\n
10,10,6,8,2,-1,-1,12,-1,-1,9,5,5,5,5,5,\n
10,10,10,6,5,5,5,5,5,5,7,10,10,10,10,10,\n
10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,\n
```

**Example Name String Representation (PlatformNameExample.lvl)**

`\t` and `\n` added for visual clarification. 

String starts with `\t1` to indicate layer 1.

```csharp
\t1Crate Brown,Crate Brown,Crate Brown,Crate Brown,Crate Brown,Crate Brown,Crate Brown,Crate Brown,\n
Crate Brown,EMPTY,EMPTY,EMPTY,EMPTY,EMPTY,EMPTY,Crate Brown,\n
Crate Brown,EMPTY,EMPTY,EMPTY,EMPTY,EMPTY,EMPTY,Crate Brown,\n
Crate Brown,EMPTY,EMPTY,Player,EMPTY,Crate Blue,EMPTY,Crate Brown,\n
Crate Brown,EMPTY,EMPTY,EMPTY,EMPTY,EMPTY,EMPTY,Crate Brown,\n
Crate Brown,Crate Goal Blue,EMPTY,EMPTY,EMPTY,EMPTY,EMPTY,Crate Brown,\n
Crate Brown,EMPTY,EMPTY,EMPTY,Coin,EMPTY,EMPTY,Crate Brown,\n
Crate Brown,Crate Brown,Crate Brown,Crate Brown,Crate Brown,Crate Brown,Crate Brown,Crate Brown,\t0Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,\n
Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,\n
Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,\n
Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,\n
Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,\n
Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,\n
Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,\n
Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,Ground Rock,\n
```