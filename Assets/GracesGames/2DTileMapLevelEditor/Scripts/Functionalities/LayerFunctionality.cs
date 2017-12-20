using UnityEngine;
using UnityEngine.UI;

using GracesGames.Common.Scripts;

namespace GracesGames._2DTileMapLevelEditor.Scripts.Functionalities {

	public class LayerFunctionality : MonoBehaviour {

		private LevelEditor _levelEditor;
		
		private int _selectedLayer;
		private int _totalyayers;
		
		// Boolean to determine whether to show all layers or only the current one
		private bool _onlyShowCurrentLayer;
		
		// UI objects to toggle onlyShowCurrentLayer
		private GameObject _layerEyeImage;

		private GameObject _layerClosedEyeImage;
		private Toggle _onlyShowCurrentLayerToggleComponent;
		
		// Text used to represent the currently selected layer
		private Text _layerText;

		public void Setup(int layers) {
			_levelEditor = LevelEditor.Instance;
			_totalyayers = layers;
			SetupClickListeners();
		}
		
		// Hook up Layer methods to Layer button
		private void SetupClickListeners() {
			// Hook up Layer Change methods to Layer Change buttons
			Utilities.FindButtonAndAddOnClickListener("+LayerButton", LayerUp);
			Utilities.FindButtonAndAddOnClickListener("-LayerButton", LayerDown);

			// Hook up ToggleOnlyShowCurrentLayer method to OnlyShowCurrentLayerToggle
			_layerEyeImage = GameObject.Find("LayerEyeImage");
			_layerClosedEyeImage = GameObject.Find("LayerClosedEyeImage");
			_onlyShowCurrentLayerToggleComponent = Utilities.FindGameObjectOrError("OnlyShowCurrentLayerToggle").GetComponent<Toggle>();
			_onlyShowCurrentLayerToggleComponent.onValueChanged.AddListener(ToggleOnlyShowCurrentLayer);

			// Instantiate the LayerText game object to display the current layer
			_layerText = Utilities.FindGameObjectOrError("LayerText").GetComponent<Text>();
		}
		
		private void Update() {
			// Update the layer text
			UpdateLayerText();
		}
		
		
		// Method that updates the LayerText
		private void UpdateLayerText() {
			_layerText.text = "" + (_selectedLayer + 1);
		}

		public int GetSelectedLayer() {
			return _selectedLayer;
		}
		
		// Method that updates which layers should be shown
		public void UpdateLayerVisibility() {
			if (_onlyShowCurrentLayer) {
				OnlyShowCurrentLayer();
			} else {
				ShowAllLayers();
			}
		}
		
		// Method that increments the selected layer
		private void LayerUp() {
			_selectedLayer = Mathf.Min(_selectedLayer + 1, _totalyayers - 1);
			UpdateLayerVisibility();
		}

		// Method that decrements the selected layer
		private void LayerDown() {
			_selectedLayer = Mathf.Max(_selectedLayer - 1, 0);
			UpdateLayerVisibility();
		}
		
		// Method that handles the UI toggle to only show the current layer
		private void ToggleOnlyShowCurrentLayer(bool enable) {
			_onlyShowCurrentLayer = enable;
			if (enable) {
				_layerEyeImage.SetActive(true);
				_layerClosedEyeImage.SetActive(false);
				_onlyShowCurrentLayerToggleComponent.targetGraphic = _layerEyeImage.GetComponent<Graphic>();
				OnlyShowCurrentLayer();
			} else {
				_layerClosedEyeImage.SetActive(true);
				_layerEyeImage.SetActive(false);
				_onlyShowCurrentLayerToggleComponent.targetGraphic = _layerClosedEyeImage.GetComponent<Graphic>();
				ShowAllLayers();
			}
		}
		
		// Method that enables all layers
		private void ShowAllLayers() {
			_levelEditor.ToggleLayerParents(true);
		}

		// Method that disables all layers except the given one
		private void OnlyShowCurrentLayer() {
			_levelEditor.ToggleLayerParents(false);
			_levelEditor.ToggleLayerParent(_selectedLayer, true);
		}
	}
}
