using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyDataList : ScriptableObject
{
    public List<EnemyData> enemyDataList;

    [Button]
    public void Output()
    {
        var str = "";
        foreach (var enemyData in enemyDataList)
        {
            str += $"{enemyData.name}\n";
        }
        Debug.LogError(str);
    }
}

[Serializable]
public class EnemyData
{
    public string name;
    public Enemy prefab;
}