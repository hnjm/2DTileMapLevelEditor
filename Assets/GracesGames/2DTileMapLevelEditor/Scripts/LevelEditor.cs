using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections.Generic;

using GracesGames._2DTileMapLevelEditor.Scripts.UI;
using GracesGames._2DTileMapLevelEditor.Scripts.Functionalities;

namespace GracesGames._2DTileMapLevelEditor.Scripts {

	[RequireComponent(typeof(SaveLoadFunctionality)),
	 RequireComponent(typeof(UndoRedoFunctionality)),
	 RequireComponent(typeof(ZoomFunctionality)),
	 RequireComponent(typeof(FillFunctionality)),
	 RequireComponent(typeof(LayerFunctionality)),
	 RequireComponent(typeof(GridFunctionality))]
	public class LevelEditor : MonoBehaviour {

		// ----- PUBLIC VARIABLES -----

		// The instance of the LevelEditor
		public static LevelEditor Instance;

		// The parent object of the Level Editor UI as prefab
		public GameObject LevelEditorUiPrefab;

		// The X,Y and Z value of the map
		public int Height = 14;

		public int Width = 16;
		public int Layers = 10;

		// The list of tiles the user can use to create maps
		// Public so the user can add all user-created prefabs
		public GameObject Tileset;

		// ----- PRIVATE VARIABLES -----

		private List<Transform> _tiles;
		
		// The user interface script for the Level Editor
		private UserInterface _uiScript;

		// Functionalities
		private SaveLoadFunctionality _saveLoadFunctionality;

		private UndoRedoFunctionality _undoRedoFunctionality;
		private FillFunctionality _fillFunctionality;
		private ZoomFunctionality _zoomFunctionality;
		private LayerFunctionality _layerFunctionality;
		private GridFunctionality _gridFunctionality;

		// Whether this script is enabled (false, if the user closes the window)
		private bool _scriptEnabled = true;

		// Define empty tile for map
		private const int Empty = -1;

		// The internal representation of the level (int values) and gameObjects (transforms)
		private int[,,] _level;

		private Transform[,,] _gameObjects;

		// Used to store the currently selected tile index and layer
		private int _selectedTileIndex = Empty;

		// GameObject as the parent for all the layers (to keep the Hierarchy window clean)
		private GameObject _tileLevelParent;

		// Dictionary as the parent for all the GameObjects per layer
		private Dictionary<int, GameObject> _layerParents = new Dictionary<int, GameObject>();

		// Transform used to preview selected tile on map
		private Transform _previewTile;

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
			SetupLevel();
			SetupUi();
			SetupFunctionalities();
		}

		// Method that checks public variables values and sets them to valid defaults when necessary
		private void ValidateStartValues() {
			Width = Mathf.Clamp(Width, 1, Width);
			Height = Mathf.Clamp(Height, 1, Height);
			Layers = Mathf.Clamp(Layers, 1, Layers);

			if (Tileset == null || Tileset.GetComponent<Tileset>() == null) {
				_tiles = new List<Transform>();
				Debug.LogError("No valid Tileset found");
			} else {
				_tiles = Tileset.GetComponent<Tileset>().Tiles;
			}
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
			// Set the SelectedTile to Empty (-1) and update the selectedTileImage
			SetSelectedTile(Empty);
		}

