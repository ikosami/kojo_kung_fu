using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public const int stageCnt = 4;

    const string keyNowStage = "now_stage";
    public static int NowStage
    {
        get
        {
            return PlayerPrefs.GetInt(keyNowStage, 1);
        }
        set
        {
            PlayerPrefs.SetInt(keyNowStage, value);
            NowArea = 0;
        }
    }

    const string keyNowArea = "now_area";
    public static int NowArea
    {
        get
        {
            return PlayerPrefs.GetInt(keyNowArea, 0);
        }
        set
        {
            PlayerPrefs.SetInt(keyNowArea, value);
        }
    }

    public static void NextStage()
    {
        if (NoDamage)
        {
            SetStageNoDamage(NowStage, true);
            Life++;
        }

        NowStage++;
        Debug.LogError("NowStage " + NowStage);
        MaxStage = Mathf.Max(MaxStage, NowStage);
    }
    const string keyMaxStage = "max_stage";
    public static int MaxStage
    {
        get
        {
            return PlayerPrefs.GetInt(keyMaxStage, 1);
        }
        set
        {
            PlayerPrefs.SetInt(keyMaxStage, value);
        }
    }
    const string keyDojo = "dojo";
    public static int Dojo
    {
        get
        {
            return PlayerPrefs.GetInt(keyDojo, 0);
        }
        set
        {
            PlayerPrefs.SetInt(keyDojo, value);
        }
    }

    const string keyHp = "hp";
    public static int Hp
    {
        get
        {
            return PlayerPrefs.GetInt(keyHp, 0);
        }
        set
        {
            PlayerPrefs.SetInt(keyHp, value);
        }
    }


    const string keyScore = "score";
    public static int Score
    {
        get
        {
            return PlayerPrefs.GetInt(keyScore, 0);
        }
        set
        {
            PlayerPrefs.SetInt(keyScore, value);
        }
    }


    const string keyLife = "life";
    public static int Life
    {
        get
        {
            var life = PlayerPrefs.GetInt(keyLife, 0);
            return life;
        }
        set
        {
            PlayerPrefs.SetInt(keyLife, value);
        }
    }
    public static int MaxLife = 2;

    const string keyNoDamage = "no_damage";
    public static bool NoDamage
    {
        get
        {
            return PlayerPrefs.GetInt(keyNoDamage, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(keyNoDamage, value ? 1 : 0);
        }
    }

    public static int HpMax = 3;

    const string keyStageNoDamage = "stage_no_damage_";
    public static bool GetStageNoDamage(int stage)
    {
        return PlayerPrefs.GetInt(keyStageNoDamage + stage.ToString(), 0) == 1;
    }
    public static void SetStageNoDamage(int stage, bool value)
    {
        PlayerPrefs.SetInt(keyStageNoDamage + stage.ToString(), value ? 1 : 0);
    }
}
