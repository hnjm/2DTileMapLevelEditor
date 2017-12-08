using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// Include for Lists and Dictionaries
using System.Collections.Generic;

namespace GracesGames._2DTileMapLevelEditor.Scripts.UI {

	public class LevelEditorUserInterface : MonoBehaviour {

		// The Level Editor using this User Interface
		private LevelEditor _levelEditor;

		// The UI panel used to store the Level Editor options
		private GameObject _levelEditorPanel;

		// UI objects to display pencil/fill mode
		private Image _pencilModeButtonImage;

		private Image _fillModeButtonImage;
		private static readonly Color32 DisabledColor = new Color32(150, 150, 150, 255);

		// UI objects to toggle onlyShowCurrentLayer
		private GameObject _layerEyeImage;

		private GameObject _layerClosedEyeImage;
		private Toggle _onlyShowCurrentLayerToggleComponent;

		// UI objects to toggle the grid
		private GameObject _gridEyeImage;

		private GameObject _gridClosedEyeImage;
		private Toggle _gridEyeToggleComponent;

		// Text used to represent the currently selected layer
		private Text _layerText;

		// Open button to reopen the level editor after closing it
		private GameObject _openButton;

		// GameObject used to show the currently selected tile
		private GameObject _selectedTile;

		// Image to indicate the currently selected tile
		private Image _selectedTileImage;

		// GameObject as the parent for all the GameObject in the tiles selection
		private GameObject _prefabParent;

		public void Setup(List<Transform> tiles, GameObject buttonPrefab, float buttonImageScale) {
			name = ("LevelEditorUI");
			_levelEditor = LevelEditor.Instance;
			_levelEditorPanel = FindGameObjectOrError("LevelEditorPanel");
			SetupSaveLoadButton();
			SetupUndoRedoButton();
			SetupModeButtons();
			SetupZoomButtons();
			SetupLayerButtons();
			SetupGridButtons();
			SetupOpenCloseButton();
			SetupSelectedTile();
			SetupPrefabsButtons(tiles, buttonPrefab, buttonImageScale);
		}

		// Finds and returns a game object by name or prints and error and increments error counter
		private GameObject FindGameObjectOrError(string objectName) {
			GameObject foundGameObject = GameObject.Find(objectName);
			if (foundGameObject == null) {
				Debug.LogError("Make sure " + objectName + " is present");
				return null;
			}
			return foundGameObject;
		}

		// Tries to find a button by name and add an on click listener action to it
		// Returns the resulting button 
		private GameObject FindButtonAndAddOnClickListener(string buttonName, UnityAction listenerAction) {
			GameObject button = FindGameObjectOrError(buttonName);
			button.GetComponent<Button>().onClick.AddListener(listenerAction);
			return button;
		}

		private void SetupSaveLoadButton() {
			// Hook up Save/Load Level method to Save/Load button
			FindButtonAndAddOnClickListener("SaveButton", _levelEditor.SaveLevel);
			FindButtonAndAddOnClickListener("LoadButton", _levelEditor.LoadLevel);
		}

		private void SetupUndoRedoButton() {
			// Hook up Undo/Redo method to Undo/Redo button
			FindButtonAndAddOnClickListener("UndoButton", _levelEditor.Undo);
			FindButtonAndAddOnClickListener("RedoButton", _levelEditor.Redo);
		}

		private void SetupModeButtons() {
			// Hook up EnablePencilMode method to PencilButton
			GameObject pencilModeButton = FindButtonAndAddOnClickListener("PencilButton", _levelEditor.DisableFillMode);
			_pencilModeButtonImage = pencilModeButton.GetComponent<Image>();

			// Hook up EnableFillMode method to FillButton
			GameObject fillModeButton = FindButtonAndAddOnClickListener("FillButton", _levelEditor.EnableFillMode);
			_fillModeButtonImage = fillModeButton.GetComponent<Image>();
		}

		private void SetupZoomButtons() {
			// Hook up Zoom methods to Zoom buttons
			FindButtonAndAddOnClickListener("ZoomInButton", _levelEditor.ZoomIn);
			FindButtonAndAddOnClickListener("ZoomOutButton", _levelEditor.ZoomOut);
			FindButtonAndAddOnClickListener("ZoomDefaultButton", _levelEditor.ZoomDefault);
		}

		private void SetupLayerButtons() {
			// Hook up Layer Change methods to Layer Change buttons
			FindButtonAndAddOnClickListener("+LayerButton", _levelEditor.LayerUp);
			FindButtonAndAddOnClickListener("-LayerButton", _levelEditor.LayerDown);

			// Hook up ToggleOnlyShowCurrentLayer method to OnlyShowCurrentLayerToggle
			GameObject onlyShowCurrentLayerToggle = FindGameObjectOrError("OnlyShowCurrentLayerToggle");
			_layerEyeImage = GameObject.Find("LayerEyeImage");
			_layerClosedEyeImage = GameObject.Find("LayerClosedEyeImage");
			_onlyShowCurrentLayerToggleComponent = onlyShowCurrentLayerToggle.GetComponent<Toggle>();
			_onlyShowCurrentLayerToggleComponent.onValueChanged.AddListener(_levelEditor.ToggleOnlyShowCurrentLayer);

			// Instantiate the LayerText game object to display the current layer
			_layerText = FindGameObjectOrError("LayerText").GetComponent<Text>();
		}

