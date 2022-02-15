using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using static UnityEngine.InputSystem.InputAction;

public class Tetromino : MonoBehaviour
{
    public delegate void tetrominoPieceLandedDelegate(Tetromino t);
    public event tetrominoPieceLandedDelegate tetrominoLandedEvent;

    [SerializeField] float timeBetweenMoves = 2f;
    [SerializeField] float timeBetweenMoveOnHold = 0.20f;
    [SerializeField] bool IsActivePiece = true;
    [SerializeField] private TetrominoInput _tetrominoInput;
    


    GameState gameState;
    
    PreviewTetromino previewPiece;

    private float minXPos = 0.5f;
    private float maxXPos = 9.5f;
    private float minYPos = 0.5f;
    private float maxYPos = 19.5f;
    private float largeNumber = 100f;


    float timeSinceLastMove = 0f;
    float timeSinceLastHorizontalMove = 0f;
    bool moveLeftHeld = false;
    bool moveRightHeld = false;
    bool moveDownHeld = false;


    private void Awake() {
        _tetrominoInput = new TetrominoInput();
    }


    private void OnEnable() {
        _tetrominoInput.Player.Enable();
    }

    private void OnDisable() {
        _tetrominoInput.Player.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        previewPiece = FindObjectOfType<PreviewTetromino>();
        gameState = FindObjectOfType<GameState>();

        if (IsActivePiece) {
            /*foreach(Transform square in gameState.GetSquares()) {
                Debug.Log("Square: " + square.position.x.ToString() + ", " + square.position.y.ToString());
            }*/

            Transform[] children = GetComponentsInChildren<Transform>();
            /*foreach (Transform child in children) {
                if (child == gameObject.transform) { continue; }

                Debug.Log("I am a square in the tetromino: " + child.position.x.ToString() + ", " + child.position.y.ToString());
            }*/

            UpdatePreviewPiece();

            _tetrominoInput.Player.MoveLeft.performed += HandleMoveLeftPerformed;
            _tetrominoInput.Player.MoveRight.performed += HandleMoveRightPerformed;
            _tetrominoInput.Player.MoveDown.performed += HandleMoveDownPerformed;
            _tetrominoInput.Player.Rotate.performed += HandleRotatePerformed;
            _tetrominoInput.Player.Drop.performed += HandleDropPerformed;

            _tetrominoInput.Player.MoveLeft.canceled += HandleMoveLeftCanceled;
            _tetrominoInput.Player.MoveRight.canceled += HandleMoveRightCanceled;
            _tetrominoInput.Player.MoveDown.canceled += HandleMoveDownCanceled;

        } else {
            AddSquaresToGameState();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(IsActivePiece) {
            MovePiece();
        }
        
    }

    private void UpdatePreviewPiece() {
        float yDifference = CalculateDistanceToLandingSpot();
        string currentY = transform.position.y.ToString();
        //Debug.Log("CurrentY: " + currentY + " Found y difference of: " + yDifference.ToString() + " Piece lands on: " + (transform.position.y - yDifference).ToString());
        Vector2 newPosition = new Vector2(transform.position.x, transform.position.y - yDifference);
        previewPiece.transform.position = newPosition; 
        previewPiece.transform.rotation = transform.rotation;

    }

    private float CalculateDistanceToLandingSpot() {
        float distanceToBottom = GetDistanceToBottomOfGrid();
        float distanceToNearestSquare = largeNumber;

        if(gameState != null) {
            //Debug.Log("Number of squares in state: " + gameState.GetSquares().Count.ToString());
        }
        if (gameState == null || gameState.GetSquares().Count == 0) {
            return distanceToBottom;
        } else {
            // Get shortest distance between a square and something below it
            distanceToNearestSquare = GetDistanceToNearestSquare();
        }

        // Subtract one from the distance if it's a square,
        // which will place the piece on top of an existing square
        return CompareFloats(distanceToNearestSquare, largeNumber) == -1 ? distanceToNearestSquare - 1 : distanceToBottom;
    }

    private float GetDistanceToBottomOfGrid() {
        // Get distance to bottom of map from lowest square in this tetromino
        Transform[] children = GetComponentsInChildren<Transform>();
        float lowestY = largeNumber;
        foreach (Transform child in children) {
            if (child == gameObject.transform) { continue; }

            if (CompareFloats(child.position.y, lowestY) == -1) {
                lowestY = child.position.y;
            }
        }

        return lowestY - minYPos;
    }

    private float GetDistanceToNearestSquare() {
        float distanceToNearestSquare = largeNumber;
        Transform[] children = GetComponentsInChildren<Transform>();
        List<Transform> squaresOnGrid = gameState.GetSquares();
        foreach (Transform child in children) {
            if (child == gameObject.transform) { continue; }

            foreach (Transform square in squaresOnGrid) {
                bool isSquareBelowPiece = CompareFloats(child.position.y, square.position.y) == 1;
                bool isSameX = CompareFloats(child.position.x, square.position.x) == 0;
                if(isSameX == false || isSquareBelowPiece == false) { continue; }

                float difference = child.position.y - square.position.y;
                bool isCloserThanPreviousLandingSpot = CompareFloats(difference, distanceToNearestSquare) == -1;
                if (isSameX) {
                    //Debug.Log("Same X found: " + child.position.y.ToString() + " - " + square.position.y.ToString() + " - " + isCloserThanPreviousLandingSpot.ToString());
                }
                if (isSameX && isCloserThanPreviousLandingSpot) {
                    //Debug.Log("Taking new difference: " + child.position.y.ToString() + " - " + square.position.y.ToString() + " = " + difference.ToString());
                    distanceToNearestSquare = difference;
                }
            }
        }

        return distanceToNearestSquare;
    }

    private void MovePiece() {
        timeSinceLastMove += Time.deltaTime;
        if (timeSinceLastMove >= timeBetweenMoves || (moveDownHeld && timeSinceLastMove >= timeBetweenMoveOnHold)) {
            MoveVertically();
        }

        if (moveLeftHeld || moveRightHeld) {
            timeSinceLastHorizontalMove += Time.deltaTime;
            if (timeSinceLastHorizontalMove >= timeBetweenMoveOnHold) {
                MoveHorizontally(moveLeftHeld ? -1f : 1f);
                //PrintTetromino();
                timeSinceLastHorizontalMove = 0f;
            }
        }
    }

    private void MoveVertically() {
        transform.position = new Vector2(transform.position.x, transform.position.y - 1f);
        timeSinceLastMove = 0f;
        if (!CheckIfNewLocationValid()) {
            transform.position = new Vector2(transform.position.x, transform.position.y + 1f);
        }
        //PrintTetromino();
    }

    void PrintTetromino() {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach(Transform child in children) {
            Debug.Log("Is This Object: " + (child == gameObject.transform ? "Yes" : "No") + "  <- to be diff " + child.position.ToString());
            Debug.Log("Local: " + child.localPosition.ToString());
            Debug.Log("Global: " + child.position.ToString());
            Debug.Log(" ");
        }
    }

    void MoveHorizontally(float xChange) {
        if(transform.position.x + xChange < minXPos || transform.position.x + xChange > maxXPos) { return; }
        transform.position = new Vector2(transform.position.x + xChange, transform.position.y);
        if (CheckIfNewLocationValid()) {
            UpdatePreviewPiece();
        } else {    
            //PrintTetromino();
            transform.position = new Vector2(transform.position.x + (xChange * -1), transform.position.y); ;
        }
    }

    void HandleDropPerformed(InputAction.CallbackContext ctx) {
        transform.position = new Vector2(transform.position.x, previewPiece.transform.position.y);
        HandlePieceLanded();
    }

    void HandlePieceLanded() {

        IsActivePiece = false;
        _tetrominoInput.Player.Disable();
        AddSquaresToGameState();
        tetrominoLandedEvent(this);
    }

    void AddSquaresToGameState() {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if (child == gameObject.transform) { continue; }
            if (gameState != null && child != null) {
                gameState.AddSquare(child);
            }
        }
    }

