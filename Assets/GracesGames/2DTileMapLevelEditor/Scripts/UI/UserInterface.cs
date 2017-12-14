using UnityEngine;
using UnityEngine.UI;

using GracesGames.Common.Scripts;

namespace GracesGames._2DTileMapLevelEditor.Scripts.UI {

	public class UserInterface : MonoBehaviour {

		// ----- PUBLIC VARIABLES -----

		// Button Prefab used to create tile selection buttons for each GameObjects.
		public GameObject ButtonPrefab;

		// Dimensions used for the representation of the GameObject tile selection buttons
		// Represented using a 0-200 slider in the editor
		[Range(1.0f, 200.0f)] public float ButtonSize = 100;

		// Dimensions used for the representation of the selected tile game object
		// Represented using a 0-200 slider in the editor
		[Range(1.0f, 200.0f)] public float SelectedTileSize = 100;

		// Scale of the images in regards to the total image rectangle size
		[Range(0.1f, 1.0f)] public float ButtonImageScale = 0.8f;

		// Sprite to indicate no tile is currently selected
		public Sprite NoSelectedTileImage;

		// UI objects to display pencil/fill mode
		public Texture2D FillCursor;

		// ----- PRIVATE VARIABLES -----

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

		// ----- SETUP -----

		public void Setup() {
			name = ("LevelEditorUI");
			_levelEditor = LevelEditor.Instance;
			_levelEditorPanel = Utilities.FindGameObjectOrError("LevelEditorPanel");
			SetupSaveLoadButton();
			SetupUndoRedoButton();
			SetupModeButtons();
			SetupZoomButtons();
			SetupLayerButtons();
			SetupGridButtons();
			SetupOpenCloseButton();
			SetupSelectedTile();
			SetupPrefabsButtons();

			// Set the initial prefab button size
			UpdatePrefabButtonsSize();
		}

		private void SetupSaveLoadButton() {
			// Hook up Save/Load Level method to Save/Load button
			Utilities.FindButtonAndAddOnClickListener("SaveButton", _levelEditor.SaveLevel);
			Utilities.FindButtonAndAddOnClickListener("LoadButton", _levelEditor.LoadLevel);
		}

		private void SetupUndoRedoButton() {
			// Hook up Undo/Redo method to Undo/Redo button
			Utilities.FindButtonAndAddOnClickListener("UndoButton", _levelEditor.Undo);
			Utilities.FindButtonAndAddOnClickListener("RedoButton", _levelEditor.Redo);
		}

		private void SetupModeButtons() {
			// Hook up EnablePencilMode method to PencilButton
			GameObject pencilModeButton =
				Utilities.FindButtonAndAddOnClickListener("PencilButton", _levelEditor.DisableFillMode);
			_pencilModeButtonImage = pencilModeButton.GetComponent<Image>();

			// Hook up EnableFillMode method to FillButton
			GameObject fillModeButton = Utilities.FindButtonAndAddOnClickListener("FillButton", _levelEditor.EnableFillMode);
			_fillModeButtonImage = fillModeButton.GetComponent<Image>();
		}

		private void SetupZoomButtons() {
			// Hook up Zoom methods to Zoom buttons
			Utilities.FindButtonAndAddOnClickListener("ZoomInButton", _levelEditor.ZoomIn);
			Utilities.FindButtonAndAddOnClickListener("ZoomOutButton", _levelEditor.ZoomOut);
			Utilities.FindButtonAndAddOnClickListener("ZoomDefaultButton", _levelEditor.ZoomDefault);
		}

		private void SetupLayerButtons() {
			// Hook up Layer Change methods to Layer Change buttons
			Utilities.FindButtonAndAddOnClickListener("+LayerButton", _levelEditor.LayerUp);
			Utilities.FindButtonAndAddOnClickListener("-LayerButton", _levelEditor.LayerDown);

			// Hook up ToggleOnlyShowCurrentLayer method to OnlyShowCurrentLayerToggle
			GameObject onlyShowCurrentLayerToggle = Utilities.FindGameObjectOrError("OnlyShowCurrentLayerToggle");
			_layerEyeImage = GameObject.Find("LayerEyeImage");
			_layerClosedEyeImage = GameObject.Find("LayerClosedEyeImage");
			_onlyShowCurrentLayerToggleComponent = onlyShowCurrentLayerToggle.GetComponent<Toggle>();
			_onlyShowCurrentLayerToggleComponent.onValueChanged.AddListener(_levelEditor.ToggleOnlyShowCurrentLayer);

			// Instantiate the LayerText game object to display the current layer
			_layerText = Utilities.FindGameObjectOrError("LayerText").GetComponent<Text>();
		}