		private void SetupGridButtons() {
			// Hook up ToggleGrid method to GridToggle
			GameObject gridEyeToggle = FindGameObjectOrError("GridEyeToggle");
			_gridEyeImage = FindGameObjectOrError("GridEyeImage");
			_gridClosedEyeImage = FindGameObjectOrError("GridClosedEyeImage");
			_gridEyeToggleComponent = gridEyeToggle.GetComponent<Toggle>();
			_gridEyeToggleComponent.onValueChanged.AddListener(_levelEditor.ToggleGrid);

			// Hook up Grid Size methods to Grid Size buttons
			FindButtonAndAddOnClickListener("GridSizeUpButton", GridOverlay.Instance.GridSizeUp);
			FindButtonAndAddOnClickListener("GridSizeDownButton", GridOverlay.Instance.GridSizeDown);

			// Hook up Grid Navigation methods to Grid Navigation buttons
			FindButtonAndAddOnClickListener("GridUpButton", GridOverlay.Instance.GridUp);
			FindButtonAndAddOnClickListener("GridDownButton", GridOverlay.Instance.GridDown);
			FindButtonAndAddOnClickListener("GridLeftButton", GridOverlay.Instance.GridLeft);
			FindButtonAndAddOnClickListener("GridRightButton", GridOverlay.Instance.GridRight);
		}

		private void SetupOpenCloseButton() {
			// Hook up CloseLevelEditorPanel method to CloseButton
			FindButtonAndAddOnClickListener("CloseButton", _levelEditor.CloseLevelEditorPanel);

			// Hook up OpenLevelEditorPanel method to OpenButton and disable at start
			_openButton = FindButtonAndAddOnClickListener("OpenButton", _levelEditor.OpenLevelEditorPanel);
			_openButton.SetActive(false);
		}

		private void SetupSelectedTile() {
			_selectedTile = FindGameObjectOrError("SelectedTile");
			// Find the image component of the SelectedTileImage GameObject
			_selectedTileImage = FindGameObjectOrError("SelectedTileImage").GetComponent<Image>();
		}

		private void SetupPrefabsButtons(List<Transform> tiles, GameObject buttonPrefab, float buttonImageScale) {
			// Find the prefabParent object and set the cellSize for the tile selection buttons
			_prefabParent = FindGameObjectOrError("Prefabs");
			if (_prefabParent.GetComponent<GridLayoutGroup>() == null) {
				Debug.LogError("Make sure prefabParent has a GridLayoutGroup component");
			}
			// Counter to determine which tile button is pressed
			int tileCounter = 0;
			//Create a button for each tile in tiles
			foreach (Transform tile in tiles) {
				int index = tileCounter;
				GameObject button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
				button.name = tile.name;
				button.GetComponent<Image>().sprite = tile.gameObject.GetComponent<SpriteRenderer>().sprite;
				button.transform.SetParent(_prefabParent.transform, false);
				button.transform.localScale = new Vector3(buttonImageScale, buttonImageScale, buttonImageScale);
				// Add a click handler to the button
				button.GetComponent<Button>().onClick.AddListener(() => { _levelEditor.ButtonClick(index); });
				tileCounter++;
			}
		}

		public void ToggleLevelEditorPanel(bool enable) {
			_levelEditorPanel.SetActive(enable);
		}

		public void ToggleOpenButton(bool enable) {
			_openButton.SetActive(enable);
		}

		public void ToggleFillMode(bool enable) {
			if (enable) {
				_fillModeButtonImage.GetComponent<Image>().color = Color.black;
				_pencilModeButtonImage.GetComponent<Image>().color = DisabledColor;
			} else {
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				_pencilModeButtonImage.GetComponent<Image>().color = Color.black;
				_fillModeButtonImage.GetComponent<Image>().color = DisabledColor;
			}
		}

		public void ToggleOnlyShowCurrentLayer(bool enable) {
			if (enable) {
				_layerEyeImage.SetActive(true);
				_layerClosedEyeImage.SetActive(false);
				_onlyShowCurrentLayerToggleComponent.targetGraphic = _layerEyeImage.GetComponent<Graphic>();
			} else {
				_layerClosedEyeImage.SetActive(true);
				_layerEyeImage.SetActive(false);
				_onlyShowCurrentLayerToggleComponent.targetGraphic = _layerClosedEyeImage.GetComponent<Graphic>();
			}
		}

		public void ToggleGridUi(bool enable) {
			if (enable) {
				_gridClosedEyeImage.SetActive(true);
				_gridEyeImage.SetActive(false);
				_gridEyeToggleComponent.targetGraphic = _gridClosedEyeImage.GetComponent<Image>();
			} else {
				_gridEyeImage.SetActive(true);
				_gridClosedEyeImage.SetActive(false);
				_gridEyeToggleComponent.targetGraphic = _gridEyeImage.GetComponent<Image>();
			}
		}

		public void SetLayerText(string text) {
			_layerText.text = text;
		}

		public void SetSelectedTileSize(Vector2 newSize) {
			_selectedTile.GetComponent<RectTransform>().sizeDelta = newSize;
		}

		public void SetSelectedTileImageSize(Vector2 newSize) {
			_selectedTileImage.GetComponent<RectTransform>().sizeDelta = newSize;
		}

		public void SetSelectedTileImageSprite(Sprite newSprite) {
			_selectedTileImage.sprite = newSprite;
		}

		public void SetPrefabParentCellSize(Vector2 newSize) {
			_prefabParent.GetComponent<GridLayoutGroup>().cellSize = newSize;
		}
	}
}