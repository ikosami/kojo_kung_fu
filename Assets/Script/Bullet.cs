using UnityEngine;
using UnityEngine.UI;

public class Bullet : MonoBehaviour
{
    [SerializeField] RectTransform bodyRect; // 本体当たり判定範囲
    [SerializeField] Image image; // 本体の画像コンポーネント

    [SerializeField] RectTransform attackRange;

    [SerializeField] Sprite[] sprites;
    int nowSpriteIndex = 0; // 現在のスプライトインデックス
    float spriteChangeTimer = 0f; // スプライト切り替え用タイマー

    public Vector2 move;

    // Update is called once per frame
    void Update()
    {
        // ゲームクリア・ポーズ・ゲームオーバー時は処理しない
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer >= 0.2f)
        {
            // スプライトを切り替える
            nowSpriteIndex = (nowSpriteIndex + 1) % sprites.Length;
            image.sprite = sprites[nowSpriteIndex];
            spriteChangeTimer = 0f;
        }

        bodyRect.anchoredPosition += move * Time.deltaTime;

        if (Util.IsHitPlayer(attackRange))
        {
            Reference.Instance.player.TakeDamage(1);
        }
    }
}