		private void SetupGridButtons() {
			// Hook up ToggleGrid method to GridToggle
			GameObject gridEyeToggle = Utilities.FindGameObjectOrError("GridEyeToggle");
			_gridEyeImage = Utilities.FindGameObjectOrError("GridEyeImage");
			_gridClosedEyeImage = Utilities.FindGameObjectOrError("GridClosedEyeImage");
			_gridEyeToggleComponent = gridEyeToggle.GetComponent<Toggle>();
			_gridEyeToggleComponent.onValueChanged.AddListener(_levelEditor.ToggleGrid);

			// Hook up Grid Size methods to Grid Size buttons
			Utilities.FindButtonAndAddOnClickListener("GridSizeUpButton", GridOverlay.Instance.GridSizeUp);
			Utilities.FindButtonAndAddOnClickListener("GridSizeDownButton", GridOverlay.Instance.GridSizeDown);

			// Hook up Grid Navigation methods to Grid Navigation buttons
			Utilities.FindButtonAndAddOnClickListener("GridUpButton", GridOverlay.Instance.GridUp);
			Utilities.FindButtonAndAddOnClickListener("GridDownButton", GridOverlay.Instance.GridDown);
			Utilities.FindButtonAndAddOnClickListener("GridLeftButton", GridOverlay.Instance.GridLeft);
			Utilities.FindButtonAndAddOnClickListener("GridRightButton", GridOverlay.Instance.GridRight);
		}

		private void SetupOpenCloseButton() {
			// Hook up CloseLevelEditorPanel method to CloseButton
			Utilities.FindButtonAndAddOnClickListener("CloseButton", _levelEditor.CloseLevelEditorPanel);

			// Hook up OpenLevelEditorPanel method to OpenButton and disable at start
			_openButton = Utilities.FindButtonAndAddOnClickListener("OpenButton", _levelEditor.OpenLevelEditorPanel);
			_openButton.SetActive(false);
		}

		private void SetupSelectedTile() {
			_selectedTile = Utilities.FindGameObjectOrError("SelectedTile");
			// Find the image component of the SelectedTileImage GameObject
			_selectedTileImage = Utilities.FindGameObjectOrError("SelectedTileImage").GetComponent<Image>();
		}

		private void SetupPrefabsButtons() {
			// Find the prefabParent object and set the cellSize for the tile selection buttons
			_prefabParent = Utilities.FindGameObjectOrError("Prefabs");
			if (_prefabParent.GetComponent<GridLayoutGroup>() == null) {
				Debug.LogError("Make sure prefabParent has a GridLayoutGroup component");
			}
			// Counter to determine which tile button is pressed
			int tileCounter = 0;
			//Create a button for each tile in tiles
			foreach (Transform tile in LevelEditor.Instance.GetTiles()) {
				int index = tileCounter;
				GameObject button = Instantiate(ButtonPrefab, Vector3.zero, Quaternion.identity);
				button.name = tile.name;
				button.GetComponent<Image>().sprite = tile.gameObject.GetComponent<SpriteRenderer>().sprite;
				button.transform.SetParent(_prefabParent.transform, false);
				button.transform.localScale = new Vector3(ButtonImageScale, ButtonImageScale, ButtonImageScale);
				// Add a click handler to the button
				button.GetComponent<Button>().onClick.AddListener(() => { _levelEditor.ButtonClick(index); });
				tileCounter++;
			}
		}

		// ----- TOGGLES -----

		// Enables/disables the Level Editor Panel
		public void ToggleLevelEditorPanel(bool enable) {
			_levelEditorPanel.SetActive(enable);
		}

		// Enables/disables the Open Button (inverse of Level Editor Panel toggle)
		public void ToggleOpenButton(bool enable) {
			_openButton.SetActive(enable);
		}

		// Updates the Fill Mode options in the interface (pencil or fill mode enabled)
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

		// Updates the cursor to the bucket if fill mode is enabled and the position is valid (Level Editor logic) 
		public void ToggleFillModeCursor(bool enable) {
			if (enable) {
				Cursor.SetCursor(FillCursor, new Vector2(30, 25), CursorMode.Auto);
			} else {
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
		}

		// Updates the Layer options in the interface
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

		// Updates the Grid options in the interface
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

		// ----- SET IMAGES -----

		// Updates the selected tile image.
		// Either sets it to the NoSelectedTileImage when the tileIndex is empty (default -1_
		// Or to the sprite of the selected tile
		public void SetSelectedTileImageSprite(int tileIndex) {
			_selectedTileImage.sprite = (tileIndex == LevelEditor.GetEmpty()
				? NoSelectedTileImage
				: LevelEditor.Instance.GetTiles()[tileIndex].gameObject.GetComponent<SpriteRenderer>().sprite);
		}

		// ----- UPDATE METHODS -----

		// Updates the User Interface so it is configurable at run-time
		void Update() {
			// Only continue if the script is enabled (level editor is open) and there are no errors
			if (_levelEditor.GetScriptEnabled()) {
				// Update the button size to scale at runtime
				UpdatePrefabButtonsSize();
				// Update the selected tile game object to scale at runtime
				UpdateSelectedTileSize();
				// Update the layer text
				UpdateLayerText();
			}
		}

		// Update the size of the prefab tile objects, the images will be square to keep the aspect ratio original
		private void UpdatePrefabButtonsSize() {
			_prefabParent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(ButtonSize, ButtonSize);
		}

		// Update the size of the selected tile game object, the images will be scaled to half that
		private void UpdateSelectedTileSize() {
			_selectedTile.GetComponent<RectTransform>().sizeDelta = new Vector2(SelectedTileSize, SelectedTileSize);
			_selectedTileImage.GetComponent<RectTransform>().sizeDelta = new Vector2(SelectedTileSize / 2, SelectedTileSize / 2);
		}

		// Method that updates the LayerText
		private void UpdateLayerText() {
			_layerText.text = "" + (LevelEditor.Instance.GetSelectedLayer() + 1);
		}
	}
}