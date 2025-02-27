using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class Util : MonoBehaviour
{
    public static List<ICharacter> GetEnemyList(RectTransform target)
    {
        return GetEnemyList(target, Vector2.zero);
    }
    public static List<ICharacter> GetEnemyList(RectTransform target, Vector2 offset)
    {
        List<ICharacter> overlappingImages = new List<ICharacter>();

        Rect targetRect = GetWorldRect(target);
        targetRect.x += offset.x;
        targetRect.y += offset.y;

        foreach (var enemy in Reference.Instance.enemyList)
        {
            Rect imgRect = GetWorldRect(enemy.BodyColRect);
            if (targetRect.Overlaps(imgRect))
            {
                overlappingImages.Add(enemy);
            }
        }

        return overlappingImages;
    }

    internal static bool IsHitPlayer(RectTransform target)
    {
        List<ICharacter> overlappingImages = new List<ICharacter>();

        Rect targetRect = GetWorldRect(target);
        Rect imgRect = GetWorldRect(Reference.Instance.player.BodyColRect);
        return (targetRect.Overlaps(imgRect));
    }

    private static Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        float xMin = Mathf.Min(corners[0].x, corners[2].x);
        float yMin = Mathf.Min(corners[0].y, corners[2].y);
        float width = Mathf.Abs(corners[2].x - corners[0].x);
        float height = Mathf.Abs(corners[2].y - corners[0].y);

        return new Rect(xMin, yMin, width, height);
    }

    /// <summary>
    /// 座標を保ったままAnchorを変更する
    /// </summary>
    /// <param name="rectTransform">自身の参照</param>
    /// <param name="targetMinAnchor">変更先のAnchorMin座標</param>
    /// <param name="targetMaxAnchor">変更先のAnchorMax座標</param>
    public static void SetAnchorWithKeepingPosition(RectTransform rectTransform, Vector2 targetMinAnchor, Vector2 targetMaxAnchor)
    {
        var parent = rectTransform.parent as RectTransform;
        if (parent == null) { Debug.LogError("Parent cannot find."); }

        var diffMin = targetMinAnchor - rectTransform.anchorMin;
        var diffMax = targetMaxAnchor - rectTransform.anchorMax;
        // anchorの更新
        rectTransform.anchorMin = targetMinAnchor;
        rectTransform.anchorMax = targetMaxAnchor;
        // 上下左右の距離の差分を計算
        var diffLeft = parent.rect.width * diffMin.x;
        var diffRight = parent.rect.width * diffMax.x;
        var diffBottom = parent.rect.height * diffMin.y;
        var diffTop = parent.rect.height * diffMax.y;
        // サイズと座標の修正
        rectTransform.sizeDelta += new Vector2(diffLeft - diffRight, diffBottom - diffTop);
        var pivot = rectTransform.pivot;
        rectTransform.anchoredPosition -= new Vector2(
             (diffLeft * (1 - pivot.x)) + (diffRight * pivot.x),
             (diffBottom * (1 - pivot.y)) + (diffTop * pivot.y)
        );
    }
}
