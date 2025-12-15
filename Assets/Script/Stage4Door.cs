using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage4Door : MonoBehaviour
{
    RectTransform Rect;
    [SerializeField] List<float> heights;
    [SerializeField] float spanTimer = 0.5f;
    Coroutine coroutine;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
    }

    public void Close()
    {
        Reference.Instance.PlayerStop = true;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(MoveIE(heights, () =>
        {
            //扉がしまったら
            var chair = FindFirstObjectByType<Stage4Chair>();
            chair.SetDoor(this);
            chair.OpenMove();
        }));
    }

    public void Open()
    {
        Reference.Instance.PlayerStop = true;
        var list = new List<float>(heights);
        list.Reverse();

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(MoveIE(list, () =>
        {
            Reference.Instance.PlayerStop = false;
            //ボス戦でなければステージクリアを発生させる
            Reference.Instance.player.MoveEnd();

        }));
    }

    private IEnumerator MoveIE(List<float> heights, Action onComplete)
    {
        foreach (var h in heights)
        {
            if (Rect != null)
            {
                var pos = Rect.anchoredPosition;
                pos.y = h;
                Rect.anchoredPosition = pos;
            }
            yield return new WaitForSeconds(spanTimer);
        }
        onComplete?.Invoke();
    }
}
