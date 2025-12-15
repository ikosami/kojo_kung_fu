using System.Collections.Generic;
using UnityEngine;

public class BossSceneManager : MonoBehaviour
{
    [SerializeField] List<GameObject> bossList;
    [SerializeField] AudioSource bgm;

    [SerializeField] AudioClip bossBgmClip;
    [SerializeField] AudioClip dojoBgmClip;
    [SerializeField] GameObject[] stageObjBacks;
    [SerializeField] GameObject[] stageObjFronts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var dojo = SaveDataManager.Dojo;
        if (dojo != 0)
        {
            Debug.LogError("Dojo " + dojo);
            Reference.Instance.SetDojo(dojo);
            bgm.clip = dojoBgmClip;
            bgm.Play();
        }
        else
        {
            var boss = bossList[SaveDataManager.NowStage];
            if (boss != null)
            {
                boss.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("このステージのボスは居ない " + SaveDataManager.NowStage);
            }
            Reference.Instance.SetStage(SaveDataManager.NowStage);
            bgm.clip = bossBgmClip;
            bgm.Play();

            var stageIndex = SaveDataManager.NowStage - 1;

            for (int i = 0; i < stageObjBacks.Length; i++)
            {
                if (stageObjBacks[i] != null)
                    stageObjBacks[i].gameObject.SetActive(i == stageIndex);
            }
            for (int i = 0; i < stageObjFronts.Length; i++)
            {
                if (stageObjFronts[i] != null)
                    stageObjFronts[i].gameObject.SetActive(i == stageIndex);
            }

        }
    }
}
