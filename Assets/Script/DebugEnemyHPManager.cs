using System;
using UnityEngine;

public class DebugEnemyHPManager : MonoBehaviour
{
    [SerializeField] DebugEnemyHP prefab;
    [SerializeField] Transform parent;

    void Start()
    {

        foreach (Transform enemy in parent)
        {
            Destroy(enemy.gameObject);
        }
        Reference.Instance.OnEnemyPop += OnEnemyPop;
    }

    private void OnEnemyPop(CharacterBase character)
    {
        var instance = Instantiate(prefab, parent);
        instance.Set(character);

    }
}
