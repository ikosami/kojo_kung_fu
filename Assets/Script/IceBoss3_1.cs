using UnityEngine;
using UnityEngine.UI;

public class IceBoss3_1 : MonoBehaviour
{
    [SerializeField] bool isEnemy = true;

    [SerializeField] RectTransform bodyRect; // 本体当たり判定範囲
    [SerializeField] Image image; // 本体の画像コンポーネント

    [SerializeField] RectTransform[] attackRanges;

    [SerializeField] Sprite[] sprites;
    int nowSpriteIndex = 0; // 現在のスプライトインデックス
    float spriteChangeTimer = 0f; // スプライト切り替え用タイマー

    int state = 0;
    [SerializeField] float floor = 2;
    float timer = 0;

    public Vector2 move;
    public Vector2 fallMove;
    bool isDamage = true;
    public bool isPlayerFollow;

    // Update is called once per frame
    void Update()
    {
        // ゲームクリア・ポーズ・ゲームオーバー時は処理しない
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        switch (state)
        {
            case 0:

                timer += Time.deltaTime;
                if (timer >= 1)
                {
                    timer = 0;
                    state = 1;
                }
                break;
            case 1:

                bodyRect.anchoredPosition += move * Time.deltaTime;
                break;
        }

        if (isDamage)
        {
            if (isEnemy)
            {
                foreach (var attackRange in attackRanges)
                {
                    if (Util.IsHitPlayer(attackRange))
                    {
                        Reference.Instance.player.TakeDamage(1);
                        isDamage = false;
                        break;
                    }
                }
            }
            else
            {
                foreach (var attackRange in attackRanges)
                {
                    var enemyList = Util.GetEnemyList(attackRange);
                    foreach (var enemy in enemyList)
                    {
                        enemy.TakeDamage(1, false);
                    }
                }
            }
        }
    }
}
