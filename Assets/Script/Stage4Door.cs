using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage4Door : MonoBehaviour
{
    RectTransform Rect;
    [SerializeField] List<float> heights;
    [SerializeField] float spanTimer = 0.5f;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
    }

    public void Close()
    {
        Reference.Instance.PlayerStop = true;
        StartCoroutine(MoveIE(heights));
    }

    public void Open()
    {
        Reference.Instance.PlayerStop = true;
        var list = new List<float>(heights);
        list.Reverse();
        StartCoroutine(MoveIE(list));
    }

    private IEnumerator MoveIE(List<float> heights)
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

        FindFirstObjectByType<Stage4Chair>().Move();
    }
}
