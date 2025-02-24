using UnityEngine;

public interface ICharacter
{
    public RectTransform Rect { get; }
    public void TakeDamage(int damage);
}
