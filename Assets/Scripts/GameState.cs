using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameState : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] int lineValue = 100;
    [SerializeField] int pointsBetweenLevels = 5000;

    int score;
    int previousScore;
    int level;


    private List<Transform> squares;

    private void Awake() {
        squares = new List<Transform>();
    }

    private void Start() {
        score = 0;
        scoreText.SetText(score.ToString());
        previousScore = 0;
        level = 1;
        levelText.SetText(level.ToString());
    }

    public void AddSquare(Transform square) {
        squares.Add(square);
    }

    public void RemoveSquare(Transform square) {
        squares.Remove(square);
    }

    public List<Transform> GetSquares() {
        return squares;
    }

    public void HandleLinesCleared(int linesCleared) {
        UpdateScore(linesCleared);
        UpdateLevel();
    }

    private void UpdateScore(int linesCleared) {
        score += Mathf.RoundToInt(linesCleared * lineValue * (1 + ((linesCleared-1) * 0.5f)));
        scoreText.SetText(score.ToString());
    }

    private void UpdateLevel() {
        if(score-previousScore >= pointsBetweenLevels) {
            previousScore = level * pointsBetweenLevels;
            level++;
            levelText.SetText(level.ToString());
        }
    }
}
