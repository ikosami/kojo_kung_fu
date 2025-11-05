using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage4Chair : MonoBehaviour
{
    RectTransform Rect;
    [SerializeField] float targetPos = 12.5f;
    [SerializeField] float moveTime = 1.5f;

    [SerializeField]Stage4_EnemyWave enemyWave;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
    }

    public void Move()
    {
        StartCoroutine(MoveIE());
    }

    private IEnumerator MoveIE()
    {
        Vector2 startPos = Rect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, targetPos);
        float time = 0f;

        while (time < moveTime)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveTime);
            Rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            Debug.LogError(Rect.anchoredPosition + " " + t);
            yield return null;
        }

        Rect.anchoredPosition = endPos;
        Reference.Instance.PlayerStop = false;

yield return new WaitForSeconds(1f);

        // 椅子の移動が完了したらウェーブシステムを開始
        if (enemyWave != null)
        {
            enemyWave.StartWaveSystem();
        }
    }
}
