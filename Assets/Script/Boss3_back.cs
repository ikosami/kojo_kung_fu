using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class Boss3_back : Enemy
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
    [SerializeField] Transform bulletPoint2;


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

    [SerializeField] FireBoss3_1 fireBoss3;
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
        hp = maxHP;
        Reference.Instance.AddEnemy(this);
        preState = 0;
        moveTimer = 0;
    }
    // 復活回数
    int reviveCount = 0;
    bool firstFire = true;
    protected override void Update()
    {
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        if (isDead) { return; }

        switch (moveState)
        {
            case -1://ダウン状態
                if (ChangeDown())
                {
                    reviveCount++;
                    if (reviveCount == 1)
                    {
                        // 初回復活は青確定
                        ChangeState(2);
                    }
                    else
                    {
                        // 2回目以降は赤青ランダム
                        if (Random.Range(0, 2) == 0)
                        {
                            ChangeState(1); // 赤変化へ
                        }
                        else
                        {
                            ChangeState(2); // 青変化へ
                        }
                    }
                }
                break;
            case 0:
                moveTimer += Time.deltaTime;
                if (moveTimer > 2)
                {
                    ChangeState(1); // ②赤変化へ
                }
                break;
            case 1://赤変化
                if (ChangeFire(fireSprite_1))
                {
                    preState = 1;
                    ChangeState(3);
                }
                break;
            case 2://青変化
                if (ChangeFire(iceSprite_1))
                {
                    preState = 2;
                    ChangeState(6);
                }
                break;
            case 3:
                if (FireBoal())
                {
                    if (firstFire)
                    {
                        // 初回は⑤火炎車確定
                        firstFire = false;
                        ChangeState(4);
                    }
                    else
                    {
                        // 2回目以降は④か⑤へランダム
                        if (Random.Range(0, 2) == 0)
                        {
                            ChangeState(3); // ④火の粉（連続）
                            firstFire = true;//無限に火の粉になってしまうため　次は火炎車
                        }
                        else
                        {
                            ChangeState(4); // ⑤火炎車
                        }
                    }
                }
                break;
            case 4:
                if (FireWheel())
                {
                    ChangeState(2);
                }
                break;
            case 5:
                if (IceBlock())
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        ChangeState(5); // 3発目（継続）
                    }
                    else
                    {
                        ChangeState(6); // つらら落としへ
                    }
                }
                break;
            case 6:
                if (IceFall())
                {
                    ChangeState(1);
                }
                break;
        }
    }

    // 追加フィールド（クラス内）
    List<float> plannedIceXs = new List<float>();
    private bool IceFall()
    {

        float duration = 3f; // 移動にかける時間（秒）
        if (moveTimer == 0)
        {
            PlanIceFallPoints();
            usedPlayerFollow = false;
            SetSprite(iceSprite_2);
            prePos = new Vector2(pos.x, pos.y);

            if (isRight)
            {
                targetPos = new Vector2(leftPoint, pos.y);
            }
            else
            {
                targetPos = new Vector2(rightPoint, pos.y);
            }
        }
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        // 攻撃＆表示を同一箇所で処理
        float attackTime1 = 0.5f;
        float attackTime2 = 1.2f;
        float attackTime3 = 1.5f;

        if (moveTimer >= attackTime1 && preTimer < attackTime1)
        {
            IceBullet2();
        }
        else if (moveTimer >= attackTime2 && preTimer < attackTime2)
        {
            IceBullet2();
        }
        else if (moveTimer >= attackTime3 && preTimer < attackTime3)
        {
            IceBullet2();
        }
        if (plannedIceXs.Count > 1 && !usedPlayerFollow)
        {
            float playerX = Reference.Instance.player.transform.position.x;
            float nextFixedX = plannedIceXs[0];

            bool isOver = false;
            if (isRight)
            {
                // 左に進行中（小さい方へ）
                if (playerX > nextFixedX)
                {
                    isOver = true;
                }
            }
            else
            {
                // 右に進行中（大きい方へ）
                if (playerX < nextFixedX)
                {
                    isOver = true;
                }
            }
            if (isOver)
            {

                usedPlayerFollow = true;

                for (int i = 0; i < plannedIceXs.Count; i++)
                {
                    plannedIceXs[0] = playerX;
                    float ice = plannedIceXs[i];
                    // -999はプレイヤー追従マーク
                    if (Mathf.Approximately(ice, -999f))
                    {
                        plannedIceXs.RemoveAt(i);
                        var offset = (isRight ? -1f : 1f) * Random.Range(50, 150);
                        var pos = plannedIceXs[plannedIceXs.Count - 1] + offset;
                        pos = Mathf.Clamp(pos, iceBulletPoints[0].transform.position.x, iceBulletPoints[1].transform.position.x);
                        plannedIceXs.Add(pos);
                    }
                }
            }
        }


        // 線形補間でY座標を更新
        if (targetPos.x != 0)
        {
            float t = Mathf.Clamp01(moveTimer / duration);
            t = Mathf.Clamp01(t);
            float newY = Mathf.Lerp(prePos.x, targetPos.x, t);
            pos = new Vector2(newY, pos.y);
            if (moveTimer > duration)
            {
                targetPos.x = 0;
                foreach (var ice in iceFallList)
                {
                    if (ice != null)
                    {
                        ice.StartFall();
                    }
                }
                iceFallList.Clear();
                SetSprite(iceSprite_1);
            }
        }

        if (moveTimer > 8f)
        {
            isRight = !isRight;
            transform.localScale = new Vector3(isRight ? -1 : 1, 1, 1);

            return true;
        }
        return false;
    }
    // 追加メソッド（クラス内）
    void PlanIceFallPoints()
    {
        plannedIceXs.Clear();

        // 0,1のワールドXで区間を決定
        Vector3 p0 = iceBulletPoints[0].position;
        Vector3 p1 = iceBulletPoints[1].position;
        float minX = Mathf.Min(p0.x, p1.x);
        float maxX = Mathf.Max(p0.x, p1.x);

        // 3点：2つランダム + 1つプレイヤー追従(-999でマーク)
        float r1 = UnityEngine.Random.Range(minX, maxX);
        float r2 = UnityEngine.Random.Range(minX, maxX);
        float followMark = -999f;

        float playerXNow = Reference.Instance.player.transform.position.x;

        var temp = new List<(float sortKey, float value)>
    {
        (r1, r1),
        (r2, r2),
        (playerXNow, followMark)
    };

        // 右へ進行: 昇順 / 左へ進行: 降順（isRightなら左へ=降順）
        bool toRight = (!isRight);
        temp.Sort((a, b) => toRight ? a.sortKey.CompareTo(b.sortKey) : b.sortKey.CompareTo(a.sortKey));

        foreach (var t in temp) plannedIceXs.Add(t.value);
    }

    private bool IceBlock()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        // 攻撃＆表示を同一箇所で処理
        float attackTime1 = 1f;
        float attackTime2 = 1.5f;
        float attackTime3 = 2f;
        float attackDur = 0.3f; // 表示継続時間

        if (moveTimer >= attackTime1 && moveTimer < attackTime1 + attackDur)
        {
            if (preTimer < attackTime1) IceBullet();
            SetSprite(iceSprite_2);
        }
        else if (moveTimer >= attackTime2 && moveTimer < attackTime2 + attackDur)
        {
            if (preTimer < attackTime2) IceBullet();
            SetSprite(iceSprite_2);
        }
        else if (moveTimer >= attackTime3 && moveTimer < attackTime3 + attackDur)
        {
            if (preTimer < attackTime3) IceBullet();
            SetSprite(iceSprite_2);
        }
        else
        {
            SetSprite(iceSprite_1);
        }

        if (moveTimer > 8f) return true;
        return false;
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
            if (preTimer < attackTime1) FireBullet(true);
            SetSprite(fireSprite_2);
        }
        else if (moveTimer >= attackTime2 && moveTimer < attackTime2 + attackDur)
        {
            if (preTimer < attackTime2) FireBullet(false);
            SetSprite(fireSprite_2);
        }
        else
        {
            SetSprite(fireSprite_1);
        }

        if (moveTimer > 8f) return true;
        return false;
    }


    bool usedPlayerFollow = false;
    void IceBullet2()
    {

        // 進行順の1点を取り出し
        float x = plannedIceXs[0];
        plannedIceXs.RemoveAt(0);
        // プレイヤー追従なら現在のXを採用
        if (Mathf.Approximately(x, -999f))
        {
            if (usedPlayerFollow) return;
            x = Reference.Instance.player.transform.position.x;
            usedPlayerFollow = true;
        }


        SoundManager.Instance.Play("ice");
        Vector3 p = bulletPoint2.transform.position; // Y/Zは既存のbulletPoint2に合わせる
        p.x = x;

        var ice = Instantiate(iceBoss3_2, p, Quaternion.identity, Reference.Instance.stageRect);
        iceFallList.Add(ice);
    }
    void IceBullet()
    {
        SoundManager.Instance.Play("ice");
        var bullet = Instantiate(iceBoss3_1, bulletPoint.transform.position, Quaternion.identity, Reference.Instance.stageRect);
        bullet.transform.localScale = new Vector3(isRight ? -1 : 1, 1, 1);
        bullet.fallMove.x *= (isRight ? 1 : -1);
    }

    void FireBullet(bool isPlayerFollow)
    {
        SoundManager.Instance.Play("fire");
        var fire = Instantiate(fireBoss3, bulletPoint.transform.position, Quaternion.Euler(0, 0, 180), Reference.Instance.stageRect);
        fire.isPlayerFollow = isPlayerFollow;
    }


    //ふわーっと浮かび上がる
    private bool ChangeDown()
    {
        float duration = 1f; // 移動にかける時間（秒）
        if (moveTimer == 0)
        {
            SoundManager.Instance.Play("boss_change");
            prePos = new Vector2(pos.x, pos.y);
            targetPos = new Vector2(isRight ? rightPoint : leftPoint, downHeight);
        }

        moveTimer += Time.deltaTime;

        // 線形補間でY座標を更新
        float t = Mathf.Clamp01(moveTimer / duration);
        pos = new Vector2(Mathf.Lerp(prePos.x, targetPos.x, t), Mathf.Lerp(prePos.y, targetPos.y, t));


        SetSprite(normalSprite_2);

        if (moveTimer > 4)
        {
            return true;
        }
        return false;
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


    int downDamage = 0;
    public override void TakeDamage(int damage, bool breakAttack, string soundName)
    {
        if (isDead) { return; }

        if (moveState != -1)
        {
            downDamage += damage;
            if (downDamage > 10)
            {
                ChangeState(-1);
            }
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
