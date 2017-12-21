using UnityEngine;

using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using GracesGames.Common.Scripts;
using GracesGames.SimpleFileBrowser.Scripts;

namespace GracesGames._2DTileMapLevelEditor.Scripts.Functionalities {

	public class LoadFunctionality : MonoBehaviour {

		// ----- PRIVATE VARIABLES -----

		// The level editor
		private LevelEditor _levelEditor;

		// The file browser
		private GameObject _fileBrowserPrefab;

		// The file extension of the file to load
		private string _fileExtension;

		// Temporary variable to save state of level editor before opening file browser and restore it after save/load
		private bool _preFileBrowserState = true;

		// ----- SETUP -----

		public void Setup(GameObject fileBrowserPrefab, string fileExtension) {
			_levelEditor = LevelEditor.Instance;
			_fileBrowserPrefab = fileBrowserPrefab;
			_fileExtension = fileExtension.Trim() == "" ? "lvl" : fileExtension;
			SetupClickListeners();
		}

		// Hook up Save/Load Level method to Save/Load button
		private void SetupClickListeners() {
			Utilities.FindButtonAndAddOnClickListener("LoadButton", OpenFileBrowser);
		}

		// ----- PUBLIC METHODS -----

		// Load from a file using a path
		public void LoadLevelUsingPath(string path) {
			// Enable the LevelEditor when the fileBrowser is done
			_levelEditor.ToggleLevelEditor(_preFileBrowserState);
			if (path.Length != 0) {
				BinaryFormatter bFormatter = new BinaryFormatter();
				// Reset the level
				_levelEditor.ResetBeforeLoad();
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

		// ----- PRIVATE METHODS -----

		// Open a file browser to load files
		private void OpenFileBrowser() {
			_preFileBrowserState = _levelEditor.GetScriptEnabled();
			// Disable the LevelEditor while the fileBrowser is open
			_levelEditor.ToggleLevelEditor(false);
			// Create the file browser and name it
			GameObject fileBrowserObject = Instantiate(_fileBrowserPrefab, transform);
			fileBrowserObject.name = "FileBrowser";
			// Set the mode to save or load
			FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
			fileBrowserScript.SetupFileBrowser(ViewMode.Landscape);
			fileBrowserScript.OpenFilePanel(this, "LoadLevelUsingPath", _fileExtension);
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
					_levelEditor.CreateBlock(int.Parse(blockIDs[j]), j, lines.Count - i - 1, layer);
				}
			}
			// Update to only show the correct layer(s)
			_levelEditor.UpdateLayerVisibility();
		}
	}
}