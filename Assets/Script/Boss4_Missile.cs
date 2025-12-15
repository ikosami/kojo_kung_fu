using UnityEngine;
using UnityEngine.UI;

public class Boss4_Missile : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("敵かどうか（true: プレイヤーにダメージ、false: 敵にダメージ）")]
    [SerializeField] bool isEnemy = true;
    [Tooltip("本体の当たり判定範囲（RectTransform）")]
    [SerializeField] RectTransform bodyRect;
    [Tooltip("本体の画像コンポーネント")]
    [SerializeField] Image image;
    [Tooltip("攻撃の当たり判定範囲（配列）")]
    [SerializeField] RectTransform[] attackRanges;

    [Header("スプライト設定")]
    [Tooltip("状態1（落下中）で使用するスプライト配列")]
    [SerializeField] Sprite[] sprites;
    [Tooltip("状態2（真横移動中）で使用するスプライト配列（0番と1番のみ使用、0.5秒ごとに切り替え）")]
    [SerializeField] Sprite[] sprites2;

    int nowSpriteIndex = 0; // 現在のスプライトインデックス
    float spriteChangeTimer = 0f; // スプライト切り替え用タイマー

    int state = -1; // -1: 状態1（落下中）、2: 状態2（真横に早く進む）
    float timer = 0;

    bool isDamage = true;
    public bool isPlayerFollow;

    [Header("状態1設定（落下中）")]
    [Tooltip("到達する高さ（UI座標、この高さに到達すると状態2に切り替わる）")]
    [SerializeField] float floor = 2;
    [Tooltip("落下速度（UI座標/秒、下方向への移動速度）")]
    [SerializeField] float descendSpeed = 200f;
    [Tooltip("横方向のゆっくりした移動速度（UI座標/秒、落下中に横に移動する速度）")]
    [SerializeField] float slowMoveSpeed = 50f;
    [Tooltip("横移動方向（1: 右方向、-1: 左方向）")]
    [SerializeField] float slowMoveDirection = 1f;

    [Header("状態2設定（真横に早く進む）")]
    [Tooltip("真横の移動速度（UI座標/秒、状態2での横方向への移動速度）")]
    [SerializeField] float fastMoveSpeed = 500f;
    [Tooltip("横移動方向（1: 右方向、-1: 左方向）")]
    [SerializeField] float fastMoveDirection = 1f;
    [Tooltip("移動継続時間（秒）、この時間経過後にミサイルが消える")]
    [SerializeField] float moveDuration = 3f;


    /// <summary>
    /// 左向きに設定する（向きと速度を*-1で反転、角度も設定可能）
    /// </summary>
    /// <param name="angle">回転角度（度、デフォルト: 0）</param>
    public void SetLeft(float angle = 0f)
    {
        // スケールを設定（左向きの場合は-1で反転）
        var scale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(scale.x) * -1f, scale.y, scale.z);

        // 速度の方向を反転（*-1）
        slowMoveDirection *= -1f;
        fastMoveDirection *= -1f;

        // 角度を設定
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// 目標高さ（floor）を設定する
    /// </summary>
    /// <param name="targetHeight">到達する高さ（UI座標）</param>
    public void SetFloor(float targetHeight)
    {
        floor = targetHeight;
    }

    // Update is called once per frame
    void Update()
    {
        // ゲームクリア・ポーズ・ゲームオーバー時は処理しない
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) return;

        switch (state)
        {
            case -1: // 状態1: 指定の高さに向かって落下中、別の速度でゆっくり進む
                {
                    var pos = bodyRect.anchoredPosition;
                    // Y方向: 指定の高さに向かって落下
                    float newY = Mathf.MoveTowards(pos.y, floor, descendSpeed * Time.deltaTime);
                    // X方向: ゆっくり横移動
                    float newX = pos.x + slowMoveSpeed * slowMoveDirection * Time.deltaTime;
                    bodyRect.anchoredPosition = new Vector2(newX, newY);

                    // 状態1用のスプライトアニメーション
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

                    // 指定の高さに到達したら状態2に切り替え
                    if (Mathf.Abs(newY - floor) <= 0.01f)
                    {
                        bodyRect.anchoredPosition = new Vector2(newX, floor); // スナップ
                        state = 2; // 状態2に切り替え
                        timer = 0f; // タイマーリセット
                        nowSpriteIndex = 0; // スプライトインデックスリセット
                        spriteChangeTimer = 0f; // スプライトタイマーリセット

                        // 状態切り替え時に即座に画像を更新
                        image.sprite = sprites2[nowSpriteIndex];
                    }
                    break;
                }

            case 2: // 状態2: 真横に早く進む、スプライト0,1を0.5秒毎に切り替え
                {
                    // 真横に早く進む
                    var pos = bodyRect.anchoredPosition;
                    float newX = pos.x + fastMoveSpeed * fastMoveDirection * Time.deltaTime;
                    bodyRect.anchoredPosition = new Vector2(newX, pos.y);

                    // 状態2用のスプライトアニメーション（0,1のみを0.5秒毎に切り替え）
                    spriteChangeTimer += Time.deltaTime;
                    if (spriteChangeTimer >= 0.5f)
                    {
                        nowSpriteIndex = (nowSpriteIndex + 1) % 2; // 0と1のみを切り替え
                        image.sprite = sprites2[nowSpriteIndex];
                        spriteChangeTimer = 0f;
                    }

                    // 一定秒数動き続けたら消える
                    timer += Time.deltaTime;
                    if (timer >= moveDuration)
                    {
                        Destroy(gameObject);
                    }
                    break;
                }
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

