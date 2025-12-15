using UnityEngine;
using UnityEngine.UI;

public class Boss4_ShockWave : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("敵かどうか（true: プレイヤーにダメージ、false: 敵にダメージ）")]
    [SerializeField] bool isEnemy = true;

    [Header("左側設定")]
    [Tooltip("左側の本体の画像コンポーネント")]
    [SerializeField] Image leftImage;
    [Tooltip("左側の攻撃の当たり判定範囲（配列）")]
    [SerializeField] RectTransform[] leftAttackRanges;

    [Header("右側設定")]
    [Tooltip("右側の本体の画像コンポーネント")]
    [SerializeField] Image rightImage;
    [Tooltip("右側の攻撃の当たり判定範囲（配列）")]
    [SerializeField] RectTransform[] rightAttackRanges;

    [Header("スプライト設定")]
    [SerializeField] Sprite[] sprites;

    [Header("移動設定")]
    [Tooltip("左右への移動速度（UI座標/秒）")]
    [SerializeField] float moveSpeed = 500f;

    [Header("タイマー設定")]
    [Tooltip("生存時間（秒）、この時間経過後に消える")]
    [SerializeField] float lifetime = 10f;

    int nowSpriteIndex = 0; // 現在のスプライトインデックス
    float spriteChangeTimer = 0f; // スプライト切り替え用タイマー
    float timer = 0f; // 生存時間タイマー
    bool[] leftHasHitDamage; // 左側の各当たり判定範囲でダメージを与えたかどうか
    bool[] rightHasHitDamage; // 右側の各当たり判定範囲でダメージを与えたかどうか

    void Start()
    {
        // 左側の各当たり判定範囲ごとにダメージフラグを初期化
        leftHasHitDamage = new bool[leftAttackRanges.Length];
        for (int i = 0; i < leftHasHitDamage.Length; i++)
        {
            leftHasHitDamage[i] = false;
        }

        // 右側の各当たり判定範囲ごとにダメージフラグを初期化
        rightHasHitDamage = new bool[rightAttackRanges.Length];
        for (int i = 0; i < rightHasHitDamage.Length; i++)
        {
            rightHasHitDamage[i] = false;
        }
    }

    void Update()
    {
        // ゲームクリア・ポーズ・ゲームオーバー時は処理しない
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) return;

        // 左側を左方向に移動
        var leftPos = leftImage.rectTransform.anchoredPosition;
        float leftNewX = leftPos.x + moveSpeed * (-1f) * Time.deltaTime; // 左方向（-1）
        leftImage.rectTransform.anchoredPosition = new Vector2(leftNewX, leftPos.y);

        // 右側を右方向に移動
        var rightPos = rightImage.rectTransform.anchoredPosition;
        float rightNewX = rightPos.x + moveSpeed * 1f * Time.deltaTime; // 右方向（1）
        rightImage.rectTransform.anchoredPosition = new Vector2(rightNewX, rightPos.y);

        // スプライトアニメーション（左右とも同じスプライトを使用）
        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer >= 0.2f)
        {
            nowSpriteIndex = (nowSpriteIndex + 1) % sprites.Length;
            leftImage.sprite = sprites[nowSpriteIndex];
            rightImage.sprite = sprites[nowSpriteIndex];
            spriteChangeTimer = 0f;
        }

        // 10秒経過したら消える
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 左側のダメージ判定（一回ずつ）
        if (isEnemy && leftAttackRanges != null && leftHasHitDamage != null)
        {
            for (int i = 0; i < leftAttackRanges.Length; i++)
            {
                if (i < leftHasHitDamage.Length && !leftHasHitDamage[i] && Util.IsHitPlayer(leftAttackRanges[i]))
                {
                    Reference.Instance.player.TakeDamage(1);
                    leftHasHitDamage[i] = true; // この当たり判定範囲ではもうダメージを与えない
                }
            }
        }
        else if (!isEnemy && leftAttackRanges != null && leftHasHitDamage != null)
        {
            for (int i = 0; i < leftAttackRanges.Length; i++)
            {
                if (i < leftHasHitDamage.Length && !leftHasHitDamage[i])
                {
                    var enemyList = Util.GetEnemyList(leftAttackRanges[i]);
                    if (enemyList.Count > 0)
                    {
                        foreach (var enemy in enemyList)
                        {
                            enemy.TakeDamage(1, false);
                        }
                        leftHasHitDamage[i] = true; // この当たり判定範囲ではもうダメージを与えない
                    }
                }
            }
        }

        // 右側のダメージ判定（一回ずつ）
        if (isEnemy && rightAttackRanges != null && rightHasHitDamage != null)
        {
            for (int i = 0; i < rightAttackRanges.Length; i++)
            {
                if (i < rightHasHitDamage.Length && !rightHasHitDamage[i] && Util.IsHitPlayer(rightAttackRanges[i]))
                {
                    Reference.Instance.player.TakeDamage(1);
                    rightHasHitDamage[i] = true; // この当たり判定範囲ではもうダメージを与えない
                }
            }
        }
        else if (!isEnemy && rightAttackRanges != null && rightHasHitDamage != null)
        {
            for (int i = 0; i < rightAttackRanges.Length; i++)
            {
                if (i < rightHasHitDamage.Length && !rightHasHitDamage[i])
                {
                    var enemyList = Util.GetEnemyList(rightAttackRanges[i]);
                    if (enemyList.Count > 0)
                    {
                        foreach (var enemy in enemyList)
                        {
                            enemy.TakeDamage(1, false);
                        }
                        rightHasHitDamage[i] = true; // この当たり判定範囲ではもうダメージを与えない
                    }
                }
            }
        }
    }
}

