using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using GracesGames.Common.Scripts;
using GracesGames.SimpleFileBrowser.Scripts;
using GracesGames._2DTileMapLevelEditor.Scripts.UI;

namespace GracesGames._2DTileMapLevelEditor.Scripts {

	public class LevelEditor : MonoBehaviour {

		// ----- PUBLIC VARIABLES -----

		// The instance of the LevelEditor
		public static LevelEditor Instance;

		// The parent object of the Level Editor UI as prefab
		public GameObject LevelEditorUiPrefab;

		// FileBrowser Prefab to open Save- and LoadFilePanel
		public GameObject FileBrowserPrefab;

		// The X,Y and Z value of the map
		public int Height = 14;

		public int Width = 16;
		public int Layers = 10;

		// The list of tiles the user can use to create maps
		// Public so the user can add all user-created prefabs
		public GameObject Tileset;

		// File extension used to save and load the levels
		public string FileExtension = "lvl";

		// ----- PRIVATE VARIABLES -----

		private List<Transform> _tiles;

		// The user interface script for the Level Editor
		private UserInterface _uiScript;

		// Whether this script is enabled (false, if the user closes the window)
		private bool _scriptEnabled = true;

		// Define empty tile for map
		private const int Empty = -1;

		// The internal representation of the level (int values) and gameObjects (transforms)
		private int[,,] _level;

		private Transform[,,] _gameObjects;

		// Used to store the currently selected tile index and layer
		private int _selectedTileIndex = Empty;

		private int _selectedLayer;

		// GameObject as the parent for all the layers (to keep the Hierarchy window clean)
		private GameObject _tileLevelParent;

		// Dictionary as the parent for all the GameObjects per layer
		private Dictionary<int, GameObject> _layerParents = new Dictionary<int, GameObject>();

		// Boolean to determine whether to show all layers or only the current one
		private bool _onlyShowCurrentLayer;

		// Transform used to preview selected tile on map
		private Transform _previewTile;

		// Stacks to keep track for undo and redo feature
		private FiniteStack<int[,,]> _undoStack;

		private FiniteStack<int[,,]> _redoStack;

		// Main camera and components for zoom feature
		private GameObject _mainCamera;

		private Camera _mainCameraComponent;
		private float _mainCameraInitialSize;

		// Boolean to determine whether to use fill mode or pencil mode
		private bool _fillMode;

		// Temporary variable to save level before getting the path using the FileBrowser
		private string _levelToSave;

		// Temporary variable to save state of level editor before opening file browser and restore it after save/load
		private bool _preFileBrowserState = true;

		// ----- METHODS -----

		// Method to Instantiate the LevelEditor instance and keep it from destroying
		void Awake() {
			if (Instance == null) {
				Instance = this;
			} else if (Instance != this) {
				Destroy(gameObject);
			}
		}

		// Method to instantiate the dependencies and variables
		void Start() {
			// Validate the start values to prevent errors
			ValidateStartValues();

			// Setup elements
			SetupGridOverlay();
			SetupCamera();
			SetupStacks();
			SetupLevel();
			SetupUi();
		}

		// Method that checks public variables values and sets them to valid defaults when necessary
		private void ValidateStartValues() {
			Width = Mathf.Clamp(Width, 1, Width);
			Height = Mathf.Clamp(Height, 1, Height);
			Layers = Mathf.Clamp(Layers, 1, Layers);
			FileExtension = FileExtension.Trim() == "" ? "lvl" : FileExtension;
			_tiles = Tileset.GetComponent<Tileset>().Tiles;
		}

		// Define the level sizes as the sizes for the grid
		private void SetupGridOverlay() {
			GridOverlay.Instance.SetGridSizeX(Width);
			GridOverlay.Instance.SetGridSizeY(Height);
		}

		// Find the camera, position it in the middle of our level and store initial zoom level
		private void SetupCamera() {
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			if (_mainCamera != null) {
				_mainCamera.transform.position = new Vector3(Width / 2.0f, Height / 2.0f, _mainCamera.transform.position.z);
				//Store initial zoom level
				_mainCameraComponent = _mainCamera.GetComponent<Camera>();
				_mainCameraInitialSize = _mainCameraComponent.orthographic
					? _mainCameraComponent.orthographicSize
					: _mainCameraComponent.fieldOfView;
			} else {
				Debug.LogError("Object with tag MainCamera not found");
			}
		}

		// Instantiate the undo and redo stack
		private void SetupStacks() {
			_undoStack = new FiniteStack<int[,,]>();
			_redoStack = new FiniteStack<int[,,]>();
		}

