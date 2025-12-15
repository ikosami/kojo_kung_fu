using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public GameObject debugPanel;
    void Update()
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            //ステージクリアを発生させる
            if (Input.GetKeyDown(KeyCode.B))
            {
                //ボス戦ならボスを倒す
                if (Reference.Instance.isBoss)
                {
                    foreach (var enemy in Reference.Instance.enemyList)
                    {
                        enemy.TakeDamage(9999, true);
                    }
                }
                else
                {
                    foreach (var enemy in Reference.Instance.enemyList)
                    {
                        enemy.TakeDamage(9999, true);
                    }

                    var stage4EnemyWave = FindAnyObjectByType<Stage4_EnemyWave>();
                    if (stage4EnemyWave != null)
                    {
                        stage4EnemyWave.DebugEnd();
                    }
                    else
                    {
                        //ボス戦でなければステージクリアを発生させる
                        Reference.Instance.player.MoveEnd();
                    }
                }
            }
            //HPを増やす
            if (Input.GetKeyDown(KeyCode.H))
            {
                SaveDataManager.Hp++;
                Debug.LogError("Hp " + SaveDataManager.Hp);
                Reference.Instance.UpdateStateView();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                debugPanel.gameObject.SetActive(!debugPanel.gameObject.activeSelf);
            }
        }

    }
}