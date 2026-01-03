using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bullet : MonoBehaviour
{
    [SerializeField] bool isEnemy = true;

    [SerializeField] RectTransform bodyRect; // 本体当たり判定範囲
    [SerializeField] Image image; // 本体の画像コンポーネント

    [SerializeField] RectTransform attackRange;

    [SerializeField] Sprite[] sprites;
    [SerializeField] bool isPowerAttack = false;
    int nowSpriteIndex = 0; // 現在のスプライトインデックス
    float spriteChangeTimer = 0f; // スプライト切り替え用タイマー
    [SerializeField] int damage = 1;

    public Vector2 move;
    bool isDamage = true;

    [Header("弾設定")]
    [SerializeField] bool isPenetrating = false; // 貫通弾かどうか
    [SerializeField] float maxTravelDistance = 500f; // 最大移動距離（通常弾・貫通弾共通）

    float travelDistance = 0f; // 現在の移動距離
    HashSet<Enemy> hitEnemies = new HashSet<Enemy>(); // 当たった敵のリスト
    Vector2 startPosition; // 開始位置

    void Start()
    {
        // 開始位置を記録
        startPosition = bodyRect.anchoredPosition;
    }

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

        Vector2 moveDelta = move * Time.deltaTime;
        bodyRect.anchoredPosition += moveDelta;

        // 移動距離を更新
        travelDistance += moveDelta.magnitude;

        // 最大移動距離を超えたら消す（通常弾・貫通弾共通）
        if (travelDistance >= maxTravelDistance)
        {
            Destroy(gameObject);
            return;
        }

        if (isDamage)
        {
            if (isEnemy)
            {
                if (Util.IsHitPlayer(attackRange))
                {
                    Reference.Instance.player.TakeDamage(1);
                    isDamage = false;
                    Destroy(gameObject);
                }
            }
            else
            {
                var enemyList = Util.GetEnemyList(attackRange);
                foreach (var enemyChar in enemyList)
                {
                    // ICharacterをEnemyにキャスト
                    Enemy enemy = enemyChar as Enemy;
                    if (enemy == null) continue;

                    // 貫通弾の場合、既に当たった敵はスキップ
                    if (isPenetrating && hitEnemies.Contains(enemy))
                    {
                        continue;
                    }

                    enemy.TakeDamage(damage, isPowerAttack);

                    if (isPenetrating)
                    {
                        // 貫通弾の場合、当たった敵を記録して続行
                        hitEnemies.Add(enemy);
                    }
                    else
                    {
                        // 通常弾の場合、最初の敵に当たったら終了
                        isDamage = false;
                        Destroy(gameObject);
                        break;
                    }
                }
            }
        }
    }
}
