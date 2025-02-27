using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Reference : MonoBehaviour
{
    private static Reference instance;
    public static Reference Instance
    {
        get
        {
            return instance;
        }
    }
    void Awake()
    {
        instance = this;
    }
    public RectTransform stage;
    public bool isPause = false;
    public bool isBoss = false;
    public Player player;
    public const int SizeX = 160;
    public const int SizeY = 120;
    public List<ICharacter> enemyList = new List<ICharacter>();
    public TextMeshProUGUI scoreText;
    int score = 0;
    public bool IsGameOver = false;

    public void AddScore(int value)
    {
        score += value;
        scoreText.text = $"1Player--:{score:000000}";
    }
}
