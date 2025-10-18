using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{

    [SerializeField] RectTransform stage;
    [SerializeField] AudioSource bgm;

    [SerializeField] AudioClip[] bgmClips;
    List<StageEntityData> stageDataList;

    [SerializeField] GameObject[] stageObjBacks;
    [SerializeField] GameObject[] stageObjFronts;

    int timing = -144;
    int index = 0; // 生成するタイミングのインデックス

    private void Start()
    {
        if (SaveDataManager.Dojo != 0 && Reference.Instance.isBoss)
            return;

        SetStage(SaveDataManager.NowStage);
    }

    public void SetStage(int num)
    {
        var stageIndex = num - 1;
        bgm.clip = bgmClips[stageIndex];
        bgm.Play();

        var stageData = Reference.Instance.stageDataList.List.Find(x => x.StageNum == num);
        stageDataList = stageData.stageDataList;

        if (!Reference.Instance.isBoss)
        {
            index = SaveDataManager.NowArea;
            float nextSpawnX = timing * (index); // timing の倍数
            Reference.Instance.stageRect.anchoredPosition = new Vector2(nextSpawnX, Reference.Instance.stageRect.anchoredPosition.y);
        }
        Reference.Instance.SetStage(num);


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

    void Update()
    {

        float currentX = stage.anchoredPosition.x;
        float nextSpawnX = timing * (index + 1); // timing の倍数

        // `timing * index` を下回ったら敵をスポーン
        if (currentX <= nextSpawnX)
        {
            SpawnEnemiesAt(nextSpawnX, index);
            index++; // 次の倍数に進める
        }
    }

    void SpawnEnemiesAt(float spawnX, int index)
    {
        if (stageDataList == null || stageDataList.Count == 0) return;

        if (index >= stageDataList.Count)
        {
            Reference.Instance.player.MoveEnd();
            return;
        }

        var stageData = stageDataList[index];
        if (stageData.IsRespawnPoint)
        {
            SaveDataManager.NowArea = index;
            Debug.LogError($"Respawn Area:{SaveDataManager.NowArea}");
        }


        var enemyDataList = Reference.Instance.enemyDataList.enemyDataList;
        foreach (var popEnemy in stageData.popEnemy)
        {
            var enemyPopData = enemyDataList[popEnemy.EnemyIndex];


            StartCoroutine(WaitAction(() =>
            {
                SpawnEnemy(enemyPopData.prefab, popEnemy.SpanwOffset);
            }, popEnemy.SpawnTime));
        }
    }

    public void SpawnEnemy(Enemy prefab, int spanwOffset)
    {
        Enemy newEnemy = Instantiate(prefab, Reference.Instance.uiController.EnemyParent);
        RectTransform enemyRect = newEnemy.Rect;
        if (enemyRect != null)
        {
            // 元のアンカーを保存
            Vector2 originalAnchorMin = enemyRect.anchorMin;
            Vector2 originalAnchorMax = enemyRect.anchorMax;

            float currentX = stage.anchoredPosition.x;

            // ステージのアンカーに合わせる
            enemyRect.anchorMin = stage.anchorMin;
            enemyRect.anchorMax = stage.anchorMax;
            // 座標を設定
            enemyRect.anchoredPosition = new Vector2(-currentX + spanwOffset, -73);

            Util.SetAnchorWithKeepingPosition(enemyRect, originalAnchorMin, originalAnchorMax);

            // 座標を設定
            enemyRect.anchoredPosition = new Vector2(enemyRect.anchoredPosition.x, -73);
        }
    }

    IEnumerator WaitAction(Action onCallBack, float time)
    {
        if (time <= 0)
        {
            onCallBack.Invoke();
            yield break;
        }
        yield return new WaitForSeconds(time);
        onCallBack.Invoke();
    }
}


