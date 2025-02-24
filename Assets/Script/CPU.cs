using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CPU : MonoBehaviour, ICharacter
{
    [SerializeField] RectTransform rect;
    public RectTransform Rect => rect;
    [SerializeField] Image image;
    [SerializeField] Sprite normalSprite1;
    [SerializeField] Sprite normalSprite2;
    [SerializeField] Sprite attackSprite1;
    private float spriteChangeTimer = 0f;
    [SerializeField] private float spriteChangeInterval = 0.5f;
    private bool isNormalSprite = true;

    [SerializeField] Sprite damageSprite;

    [SerializeField] Vector3 moveSpeed = new Vector3(0.4f, 0, 0);

    //攻撃しようとして止まっている
    bool isAttack = false;
    [SerializeField] float attackTime = 0;

    int hp = 3;
    bool isDead { get { return hp <= 0; } }
    float damageWaitTime = 0;

    void Start()
    {
        hp = 3;
        Reference.Instance.enemyList.Add(this);
    }

    void Update()
    {
        if (isDead) { return; }
        if (damageWaitTime > 0)
        {
            damageWaitTime -= Time.deltaTime;
            if (damageWaitTime <= 0)
            {
                image.sprite = isNormalSprite ? normalSprite1 : normalSprite2;
            }
            else
            {
                return;
            }
        }

        Move();
        var pos = rect.anchoredPosition;
        //HandleJump(ref pos);
        rect.anchoredPosition = pos;
        HandleAttack();
        HandleNormalSpriteAnimation();

    }

    private void HandleAttack()
    {
        if (!isAttack) { return; }
        attackTime += Time.deltaTime;

        if (attackTime < 0.5f)
        {
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
        }
        else if (attackTime < 1f)
        {
            if (image.sprite != attackSprite1)
                image.sprite = attackSprite1;
        }
        else if (attackTime < 1.5f)
        {
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
        }
        else
        {
            isAttack = false;
            spriteChangeTimer = 0;
        }
    }

    private void Move()
    {
        if (isAttack) { return; }

        var player = Reference.Instance.player;
        if (player.transform.position.x > transform.position.x)
        {
            transform.position += moveSpeed * Time.deltaTime;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.position -= moveSpeed * Time.deltaTime;
            transform.localScale = new Vector3(-1, 1, 1);
        }

        //攻撃範囲に入ったら攻撃
        if (Mathf.Abs(player.transform.position.x - transform.position.x) < 50)
        {
            isAttack = true;
            attackTime = 0;
        }
    }

    private void HandleNormalSpriteAnimation()
    {
        if (isAttack) { return; }
        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer >= spriteChangeInterval)
        {
            spriteChangeTimer -= spriteChangeInterval;
            isNormalSprite = !isNormalSprite;
        }
        image.sprite = isNormalSprite ? normalSprite1 : normalSprite2;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) { return; }

        image.sprite = damageSprite;
        damageWaitTime = 0.5f;

        hp -= damage;
        if (hp <= 0)
        {
            Reference.Instance.AddScore(800);
            StartCoroutine(Dead());
        }
    }
    private IEnumerator Dead()
    {
        Vector3 startPos = transform.position;
        float duration = 1.0f; // 飛び上がりから落下までの時間
        float distance = transform.localScale.x * -200f; // 後方への移動距離
        float gravity = -2000f; // 重力加速度
        float elapsed = 0f;

        // 初速度を計算（自由落下を考慮）
        float velocityY = 700;

        Vector3 velocity = new Vector3(distance / duration, velocityY, 0f); // X方向 & Y方向の初速度

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // X方向は一定速度で移動、Y方向は重力で加速度的に変化
            velocity.y += gravity * Time.deltaTime; // 重力の影響を加える

            // 現在の位置を更新
            transform.position += velocity * Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // 少し待機
        gameObject.SetActive(false); // オブジェクトを非表示にする
    }



}
