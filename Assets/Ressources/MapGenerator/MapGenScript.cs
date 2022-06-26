using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenScript : MonoBehaviour {

    public Transform trans;
	public float TileDimension=4f;
	public bool ClusterMode=false;
	public GameObject[] prefabClusterBlue;
	public GameObject[] prefabClusterBlack;
	public GameObject[] prefabClusterWhite;
	public GameObject[] prefabClusterRed;
	public GameObject[] prefabClusterGreen;
	public GameObject[] prefabClusterCyan;
	public GameObject[] prefabClusterMagenta;

    public GameObject[] prefabFloor;
    public GameObject[] prefabWall;
    public GameObject[] prefabCurveL;
    public GameObject[] prefabDiagonal;
	public GameObject[] prefabCollumn;
    public GameObject[] prefabCeiling;
    public Texture2D Map;

    private int width;
    private int height;

    
    public void PressButon() {
		if(!ClusterMode)
			GenerateMap();
		else
			GenerateClusterMap();
    }

	private void GenerateClusterMap(){ //here we generate maps using the cluster system. There is no rotation here.
		float multiplierFactor = TileDimension + float.Epsilon;
		width = Map.width;
		height = Map.height;
		Color[] pixels = Map.GetPixels();
		for (int i = 0; i < height; i++) {
			for (int j = 0; j < width; j++) {
				Color pixelColor = pixels[i * height + j];
				if (pixelColor == Color.white) {
					if(prefabClusterWhite.Length!=0) {
						GameObject inst = GameObject.Instantiate(randomPrefab(prefabClusterWhite), trans);
						inst.transform.position = new Vector3(j* multiplierFactor, 0, i* multiplierFactor);
					}
				}
				if (pixelColor == Color.black) {
					if(prefabClusterBlack.Length!=0) {
						GameObject inst = GameObject.Instantiate(randomPrefab(prefabClusterBlack), trans);
						inst.transform.position = new Vector3(j* multiplierFactor, 0, i* multiplierFactor);
					}
				}
				if (pixelColor == Color.blue) {
					if(prefabClusterBlue.Length!=0) {
						GameObject inst = GameObject.Instantiate(randomPrefab(prefabClusterBlue), trans);
						inst.transform.position = new Vector3(j* multiplierFactor, 0, i* multiplierFactor);
					}
				}
				if (pixelColor == Color.red) {
					if(prefabClusterRed.Length!=0) {
						GameObject inst = GameObject.Instantiate(randomPrefab(prefabClusterRed), trans);
						inst.transform.position = new Vector3(j* multiplierFactor, 0, i* multiplierFactor);
					}
				}
				if (pixelColor == Color.green) {
					if(prefabClusterGreen.Length!=0) {
						GameObject inst = GameObject.Instantiate(randomPrefab(prefabClusterGreen), trans);
						inst.transform.position = new Vector3(j* multiplierFactor, 0, i* multiplierFactor);
					}
				}
				if (pixelColor == Color.cyan) {
					if(prefabClusterCyan.Length!=0) {
						GameObject inst = GameObject.Instantiate(randomPrefab(prefabClusterCyan), trans);
						inst.transform.position = new Vector3(j* multiplierFactor, 0, i* multiplierFactor);
					}
				}
				if (pixelColor == Color.magenta) {
					if(prefabClusterMagenta.Length!=0) {
						GameObject inst = GameObject.Instantiate(randomPrefab(prefabClusterMagenta), trans);
						inst.transform.position = new Vector3(j* multiplierFactor, 0, i* multiplierFactor);
					}
				}
			}
		}
	}

    private void GenerateMap() { //Here, regular maps are generated, using our rotation system.
		float multiplierFactor = TileDimension + float.Epsilon;
		width = Map.width;
		height = Map.height;
		Color[] pixels = Map.GetPixels();
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                Color pixelColor = pixels[i * height + j]; //Each color prefab is assign as follows:
				if (pixelColor == Color.white) { //Floor
					if(prefabFloor.Length!=0) {
                   		GameObject inst = GameObject.Instantiate(randomPrefab(prefabFloor), trans);
                    	inst.transform.position = new Vector3(j* multiplierFactor, 0, i* multiplierFactor);
					}
                }
                if (pixelColor == Color.cyan) { //Ceiling
					if(prefabFloor.Length!=0) {
                   		GameObject inst = GameObject.Instantiate(randomPrefab(prefabCeiling), trans);
                    	inst.transform.position = new Vector3(j* multiplierFactor, 0, i* multiplierFactor);
					}
                }
                if (pixelColor == Color.red) //Wall
                { 
					if(prefabWall.Length!=0) {
                    	GameObject inst = GameObject.Instantiate(randomPrefab(prefabWall), trans);
                    	inst.transform.position = new Vector3(j * multiplierFactor, 0, i * multiplierFactor);
						inst.transform.Rotate(new Vector3(0, FindRotationW(pixels, i, j), 0), Space.Self);
					}
                }
                if (pixelColor == Color.green) //Retangular L Curve
                {
					if(prefabCurveL.Length!=0) {
                    	GameObject inst = GameObject.Instantiate(randomPrefab(prefabCurveL), trans);
                    	inst.transform.position = new Vector3(j * multiplierFactor, 0, i * multiplierFactor);
						inst.transform.Rotate(new Vector3(0, FindRotationL(pixels, i, j), 0), Space.Self);
					}
                }
				if (pixelColor == Color.magenta) //Diagonal
                {
					if(prefabDiagonal.Length!=0) {
                    	GameObject inst = GameObject.Instantiate(randomPrefab(prefabDiagonal), trans);
                    	inst.transform.position = new Vector3(j * multiplierFactor, 0, i * multiplierFactor);
					}
					//inst.transform.Rotate(new Vector3(0, 0, FindDiagonalRotation(pixels, i, j)));
                }
				if (pixelColor == Color.blue) //Collumn
				{
					if(prefabCollumn.Length!=0) {
						GameObject inst = GameObject.Instantiate(randomPrefab(prefabCollumn), trans);
						inst.transform.position = new Vector3(j * multiplierFactor, 0, i * multiplierFactor);
						inst.transform.Rotate(new Vector3(0, FindRotationC(pixels, i, j), 0), Space.Self);
					}
				}
            }
        }
    }

    //Find wall rotation for a given wall at i and j
	private float FindRotationW(Color[] pixels, int i, int j) {
        //void on the right
		float Rotation = 0f;
		//void on the bottom
		if (i-1>=0 && (pixels[(i-1)*height + j]== Color.black || pixels[(i-1)*height + j]== Color.cyan)){
			Rotation = 90f;
		}
		//void on the left
		else if (j-1>=0 && (pixels[i*height + (j-1)]==Color.black || pixels[i*height + (j-1)]==Color.cyan)){
			Rotation = 180f;
		}
		else if (i+1<height && (pixels[(i+1)*height + j]==Color.black || pixels[(i+1)*height + j]==Color.cyan)){
			Rotation = -90f;
		}
		return Rotation;
    }

    //Find a Collumn wall rotation
    private float FindRotationC(Color[] pixels, int i, int j) {
		//void is on right/up
		float Rotation = 0f;
		//void is on right/down
		if(i-1>=0 && j+1<width && (pixels[(i-1)*height+(j+1)]==Color.black || pixels[(i-1)*height+(j+1)]==Color.cyan))
			Rotation = 90f;
		//voif is on bottom/left
		else if (i-1>=0 && j-1>=0 && (pixels[(i-1)*height+(j-1)]==Color.black || pixels[(i-1)*height+(j-1)]==Color.cyan))
			Rotation = 180f;
		else if (i+1<height && j-1>=0 && (pixels[(i+1)*height+(j-1)]==Color.black || pixels[(i+1)*height+(j-1)]==Color.cyan))
			Rotation = -90f;
		return Rotation;
	}

    //Find Rotation for a Curve L wall
	private float FindRotationL(Color[] pixels, int i, int j) {
		//void is in the top And right
		float rotation = 0;
		//void is in the right And bottom
		if (((pixels[i * height + j - 1] == Color.black || pixels[i * height + j - 1] == Color.cyan)) && ((pixels[(i - 1) * height + j] == Color.black) || (pixels[(i - 1) * height + j] == Color.cyan)))
			rotation = 180;
		//void is up And Left
		if (((pixels[i * height + j - 1] == Color.black) || (pixels[i * height + j - 1] == Color.cyan)) && ((pixels[(i+1) * height + j] == Color.black) || (pixels[(i+1) * height + j] == Color.cyan)))
			rotation = -90;
		if (((pixels[i * height + j + 1] == Color.black) || (pixels[i * height + j + 1] == Color.cyan)) && ((pixels[(i - 1) * height + j] == Color.black) || (pixels[(i - 1) * height + j] == Color.cyan)))
			rotation = 90;
		return rotation;
	}


    //Return a random prefab from a given array of prefabs
    private GameObject randomPrefab(GameObject[] prefabArray) {
		if (prefabArray.Length > 0)
        	return prefabArray[Random.Range(0, prefabArray.Length-1)];
		return null;
    }
}
