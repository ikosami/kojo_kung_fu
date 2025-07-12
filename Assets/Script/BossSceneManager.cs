using System.Collections.Generic;
using UnityEngine;

public class BossSceneManager : MonoBehaviour
{
    [SerializeField] List<GameObject> bossList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var dojo = SaveDataManager.Dojo;
        if (dojo != 0)
        {
            bossList[SaveDataManager.NowStage].gameObject.SetActive(true);
        }
        else
        {
            Reference.Instance.SetDojo(dojo);
        }
    }
}
