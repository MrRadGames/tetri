using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{

    [SerializeField] GameState gameState;

    List<List<Transform>> rows = new List<List<Transform>>();
    // Magic number but it's number of rows and not going to change
    int numberOfRows = 20;
    int lowestDeletedRow = 100;

    List<int> deletedRows = new List<int>();

    private void Start() {
        for(int idx = 0; idx < numberOfRows; idx++) {
            rows.Add(new List<Transform>());
        }
    }

    public void CheckForRowsToClear() {
        List<Transform> squares = gameState.GetSquares();
        if (squares.Count < 10) { return; }

        BuildRows(squares);
        bool clearedLines = ClearCompletedLines();
        if(clearedLines) {
            gameState.HandleLinesCleared(deletedRows.Count);
            MovePiecesDown();
        }
        ResetState();
    }

    /**
     *  Group every square by the row of the grid they belong to.
     */
    private void BuildRows(List<Transform> squares) {
        foreach (Transform square in squares) {
            int yIndex = (int)Mathf.Floor(Mathf.Abs(square.position.y));
            rows[yIndex].Add(square);
        }
    }

    /**
     * For each row in the grid, if the row is full we will delete the square from the board and gamestate
     */
    private bool ClearCompletedLines() {
        bool didClearLines = false;
        for(int index = 0; index < numberOfRows; index++) {
            //Debug.Log("Row: " + index.ToString() + " - Count: " + rows[index].Count.ToString());
            if(rows[index] == null || rows[index].Count < 10) { continue; }

            didClearLines = true;

            deletedRows.Add(index);

            //Debug.Log("Clearing out row: " + index.ToString());

            if(index < lowestDeletedRow) {
                lowestDeletedRow = index;
            }

            foreach(Transform square in rows[index]) {
                gameState.RemoveSquare(square);
                Destroy(square.gameObject);
            }
        }
        return didClearLines;
    }

    /**
     * For each row above the lowest deleted row, move the squares down by the amount of rows deleted beneath it
     */
    private void MovePiecesDown() {
        //Debug.Log("lowest deleted row: " + lowestDeletedRow);
        int startingIndex = lowestDeletedRow + 1;
        //Debug.Log("PIECES AFTER MOVING");
        for (int index = startingIndex; index < numberOfRows; index++) {
            int numberOfRowsToMoveDown = 0;
            foreach(int deletedIndex in deletedRows) {
                if(deletedIndex < index) {
                    numberOfRowsToMoveDown++;
                }
            }

            //Debug.Log("Row Index: " + index.ToString() + " - Num to move down: " + numberOfRowsToMoveDown.ToString());

            if(numberOfRows == 0) { continue; }
            foreach(Transform square in rows[index]) {
                square.position = new Vector2(square.position.x, square.position.y - numberOfRowsToMoveDown);
                //Debug.Log("NEW POSITION: " + square.position.x.ToString() + " , " + square.position.y.ToString());
            }
        }

        //Debug.Log("STATE AFTER MOVING");
        /*foreach (Transform square in gameState.GetSquares()) {
            Debug.Log("STATE POSITION: " + square.position.x.ToString() + " , " + square.position.y.ToString());
        }*/
    }

    /**
     * Reset the grid for the next time a piece lands
     */
    private void ResetState() {
        for (int index = 0; index < numberOfRows; index++) {
            rows[index] = new List<Transform>();
        }

        deletedRows.Clear();

        lowestDeletedRow = 100;
    }
}
