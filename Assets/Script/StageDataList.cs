using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu]
public class StageDataList : ScriptableObject
{
    public List<StageData> List;


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
                    str += $"{stageData.StageNum}\t{pop.EnemyIndex}\t{pop.SpawnTime}\n";
                }
            }
        }
        Debug.LogError(str);
    }
}