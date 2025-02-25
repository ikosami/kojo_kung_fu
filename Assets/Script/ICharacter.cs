using UnityEngine;

public interface ICharacter
{
    public GameObject GameObject { get; }
    public RectTransform Rect { get; }
    public RectTransform BodyColRect { get; }
    public void TakeDamage(int damage);
}
