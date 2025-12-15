using UnityEngine;
using UnityEngine.UI;

public class Boss4_MissileAir : MonoBehaviour
{
    [SerializeField] bool isEnemy = true;

    [SerializeField] RectTransform bodyRect; // 本体当たり判定範囲
    [SerializeField] Image image; // 本体の画像コンポーネント

    [SerializeField] RectTransform attackRange;
    [SerializeField] RectTransform fireAttackRange; // 炎として残る時の当たり判定範囲

    [SerializeField] Sprite[] missileSprites; // ミサイル用のスプライト配列
    [SerializeField] Sprite[] fireSprites; // 炎用のスプライト配列
    Sprite[] currentSprites; // 現在使用中のスプライト配列
    int nowSpriteIndex = 0; // 現在のスプライトインデックス
    float spriteChangeTimer = 0f; // スプライト切り替え用タイマー

    int state = 0;
    float timer = 0;

    [SerializeField] float maxHeight = -10f; // 上昇限界の高さ（画面上部付近）
    [SerializeField] float floorHeight = -73f; // 地面の高さ（PlayerやEnemyと同じ）
    [SerializeField] float fireDuration = 3.5f; // 炎として残る時間

    public Vector2 move; // 上昇用の移動ベクトル（主にY方向の速度）
    bool isDamage = true;
    public bool isPlayerFollow;

    float fallVelocity = 0f; // 落下速度
    float lastDamageTime = 0f; // 最後にダメージを与えた時間
    [SerializeField] float damageInterval = 0.5f; // ダメージを与える間隔（炎として残っている間）

    private void Awake()
    {
        // 上昇時は上を向く（右向き0度から-90度回転）
        // Awakeで設定することで、生成直後に確実に回転を設定
        transform.rotation = Quaternion.Euler(0, 0, -90f);
    }

