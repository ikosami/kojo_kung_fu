using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{

    [SerializeField] RectTransform stage;
    [SerializeField] List<StageData> stageDataList;
    [SerializeField] List<EnemyData> enemyDataList;

    int timing = -193;
    int index = 0; // 生成するタイミングのインデックス

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

        var stageData = stageDataList[index];
        if (stageData.IsStageEnd)
        {
            Reference.Instance.player.MoveEnd();
        }

        foreach (var popEnemy in stageData.popEnemy)
        {
            var enemyPopData = enemyDataList[popEnemy.EnemyIndex];
            StartCoroutine(WaitAction(() =>
            {
                Enemy newEnemy = Instantiate(enemyPopData.prefab, stage);
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
                    enemyRect.anchoredPosition = new Vector2(-currentX + enemyPopData.offset, -73);

                    Util.SetAnchorWithKeepingPosition(enemyRect, originalAnchorMin, originalAnchorMax);

                    // 座標を設定
                    enemyRect.anchoredPosition = new Vector2(enemyRect.anchoredPosition.x, -73);
                }
            }, popEnemy.SpawnTime));
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


[Serializable]
class StageData
{
    public List<PopData> popEnemy;

    public bool IsStageEnd = false;
}

[Serializable]
class PopData
{
    public int EnemyIndex;
    public int SpawnTime;
}


[Serializable]
class EnemyData
{
    public string name;
    public Enemy prefab;
    public int offset;
}