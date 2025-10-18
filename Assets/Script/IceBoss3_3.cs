using UnityEngine;
using UnityEngine.UI;

public class IceBoss3_3 : MonoBehaviour
{
    [SerializeField] bool isEnemy = true;

    [SerializeField] RectTransform bodyRect; // 本体当たり判定範囲
    [SerializeField] Image image; // 本体の画像コンポーネント

    [SerializeField] RectTransform[] attackRanges;

    [SerializeField] Sprite[] sprites;
    int nowSpriteIndex = 0; // 現在のスプライトインデックス
    float spriteChangeTimer = 0f; // スプライト切り替え用タイマー

    int state = -1;
    [SerializeField] float floor = 2;
    float timer = 0;

    public Vector2 fallMove;
    bool isDamage = true;
    public bool isPlayerFollow;
    [SerializeField] float descendSpeed = 200f; // 生成直後の降下速度(UI座標/秒)
    Vector2 shakeBasePos;
    [SerializeField] float shakeFreq = 50f;
    [SerializeField] float shakeAmp = 5f;
    float noiseSeedX, noiseSeedY;
    float initialX;

    private void Start()
    {
        initialX = bodyRect.anchoredPosition.x; // Xを固定
    }

    // 外部から呼ぶ開始関数
    public void StartFall()
    {
        state = 0; // 揺れ開始
        timer = 0;
        shakeBasePos = bodyRect.anchoredPosition; // 基準記録
        noiseSeedX = Random.value * 1000f;
        noiseSeedY = Random.value * 1000f;
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
            case -1: // 生成直後: 所定高さ(floor)まで降下して待機
                {
                    var pos = bodyRect.anchoredPosition;
                    float newY = Mathf.MoveTowards(pos.y, floor, descendSpeed * Time.deltaTime);
                    bodyRect.anchoredPosition = new Vector2(initialX, newY);

                    if (Mathf.Abs(newY - floor) <= 0.01f)
                    {
                        bodyRect.anchoredPosition = new Vector2(initialX, floor); // スナップ
                        shakeBasePos = bodyRect.anchoredPosition; // 待機基準
                        state = 2; // 待機
                    }
                    break;
                }

            case 2: // 待機（StartFall呼び出し待ち）
                break;
            case 0: // 揺れ
                timer += Time.deltaTime;
                float nx = Mathf.PerlinNoise(noiseSeedX, timer * shakeFreq) * 2f - 1f;
                float ny = Mathf.PerlinNoise(noiseSeedY, timer * shakeFreq) * 2f - 1f;
                bodyRect.anchoredPosition = shakeBasePos + new Vector2(nx * shakeAmp, ny * shakeAmp);

                if (timer >= 1f) { timer = 0; state = 1; }
                break;

            case 1: // 無限落下
                bodyRect.anchoredPosition += fallMove * Time.deltaTime;
                timer += Time.deltaTime;
                if (timer >= 3f)
                {
                    Destroy(gameObject);
                }
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
