using UnityEditor;
using UnityEngine;

public class SaveDataEditor : MonoBehaviour
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    private static void ClearPlayerPrefs()
    {
        //if (EditorUtility.DisplayDialog("確認", "PlayerPrefsを削除しますか？", "はい", "いいえ"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs を削除しました。");
        }
    }
}