		// Set variables and creates empty level with the correct size
		private void SetupLevel() {
			// Get or create the tileLevelParent object so we can make it our newly created objects' parent
			_tileLevelParent = GameObject.Find("TileLevel") ?? new GameObject("TileLevel");

			// Instantiate the level and gameObject to an empty level and empty Transform
			_level = CreateEmptyLevel();
			_gameObjects = new Transform[Width, Height, Layers];
		}

		private void SetupUi() {
			// Instantiate the LevelEditorUI
			GameObject canvas = GameObject.Find("Canvas");
			if (canvas != null) {
				GameObject levelEditorUi = Instantiate(LevelEditorUiPrefab, canvas.transform);
				_uiScript = levelEditorUi.GetComponent<UserInterface>();
			} else {
				Debug.LogError("Make sure there is a canvas GameObject present in the Hierarcy (Create UI/Canvas)");
			}
			// Setup the UI
			_uiScript.Setup();
			// Initally disable fill mode
			DisableFillMode();
			// Initialy enable grid
			ToggleGrid(true);
			// Set the SelectedTile to Empty (-1) and update the selectedTileImage
			SetSelectedTile(Empty);
		}

		// Method to set the selectedTile variable and the selectedTileImage
		private void SetSelectedTile(int tileIndex) {
			// Update selectedTile variable
			_selectedTileIndex = tileIndex;
			// If EMPTY, set selectedTileImage to noSelectedTileImage else to the corresponding Prefab tile image
			_uiScript.SetSelectedTileImageSprite(tileIndex);
		}

		// Returns whether the script is enabled (e.g. whether input is registered) 
		public bool GetScriptEnabled() {
			return _scriptEnabled;
		}

		// Returns the static representation of an EMPTY tile
		public static int GetEmpty() {
			return Empty;
		}

		// Returns the array of Tiles
		public List<Transform> GetTiles() {
			return _tiles;
		}

		// Returns the currently selected layer
		public int GetSelectedLayer() {
			return _selectedLayer;
		}

		// Method to switch selectedTile on tile selection
		public void ButtonClick(int tileIndex) {
			SetSelectedTile(tileIndex);
			if (_previewTile != null) {
				DestroyImmediate(_previewTile.gameObject);
			}
			_previewTile = Instantiate(GetTiles()[_selectedTileIndex], new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100),
				Quaternion.identity);
			foreach (Collider2D c in _previewTile.GetComponents<Collider2D>()) {
				c.enabled = false;
			}
		}

