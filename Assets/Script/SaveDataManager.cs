using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public const int stageCnt = 2;

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
        }
    }

    public static void NextStage()
    {
        if (NowStage + 1 <= stageCnt)
        {
            NowStage++;
        }
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
}
