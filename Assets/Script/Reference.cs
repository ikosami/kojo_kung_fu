using System;
using System.Collections.Generic;
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

        score = PlayerPrefs.GetInt("score", 0);
        hp = PlayerPrefs.GetInt("hp", 0);
        AddScore(score);
    }
    public RectTransform stageRect => uiController.StageRect;
    public bool isPause = false;


    public bool IsGameOver = false;
    public bool isGameOverEnd = false;
    public bool isBoss = false;
    public Player player;
    public const int SizeX = 160;
    public const int SizeY = 120;
    public List<ICharacter> enemyList = new List<ICharacter>();
    public GameObject completePanel;
    public AudioSource bgm;

    public EnemyDataList enemyDataList;
    public StageDataList stageDataList;

    public int score = 0;
    public int hp = 5;
    public int StageNum = 1; // 現在のステージ番号
    public bool IsClear = false;
    public UIController uiController;

    public void SetStage(int stageNum)
    {
        StageNum = stageNum;
        uiController.SetStage(stageNum);
    }
    internal void SetDojo(int dojo)
    {
        uiController.SetDojo(dojo);
    }



    public void AddScore(int value)
    {
        score += value;
        uiController.scoreText.text = $"1Player--{score:000000}";

        PlayerPrefs.SetInt("score", score);
    }
    public int GetScore()
    {
        return PlayerPrefs.GetInt("score", 0);
    }
}
