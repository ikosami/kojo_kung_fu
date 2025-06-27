using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyDataList : ScriptableObject
{
    public List<EnemyData> enemyDataList;
}

[Serializable]
public class EnemyData
{
    public string name;
    public Enemy prefab;
    public int offset;
}