using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StageData : ScriptableObject
{
    public Sprite stageBackSprite;
    public Sprite stageFrontSprite;
    public int StageNum;
    public List<StageEntityData> stageDataList;
}

[Serializable]
public class StageEntityData
{
    public List<PopData> popEnemy;
}
[Serializable]
public class PopData
{
    public int EnemyIndex;
    public int SpawnTime;
}

