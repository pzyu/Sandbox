using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {
	public int rows, cols;
	public GameObject gridPrefab;

	private float _gridWidth, _gridHeight;

	private Orb[,] _orbArray;
	private Orb _grabbedOrb, _hoverOrb;

	private bool _isSwapped;
	private int _matchCount = 3;
    private List<Orb> _destroyedOrbs;

	private float _debugBoardDelay = 0.1f;
	private bool _canUpdateDebug = false;
	private int _debugX, _debugY;

	private bool _hasMatches = true;

	// Initialize GridManager
	public void Initialize() {
		_gridWidth = gridPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
		_gridHeight = gridPrefab.GetComponent<SpriteRenderer>().bounds.size.y;
		_isSwapped = false;
		InitializeGrid();
		//InvokeRepeating("CheckBoardDebug", 0, _debugBoardDelay);
	}

	// Use this for initialization
	void Start() {
		Initialize();
	}

	// Update is called once per frame
	void Update() {
		if (_hasMatches) {
			Debug.Log("Match exists");
			_hasMatches = false;
            // Check board for matches, shift orbs down, check again, reset destroyed ones, check board and shift again, then last check
			CheckBoard();
			ShiftOrbsDown();
            /*
			CheckBoard();
            ResetDestroyedOrbs();
            CheckBoard();
            ShiftOrbsDown();
            CheckBoard();*/
		}
	}

	// Initialize grid based on rows and cols
	// TODO: Make sure no matches
	void InitializeGrid() {
		float baseX = transform.position.x;
		float baseY = transform.position.y;
		float baseZ = transform.position.z;

		_debugX = _debugY = 0;
		_orbArray = new Orb[cols, rows];

		//Debug.Log("Initializing with " + rows + " rows and " + cols + " columns");
			
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < cols; x++) {
				GameObject currentOrb = Instantiate(gridPrefab, new Vector3(baseX + (_gridWidth * x), baseY + (-_gridHeight * y), baseZ), Quaternion.identity);
				_orbArray[x, y] = currentOrb.GetComponent<Orb>(); 
				_orbArray[x, y].Initialize(x, y, this);
			}
		}
	}

	// Swap orb firstOrb with orb secondOrb
	void SwapOrbs(Orb firstOrb, Orb secondOrb) {
		//Debug.Log("Swapping grabbed " + "(" + firstOrb.GetX() + ", " + firstOrb.GetX() + ") with hover (" + secondOrb.GetX() + ", " + secondOrb.GetY() + ")");

		Vector3 tempPos = secondOrb.transform.position;
        Vector3 tempDest = secondOrb.GetDestination();

		secondOrb.transform.position = firstOrb.GetDestination();
		secondOrb.SetDestination(firstOrb.GetDestination());
        firstOrb.SetDestination(tempDest);
		firstOrb.transform.position = tempPos;

		int tempX = firstOrb.GetX();
		int tempY = firstOrb.GetY();

		//Debug.Log("First was: " + _orbArray[firstOrb.GetX(), firstOrb.GetY()].GetX() + ", " + _orbArray[firstOrb.GetX(), firstOrb.GetY()].GetY());
		_orbArray[firstOrb.GetX(), firstOrb.GetY()] = secondOrb;
		firstOrb.SetX(secondOrb.GetX());
		firstOrb.SetY(secondOrb.GetY());
		firstOrb.UpdateDebugText();
		//Debug.Log("First is now: " + _orbArray[firstOrb.GetX(), firstOrb.GetY()].GetX() + ", " + _orbArray[firstOrb.GetX(), firstOrb.GetY()].GetY());

		//Debug.Log("Second was: " + _orbArray[secondOrb.GetX(), secondOrb.GetY()].GetX() + ", " + _orbArray[secondOrb.GetX(), secondOrb.GetY()].GetY());
		_orbArray[secondOrb.GetX(), secondOrb.GetY()] = firstOrb;
		secondOrb.SetX(tempX);
		secondOrb.SetY(tempY);
		secondOrb.UpdateDebugText();
		//Debug.Log("Second is now: " + _orbArray[secondOrb.GetX(), secondOrb.GetY()].GetX() + ", " + _orbArray[secondOrb.GetX(), secondOrb.GetY()].GetY());

		_isSwapped = false;
	}

	void SwapHandling() {
		if (!_isSwapped) {
			if (Mathf.Abs(_grabbedOrb.GetX() - _hoverOrb.GetX()) == 1 || Mathf.Abs(_grabbedOrb.GetY() - _hoverOrb.GetY()) == 1) {
				_isSwapped = true;
				SwapOrbs(_grabbedOrb, _hoverOrb);
			}
		}
	}

	public void HoverOrb(Orb orb) {
		//Debug.Log("Over: " + orb.GetX() + ", " + orb.GetY());
		if (_grabbedOrb != null && !_grabbedOrb.Equals(orb)) {
			_hoverOrb = orb;
			Debug.Log("Over: " + _hoverOrb.GetX() + ", " + _hoverOrb.GetY());
			SwapHandling();
		}
	}

	// Grabs an orb
	public void GrabOrb(Orb orb) {
		_grabbedOrb = orb;
		//Debug.Log("Grabbing: " + _grabbedOrb.GetX() + ", " + _grabbedOrb.GetY());	
	}

	// Releases an orb
	public void ReleaseOrb(Orb orb) {
		Debug.Log("Releasing: " + orb.GetX() + ", " + orb.GetY());	
		_grabbedOrb = null;

		//ResetBoardDebug();

		CheckBoard();
		ShiftOrbsDown();

		_canUpdateDebug = true;
	}

	// Check board for match 3 combos O(n^2) 
	void CheckBoard() {
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < cols; x++) {
				CheckRow(x, y);
				CheckCol(x, y);
			}
		}
	}

	// Find gaps below each orb and shift down
	void ShiftOrbsDown() {
		//int x = 0;
		for (int x = 0; x < cols; x++) {
			// Check from last row then up
			for (int y = rows - 1; y >= 0; y--) {
				Orb currentOrb = _orbArray[x, y];
				int startingRow = y;
				//Debug.Log("Current orb: " + currentOrb.GetX() + ", " + currentOrb.GetY());
				if (currentOrb.IsDestroyed()) {
					//currentOrb.Reset();
					continue;
				}

				// Checking below current orb
				while (startingRow + 1 < rows) {
					startingRow++;
					if (_orbArray[x, startingRow].IsDestroyed()) {
						//Debug.Log("Before orb: " + currentOrb.GetX() + ", " + currentOrb.GetY());
						//Debug.Log("Deleted orb: " + _orbArray[x, startingRow].GetX() + ", " + _orbArray[x, startingRow].GetY());
						SwapOrbs(currentOrb, _orbArray[x, startingRow]);
						//_orbArray[x, startingRow].Reset();
						//Debug.Log("After orb: " + currentOrb.GetX() + ", " + currentOrb.GetY());
					}
				}
			}
		}
	}

	// Debug board
	void CheckBoardDebug() {
		return;

		if (!_canUpdateDebug) {
			return;
		}


		while (_hasMatches) {

			CheckRow(_debugX, _debugY);
			CheckCol(_debugX, _debugY);
			ShiftOrbsDown();

			_hasMatches = false;
			_orbArray[_debugX, _debugY].GetComponent<SpriteRenderer>().color = Color.gray;
			if (_orbArray[_debugX, _debugY].IsDestroyed()) {
				_orbArray[_debugX, _debugY].Reset();
				_hasMatches = true;
			}
		} 

		_debugX++;

		if (_debugX >= cols) {
			_debugX = 0;
			_debugY++;
		}

		if (_debugY >= rows) {
			_debugY = 0;
		}

		// If no more matches
		if (!_hasMatches) {
			_hasMatches = false;
			_canUpdateDebug = false;
			Debug.Log("Check done");
			for (int y = 0; y < rows; y++) {
				for (int x = 0; x < cols; x++) {
					_orbArray[x, y].GetComponent<SpriteRenderer>().color = Color.white;
				}
			}
			_debugX = _debugY = 0;
		}
	}

	void ResetDestroyedOrbs() {
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < cols; x++) {
                if (_orbArray[x, y].IsDestroyed()) {
                    _orbArray[x, y].GetComponent<Orb>().Reset();
                }
			}
		}
	}

	// Check each columns in a row
	void CheckRow(int x, int y) {
		// Set up initial values
		List<Orb> tempArray = new List<Orb>();
		if (_orbArray[x, y].IsDestroyed()) { 
			//Debug.Log("Skipping " + x + ", " + y);
			//return;
		}
		//Debug.Log("Checking" + x + ", " + y);

		tempArray.Add(_orbArray[x, y]);

		Orb.TYPE currentType = _orbArray[x, y].GetType();
		int leftCounter = x;
		int rightCounter = x;

		// Check left 
		while (leftCounter > 0) {
			leftCounter--;
			//Debug.Log("Looking at: " + _orbArray[leftCounter, y].GetType() + " current is: " + currentType);
			if (_orbArray[leftCounter, y].GetType() == currentType && !_orbArray[leftCounter, y].IsDestroyed()) {
				tempArray.Add(_orbArray[leftCounter, y]);
			} else {
				break;
			}
		}

		// Check right 
		while (rightCounter < cols - 1) {
			rightCounter++;
			//Debug.Log("Looking at: " + _orbArray[leftCounter, y].GetType() + " current is: " + currentType);
			if (_orbArray[rightCounter, y].GetType() == currentType && !_orbArray[rightCounter, y].IsDestroyed()) {
				tempArray.Add(_orbArray[rightCounter, y]);
			} else {
				break;
			}
		}

		if (tempArray.Count >= _matchCount) {
			Debug.Log(tempArray.Count + " of " + currentType + " found horizontally");
			_hasMatches = true;
			foreach (Orb orb in tempArray) {
				orb.DestroyOrb();
			}
			tempArray.Clear();
		}
	}

	// Check each row of a col
	void CheckCol(int x, int y) {
		// Set up initial values
		List<Orb> tempArray = new List<Orb>();
		if (_orbArray[x, y].IsDestroyed()) { 
			//Debug.Log("Skipping " + x + ", " + y);
			//return;
		}
		//Debug.Log("Checking" + x + ", " + y);

		tempArray.Add(_orbArray[x, y]);

		Orb.TYPE currentType = _orbArray[x, y].GetType();
		int upCounter = y;
		int downCounter = y;

		// Check up 
		while (upCounter > 0) {
			upCounter--;
			//Debug.Log("Looking at: " + _orbArray[x, upCounter].GetType() + " current is: " + currentType);
			if (_orbArray[x, upCounter].GetType() == currentType && !_orbArray[x, upCounter].IsDestroyed()) {
				tempArray.Add(_orbArray[x, upCounter]);
			} else {
				break;
			}
		}

		// Check right 
		while (downCounter < rows - 1) {
			downCounter++;
			//Debug.Log("Looking at: " + _orbArray[x, downCounter].GetType() + " current is: " + currentType);
			if (_orbArray[x, downCounter].GetType() == currentType && !_orbArray[x, downCounter].IsDestroyed()) {
				tempArray.Add(_orbArray[x, downCounter]);
			} else {
				break;
			}
		}
		if (tempArray.Count >= _matchCount) {
			Debug.Log(tempArray.Count + " of " + currentType + " found vertically");
			_hasMatches = true;
			foreach (Orb orb in tempArray) {
				orb.DestroyOrb();
			}
			tempArray.Clear();
		}
	}
}
