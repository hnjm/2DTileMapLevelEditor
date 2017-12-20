using UnityEngine;

using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using GracesGames.Common.Scripts;
using GracesGames.SimpleFileBrowser.Scripts;

namespace GracesGames._2DTileMapLevelEditor.Scripts.Functionalities {

	public class SaveLoadFunctionality : MonoBehaviour {

		private LevelEditor _levelEditor;

		// Temporary variable to save level before getting the path using the FileBrowser
		private string _levelToSave;

		// Temporary variable to save state of level editor before opening file browser and restore it after save/load
		private bool _preFileBrowserState = true;

		// FileBrowser Prefab to open Save- and LoadFilePanel
		public GameObject FileBrowserPrefab;

		// File extension used to save and load the levels
		public string FileExtension = "lvl";

		public void Setup() {
			_levelEditor = LevelEditor.Instance;
			FileExtension = FileExtension.Trim() == "" ? "lvl" : FileExtension;
			SetupClickListeners();
		}

		// Hook up Save/Load Level method to Save/Load button
		private void SetupClickListeners() {
			Utilities.FindButtonAndAddOnClickListener("SaveButton", SaveLevel);
			Utilities.FindButtonAndAddOnClickListener("LoadButton", LoadLevel);
		}

		// SAVING METHODS

		// Method to determine whether a layer is empty
		private bool EmptyLayer(int[,,] level, int width, int height, int layer, int empty) {
			bool result = true;
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					if (level[x, y, layer] != empty) {
						result = false;
					}
				}
			}
			return result;
		}

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
							newRow += +levelToSave[x, y, layer] + ",";
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
			OpenFileBrowser(FileBrowserMode.Save);
		}

		// Save to a file using a path
		public void SaveLevelUsingPath(string path) {
			// Enable the LevelEditor when the fileBrowser is done
			LevelEditor.Instance.ToggleLevelEditor(_preFileBrowserState);
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

		// LOADING METHODS

		private void LoadLevel() {
			// Open file browser to get the path and file name
			OpenFileBrowser(FileBrowserMode.Load);
		}

		// Load from a file using a path
		public void LoadLevelUsingPath(string path) {
			// Enable the LevelEditor when the fileBrowser is done
			LevelEditor.Instance.ToggleLevelEditor(_preFileBrowserState);
			if (path.Length != 0) {
				BinaryFormatter bFormatter = new BinaryFormatter();
				// Reset the level
				LevelEditor.Instance.ResetBeforeLoad();
				FileStream file = File.OpenRead(path);
				// Convert the file from a byte array into a string
				string levelData = bFormatter.Deserialize(file) as string;
				// We're done working with the file so we can close it
				file.Close();
				LoadLevelFromStringLayers(levelData);
			} else {
				Debug.Log("Invalid path given");
			}
		}

		// Method that loads the layers
		private void LoadLevelFromStringLayers(string content) {
			// Split our level on layers by the new tabs (\t)
			List<string> layers = new List<string>(content.Split('\t'));
			foreach (string layer in layers) {
				if (layer.Trim() != "") {
					LoadLevelFromString(int.Parse(layer[0].ToString()), layer.Substring(1));
				}
			}
		}

		// Method that loads one layer
		private void LoadLevelFromString(int layer, string content) {
			// Split our layer on rows by the new lines (\n)
			List<string> lines = new List<string>(content.Split('\n'));
			// Place each block in order in the correct x and y position
			for (int i = 0; i < lines.Count; i++) {
				string[] blockIDs = lines[i].Split(',');
				for (int j = 0; j < blockIDs.Length - 1; j++) {
					LevelEditor.Instance.CreateBlock(int.Parse(blockIDs[j]), j, lines.Count - i - 1, layer);
				}
			}
			// Update to only show the correct layer(s)
			LevelEditor.Instance.UpdateLayerVisibility();
		}

		// Open a file browser to save and load files
		private void OpenFileBrowser(FileBrowserMode fileBrowserMode) {
			_preFileBrowserState = _levelEditor.GetScriptEnabled();
			// Disable the LevelEditor while the fileBrowser is open
			_levelEditor.ToggleLevelEditor(false);
			// Create the file browser and name it
			GameObject fileBrowserObject = Instantiate(FileBrowserPrefab, transform);
			fileBrowserObject.name = "FileBrowser";
			// Set the mode to save or load
			FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
			fileBrowserScript.SetupFileBrowser(ViewMode.Landscape);
			if (fileBrowserMode == FileBrowserMode.Save) {
				fileBrowserScript.SaveFilePanel(this, "SaveLevelUsingPath", "Level", FileExtension);
			} else {
				fileBrowserScript.OpenFilePanel(this, "LoadLevelUsingPath", FileExtension);
			}
		}
	}
}
