using UnityEngine;
using UnityEngine.UI;

public class IceBoss3_1 : MonoBehaviour
{
    [SerializeField] bool isEnemy = true;

    [SerializeField] RectTransform bodyRect; // 本体当たり判定範囲
    [SerializeField] Image image; // 本体の画像コンポーネント

    [SerializeField] RectTransform attackRange;

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
                bodyRect.anchoredPosition += move * Time.deltaTime;
                timer += Time.deltaTime;

                if (bodyRect.anchoredPosition.y <= floor)
                {
                    var pos = bodyRect.anchoredPosition;
                    pos.y = floor;
                    bodyRect.anchoredPosition = pos;

                    timer = 0;
                    state = 1;
                }
                break;
            case 1:

                bodyRect.anchoredPosition += fallMove * Time.deltaTime;
                if (bodyRect.anchoredPosition.x <= -300)
                {
                    Destroy(gameObject);
                }
                break;
        }

        if (isDamage)
        {
            if (isEnemy)
            {
                if (Util.IsHitPlayer(attackRange))
                {
                    Reference.Instance.player.TakeDamage(1);
                    isDamage = false;
                }
            }
            else
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