		// Method to create an empty level by looping through the Height, Width and Layers 
		// and setting the value to EMPTY (-1)
		private int[,,] CreateEmptyLevel() {
			int[,,] emptyLevel = new int[Width, Height, Layers];
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					for (int z = 0; z < Layers; z++) {
						emptyLevel[x, y, z] = Empty;
					}
				}
			}
			return emptyLevel;
		}

		// Method to determine for a given x, y, z, whether the position is valid (within Width, Height and Layers)
		private bool ValidPosition(int x, int y, int z) {
			if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Layers) {
				return false;
			} else {
				return true;
			}
		}

		// Method that creates a GameObject on click
		private void CreateBlock(int value, int xPos, int yPos, int zPos) {
			// The transform to create
			Transform toCreate = null;
			// Return on invalid positions
			if (!ValidPosition(xPos, yPos, zPos)) {
				return;
			}
			// Set the value for the internal level representation
			_level[xPos, yPos, zPos] = value;
			// If the value is not empty, set it to the correct tile
			if (value != Empty) {
				toCreate = GetTiles()[value];
			}
			if (toCreate != null) {
				//Create the object we want to create
				Transform newObject =
					Instantiate(toCreate, new Vector3(xPos, yPos, toCreate.position.z), Quaternion.identity);
				//Give the new object the same name as our tile prefab
				newObject.name = toCreate.name;
				// Set the object's parent to the layer parent variable so it doesn't clutter our Hierarchy
				newObject.parent = GetLayerParent(zPos).transform;
				// Add the new object to the gameObjects array for correct administration
				_gameObjects[xPos, yPos, zPos] = newObject;
			}
		}

		// Rebuild the level (e.g. after using undo/redo)
		// Reset the Transforms and Layer, then loop trough level array and create blocks
		private void RebuildLevel() {
			ResetTransformsAndLayers();
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					for (int z = 0; z < Layers; z++) {
						CreateBlock(_level[x, y, z], x, y, z);
					}
				}
			}
		}

		// Load last saved level from undo stack and rebuild level
		public void Undo() {
			// See if there is anything on the undo stack
			if (_undoStack.Count > 0) {
				// If so, push it to the redo stack
				_redoStack.Push(_level);
			}
			// Get the last level entry
			int[,,] undoLevel = _undoStack.Pop();
			if (undoLevel != null) {
				// Set level and rebuild the level
				_level = undoLevel;
				RebuildLevel();
			}
		}

		// Load last saved level from redo tack and rebuild level
		public void Redo() {
			// See if there is anything on the redo stack
			if (_redoStack.Count > 0) {
				// If so, push it to the redo stack
				_undoStack.Push(_level);
			}
			// Get the last level entry
			int[,,] redoLevel = _redoStack.Pop();
			if (redoLevel != null) {
				// Set level and rebuild the level
				_level = redoLevel;
				RebuildLevel();
			}
		}

		// Increment the orthographic size or field of view of the camera, thereby zooming in
		public void ZoomIn() {
			if (_mainCameraComponent.orthographic) {
				_mainCameraComponent.orthographicSize = Mathf.Max(_mainCameraComponent.orthographicSize - 1, 1);
			} else {
				_mainCameraComponent.fieldOfView = Mathf.Max(_mainCameraComponent.fieldOfView - 1, 1);
			}
		}

		// Decrement the orthographic size or field of view of the camera, thereby zooming out
		public void ZoomOut() {
			if (_mainCameraComponent.orthographic) {
				_mainCameraComponent.orthographicSize += 1;
			} else {
				_mainCameraComponent.fieldOfView += 1;
			}
		}

		// Resets the orthographic size or field of view of the camera, thereby resetting the zoom level
		public void ZoomDefault() {
			if (_mainCameraComponent.orthographic) {
				_mainCameraComponent.orthographicSize = _mainCameraInitialSize;
			} else {
				_mainCameraComponent.fieldOfView = _mainCameraInitialSize;
			}

		}

		// Clicked on position, so check if it is the same, and (destroy and) build if neccesary
		private void ClickedPosition(int posX, int posY) {
			// If it's the same, just keep the previous one and do nothing, else (destroy and) build
			if (_level[posX, posY, _selectedLayer] != _selectedTileIndex) {
				// Push level on undoStack since it is going to change
				_undoStack.Push(_level.Clone() as int[,,]);
				// If the position is not empty, destroy the the current element (using gameObjects array)
				if (_level[posX, posY, _selectedLayer] != Empty) {
					DestroyImmediate(_gameObjects[posX, posY, _selectedLayer].gameObject);
				}
				// Create the new game object
				CreateBlock(_selectedTileIndex, posX, posY, _selectedLayer);
			}
		}

		// Fill from position recursively. Only fill if the position is valid and empty
		private void Fill(int posX, int posY, bool undoPush) {
			// Check valid and empty
			if (ValidPosition(posX, posY, _selectedLayer) && _level[posX, posY, _selectedLayer] == Empty) {
				if (undoPush) {
					// Push level on undoStack since it is going to change
					_undoStack.Push(_level.Clone() as int[,,]);
				}
				// Create a block on the position
				CreateBlock(_selectedTileIndex, posX, posY, _selectedLayer);
				// Fill x+1, x-1, y+1, y-1
				Fill(posX + 1, posY, false);
				Fill(posX - 1, posY, false);
				Fill(posX, posY + 1, false);
				Fill(posX, posY - 1, false);
			}
		}

		// Toggle fill mode (between fill and pencil mode)
		private void ToggleFillMode() {
			if (_fillMode) {
				DisableFillMode();
			} else {
				EnableFillMode();
			}
		}

		// Enable fill mode and update UI
		public void EnableFillMode() {
			_fillMode = true;
			_uiScript.ToggleFillMode(true);
		}

		// Disable fill mode and update UI and cursor
		public void DisableFillMode() {
			_fillMode = false;
			_uiScript.ToggleFillMode(false);
		}

		// Handles input (creation and deletion on click)
		void Update() {
			// Only continue if the script is enabled (level editor is open)
			if (_scriptEnabled) {
				// Save the world point were the mouse clicked
				Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				// Update fill mode cursor
				UpdateFillModeCursor(worldMousePosition);
				// Update preview tile position
				UpdatePreviewTilePosition(worldMousePosition);
				// Check button input
				CheckButtonInput();
				// Get the mouse position before click
				Vector3 mousePos = Input.mousePosition;
				// Set the position in the z axis to the opposite of the camera's so that the position is on the world
				//  so ScreenToWorldPoint will give us valid values.
				mousePos.z = Camera.main.transform.position.z * -1;
				Vector3 pos = Camera.main.ScreenToWorldPoint(mousePos);
				// Deal with the mouse being not exactly on a block
				int posX = Mathf.FloorToInt(pos.x + .5f);
				int posY = Mathf.FloorToInt(pos.y + .5f);
				// Handle input only when a valid position is clicked
				if (ValidPosition(posX, posY, _selectedLayer)) {
					HandleInput(posX, posY);
				}
			}
		}

		// Check for mouse button clicks and handle accordingly
		private void HandleInput(int posX, int posY) {
			// Left click - Create object (check hotControl and not over UI)
			if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0 && !EventSystem.current.IsPointerOverGameObject()) {
				// Only allow additions if the selectedTile is not EMPTY (cannot add/fill nothing)
				if (_selectedTileIndex != Empty) {
					// If fill mode, fill, else click position (pencil mode)
					if (_fillMode) {
						Fill(posX, posY, true);
					} else {
						ClickedPosition(posX, posY);
					}
				}
			}
			// Right clicking - Delete object (check hotControl and not over UI)
			if (Input.GetMouseButton(1) && GUIUtility.hotControl == 0 && !EventSystem.current.IsPointerOverGameObject()) {
				// If we hit something (!= EMPTY), we want to destroy the object and update the gameObject array and level array
				if (_level[posX, posY, _selectedLayer] != Empty) {
					DestroyImmediate(_gameObjects[posX, posY, _selectedLayer].gameObject);
					_level[posX, posY, _selectedLayer] = Empty;
				}
				// If we hit nothing and previewTile is null, remove it
				else if (_previewTile != null) {
					DestroyImmediate(_previewTile.gameObject);
					// Set selected tile and image to EMPTY
					SetSelectedTile(Empty);
				}
			}
		}

		// If fill mode is enabled, update cursor (only show fill cursor on grid)
		private void UpdateFillModeCursor(Vector3 worldMousePosition) {
			if (_fillMode) {
				// If valid position, set cursor to bucket
				_uiScript.ToggleFillModeCursor(ValidPosition((int) worldMousePosition.x, (int) worldMousePosition.y, 0));
			}
		}

		// Update previewTile position
		private void UpdatePreviewTilePosition(Vector3 worldMousePosition) {
			if (_previewTile != null) {
				if (ValidPosition((int) worldMousePosition.x, (int) worldMousePosition.y, 0)) {
					_previewTile.position =
						new Vector3(Mathf.RoundToInt(worldMousePosition.x), Mathf.RoundToInt(worldMousePosition.y), -1);
				}
			}
		}

		// Check for any button presses (undo/redo, zooming and fill/pencil mode)
		private void CheckButtonInput() {
			// If Z is pressed, undo action
			if (Input.GetKeyDown(KeyCode.Z)) {
				Undo();
			}
			// If Y is pressed, redo action
			if (Input.GetKeyDown(KeyCode.Y)) {
				Redo();
			}
			// If Equals is pressed, zoom in
			if (Input.GetKeyDown(KeyCode.Equals)) {
				ZoomIn();
			}
			// if Minus is pressed, zoom out
			if (Input.GetKeyDown(KeyCode.Minus)) {
				ZoomOut();
			}
			// If 0 is pressed, reset zoom
			if (Input.GetKeyDown(KeyCode.Alpha0)) {
				ZoomDefault();
			}
			// If F is pressed, toggle FillMode;
			if (Input.GetKeyDown(KeyCode.F)) {
				ToggleFillMode();
			}
		}

		// Method that toggles the grid
		public void ToggleGrid(bool enable) {
			GridOverlay.Instance.enabled = enable;
			// Update UI 
			_uiScript.ToggleGridUi(enable);
		}

		// Method that increments the selected layer
		public void LayerUp() {
			_selectedLayer = Mathf.Min(_selectedLayer + 1, Layers - 1);
			UpdateLayerVisibility();
		}

		// Method that decrements the selected layer
		public void LayerDown() {
			_selectedLayer = Mathf.Max(_selectedLayer - 1, 0);
			UpdateLayerVisibility();
		}

		// Method that handles the UI toggle to only show the current layer
		public void ToggleOnlyShowCurrentLayer(bool onlyShow) {
			_onlyShowCurrentLayer = onlyShow;
			// Update UI
			_uiScript.ToggleOnlyShowCurrentLayer(_onlyShowCurrentLayer);
			// Update layer visibility
			UpdateLayerVisibility();
		}

		// Method that updates which layers should be shown
		private void UpdateLayerVisibility() {
			if (_onlyShowCurrentLayer) {
				OnlyShowCurrentLayer();
			} else {
				ShowAllLayers();
			}
		}

		// Method that enables/disables all layerParents
		private void ToggleLayerParents(bool show) {
			foreach (GameObject layerParent in _layerParents.Values) {
				layerParent.SetActive(show);
			}
		}

		// Method that enables all layers
		private void ShowAllLayers() {
			ToggleLayerParents(true);
		}

		// Method that disables all layers except the current one
		private void OnlyShowCurrentLayer() {
			ToggleLayerParents(false);
			GetLayerParent(_selectedLayer).SetActive(true);
		}

		// Method to determine whether a layer is empty
		private bool EmptyLayer(int layer) {
			bool result = true;
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					if (_level[x, y, layer] != Empty) {
						result = false;
					}
				}
			}
			return result;
		}

		// Method that returns the parent GameObject for a layer
		private GameObject GetLayerParent(int layer) {
			if (!_layerParents.ContainsKey(layer)) {
				GameObject layerParent = new GameObject("Layer " + layer);
				layerParent.transform.parent = _tileLevelParent.transform;
				_layerParents.Add(layer, layerParent);
			}
			return _layerParents[layer];
		}

		// Close the level editor panel, test level mode
		public void CloseLevelEditorPanel() {
			_scriptEnabled = false;
			_uiScript.ToggleLevelEditorPanel(false);
			_uiScript.ToggleOpenButton(true);
		}

		// Open the level editor panel, level editor mode
		public void OpenLevelEditorPanel() {
			_uiScript.ToggleLevelEditorPanel(true);
			_uiScript.ToggleOpenButton(false);
			_scriptEnabled = true;
		}

		// Enables/disables the level editor, (script, overlay and panel)
		public void ToggleLevelEditor(bool enable) {
			_scriptEnabled = enable;
			GridOverlay.Instance.enabled = enable;
			_uiScript.ToggleLevelEditorPanel(enable);
		}

		// Open a file browser to save and load files
		public void OpenFileBrowser(FileBrowserMode fileBrowserMode) {
			_preFileBrowserState = _scriptEnabled;
			// Disable the LevelEditor while the fileBrowser is open
			ToggleLevelEditor(false);
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

		// Save the level to a variable and file using FileBrowser and SaveLevelUsingPath
		public void SaveLevel() {
			List<string> newLevel = new List<string>();
			// Loop through the layers
			for (int layer = 0; layer < Layers; layer++) {
				// If the layer is not empty, add it and add \t at the end"
				if (!EmptyLayer(layer)) {
					// Loop through the rows and add \n at the end"
					for (int y = 0; y < Height; y++) {
						string newRow = "";
						for (int x = 0; x < Width; x++) {
							newRow += +_level[x, y, layer] + ",";
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
			ToggleLevelEditor(_preFileBrowserState);
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

		// Method that resets the GameObjects and layers
		private void ResetTransformsAndLayers() {
			// Destroy everything inside our currently level that's created dynamically
			foreach (Transform child in _tileLevelParent.transform) {
				Destroy(child.gameObject);
			}
			_layerParents = new Dictionary<int, GameObject>();
		}

		// Method that resets the level and GameObject before a load
		private void ResetBeforeLoad() {
			// Destroy everything inside our currently level that's created dynamically
			foreach (Transform child in _tileLevelParent.transform) {
				Destroy(child.gameObject);
			}
			_level = CreateEmptyLevel();
			_layerParents = new Dictionary<int, GameObject>();
			// Reset undo and redo stacks
			_undoStack = new FiniteStack<int[,,]>();
			_redoStack = new FiniteStack<int[,,]>();
		}

		// Load the level from a file using FileBrowser and LoadLevelUsingPath
		public void LoadLevel() {
			// Open file browser to get the path and file name
			OpenFileBrowser(FileBrowserMode.Load);
		}

		// Load from a file using a path
		public void LoadLevelUsingPath(string path) {
			// Enable the LevelEditor when the fileBrowser is done
			ToggleLevelEditor(_preFileBrowserState);
			if (path.Length != 0) {
				BinaryFormatter bFormatter = new BinaryFormatter();
				// Reset the level
				ResetBeforeLoad();
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
					CreateBlock(int.Parse(blockIDs[j]), j, lines.Count - i - 1, layer);
				}
			}
			// Update to only show the correct layer(s)
			UpdateLayerVisibility();
		}
	}
}