    void HandleMoveDownPerformed(InputAction.CallbackContext ctx) {
        if (ctx.interaction is HoldInteraction) {
            moveDownHeld = true;
        }
        else {
            MoveVertically();
        }
    }

    void HandleMoveDownCanceled(InputAction.CallbackContext ctx) {
        moveDownHeld = false;
    }

    void HandleMoveRightPerformed(InputAction.CallbackContext ctx) {
        if (ctx.interaction is HoldInteraction) {
            moveRightHeld = true;
            timeSinceLastHorizontalMove = timeBetweenMoveOnHold;
        }
        else {
            MoveHorizontally(1f);
        }
    }

    void HandleMoveRightCanceled(InputAction.CallbackContext ctx) {
        moveRightHeld = false;
    }

    void HandleMoveLeftPerformed(InputAction.CallbackContext ctx) {
        if(ctx.interaction is HoldInteraction) {
            moveLeftHeld = true;
            timeSinceLastHorizontalMove = timeBetweenMoveOnHold;
        } else {
            MoveHorizontally(-1f);
        }
        
    }

    void HandleMoveLeftCanceled(InputAction.CallbackContext ctx) {
        moveLeftHeld = false;
    }

    void HandleRotatePerformed(InputAction.CallbackContext ctx) {
        Debug.Log("tag: " + gameObject.tag);
        if(gameObject.tag == "O-Tetro") { return; }
        Debug.Log("trying to rotate");
        // TODO: May need to update this to also go back to original if at the end, it is conflicting with another piece
        if (CompareFloats(transform.position.x, minXPos) <= 0) {
            transform.position = new Vector2(transform.position.x + 1f, transform.position.y);
        }
        else if (CompareFloats(transform.position.x, maxXPos) >= 0) {
            transform.position = new Vector2(transform.position.x - 1f, transform.position.y);
        }
        
        if (CompareFloats(transform.position.y, minYPos) <= 0) {
            transform.position = new Vector2(transform.position.x, transform.position.y + 1f);
        } else {
            /*Debug.Log("Transform Y: " + transform.position.y.ToString());
            Debug.Log("Min Y: " + minYPos.ToString());
            Debug.Log("Comparison: " + CompareFloats(transform.position.y, minYPos).ToString());*/
            
        }

        transform.Rotate(new Vector3(0f, 0f, -90f));
        if (CheckIfNewLocationValid()) {
            UpdatePreviewPiece();
        } else { 
            //PrintTetromino();
            transform.Rotate(new Vector3(0f, 0f, 90f));
        }
    }

