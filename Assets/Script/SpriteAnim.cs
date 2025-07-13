using UnityEngine;
using UnityEngine.UI;

public class SpriteAnim : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Sprite[] sprites;
    [SerializeField] float time = 0.5f;
    float timer = 0;
    private int index = 0;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= time)
        {
            timer = 0;
            index = (index + 1) % sprites.Length;
            image.sprite = sprites[index];
        }
    }
}
