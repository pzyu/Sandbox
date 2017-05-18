using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour {
	private int _x, _y;
	private int _neighbors;
	private bool _isDestroyed;

	private Vector3 _destination;
    private float _speed = 10f;

	private Vector3 _screenPoint;
	private Vector3 _offset;
	private bool _isGrabbed;
    private bool _canSwap;

	private TextMesh _debugText;
    private string _debugTextFormatter = "X:{0} Y:{1}\nG:{2}\nS:{3}";

	public Sprite[] spriteArray = new Sprite[5];
	public enum TYPE {
		GIRAFFE,
		MONKEY,
		PANDA,
		PARROT,
		PIG,
		NULL
	};
	private TYPE _orbType = TYPE.NULL;

	private GridManager _gridManager;
	private BoxCollider _boxCollider;

	// Initialize variables
	public void Initialize(int x, int y, GridManager gridManager) {
		_x = x;
		_y = y;
		_neighbors = 1;
		_isDestroyed = false;

		_debugText = transform.GetChild(0).GetComponent<TextMesh>();
		_gridManager = gridManager;
		_boxCollider = GetComponent<BoxCollider>();

		_destination = transform.position;
		_isGrabbed = false;

		GetComponent<SpriteRenderer>().color = Color.white;

		//Debug.Log("Initializing orb at: " + _x + ", " + _y);

		UpdateDebugText();
		RandomizeType();
	}

	// Randomize type and change texture
	void RandomizeType() {
		int rand = Random.Range(0, 4);

		_orbType = (TYPE)rand;
		GetComponent<SpriteRenderer>().sprite = spriteArray[rand];
	}

	// Use this for initialization
	void Start() {
	}
	
	// Update is called once per frame
	void Update() {
        if (!_isGrabbed) {
            transform.position = Vector3.MoveTowards(transform.position, _destination, _speed * Time.deltaTime);
        }

        UpdateDebugText();
	}

	public void UpdateDebugText() {
		//Debug.Log("Updated X:" + _x + ", Y:" + _y);
        _debugText.text = string.Format(_debugTextFormatter, _x, _y, _isGrabbed, _canSwap);
	}

	// Getter/Setters
	public int GetX() {
		return _x;
	}

	public void SetX(int x) {
		_x = x;
	}

	public int GetY() {
		return _y;
	}

	public void SetY(int y) {
		_y = y;
	}

	public bool IsDestroyed() {
		return _isDestroyed;
	}

	public void SetDestroyed(bool isDestroyed) {
		_isDestroyed = isDestroyed;
	}

    public bool CanSwap() {
        return _canSwap;
    }

    public void SetSwap(bool canSwap) {
        _canSwap = canSwap;
    }

	public int GetNeighbors() {
		return _neighbors;
	}

	public void SetNeighbors(int neighbors) {
		_neighbors = neighbors;
	}

	public Vector3 GetDestination() {
		return _destination;
	}

	public void SetDestination(Vector3 destination) {
        Debug.Log("Setting dest of: " + _x + ", " + _y);
		_destination = destination;
	}

	public new TYPE GetType() {
		return _orbType;
	}

	public void OnMouseDown() {
		_screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		_offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPoint.z));
		Debug.Log("Picked up: " + _x + ", " + _y);
	}

	public void OnMouseDrag() {
		_isGrabbed = true;

		Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPoint.z);
		Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);// + _offset;
		transform.position = new Vector3(curPosition.x, curPosition.y, -0.1f);

		// Push collider to the back so we can detect other orbs
		_boxCollider.center = new Vector3(0, 0, 0.5f);

		//Debug.Log("Drag: " + _x + ", " + _y);

		_gridManager.GrabOrb(this);
	}

	public void OnMouseUp() {
		_isGrabbed = false;
		transform.position = _destination;
		_boxCollider.center = new Vector3(0, 0, 0);

		_gridManager.ReleaseOrb(this);
	}

	public void OnMouseOver() {
		if (!_isGrabbed) {
			_gridManager.HoverOrb(this);
		}
	}

	public void DestroyOrb() {
		//_orbType = TYPE.NULL;
		GetComponent<SpriteRenderer>().color = Color.green;
		_isDestroyed = true;
		//transform.position = new Vector3(transform.position.x, transform.position.y, 10);
	}

	public void Reset() {
		Debug.Log("Resetting X:" + _x + ", Y:" + _y);
		RandomizeType();
		GetComponent<SpriteRenderer>().color = Color.white;
		_isDestroyed = false;
	}
}
