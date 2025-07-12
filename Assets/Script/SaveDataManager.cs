using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
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
}
