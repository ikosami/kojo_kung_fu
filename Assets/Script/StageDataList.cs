using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu]
public class StageDataList : ScriptableObject
{
    public List<StageData> List;
    [SerializeField] EnemyDataList enemyDataList;


    [Multiline]
    public string memo;

    [Button]
    public void OutputLog()
    {
        var str = "";
        for (int stagei = 0; stagei < List.Count; stagei++)
        {
            StageData stageData = List[stagei];
            for (int i = 0; i < stageData.stageDataList.Count; i++)
            {
                StageEntityData entityData = stageData.stageDataList[i];
                foreach (var pop in entityData.popEnemy)
                {
                    str += $"{stageData.StageNum}\t{i}\t{enemyDataList.enemyDataList[pop.EnemyIndex].name}\t{pop.SpawnTime}\n";
                }
            }
        }
        Debug.LogError(str);
    }
    [Button]
    public void Input()
    {
        foreach (var stageData in List)
        {
            stageData.stageDataList.Clear();
        }

        var lines = memo.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, StageData> stageDict = new Dictionary<int, StageData>();
        foreach (var stage in List)
        {
            stageDict[stage.StageNum] = stage;
        }

        foreach (var line in lines)
        {
            var parts = line.Split('\t');
            if (parts.Length < 4) continue;

            if (!int.TryParse(parts[0], out int stageNum)) continue;
            if (!int.TryParse(parts[1], out int entityIndex)) continue;
            var enemyName = parts[2];
            if (!int.TryParse(parts[3], out int spawnTime)) continue;
            if (!int.TryParse(parts[4], out int spawnOffset)) continue;

            if (!stageDict.TryGetValue(stageNum, out var stageData)) continue;

            int enemyIndex = enemyDataList.enemyDataList.FindIndex(e => e.name == enemyName);
            if (enemyIndex == -1) continue;


            // 必要に応じて stageDataList を拡張（インデックスに対応）
            while (stageData.stageDataList.Count <= entityIndex)
            {
                stageData.stageDataList.Add(new StageEntityData { popEnemy = new List<PopData>() });
            }

            stageData.stageDataList[entityIndex].popEnemy.Add(new PopData
            {
                EnemyIndex = enemyIndex,
                SpawnTime = spawnTime,
                SpanwOffset = spawnOffset
            });
#if UNITY_EDITOR
            EditorUtility.SetDirty(stageData);
#endif
        }

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
        Debug.LogError("OK");
    }


}