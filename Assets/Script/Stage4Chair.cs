using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage4Chair : MonoBehaviour
{
    RectTransform Rect;
    [SerializeField] float targetPos = 12.5f;
    [SerializeField] float moveTime = 1.5f;

    [SerializeField] Stage4_EnemyWave enemyWave;

    Vector2 originalPos; // 椅子の元の位置を保存

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        originalPos = Rect.anchoredPosition; // 初期位置を保存
        enemyWave.OnAllWaveCompleted += ReturnMove;
    }

    public void OpenMove()
    {
        StartCoroutine(OpenMoveIE());
    }

    private IEnumerator OpenMoveIE()
    {
        Vector2 endPos = new Vector2(Rect.anchoredPosition.x, targetPos);
        yield return StartCoroutine(MoveToPosition(endPos));

        Reference.Instance.PlayerStop = false;

        yield return new WaitForSeconds(1f);

        // 椅子の移動が完了したらウェーブシステムを開始
        if (enemyWave != null)
        {
            enemyWave.StartWaveSystem();
        }
    }

    public void ReturnMove()
    {
        StartCoroutine(ReturnMoveIE());
    }

    private IEnumerator ReturnMoveIE()
    {
        yield return StartCoroutine(MoveToPosition(originalPos));

        Reference.Instance.PlayerStop = false;

        yield return new WaitForSeconds(1f);
        // 椅子の移動が完了したらドアを開く
        if (door != null)
        {
            door.Open();
        }
    }

    /// <summary>
    /// 共通の位置移動処理
    /// </summary>
    /// <param name="targetPosition">目標位置</param>
    private IEnumerator MoveToPosition(Vector2 targetPosition)
    {
        Vector2 startPos = Rect.anchoredPosition;
        Vector2 endPos = targetPosition;
        float time = 0f;

        while (time < moveTime)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveTime);
            Rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        Rect.anchoredPosition = endPos;
    }

    Stage4Door door;

    internal void SetDoor(Stage4Door stage4Door)
    {
        this.door = stage4Door;
    }
}
