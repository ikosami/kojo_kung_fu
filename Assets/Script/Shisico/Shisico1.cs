using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Shisico1 : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Sprite normalSprite1;
    [SerializeField] Sprite normalSprite2;
    [SerializeField] protected float spriteChangeInterval = 0.5f; // スプライト切り替え間隔

    float spriteChangeTimer = 0;
    bool isNormalSprite = true;

    [SerializeField] float moveSpeed = 20f;      // 移動速度（ピクセル/秒）
    [SerializeField] float moveDuration = 1f;     // 移動時間（秒）

    // Update is called once per frame
    void Update()
    {
        SpriteChange();
    }

    void SpriteChange()
    {
        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer >= spriteChangeInterval)
        {
            spriteChangeTimer -= spriteChangeInterval;
            isNormalSprite = !isNormalSprite;
        }
        image.sprite = isNormalSprite ? normalSprite1 : normalSprite2;
    }

    public IEnumerator EventMove()
    {
        transform.localScale = new Vector3(1, 1, 1);

        //コルーチンで右に移動（一定速度で一定時間）
        var rect = GetComponent<RectTransform>();

        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float moveDistance = moveSpeed * Time.deltaTime;
            Vector2 currentPos = rect.anchoredPosition;
            currentPos.x += moveDistance;
            rect.anchoredPosition = currentPos;
            yield return null;
        }
    }
}
