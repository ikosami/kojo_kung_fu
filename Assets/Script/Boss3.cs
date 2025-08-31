using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

    [SerializeField] Transform bulletPoint;


    [SerializeField] int moveState = 0;
    float moveTimer = 0;

    bool isRight = true;
    Vector2 targetPos;
    Vector2 prePos;
    [SerializeField] float attackHeight = 200;
    [SerializeField] float leftPoint = 30;
    [SerializeField] float rightPoint = 130;

    [SerializeField] FireBoss3 fireBoss3;
    [SerializeField] GameObject fireWheel;
    [SerializeField] RectTransform[] fireWheelAttacks;

    //属性
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
                if (moveTimer > 2)
                {
                    ChangeState(1);
                }
                break;
            case 1:
                if (ChangeFire(fireSprite_1))
                {
                    preState = 1;
                    ChangeState(3);
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
                if (FireBoal())
                {
                    ChangeState(4);
                }
                break;
            case 4:
                if (FireWheel())
                {
                    ChangeState(3);
                }
                break;
        }
    }

    private bool FireWheel()
    {
        float duration = 3f; // 移動にかける時間（秒）
        if (moveTimer == 0)
        {
            SoundManager.Instance.Play("fire");
            prePos = new Vector2(pos.x, pos.y);

            if (isRight)
            {
                targetPos = new Vector2(leftPoint, pos.y);
            }
            else
            {
                targetPos = new Vector2(rightPoint, pos.y);
            }
            fireWheel.SetActive(true);
            SetSprite(fireSprite_2);
        }


        if (isAttackDamage)
        {
            foreach (var attackRange in fireWheelAttacks)
            {
                if (Util.IsHitPlayer(attackRange))
                {
                    SoundManager.Instance.Play("enemy_attack");
                    Reference.Instance.player.TakeDamage(1);
                    isAttackDamage = false;
                    break;
                }
            }
        }


        moveTimer += Time.deltaTime;
        // 線形補間でY座標を更新
        float t = Mathf.Clamp01(moveTimer / duration);
        float newY = Mathf.Lerp(prePos.x, targetPos.x, t);
        pos = new Vector2(newY, pos.y);

        if (moveTimer > duration)
        {
            fireWheel.SetActive(false);
            SetSprite(fireSprite_1);
            isRight = !isRight;
            transform.localScale = new Vector3(isRight ? -1 : 1, 1, 1);

            return true;
        }
        return false;
    }

    private bool FireBoal()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        // 攻撃＆表示を同一箇所で処理
        float attackTime1 = 1f;
        float attackTime2 = 1.5f;
        float attackDur = 0.3f; // 表示継続時間

        if (moveTimer >= attackTime1 && moveTimer < attackTime1 + attackDur)
        {
            if (preTimer < attackTime1) FireBullet();
            SetSprite(fireSprite_2);
        }
        else if (moveTimer >= attackTime2 && moveTimer < attackTime2 + attackDur)
        {
            if (preTimer < attackTime2) FireBullet();
            SetSprite(fireSprite_2);
        }
        else
        {
            SetSprite(fireSprite_1);
        }

        if (moveTimer > 8f) return true;
        return false;
    }

    void FireBullet()
    {
        SoundManager.Instance.Play("fire");
        Instantiate(fireBoss3, bulletPoint.transform.position, Quaternion.Euler(0, 0, 180), Reference.Instance.stageRect);
    }

    //ふわーっと浮かび上がる
    private bool ChangeFire(Sprite target)
    {
        float duration = 1f; // 移動にかける時間（秒）
        if (moveTimer == 0)
        {
            SoundManager.Instance.Play("boss_change");
            prePos = new Vector2(pos.x, pos.y);
        }

        moveTimer += Time.deltaTime;

        // 線形補間でY座標を更新
        float t = Mathf.Clamp01(moveTimer / duration);
        float newY = Mathf.Lerp(prePos.y, attackHeight, t);
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

        if (moveTimer < 0.5f)
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
        isAttackDamage = true;
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