		private void SetupFunctionalities() {
			_saveLoadFunctionality = GetComponent<SaveLoadFunctionality>();
			_saveLoadFunctionality.Setup();

			_undoRedoFunctionality = GetComponent<UndoRedoFunctionality>();
			_undoRedoFunctionality.Setup();

			_fillFunctionality = GetComponent<FillFunctionality>();
			_fillFunctionality.Setup();

			_zoomFunctionality = GetComponent<ZoomFunctionality>();
			_zoomFunctionality.Setup(Width, Height);

			_layerFunctionality = GetComponent<LayerFunctionality>();
			_layerFunctionality.Setup(Layers);

			_gridFunctionality = GetComponent<GridFunctionality>();
			_gridFunctionality.Setup(Width, Height);
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

		// Returns the level
		public int[,,] GetLevel() {
			return _level;
		}

		public void SetLevel(int[,,] newLevel) {
			_level = newLevel;
			RebuildGameObjects();
		}

		// Returns the array of Tiles
		public List<Transform> GetTiles() {
			return _tiles;
		}

		// Handles input (creation and deletion on click)
		private void Update() {
			// Only continue if the script is enabled (level editor is open)
			if (!_scriptEnabled) return;
			// Save the world point were the mouse clicked
			Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			// Update fill mode cursor
			_fillFunctionality.UpdateFillModeCursor(ValidPosition((int) worldMousePosition.x, (int) worldMousePosition.y, 0));
			// Update preview tile position
			UpdatePreviewTilePosition(worldMousePosition);
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
			HandleInput(posX, posY);
		}

		// Method to switch selectedTile on tile selection
		public void ButtonClick(int tileIndex) {
			SetSelectedTile(tileIndex);
			if (_previewTile != null) {
				DestroyImmediate(_previewTile.gameObject);
			}
			_previewTile = Instantiate(GetTiles()[_selectedTileIndex],
				new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100),
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
			return x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Layers;
		}

		// Method that creates a GameObject on click
		public void CreateBlock(int value, int xPos, int yPos, int zPos) {
			// Return on invalid positions
			if (!ValidPosition(xPos, yPos, zPos)) {
				return;
			}
			// Set the value for the internal level representation
			_level[xPos, yPos, zPos] = value;
			// If the value is not empty, set it to the correct tile
			if (value != Empty) {
				BuildBlock(GetTiles()[value], xPos, yPos, zPos, GetLayerParent(zPos).transform);
			}
		}
		
		private void BuildBlock(Transform toCreate, int xPos, int yPos, int zPos, Transform parent) {
			//Create the object we want to create
			Transform newObject = Instantiate(toCreate, new Vector3(xPos, yPos, toCreate.position.z), Quaternion.identity);
			//Give the new object the same name as our tile prefab
			newObject.name = toCreate.name;
			// Set the object's parent to the layer parent variable so it doesn't clutter our Hierarchy
			newObject.parent = parent;
			// Add the new object to the gameObjects array for correct administration
			_gameObjects[xPos, yPos, zPos] = newObject;
		}
		
		private void DestroyBlock(int posX, int posY, int posZ) {
			DestroyImmediate(_gameObjects[posX, posY, posZ].gameObject);
		}

		// Rebuild the level (e.g. after using undo/redo)
		// Reset the Transforms and Layer, then loop trough level array and create blocks
		private void RebuildGameObjects() {
			ResetTransformsAndLayers();
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					for (int z = 0; z < Layers; z++) {
						if (_level[x, y, z] != Empty) {
							BuildBlock(GetTiles()[_level[x, y, z]], x, y, z, GetLayerParent(z).transform);
						}
					}
				}
			}
		}

		// Clicked on position, so check if it is the same, and (destroy and) build if neccesary
		private void ClickedPosition(int posX, int posY, int selectedLayer) {
			// If it's the same, just keep the previous one and do nothing, else (destroy and) build
			if (_level[posX, posY, selectedLayer] != _selectedTileIndex) {
				// Push level on undoStack since it is going to change
				_undoRedoFunctionality.PushLevel(_level);
				// If the position is not empty, destroy the the current element (using gameObjects array)
				if (_level[posX, posY, selectedLayer] != Empty) {
					DestroyBlock(posX, posY, selectedLayer);
				}
				// Create the new game object
				CreateBlock(_selectedTileIndex, posX, posY, selectedLayer);
			}
		}

		// Fill from position recursively. Only fill if the position is valid and empty
		private void Fill(int posX, int posY, int selectedLayer, bool undoPush) {
			// Check valid and empty
			if (ValidPosition(posX, posY, selectedLayer) && _level[posX, posY, selectedLayer] == Empty) {
				if (undoPush) {
					// Push level on undoStack since it is going to change
					_undoRedoFunctionality.PushLevel(_level);
				}
				// Create a block on the position
				CreateBlock(_selectedTileIndex, posX, posY, selectedLayer);
				// Fill x+1, x-1, y+1, y-1
				Fill(posX + 1, posY, selectedLayer, false);
				Fill(posX - 1, posY, selectedLayer, false);
				Fill(posX, posY + 1, selectedLayer, false);
				Fill(posX, posY - 1, selectedLayer, false);
			}
		}

		// Check for mouse button clicks and handle accordingly
		private void HandleInput(int posX, int posY) {
			int selectedLayer = _layerFunctionality.GetSelectedLayer(); 
			if (!ValidPosition(posX, posY, selectedLayer)) {
				return;
			}
			// Left click - Create object (check hotControl and not over UI)
			if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0 && !EventSystem.current.IsPointerOverGameObject()) {
				// Only allow additions if the selectedTile is not EMPTY (cannot add/fill nothing)
				if (_selectedTileIndex != Empty) {
					// If fill mode, fill, else click position (pencil mode)
					if (_fillFunctionality.GetFillMode()) {
						Fill(posX, posY, selectedLayer, true);
					} else {
						ClickedPosition(posX, posY, selectedLayer);
					}
				}
			}
			// Right clicking - Delete object (check hotControl and not over UI)
			if (Input.GetMouseButton(1) && GUIUtility.hotControl == 0 && !EventSystem.current.IsPointerOverGameObject()) {
				// If we hit something (!= EMPTY), we want to destroy the object and update the gameObject array and level array
				if (_level[posX, posY, selectedLayer] != Empty) {
					DestroyBlock(posX, posY, selectedLayer);
					_level[posX, posY, selectedLayer] = Empty;
				}
				// If we hit nothing and previewTile is null, remove it
				else if (_previewTile != null) {
					DestroyImmediate(_previewTile.gameObject);
					// Set selected tile and image to EMPTY
					SetSelectedTile(Empty);
				}
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

		public void UpdateLayerVisibility() {
			_layerFunctionality.UpdateLayerVisibility();
		}
		
		// Method that enables/disables all layerParents
		public void ToggleLayerParent(int layer, bool show) {
			GetLayerParent(layer).SetActive(show);
		}

		// Method that enables/disables all layerParents
		public void ToggleLayerParents(bool show) {
			foreach (GameObject layerParent in _layerParents.Values) {
				layerParent.SetActive(show);
			}
		}

		// Method that returns the parent GameObject for a layer
		private GameObject GetLayerParent(int layer) {
			if (_layerParents.ContainsKey(layer))
				return _layerParents[layer];
			GameObject layerParent = new GameObject("Layer " + layer);
			layerParent.transform.parent = _tileLevelParent.transform;
			_layerParents.Add(layer, layerParent);
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

		// Method that resets the GameObjects and layers
		private void ResetTransformsAndLayers() {
			// Destroy everything inside our currently level that's created dynamically
			foreach (Transform child in _tileLevelParent.transform) {
				Destroy(child.gameObject);
			}
			_layerParents = new Dictionary<int, GameObject>();
		}

		// Method that resets the level and GameObject before a load
		public void ResetBeforeLoad() {
			// Destroy everything inside our currently level that's created dynamically
			foreach (Transform child in _tileLevelParent.transform) {
				Destroy(child.gameObject);
			}
			_level = CreateEmptyLevel();
			_layerParents = new Dictionary<int, GameObject>();
			// Reset undo and redo stacks
			_undoRedoFunctionality.Reset();
		}
	}
}