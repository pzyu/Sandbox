using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricGridManager : MonoBehaviour {

	public int rows, cols;
	public GameObject gridPrefab;

	//private float _gridSize;

	// Initialize GridManager
	public void Initialize() {
		//_gridSize = gridPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
		//InitializeGrid();
	}

	// Use this for initialization
	void Start () {
		Initialize();
	}

	// Update is called once per frame
	void Update () {
		
	}

	// Initialize grid based on rows and cols
	/*void InitializeGrid() {
		float baseX = transform.position.x;
		float baseY = transform.position.y;
		float baseZ = transform.position.z;

		float isometricOffset = _gridSize;
			
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < cols; x++) {
				float offsetX = 0;
				float offsetY = isometricOffset * 1.35f;
				// If row is odd
				if (y % 2 != 1) {
					offsetX = isometricOffset / 2;
					offsetY = isometricOffset * 1.35f;
				}

				GameObject currentGrid = Instantiate(gridPrefab, new Vector3(baseX + offsetX + (_gridSize * x), baseY + (-_gridSize/offsetY * y), baseZ), Quaternion.identity);
				currentGrid.GetComponent<Orb>().Initialize(x, y);
			}
		}
	}*/
}
