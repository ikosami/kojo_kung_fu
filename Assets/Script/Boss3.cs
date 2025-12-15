using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss3 : Enemy, ICharacter
{
    [SerializeField] Sprite normalSprite_idle;
    [SerializeField] Sprite normalSprite_fly;
    [SerializeField] Sprite normalSprite_attack_idle;
    [SerializeField] Sprite normalSprite_attack;
    [SerializeField] Sprite normalSprite_fall;
    [SerializeField] Sprite normalSprite_damage;


    [SerializeField] Transform bulletPoint;
    [SerializeField] Transform[] bulletFireBallPoints;
    [SerializeField] Transform bulletPointIce;


    [SerializeField] int moveState = 0;
    float moveTimer = 0;

    bool isRight = true;
    Vector2 targetPos;
    Vector2 prePos;

    [SerializeField] Transform[] iceBulletPoints;

    [SerializeField] float attackHeight = 200;
    [SerializeField] float downHeight = 100;
    [SerializeField] float leftPoint = 30;
    [SerializeField] float rightPoint = 130;


    [SerializeField] IceBoss3_1 iceBoss3_1;
    [SerializeField] IceBoss3_2 iceBoss3_2;
    List<IceBoss3_2> iceFallList = new List<IceBoss3_2>();

    [SerializeField] FireBoss3_1 fireBoss3_1;
    [SerializeField] FireBoss3_2 fireBoss3_2;
    [SerializeField] GameObject damageFieldObj;
    [SerializeField] RectTransform[] damageFieldAttacks;

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

    List<int> moveList = new List<int> { 1, 2, 3, 4 };

    protected override void Update()
    {
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        if (isDead) { return; }


        if (damageFieldObj.activeSelf)
        {
            foreach (var attackRange in damageFieldAttacks)
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
        Damaging();


        switch (moveState)
        {
            case -2://初期待機

                moveTimer += Time.deltaTime;
                if (moveTimer > 1)
                {
                    ChangeState(0);
                }
                break;
            case -1://ダウン状態
                if (ChangeDown())
                {
                    ChangeState(0);
                }
                break;
            case 0:
                if (FlyMove())
                {
                    moveList = new List<int> { 1, 2, 3, 4 };

                    SetRandomState();
                }
                break;
            case 1:
                if (FireFall())
                {
                    SetRandomState();
                }
                break;
            case 2:
                if (FireBoal())
                {
                    SetRandomState();
                }
                break;
            case 3:
                if (IceBlock())
                {
                    SetRandomState();
                }
                break;
            case 4:
                if (IceFall())
                {
                    SetRandomState();
                }
                break;
        }
    }

    protected override bool Damaging()
    {
        if (moveState != -1)
        {
            return false;
        }
        // ダメージ演出中はスプライトをダメージ用にし、一定時間後に戻す
        if (damageWaitTime > 0)
        {
            damageWaitTime -= Time.deltaTime;
            if (damageWaitTime <= 0)
            {
                image.sprite = normalSprite_fall;
            }
            else
            {
                return true;
            }
        }

        return true;
    }

    private void SetRandomState()
    {
        if (moveList.Count == 0)
        {
            ChangeState(-1);
            return;
        }

        var random = Random.Range(0, moveList.Count);
        var state = moveList[random];
        moveList.RemoveAt(random);

        ChangeState(state);
    }

    private bool IceFall()
    {
        if (moveTimer == 0)
        {
            SoundManager.Instance.Play("ice");

            int offset = 3;
            // 右端の offset+1 までを避けてランダム取得
            int random = Random.Range(offset, iceBulletPoints.Length - offset - 1);
            iceFallList.Clear();
            for (int i = 0; i < iceBulletPoints.Length; i++)
            {
                if (i == random || i == random + 1) continue;

                Transform ice = iceBulletPoints[i];
                var iceObj = Instantiate(iceBoss3_2, ice.transform.position, Quaternion.identity, Reference.Instance.stageRect);
                iceFallList.Add(iceObj);
            }
            SetSprite(normalSprite_attack_idle);

        }
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;
        var fallTimer = 1.5f;
        if (moveTimer > fallTimer && preTimer < fallTimer)
        {
            foreach (var ice in iceFallList)
            {
                if (ice != null)
                {
                    ice.Fall();
                }
            }
            iceFallList.Clear();
            SetSprite(normalSprite_attack);
        }
        if (moveTimer > fallTimer + 1)
        {
            SetSprite(normalSprite_fly);
        }


        if (moveTimer > 6f)
        {
            return true;
        }
        return false;
    }

    private bool IceBlock()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        float startTime = 1f;     // 最初の攻撃開始時刻
        float interval = 2f;    // 一定間隔
        int attackCount = 4;      // 回数
        float attackDur = 1f;   // 表示継続時間
        float endWait = 2f; // 終了後の待機時間

        bool attacking = false;
        for (int i = 0; i < attackCount; i++)
        {
            float t = startTime + interval * i;
            if (moveTimer >= t && moveTimer < t + attackDur)
            {
                if (preTimer < t) IceBullet();
                SetSprite(normalSprite_attack);
                attacking = true;
                break;
            }
        }


        if (startTime + interval * (attackCount - 1) + attackDur < moveTimer)
        {
            SetSprite(normalSprite_fly);
        }
        else if (!attacking)
        {
            SetSprite(normalSprite_attack_idle);
        }


        if (moveTimer > startTime + interval * (attackCount - 1) + attackDur + endWait)
            return true;
        return false;
    }

    private bool FireFall()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        float startTime = 1f;     // 最初の攻撃開始時刻
        float interval = 0.75f;    // 一定間隔
        int attackCount = 6;      // 回数
        float attackDur = 0.3f;   // 表示継続時間
        float endWait = 2f; // 終了後の待機時間

        bool attacking = false;
        for (int i = 0; i < attackCount; i++)
        {
            float t = startTime + interval * i;
            if (moveTimer >= t && moveTimer < t + attackDur)
            {
                if (preTimer < t) FireBullet(false); // インデックスで切替例
                SetSprite(normalSprite_attack);
                attacking = true;
                break;
            }
        }

        if (startTime + interval * (attackCount - 1) + attackDur < moveTimer)
        {
            SetSprite(normalSprite_fly);
        }
        else if (!attacking)
        {
            SetSprite(normalSprite_attack_idle);
        }


        if (moveTimer > startTime + interval * (attackCount - 1) + attackDur + endWait)
            return true;
        return false;
    }
    private bool FireBoal()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        float startTime = 1f;     // 最初の攻撃開始時刻
        float interval = 1.5f;    // 一定間隔
        int attackCount = 5;      // 回数
        float attackDur = 1f;   // 表示継続時間
        float endWait = 2f; // 終了後の待機時間

        bool attacking = false;
        for (int i = 0; i < attackCount; i++)
        {
            float t = startTime + interval * i;
            if (moveTimer >= t && moveTimer < t + attackDur)
            {
                if (preTimer < t) FireBall(i % 2);
                SetSprite(normalSprite_attack);
                attacking = true;
                break;
            }
        }


        if (startTime + interval * (attackCount - 1) + attackDur < moveTimer)
        {
            SetSprite(normalSprite_fly);
        }
        else if (!attacking)
        {
            SetSprite(normalSprite_attack_idle);
        }


        if (moveTimer > startTime + interval * (attackCount - 1) + attackDur + endWait)
            return true;
        return false;
    }


    void IceBullet()
    {
        SoundManager.Instance.Play("ice");
        var bullet = Instantiate(iceBoss3_1, bulletPointIce.transform.position, Quaternion.identity, Reference.Instance.stageRect);
    }
    void FireBullet(bool isPlayerFollow)
    {
        SoundManager.Instance.Play("fire");
        var fire = Instantiate(fireBoss3_1, bulletPoint.transform.position, Quaternion.Euler(0, 0, 180), Reference.Instance.stageRect);
        fire.isPlayerFollow = isPlayerFollow;
    }
    private void FireBall(int index)
    {
        SoundManager.Instance.Play("fire");
        var point = bulletFireBallPoints[index];
        var fire = Instantiate(fireBoss3_2, point.transform.position, Quaternion.identity, Reference.Instance.stageRect);
    }

    //ふわーっと浮かび上がる
    private bool FlyMove()
    {
        SetSprite(normalSprite_fly);

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


        if (moveTimer > duration)
        {
            damageFieldObj.SetActive(true);
            return true;
        }
        return false;
    }


    //落ちる
    private bool ChangeDown()
    {
        float duration = 1f; // 移動にかける時間（秒）
        if (moveTimer == 0)
        {
            damageFieldObj.SetActive(false);
            SoundManager.Instance.Play("boss_change");
            prePos = new Vector2(pos.x, pos.y);
            targetPos = new Vector2(isRight ? rightPoint : leftPoint, downHeight);
            SetSprite(normalSprite_fall);
        }

        moveTimer += Time.deltaTime;

        // 線形補間でY座標を更新
        float t = Mathf.Clamp01(moveTimer / duration);
        pos = new Vector2(Mathf.Lerp(prePos.x, targetPos.x, t), Mathf.Lerp(prePos.y, targetPos.y, t));

        if (moveTimer > 8)
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


    int downDamage = 0;
    public override void TakeDamage(int damage, bool breakAttack, string soundName)
    {
        if (isDead) { return; }

        if (moveState == -1)
        {
            SetSprite(normalSprite_damage);
            damageWaitTime = 0.5f;
        }


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
