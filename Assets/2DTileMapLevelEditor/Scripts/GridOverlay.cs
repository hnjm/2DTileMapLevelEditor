using UnityEngine;
using System.Collections;

public class GridOverlay : MonoBehaviour {

	public static GridOverlay instance = null;

	private int gridSizeX = 10;
	private int gridSizeY = 10;
	private int gridSizeZ = 0;

	public float smallStep = 0.5f;
	public float largeStep = 1;

	public float startX = 0;
	public float startY = 0;
	public float startZ = 0;

	private float offsetX = -0.5f;
	private float offsetY = -0.5f;

	public Material lineMaterial;

	public Color mainColor = new Color(1f,1f,1f,1f);

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if(instance != this){
			Destroy(gameObject);
		}
	}

	public void SetGridSizeX(int x){
		gridSizeX = x;
	}

	public void SetGridSizeY(int y){
		gridSizeY = y;
	}

	public void GridUp(){
		offsetY += smallStep;
	}

	public void GridDown(){
		offsetY -= smallStep;
	}

	public void GridLeft(){
		offsetX -= smallStep;
	}

	public void GridRight(){
		offsetX += smallStep;
	}

	void OnPostRender()
	{        
		// set the current material
		lineMaterial.SetPass (0);

		GL.Begin (GL.LINES);

		GL.Color (mainColor);

		//Layers
		for (float j = 0; j <= gridSizeY; j += largeStep) {
			//X axis lines
			for (float i = 0; i <= gridSizeZ; i += largeStep) {
				GL.Vertex3 (startX + offsetX, j + offsetY, startZ + i);
				GL.Vertex3 (gridSizeX + offsetX, j + offsetY, startZ + i);
			}

			//Z axis lines
			for (float i = 0; i <= gridSizeX; i += largeStep) {
				GL.Vertex3 (startX + i + offsetX, j + offsetY, startZ);
				GL.Vertex3 (startX + i + offsetX, j + offsetY, gridSizeZ);
			}
		}

		//Y axis lines
		for (float i = 0; i <= gridSizeZ; i += largeStep) {
			for (float k = 0; k <= gridSizeX; k += largeStep) {
				GL.Vertex3 (startX + k + offsetX, startY + offsetY, startZ + i);
				GL.Vertex3 (startX + k + offsetX, gridSizeY + offsetY, startZ + i);
			}
		}

		GL.End ();
	}
}
