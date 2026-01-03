using UnityEngine;
using UnityEngine.UI;

public class CharacterBase : MonoBehaviour
{
    public int hp;
    public int maxHP;
    public bool isDead => hp <= 0;
    public RectTransform Rect => transform as RectTransform;
    public RectTransform BodyColRect;

    public virtual void TakeDamage(int damage, bool breakAttack, string soundName = "")
    {
        hp -= damage;
    }
    internal Image GetImage()
    {
        var img = GetComponent<Image>();

        if (img == null)
        {
            img = transform.GetChild(0).GetComponent<Image>();
        }
        return img;
    }
}
