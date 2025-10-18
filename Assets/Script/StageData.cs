using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StageData : ScriptableObject
{
    public Sprite[] stageBackSprites;
    public Sprite[] stageFrontSprites;
    public int StageNum;
    public List<StageEntityData> stageDataList;
}

[Serializable]
public class StageEntityData
{
    public List<PopData> popEnemy;
    public bool IsRespawnPoint
    {
        get
        {
            foreach (var pop in popEnemy)
            {
                if (pop.IsRespawnPoint) return true;
            }
            return false;
        }
    }
}
[Serializable]
public class PopData
{
    public int EnemyIndex;
    public int SpawnTime;
    public int SpanwOffset;
    public bool IsRespawnPoint;
}

