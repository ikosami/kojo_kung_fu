using UnityEngine;
using UnityEngine.UI;

public class Boss4_Shoot : MonoBehaviour
{
    [SerializeField] bool isEnemy = true;

    [SerializeField] RectTransform bodyRect; // 本体当たり判定範囲
    [SerializeField] Image image; // 本体の画像コンポーネント

    [SerializeField] RectTransform[] attackRanges;

    [SerializeField] Sprite[] sprites;
    int nowSpriteIndex = 0; // 現在のスプライトインデックス
    float spriteChangeTimer = 0f; // スプライト切り替え用タイマー

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

        // スプライトアニメーション
        if (sprites != null && sprites.Length > 0)
        {
            spriteChangeTimer += Time.deltaTime;
            if (spriteChangeTimer >= 0.2f)
            {
                nowSpriteIndex = (nowSpriteIndex + 1) % sprites.Length;
                image.sprite = sprites[nowSpriteIndex];
                spriteChangeTimer = 0f;
            }
        }

        bodyRect.anchoredPosition += move * Time.deltaTime;

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

