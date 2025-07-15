using System.Collections.Generic;
using UnityEngine;

public class BossSceneManager : MonoBehaviour
{
    [SerializeField] List<GameObject> bossList;
    [SerializeField] AudioSource bgm;

    [SerializeField] AudioClip bossBgmClip;
    [SerializeField] AudioClip dojoBgmClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var dojo = SaveDataManager.Dojo;
        if (dojo != 0)
        {
            Debug.LogError("DOjo " + dojo);
            Reference.Instance.SetDojo(dojo);
            bgm.clip = dojoBgmClip;
            bgm.Play();
        }
        else
        {
            bossList[SaveDataManager.NowStage].gameObject.SetActive(true);
            Reference.Instance.SetStage(SaveDataManager.NowStage);
            bgm.clip = bossBgmClip;
            bgm.Play();
        }
    }
}
