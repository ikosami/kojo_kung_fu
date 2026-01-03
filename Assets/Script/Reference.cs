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
        SetScore(score);
    }
    public RectTransform stageRect => uiController.StageRect;

    /// <summary>
    /// プレイヤーの動作を停止させるフラグ
    /// trueの場合、プレイヤーの移動やアクションが停止される
    /// イベントシーンなどで使用される
    /// </summary>
    public bool PlayerStop = false;

    public bool isPause = false;


    public bool IsGameOver = false;
    public bool isGameOverEnd = false;
    public bool isBoss = false;
    public bool isDojo = false;
    /// <summary>
    /// ステージのスクロールを停止させるフラグ
    /// trueの場合、プレイヤーが特定の位置に到達した際にステージのスクロールが停止される
    /// イベント発生時などに使用される
    /// </summary>
    public bool isScroolStop = false;
    public Player player;
    public const int SizeX = 160;
    public const int SizeY = 120;
    public List<CharacterBase> enemyList = new List<CharacterBase>();
    public GameObject completePanel;
    public AudioSource bgm;

    public EnemyDataList enemyDataList;
    public StageDataList stageDataList;

    public Transform screenLeftUp;
    public Transform screenRightDown;

    public int score = 0;
    public int hp = 5;
    public int StageNum = 1; // 現在のステージ番号
    public bool IsClear = false;
    public UIController uiController;

    public Action<CharacterBase> OnEnemyPop;

    private void Start()
    {
        if (SaveDataManager.Hp <= 0)
            SaveDataManager.Hp = 3;
        if (SaveDataManager.Life < 0)
        {
            SaveDataManager.Life = 0;
        }
        UpdateStateView();
    }

    public void AddEnemy(CharacterBase enemy)
    {
        enemyList.Add(enemy);
        OnEnemyPop?.Invoke(enemy);
    }

    public void SetStage(int stageNum)
    {
        StageNum = stageNum;
        uiController.SetStage(stageNum);
    }
    internal void SetDojo(int dojo)
    {
        isDojo = true;
        uiController.SetDojo(dojo);
        StageNum = SaveDataManager.NowStage;
        SaveDataManager.Hp = 3;
        SaveDataManager.NoDamage = true;
    }



    public void SetScore(int value)
    {
        score = value;
        uiController.scoreText.text = $"1Player--{score:000000}";

        SaveDataManager.Score = score;
    }
    public void AddScore(int value)
    {
        score += value;
        uiController.scoreText.text = $"1Player--{score:000000}";

        SaveDataManager.Score = score;
    }
    public int GetScore()
    {
        return SaveDataManager.Score;
    }

    internal void UpdateStateView()
    {
        uiController.SetHp(SaveDataManager.Hp);
        uiController.SetLife();
    }

    internal void StageComplete(int bossNum)
    {
        SaveDataManager.Dojo = bossNum;

        if (SaveDataManager.NoDamage)
        {
            SoundManager.Instance.Play("stage_clear_nodamage");
        }
        else
        {
            SoundManager.Instance.Play("stage_clear");
        }

        SaveDataManager.NextStage();
    }
}
