---
Layout: page
title: File Format
permalink: /file_format/
---

# File Format

***

**Text Version**

The 2D Tile Map Level Editor loops trough the level and creates a string representation of the level.

The following hierarchy is used to save the levels:

1. Elements in one row
2. Rows in one layer
3. Layers in one level

The index of the prefab for each position is saved and separated using a character per hierarchy element.

1. Each element in one row is followed by a `,`
2. Each row in one layer is followed by a `\n (new line)`
3. Each layer in one level is followed by a `\t (tab)`

To create the level the right side up (Unity Y-axis goes down), the rows are reversed.

After that the level is serialized using a BinaryFormatter and saved to a file. 

***

**Code Version**

```csharp
// Save the level to a variable and file using FileBrowser and SaveLevelUsingPath
private void SaveLevel()
{
    List<string> newLevel = new List<string> ();
    // Loop through the layers
    for (int layer = 0; layer < LAYERS; layer++) {
        // If the layer is not empty, add it and add \t at the end"
        if (!EmptyLayer (layer)) {
            // Loop through the rows and add \n at the end"
            for (int y = 0; y < HEIGHT; y++) {
                string newRow = "";
                for (int x = 0; x < WIDTH; x++) {
                    newRow += +level [x, y, layer] + ",";
                }
                if (y != 0) {
                    newRow += "\n";
                }
                newLevel.Add (newRow);
            }
            newLevel.Add ("\t" + layer);
        }
    }

    // Reverse the rows to make the final version rightside up
    newLevel.Reverse ();
    string levelComplete = "";
    foreach (string level in newLevel) {
        levelComplete += level;
    }
    // Temporarily save the level to save it using SaveLevelUsingPath
    levelToSave = levelComplete;
    // Open file browser to get the path and file name
    OpenFileBrowser (FileBrowserMode.Save);
}
```

The variable `levelToSave` is then used in the `SaveLevelUsingPath` method shown below:

```csharp
// Save to a file using a path
public void SaveLevelUsingPath (string path)
{
    // Enable the LevelEditor when the fileBrowser is done
    ToggleLevelEditor (preFileBrowserState);
    if (path.Length != 0) {
        // Save the level to file
        BinaryFormatter bFormatter = new BinaryFormatter ();
        FileStream file = File.Create (path);
        bFormatter.Serialize (file, levelToSave);
        file.Close ();
        // Reset the temporary variable
        levelToSave = null;
    } else {
        Debug.Log ("Invalid path given");
    }
}
```

***

**Example String Representation (PlatformLevel.lvl)**

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