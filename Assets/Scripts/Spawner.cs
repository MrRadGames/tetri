using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] Tetromino[] tetrominoOptions;
    [SerializeField] LineManager lineManager;
    [SerializeField] PreviewTetromino previewTetromino;
    [SerializeField] GameObject previewSquare;

    Tetromino newTetromino;

    private void Start() {
        GenerateNewTetromino();
    }

    public void onPieceLanded(Tetromino piece) {
        lineManager.CheckForRowsToClear();
        GenerateNewTetromino();
    }

    private void GenerateNewTetromino() {
        ClearPreviewPiece();
        SelectTetrominoToSpawn();
        BuildPreviewPiece();
        SpawnTetromino();
    }

    private void ClearPreviewPiece() {
        Transform[] children = previewTetromino.GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if (child == previewTetromino.transform) { continue; }

            Destroy(child.gameObject);
        }
    }

    private void SelectTetrominoToSpawn() {
        int index = Random.Range(0, tetrominoOptions.Length);
        newTetromino = tetrominoOptions[index];
    }

    private void BuildPreviewPiece() {
        previewTetromino.transform.rotation = newTetromino.transform.rotation;

        Transform[] children = newTetromino.GetComponentsInChildren<Transform>();
        foreach (Transform square in children) {
            if (square == newTetromino.transform) { continue; }
            Debug.Log("X, Y: " + square.localPosition.x + ", " + square.localPosition.y);
            Vector2 localLocation = new Vector2(square.localPosition.x, square.localPosition.y);
            GameObject newChild = Instantiate(previewSquare, previewTetromino.transform);
            newChild.transform.SetParent(previewTetromino.transform);
            newChild.transform.localPosition = localLocation;
        }

        Transform[] newChildren = previewTetromino.GetComponentsInChildren<Transform>();
        foreach (Transform square in newChildren) {
            if (square == previewTetromino.transform) { continue; }
            Debug.Log("X, Y: " + square.localPosition.x + ", " + square.localPosition.y);
        }
    }

    private void SpawnTetromino() {
        Tetromino newPiece = Instantiate(newTetromino, transform.position, transform.rotation);
        newPiece.tetrominoLandedEvent += onPieceLanded;
    }

    


}
