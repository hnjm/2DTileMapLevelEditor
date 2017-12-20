using UnityEngine;
using UnityEngine.UI;

using GracesGames.Common.Scripts;

namespace GracesGames._2DTileMapLevelEditor.Scripts.Functionalities {

	public class FillFunctionality : MonoBehaviour {

		// UI objects to display pencil/fill mode
		public Texture2D FillCursor;

		// Boolean to determine whether to use fill mode or pencil mode
		private bool _fillMode;

		// UI objects to display pencil/fill mode
		private Image _pencilModeButtonImage;

		private Image _fillModeButtonImage;
		private static readonly Color32 DisabledColor = new Color32(150, 150, 150, 255);

		public void Setup() {
			SetupClickListeners();
			// Initally disable fill mode
			DisableFillMode();
		}

		// Hook up Mode methods to Mode button
		private void SetupClickListeners() {
			// Hook up EnablePencilMode method to PencilButton
			GameObject pencilModeButton =
				Utilities.FindButtonAndAddOnClickListener("PencilButton", DisableFillMode);
			_pencilModeButtonImage = pencilModeButton.GetComponent<Image>();

			// Hook up EnableFillMode method to FillButton
			GameObject fillModeButton = Utilities.FindButtonAndAddOnClickListener("FillButton", EnableFillMode);
			_fillModeButtonImage = fillModeButton.GetComponent<Image>();
		}
		
		public bool GetFillMode() {
			return _fillMode;
		}

		private void Update() {
			// If F is pressed, toggle FillMode;
			if (Input.GetKeyDown(KeyCode.F)) {
				ToggleFillMode();
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
		private void EnableFillMode() {
			_fillMode = true;
			_fillModeButtonImage.GetComponent<Image>().color = Color.black;
			_pencilModeButtonImage.GetComponent<Image>().color = DisabledColor;
		}

		// Disable fill mode and update UI and cursor
		private void DisableFillMode() {
			_fillMode = false;
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			_pencilModeButtonImage.GetComponent<Image>().color = Color.black;
			_fillModeButtonImage.GetComponent<Image>().color = DisabledColor;
		}

		// If fill mode is enabled, update cursor (only show fill cursor on grid)
		public void UpdateFillModeCursor(bool validPosition) {
			if (_fillMode && validPosition) {
				// If valid position, set cursor to bucket
				Cursor.SetCursor(FillCursor, new Vector2(30, 25), CursorMode.Auto);
			} else {
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
		}
	}
}