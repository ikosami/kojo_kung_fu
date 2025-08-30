using System;
using System.Collections;
using UnityEngine;

public class Boss3 : Enemy, ICharacter
{
    [SerializeField] Sprite normalSprite_1;
    [SerializeField] Sprite normalSprite_2;
    [SerializeField] Sprite normalSprite_3;

    [SerializeField] Sprite fireSprite_1;
    [SerializeField] Sprite fireSprite_2;
    [SerializeField] Sprite fireSprite_3;
    [SerializeField] Sprite fireSprite_4;

    [SerializeField] Sprite iceSprite_1;
    [SerializeField] Sprite iceSprite_2;
    [SerializeField] Sprite iceSprite_3;

    [SerializeField] Bullet bulletPrefab;
    [SerializeField] Transform bulletPoint;

    [SerializeField] RectTransform[] fallAttackRects;

    [SerializeField] int moveState = 0;
    float moveTimer = 0;

    float preHeight = 0;
    [SerializeField] float attackHeight = 200;

    int preState = 0;

    public Vector2 pos
    {
        get
        {
            return rect.anchoredPosition;
        }
        set
        {
            rect.anchoredPosition = value;
        }
    }


    protected override void Start()
    {
        hp = hpMax;
        Reference.Instance.enemyList.Add(this);
        preState = 0;
        moveTimer = 0;
    }

    protected override void Update()
    {
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        if (isDead) { return; }

        switch (moveState)
        {
            case 0:
                moveTimer += Time.deltaTime;
                if (moveTimer > 1)
                {
                    ChangeState(1);
                }
                break;
            case 1:
                if (ChangeFire(fireSprite_1))
                {
                    preState = 1;
                    ChangeState(2);
                }
                break;
            case 2:
                if (ChangeFire(iceSprite_1))
                {
                    preState = 2;
                    ChangeState(1);
                }
                break;
            case 3:
                break;
        }
    }

    //ふわーっと浮かび上がる
    private bool ChangeFire(Sprite target)
    {
        float duration = 1f; // 移動にかける時間（秒）
        if (moveTimer == 0)
        {
            preHeight = pos.y;
        }

        moveTimer += Time.deltaTime;

        // 線形補間でY座標を更新
        float t = Mathf.Clamp01(moveTimer / duration);
        float newY = Mathf.Lerp(preHeight, attackHeight, t);
        pos = new Vector2(pos.x, newY);

        Sprite sprite = null;
        if (preState == 0)
        {
            sprite = normalSprite_1;
        }
        else if (preState == 1)
        {
            sprite = fireSprite_1;
        }
        else if (preState == 2)
        {
            sprite = iceSprite_1;
        }

        if (moveTimer > 0.5f)
        {
            SetSprite(((int)(moveTimer * 15)) % 2 == 0 ? sprite : target);
        }
        else
        {
            SetSprite(target);
        }

        if (moveTimer > duration)
        {
            return true;
        }
        return false;
    }


    void ChangeState(int state)
    {
        moveState = state;
        moveTimer = 0;
    }

    public override void TakeDamage(int damage, bool breakAttack, string soundName)
    {
        if (isDead) { return; }

        hp -= damage;
        if (hp <= 0)
        {
            Reference.Instance.bgm.gameObject.SetActive(false);
            Reference.Instance.AddScore(3500);
            StartCoroutine(Dead());
        }
    }
    private IEnumerator Dead()
    {
        Vector3 startPos = transform.position;
        float duration = 2.0f; // 飛び上がりから落下までの時間
        float distance = transform.localScale.x * -200f; // 後方への移動距離
        float gravity = -2000f; // 重力加速度
        float elapsed = 0f;

        // 初速度を計算（自由落下を考慮）
        float velocityY = 700;

        Vector3 velocity = new Vector3(distance / duration, velocityY, 0f); // X方向 & Y方向の初速度

        while (elapsed < duration)
        {
            while (Reference.Instance.isPause || Reference.Instance.IsGameOver)
            {
                yield return null;
            }
            elapsed += Time.deltaTime;

            // X方向は一定速度で移動、Y方向は重力で加速度的に変化
            velocity.y += gravity * Time.deltaTime; // 重力の影響を加える

            // 現在の位置を更新
            transform.position += velocity * Time.deltaTime;

            yield return null;
        }

        Reference.Instance.StageComplete(BossNum);
        yield return new WaitForSeconds(2f); // 少し待機
        Reference.Instance.player.MoveEnd();
    }

    [SerializeField] int BossNum = 1;



}
