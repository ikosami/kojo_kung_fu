using System.Collections;
using UnityEngine;

public class FallItem : MonoBehaviour
{
    public Vector3 fallPower = new Vector3(200f, 700, 0); // 初期の落下力
    [SerializeField] Transform body;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Dead()); // 死亡処理を開始
    }

    /// <summary>
    /// 死亡時のアニメーションと非表示処理。
    /// </summary>
    private IEnumerator Dead()
    {
        Vector3 startPos = transform.position;
        float duration = 2.0f; // 飛び上がりから落下までの時間
        float distance = fallPower.x; // 後方への移動距離
        float gravity = -2000f; // 重力加速度
        float elapsed = 0f;

        // 初速度を計算（自由落下を考慮）
        float velocityY = fallPower.y;

        Vector3 velocity = new Vector3(distance / duration, velocityY, 0f); // X方向 & Y方向の初速度

        while (elapsed < duration)
        {
            // ポーズ・ゲームオーバー中は停止
            while (Reference.Instance.isPause || Reference.Instance.IsGameOver)
            {
                yield return null;
            }
            elapsed += Time.deltaTime;

            // X方向は一定速度で移動、Y方向は重力で加速度的に変化
            velocity.y += gravity * Time.deltaTime; // 重力の影響を加える

            // 現在の位置を更新
            body.transform.position += velocity * Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // 少し待機
        gameObject.SetActive(false); // オブジェクトを非表示にする
    }
}