    bool CheckIfNewLocationValid() {
        bool isLocationValid = CheckIfTetrominoInBounds();
        if (isLocationValid) {
            isLocationValid = !CheckIfPieceOverlaps();
        }

        return isLocationValid;
    }

    bool CheckIfTetrominoInBounds() {
        bool isWithinBounds = true;

        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if (child == gameObject.transform) { continue; }
            
            // Log shows child pos x is 0.99999 and min 0.5
            // so either need to like...do a smarter compare or maybe
            // check if child pos x is < Mathf.floor of minX which would keep minX
            // as 4.5 for other places but would still work here.  Not sure.
            if(CompareFloats(child.position.x, maxXPos) == 1 || CompareFloats(child.position.x, minXPos) == -1 || CompareFloats(child.position.y, minYPos) == -1) {
                isWithinBounds = false;
                break;
            }
        }

        return isWithinBounds;
    }

    bool CheckIfPieceOverlaps() {
        Transform[] children = GetComponentsInChildren<Transform>();
        bool doesPieceConflict = false;
        foreach (Transform child in children) {
            if (child == gameObject.transform) { continue; }

            foreach (Transform square in gameState.GetSquares()) {
                bool isSameX = CompareFloats(child.position.x, square.position.x) == 0;
                bool isSameY = CompareFloats(child.position.y, square.position.y) == 0;

                if (isSameX && isSameY) {
                    doesPieceConflict = true;
                    break;
                }
            }

            if(doesPieceConflict) {
                break;
            }
        }

        return doesPieceConflict;
    }

    /** 
     * Comparing floats is not really safe, but with everything being 1 unit,
     * we can just compare the difference using a tolerance and thus 
     * 
     * i < j -> return -1
     * i == j -> return 0
     * i > j -> return 1
     * 
     */
    private int CompareFloats(float i, float j) {
        float tolerance = 0.15f;
        float difference = i - j;
        if (Mathf.Abs(difference) <= tolerance) {
            return 0;
        } else if(difference < 0) {
            return -1;
        } else {
            return 1;
        }
    }

}
