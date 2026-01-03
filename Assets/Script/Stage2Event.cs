using System.Collections;
using UnityEngine;

public class Stage2Event : MonoBehaviour
{
    [SerializeField] float moveDistance = -50f;   // カメラを右に動かす = ステージを左に動かす（マイナス方向）
    [SerializeField] float moveSpeed = 100f;       // 移動速度（ピクセル/秒）
    [SerializeField] float stopDuration = 1f;      // 移動後に停止する時間（秒）

    [SerializeField] Shisico1 shisico1;

    public void OnStage2Event()
    {
        StartCoroutine(MoveCameraSequence());
    }

    private IEnumerator MoveCameraSequence()
    {
        // プレイヤーの動きを止める
        Reference.Instance.PlayerStop = true;

        var pos = shisico1.transform.position;
        pos.x = Reference.Instance.player.transform.position.x + 530;
        shisico1.transform.position = pos;

        yield return new WaitForSeconds(1);

        var stageRect = Reference.Instance.stageRect;
        var playerRect = Reference.Instance.player.Rect;

        // 開始位置と目標位置を保存
        Vector2 startPos = stageRect.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x + moveDistance, startPos.y);

        // フェーズ1: 右に動く（ステージRectは左方向へ移動する = x が減少）
        yield return MoveCameraToPosition(stageRect, playerRect, targetPos, moveSpeed);

        // フェーズ2: 停止
        yield return new WaitForSeconds(stopDuration);
        yield return shisico1.EventMove();

        yield return new WaitForSeconds(1);

        // フェーズ3: 元の位置に戻る
        yield return MoveCameraToPosition(stageRect, playerRect, startPos, moveSpeed);

        // プレイヤーの動きを再開
        Reference.Instance.PlayerStop = false;
        Reference.Instance.isScroolStop = false;
        Destroy(shisico1.gameObject);
    }

    /// <summary>
    /// カメラ（stageRect）を目標位置まで移動させ、プレイヤーの画面上位置を固定する
    /// </summary>
    /// <param name="stageRect">ステージ（カメラ）のRectTransform</param>
    /// <param name="playerRect">プレイヤーのRectTransform</param>
    /// <param name="targetPos">目標位置</param>
    /// <param name="speed">移動速度（ピクセル/秒）</param>
    private IEnumerator MoveCameraToPosition(RectTransform stageRect, RectTransform playerRect, Vector2 targetPos, float speed)
    {
        while (Mathf.Abs(stageRect.anchoredPosition.x - targetPos.x) > 0.1f)
        {
            Vector2 current = stageRect.anchoredPosition;
            float step = speed * Time.deltaTime;
            float newX = Mathf.MoveTowards(current.x, targetPos.x, step);

            // stageRectの移動量を計算
            float deltaX = newX - current.x;

            // ステージ（カメラ）を動かす
            stageRect.anchoredPosition = new Vector2(newX, current.y);

            // プレイヤーは見た目上動かないよう、ステージの移動量と逆方向に動かす
            var p = playerRect.anchoredPosition;
            p.x += deltaX;
            playerRect.anchoredPosition = p;
            yield return null;
        }

        // 目標位置をきっちり保証
        stageRect.anchoredPosition = targetPos;
    }
}
