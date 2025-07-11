using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

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
        foreach (var stageData in List)
        {
            foreach (var entityData in stageData.stageDataList)
            {
                foreach (var pop in entityData.popEnemy)
                {
                    str += $"{stageData.StageNum}\t{enemyDataList.enemyDataList[pop.EnemyIndex].name}\t{pop.SpawnTime}\n";
                }
            }
        }
        Debug.LogError(str);
    }
    public void Input()
    {
        var datas = memo.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);
        List<StageData> stageList = new List<StageData>();

        foreach (var line in datas)
        {
            var parts = line.Split('\t');
            if (parts.Length < 3) continue;

            if (!int.TryParse(parts[0], out int stageNum)) continue;
            string enemyName = parts[1];
            if (!float.TryParse(parts[2], out float spawnTime)) continue;

            int enemyIndex = enemyDataList.enemyDataList.FindIndex(e => e.name == enemyName);
            if (enemyIndex == -1) continue;

            var stageData = stageList.Find(s => s.StageNum == stageNum);
            if (stageData == null)
            {
                stageData = new StageData { StageNum = stageNum, stageDataList = new List<StageEntityData>() };
                stageList.Add(stageData);
            }

            StageEntityData entityData;
            if (stageData.stageDataList.Count > 0)
            {
                entityData = stageData.stageDataList[0]; // 必要に応じて適切な位置に追加
            }
            else
            {
                entityData = new PopData { popEnemy = new List<PopData>() };
                stageData.stageDataList.Add(entityData);
            }

            entityData.popEnemy.Add(new PopData
            {
                EnemyIndex = enemyIndex,
                SpawnTime = spawnTime
            });
        }

        List = stageList;
    }

}