    private void Start()
    {
        // 初期状態はミサイルのスプライト
        currentSprites = missileSprites;
        if (currentSprites != null && currentSprites.Length > 0)
        {
            image.sprite = currentSprites[0];
        }

        // 生成時にワールド座標で配置されている場合、anchoredPositionに変換
        // Boss4.csでstageRectを親にしているため、anchoredPositionを使用可能な状態にする
        if (bodyRect != null && Reference.Instance.stageRect != null)
        {
            // 親がstageRectの場合、ワールド座標からanchoredPositionへの変換は自動的に行われる
            // ただし、初期位置が正しく設定されるようにする
            var currentPos = bodyRect.anchoredPosition;
            // 初期位置の確認（必要に応じて調整）
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ゲームクリア・ポーズ・ゲームオーバー時は処理しない
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        // スプライトアニメーション
        if (currentSprites != null && currentSprites.Length > 0)
        {
            spriteChangeTimer += Time.deltaTime;
            if (spriteChangeTimer >= 0.2f)
            {
                nowSpriteIndex = (nowSpriteIndex + 1) % currentSprites.Length;
                image.sprite = currentSprites[nowSpriteIndex];
                spriteChangeTimer = 0f;
            }
        }

        switch (state)
        {
            case 0: // 上昇処理
                {
                    // 上昇中は上を向く（回転を維持）
                    if (transform.rotation.eulerAngles.z != 270f) // -90度 = 270度
                    {
                        transform.rotation = Quaternion.Euler(0, 0, -90f);
                    }

                    var pos = bodyRect.anchoredPosition;
                    // 指定速度で上昇
                    pos += move * Time.deltaTime;
                    bodyRect.anchoredPosition = pos;

                    // 指定の地点（上昇限界）に達したらワープ
                    if (pos.y >= maxHeight)
                    {
                        // TransformをRectTransformとして扱い、anchoredPositionを取得
                        RectTransform screenLeftUpRect = Reference.Instance.screenLeftUp as RectTransform;
                        RectTransform screenRightDownRect = Reference.Instance.screenRightDown as RectTransform;

                        if (screenLeftUpRect != null && screenRightDownRect != null)
                        {
                            // 両方がstageRectの子要素として同じ親を持つ場合、anchoredPositionを直接使用
                            Vector2 leftUpPos = screenLeftUpRect.anchoredPosition;
                            Vector2 rightDownPos = screenRightDownRect.anchoredPosition;

                            // ランダムな位置を計算
                            float randomX = Random.Range(rightDownPos.x, leftUpPos.x);
                            bodyRect.anchoredPosition = new Vector2(randomX, bodyRect.anchoredPosition.y);
                        }

                        state = 1; // 落下状態へ
                        fallVelocity = 0f; // 落下速度をリセット
                        timer = 0f; // タイマーをリセット
                        // 落下時は下を向く（90度回転）
                        transform.rotation = Quaternion.Euler(0, 0, 90f);
                    }
                    break;
                }

            case 1: // 落下処理
                {
                    // 落下中は下を向く（回転を維持）
                    if (transform.rotation.eulerAngles.z != 90f)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, 90f);
                    }

                    var pos = bodyRect.anchoredPosition;

                    // 落下速度は上昇速度と同じ（move.yの絶対値）
                    float fallSpeed = Mathf.Abs(move.y);
                    pos.y -= fallSpeed * Time.deltaTime;

                    // 地面に着いたら
                    if (pos.y <= floorHeight)
                    {
                        pos.y = floorHeight; // 地面に固定
                        bodyRect.anchoredPosition = pos;
                        state = 2; // 炎として残る状態へ
                        timer = 0f;
                        // 炎は回転なし（初期状態にリセット）
                        transform.rotation = Quaternion.identity;

                        // ミサイルから炎に切り替え
                        currentSprites = fireSprites;
                        nowSpriteIndex = 0;
                        spriteChangeTimer = 0f;
                        image.sprite = fireSprites[0];
                    }
                    else
                    {
                        bodyRect.anchoredPosition = pos;
                    }
                    break;
                }

            case 2: // 炎として残る
                {
                    timer += Time.deltaTime;
                    if (timer >= fireDuration)
                    {
                        // 3.5秒経過したら破棄
                        Destroy(gameObject);
                    }
                    // 炎として残っている間もスプライトアニメーションは継続
                    // （スプライトアニメーションはstateに関係なく実行される）
                    break;
                }
        }

        // 当たり判定処理（全stateで継続）
        if (isDamage)
        {
            // state 2（炎として残る）の場合は炎用の当たり判定を使用
            RectTransform currentAttackRange = (state == 2 && fireAttackRange != null) ? fireAttackRange : attackRange;

            if (isEnemy)
            {
                if (Util.IsHitPlayer(currentAttackRange))
                {
                    // state 2（炎として残る）の場合は継続してダメージを与える
                    if (state == 2)
                    {
                        // 一定間隔でダメージを与える
                        if (Time.time >= lastDamageTime + damageInterval)
                        {
                            Reference.Instance.player.TakeDamage(1);
                            lastDamageTime = Time.time;
                        }
                    }
                    else
                    {
                        // 落下中は一度だけダメージを与える
                        Reference.Instance.player.TakeDamage(1);
                        isDamage = false;
                    }
                }
            }
            else
            {
                var enemyList = Util.GetEnemyList(currentAttackRange);
                foreach (var enemy in enemyList)
                {
                    // state 2（炎として残る）の場合は継続してダメージを与える
                    if (state == 2)
                    {
                        // 一定間隔でダメージを与える
                        if (Time.time >= lastDamageTime + damageInterval)
                        {
                            enemy.TakeDamage(1, false);
                            lastDamageTime = Time.time;
                        }
                    }
                    else
                    {
                        // 落下中は一度だけダメージを与える
                        enemy.TakeDamage(1, false);
                    }
                }
                if (state != 2)
                {
                    isDamage = false;
                }
            }
        }
    }
}

