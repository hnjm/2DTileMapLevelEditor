using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class FileBrowser : MonoBehaviour {

	private int errorCounter = 0;

	private string currentPath = Directory.GetCurrentDirectory ();
	private string currentFile;
	private string selectedFile;

	private GameObject directoryUpButton;
	public float timeBetweenClicks = 0.3f;  // Allow 3 clicks per second
	private float timestamp;

	private GameObject closeFileBrowserButton;
	private GameObject selectFileButton;


	private GameObject pathText;
	private GameObject saveFileText;
	private GameObject loadFileText;

	// GameObject as the parent for all the GameObject in the tiles selection
	private GameObject directoriesParent;

	private GameObject filesParent;

	// Button Prefab used to create tile selection buttons for each GameObjects.
	public GameObject fileBrowserButtonPrefab;

	// UI
	public GameObject fileBrowserUIPrefab;

	public string fileFilter = null;

	private string saveFileName;

	private MonoBehaviour other = null;
	private string callbackMethod;

	private bool saving;

	public Sprite saveImage;
	public Sprite loadImage;


	// Use this for initialization
	void Awake () {

		//------ UI ---------

		// Instantiate the LevelEditorUI
		GameObject canvas = GameObject.Find("Canvas");
		if (canvas == null) {
			errorCounter++;
			Debug.LogError ("Make sure there is a canvas GameObject present in the Hierary (Create UI/Canvas)");
		}

		GameObject fileBrowserUITInstance = Instantiate (fileBrowserUIPrefab, canvas.transform);
		fileBrowserUITInstance.name = "FileBrowserUI";

		SetupFileBrowserTest ();
	}

	private GameObject FindGameObjectOrError(string name){
		GameObject gameObject = GameObject.Find (name);
		if (gameObject == null) {
			errorCounter++;
			Debug.LogError ("Make sure " + name + " is present");
			return null;
		} else {
			return gameObject;
		}
	}

	private void SetupFileBrowserTest(){
		directoryUpButton = FindGameObjectOrError ("DirectoryUpButton");
		directoryUpButton.GetComponent<Button> ().onClick.AddListener (() => {
			DirectoryUpButtonClick (currentPath);
		});

		closeFileBrowserButton = FindGameObjectOrError ("CloseFileBrowserButton");
		closeFileBrowserButton.GetComponent<Button> ().onClick.AddListener (() => {
			CloseFileBrowserButtonClick ();
		});

		selectFileButton = FindGameObjectOrError ("SelectFileButton");
		selectFileButton.GetComponent<Button> ().onClick.AddListener (() => {
			SelectFileButtonClick ();
		});

		pathText = FindGameObjectOrError ("PathText");
		loadFileText = FindGameObjectOrError ("LoadFileText");
		saveFileText = FindGameObjectOrError ("SaveFileText");

		saveFileText.GetComponent<InputField> ().onEndEdit.AddListener (SetSaveFileName);


		directoriesParent = GameObject.Find ("Directories");
		filesParent = GameObject.Find ("Files");
		UpdateFileBrowser ();
	}

	private void UpdateFileBrowser(){
		if (pathText != null && pathText.GetComponent<Text> () != null) {
			pathText.GetComponent<Text> ().text = currentPath;
		}
		if (loadFileText != null && loadFileText.GetComponent<Text> () != null) {
			loadFileText.GetComponent<Text> ().text = currentFile;
		}

		if (directoriesParent.transform.childCount > 0) {
			foreach (Transform child in directoriesParent.transform) {
				GameObject.Destroy(child.gameObject);
			}
		}
		foreach (string dir in Directory.GetDirectories (currentPath)) {
			GameObject button = Instantiate (fileBrowserButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			button.GetComponent<Text> ().text = new DirectoryInfo(dir).Name;
			button.transform.SetParent (directoriesParent.transform, false);
			button.transform.localScale = new Vector3 (1f, 1f, 1f);
			button.GetComponent<Button> ().onClick.AddListener (() => {
				FileBrowserDirectoryButtonClick (dir);
			});
		}

		if (filesParent.transform.childCount > 0) {
			foreach (Transform child in filesParent.transform) {
				GameObject.Destroy(child.gameObject);
			}
		}
		string[] files = (fileFilter == null ? Directory.GetFiles (currentPath) : Directory.GetFiles (currentPath, fileFilter));
		foreach (string file in Directory.GetFiles (currentPath)) {
			GameObject button = Instantiate (fileBrowserButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			button.GetComponent<Text> ().text = Path.GetFileName(file);
			button.transform.SetParent (filesParent.transform, false);
			button.transform.localScale = new Vector3 (1f, 1f, 1f);
			button.GetComponent<Button> ().onClick.AddListener (() => {
				FileBrowserFileButtonClick (file);
			});
		}
	}

	// Method to switch selectedTile on tile selection
	private void FileBrowserDirectoryButtonClick (string path)
	{
		currentPath = path;
		UpdateFileBrowser ();
	}


	// Method to switch selectedTile on tile selection
	private void FileBrowserFileButtonClick (string clickedFile)
	{
		if (saving) {
			string clickedFileName = Path.GetFileName (clickedFile);
			saveFileName = clickedFileName;
			saveFileText.GetComponent<InputField> ().text = Path.GetFileName(clickedFileName);
		} else {
			currentFile = clickedFile;
		}
		UpdateFileBrowser ();
	}

	private void CloseFileBrowserButtonClick(){
		Destroy (GameObject.Find("FileBrowserUI"));
		Destroy (GameObject.Find("FileBrowser"));
		other.SendMessage("ToggleLevelEditor", true);
	}

	private void DirectoryUpButtonClick(string path){
		if (Time.time >= timestamp) {
			timestamp = Time.time + timeBetweenClicks;
			if (Directory.GetParent (path) != null) {
				currentPath = Directory.GetParent (path).FullName;
				UpdateFileBrowser ();
			}
		}
	}

	private void SelectFileButtonClick(){
		if (saving) {
			other.SendMessage (callbackMethod, currentPath + "/" + saveFileName);
		} else {
			other.SendMessage (callbackMethod, currentFile);
		}
		Destroy (GameObject.Find("FileBrowserUI"));
		Destroy (GameObject.Find("FileBrowser"));
	}

	public void SetSaveFileName(string saveFileName){
		this.saveFileName = saveFileName;
	}

	//SaveFilePanel ("Save level", "", "LevelName", fileExtension);
	public void SaveFilePanel (MonoBehaviour other, string callbackMethod){
		saving = true;
		saveFileText.SetActive (true);
		loadFileText.SetActive (false);
		selectFileButton.GetComponent<Image>().sprite = saveImage;
		this.other = other;
		this.callbackMethod = callbackMethod;
	}
	//OpenFilePanel ("Open level", "", fileExtension);
	public void OpenFilePanel(MonoBehaviour other, string callbackMethod){
		saving = false;
		loadFileText.SetActive (true);
		selectFileButton.GetComponent<Image>().sprite = loadImage;
		saveFileText.SetActive (false);
		this.other = other;
		this.callbackMethod = callbackMethod;
	}
}
