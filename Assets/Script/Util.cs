using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Util : MonoBehaviour
{
    public static List<ICharacter> GetEnemyList(RectTransform target)
    {
        List<ICharacter> overlappingImages = new List<ICharacter>();

        Rect targetRect = GetWorldRect(target);

        foreach (var enemy in Reference.Instance.enemyList)
        {
            Rect imgRect = GetWorldRect(enemy.Rect);
            if (targetRect.Overlaps(imgRect))
            {
                overlappingImages.Add(enemy);
            }
        }

        return overlappingImages;
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

}
