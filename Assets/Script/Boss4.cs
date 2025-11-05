using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss4 : Enemy, ICharacter
{
    [SerializeField] Sprite attackSprite1_1;  // 近接攻撃用
    [SerializeField] Sprite attackSprite1_2;  // 近接攻撃用
    [SerializeField] Sprite attackSprite2_1;  // 突進用
    [SerializeField] Sprite attackSprite2_2;  // 突進用
    [SerializeField] Sprite attackSprite3_1;  // スタンプ用
    [SerializeField] Sprite attackSprite3_2;  // スタンプ用
    [SerializeField] Sprite normalSprite_attack_idle;  // 射撃待機用
    [SerializeField] Sprite normalSprite_attack;       // 射撃攻撃用

    [SerializeField] RectTransform[] attackRange1;  // 近接攻撃用
    [SerializeField] new RectTransform[] attackRange2;   // 突進用
    [SerializeField] RectTransform[] fallAttackRects;  // スタンプ落下用

    [SerializeField] Transform bulletPointIce;  // 射撃発射ポイント
    [SerializeField] Transform[] bulletFireBallPoints;  // ミサイル上空発射用
    [SerializeField] Transform[] iceBulletPoints;  // ミサイル用

    [SerializeField] Boss4_Shoot boss4_Shoot;  // 射撃用
    [SerializeField] Boss4_Missile boss4_Missile;  // ミサイル用
    [SerializeField] Boss4_MissileAir boss4_MissileAir;  // ミサイル上空発射用

    [SerializeField] int moveState = 0;
    float moveTimer = 0;

    new Vector3 dir;
    float attack2Speed = 0;
    bool isAttack2Stop = false;
    bool isDamageHit = false;
    List<Boss4_Missile> iceFallList = new List<Boss4_Missile>();

    bool isFirstAttack = true;  // 開幕攻撃かどうか
    int attackPattern = 2;  // 現在のパターン位置（②～⑥）

    protected override void Start()
    {
        hp = hpMax;
        Reference.Instance.enemyList.Add(this);
        moveTimer = 0;
        isFirstAttack = true;
        attackPattern = 2;
        
        // 初期位置を地面に設定
        var pos = rect.anchoredPosition;
        pos.y = BaseHeight;
        rect.anchoredPosition = pos;
        
        ChangeState(3);  // 開幕スタンプ
    }

    protected override void Update()
    {
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        if (isDead) { return; }

        if (Damaging())
        {
            return;
        }

        switch (moveState)
        {
            case 0:  // 待機・移動
                if (IdleMove())
                {
                    NextPattern();
                }
                break;
            case 1:  // 近接攻撃
                if (MeleeAttack())
                {
                    NextPattern();
                }
                break;
            case 2:  // 突進
                if (DashAttack())
                {
                    NextPattern();
                }
                break;
            case 3:  // スタンプ
                if (StampAttack())
                {
                    StampAttackFinish();
                }
                break;
            case 4:  // 射撃
                if (ShootAttack())
                {
                    ShootAttackFinish();
                }
                break;
            case 5:  // ミサイル
                if (MissileAttack())
                {
                    NextPattern();
                }
                break;
            case 6:  // ミサイル上空発射
                if (MissileAirAttack())
                {
                    NextPattern();
                }
                break;
        }
    }

    protected override bool Damaging()
    {
        if (damageWaitTime > 0)
        {
            damageWaitTime -= Time.deltaTime;
            if (damageWaitTime <= 0)
            {
                image.sprite = isNormalSprite ? normalSprite1 : normalSprite2;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    // プレイヤーが近くにいるか判定
    bool IsPlayerNear()
    {
        var player = Reference.Instance.player;
        float distance = Mathf.Abs(player.transform.position.x - transform.position.x);
        return distance < 80f;  // 距離80未満なら近い
    }

    // 次のパターンに進む
    void NextPattern()
    {
        switch (attackPattern)
        {
            case 2:  // ②近接攻撃 → ③へ
                attackPattern = 3;
                // ③ミサイル 又は ミサイル上空発射
                if (Random.Range(0, 2) == 0)
                {
                    ChangeState(5);  // ミサイル
                }
                else
                {
                    ChangeState(6);  // ミサイル上空発射
                }
                break;
            case 3:  // ③ミサイル/ミサイル上空発射 → ④へ
                attackPattern = 4;
                // ④突進 又は スタンプ
                if (Random.Range(0, 2) == 0)
                {
                    ChangeState(2);  // 突進
                }
                else
                {
                    ChangeState(3);  // スタンプ
                }
                break;
            case 4:  // ④突進/スタンプ → ⑤へ
                attackPattern = 5;
                // ⑤PLが近くにいる場合は近接攻撃、遠い場合はスタンプ
                if (IsPlayerNear())
                {
                    ChangeState(1);  // 近接攻撃
                }
                else
                {
                    ChangeState(3);  // スタンプ
                }
                break;
            case 5:  // ⑤近接攻撃/スタンプ → ⑥へ
                attackPattern = 6;
                ChangeState(4);  // ⑥射撃
                break;
            case 6:  // ⑥射撃 → ②へ（これはcase 4で処理される）
                attackPattern = 2;
                ChangeState(1);  // ②近接攻撃
                break;
        }
    }

    void ChangeState(int state)
    {
        moveState = state;
        moveTimer = 0;
        isAttackDamage = true;
        isAttack2Stop = false;
        isDamageHit = false;
        attack2Speed = 0;
    }

    // スタンプ攻撃終了処理
    void StampAttackFinish()
    {
        if (isFirstAttack)
        {
            // 開幕後：PLが近くにいる場合②へ、居ない場合は③へ
            if (IsPlayerNear())
            {
                attackPattern = 2;  // ②近接攻撃
                ChangeState(1);
            }
            else
            {
                attackPattern = 3;  // ③ミサイルまたはミサイル上空発射
                NextPattern();
            }
            isFirstAttack = false;
        }
        else
        {
            NextPattern();
        }
    }

    // 射撃攻撃終了処理
    void ShootAttackFinish()
    {
        // ⑥射撃 → ②へ
        attackPattern = 2;
        ChangeState(1);  // ②近接攻撃
    }

    // 待機・移動処理
    private bool IdleMove()
    {
        Move();
        HandleNormalSpriteAnimation();
        return MoveAndCheckAttack();
    }

    // 移動と攻撃開始判定
    private bool MoveAndCheckAttack()
    {
        var player = Reference.Instance.player;
        // プレイヤーが攻撃範囲に入った場合、攻撃を開始
        if (Mathf.Abs(player.transform.position.x - transform.position.x) < 80)
        {
            return true;
        }
        return false;
    }

    // 移動処理
    protected override void Move()
    {
        var pos = rect.anchoredPosition;

        // ボスが空中にいる場合、重力を適用して落下させる
        if (pos.y > BaseHeight)
        {
            currentJumpVelocity -= gravity * Time.deltaTime;
            pos.y += currentJumpVelocity * Time.deltaTime;
            rect.anchoredPosition = pos;

            // ボスが地面に到達した場合、位置を修正し、ジャンプ速度をリセットする
            if (pos.y <= BaseHeight)
            {
                pos.y = BaseHeight;
                rect.anchoredPosition = pos;
                currentJumpVelocity = 0;
            }
            return;
        }

        var player = Reference.Instance.player;

        // ボスが地面にいる場合、プレイヤーの位置に応じて移動方向を決定する
        if (pos.y <= BaseHeight)
        {
            // 地面に固定（毎フレーム実行）
            pos.y = BaseHeight;
            rect.anchoredPosition = pos;
            
            if (player.transform.position.x > transform.position.x)
            {
                dir = moveSpeed;
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                dir = -moveSpeed;
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        // ボスを移動させる（Boss1、Boss2と同じくtransform.positionを使用）
        transform.position += dir * Time.deltaTime;
    }

    // 近接攻撃（Boss1のAttack1と同じ）
    private bool MeleeAttack()
    {
        // 近接攻撃中は移動しない（Boss1と同じ）
        moveTimer += Time.deltaTime;

        if (moveTimer < 1f)
        {
            SetSprite(attackSprite1_1);
        }
        else if (moveTimer < 2f)
        {
            if (isAttackDamage)
            {
                foreach (var range in attackRange1)
                {
                    if (Util.IsHitPlayer(range))
                    {
                        Reference.Instance.player.TakeDamage(1);
                        break;
                    }
                }
                SoundManager.Instance.Play("boss_attack_1");
                isAttackDamage = false;
            }
            SetSprite(attackSprite1_2);
        }
        else if (moveTimer < 3f)
        {
            SetSprite(normalSprite1);
        }
        else
        {
            return true;
        }
        return false;
    }

    // 突進（Boss1のAttack2と同じ）
    private bool DashAttack()
    {
        moveTimer += Time.deltaTime;

        float moveTime = 5;

        if (moveTimer < 1)
        {
            SetSprite(attackSprite2_1);
            attack2Speed = 0;
            isAttack2Stop = false;
            isDamageHit = false;
        }
        else if (moveTimer < moveTime)
        {
            if (isAttackDamage)
            {
                SoundManager.Instance.Play("boss_attack_2_1");
                isAttackDamage = false;
            }
            SetSprite(attackSprite2_2);

            if (!isAttack2Stop)
            {
                if (!isDamageHit)
                {
                    foreach (var range in attackRange2)
                    {
                        if (Util.IsHitPlayer(range))
                        {
                            Reference.Instance.player.TakeDamage(1, false, "boss_attack_2_2");
                            isDamageHit = true;
                            break;
                        }
                    }
                }

                attack2Speed += Time.deltaTime * 3;
                transform.position += dir * attack2Speed * Time.deltaTime;

                var pos = rect.anchoredPosition;

                if (pos.x > 149 && dir.x > 0)
                {
                    pos.x = 149;
                    rect.anchoredPosition = pos;
                    moveTimer = moveTime - 1;
                    isAttack2Stop = true;
                    SoundManager.Instance.Play("boss_attack_2_2");
                }
                if (pos.x < 11 && dir.x < 0)
                {
                    pos.x = 11;
                    rect.anchoredPosition = pos;
                    moveTimer = moveTime - 1;
                    isAttack2Stop = true;
                    SoundManager.Instance.Play("boss_attack_2_2");
                }
            }
        }
        else if (moveTimer < 6)
        {
            SetSprite(normalSprite1);
        }
        else
        {
            return true;
        }
        return false;
    }

    // スタンプ（Boss2のAttack3と同じ）
    private bool StampAttack()
    {
        if (moveTimer < 1f)
        {
            SetSprite(normalSprite1);
            moveTimer += Time.deltaTime;
            if (moveTimer >= 1)
            {
                SoundManager.Instance.Play("boss2_stamp");
            }
        }
        else if (moveTimer < 2f)
        {
            SetSprite(attackSprite3_1);
            
            // dirを更新（プレイヤーの方向に向ける）
            var player = Reference.Instance.player;
            if (player.transform.position.x > transform.position.x)
            {
                dir = moveSpeed;
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                dir = -moveSpeed;
                transform.localScale = new Vector3(-1, 1, 1);
            }
            
            // ジャンプ: Y方向に上昇、X方向に移動（Boss2と同じ）
            transform.position += (dir + new Vector3(0, moveSpeed.x, 0)) * Time.deltaTime * 2.5f;
            moveTimer += Time.deltaTime;
        }
        else if (moveTimer < 2.5f)
        {
            SetSprite(attackSprite3_2);

            if (isAttackDamage)
            {
                foreach (var attackRange in fallAttackRects)
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

            // 落下: Y方向に下降（Boss2と同じ）
            transform.position += new Vector3(0, -moveSpeed.x, 0) * Time.deltaTime * 5f;
            moveTimer += Time.deltaTime;
            
            if (moveTimer >= 2.5)
            {
                var pos = rect.anchoredPosition;
                pos.y = BaseHeight;
                rect.anchoredPosition = pos;
                SoundManager.Instance.Play("boss2_fall");
            }
        }
        else if (moveTimer < 4f)
        {
            // 地面に固定
            var pos = rect.anchoredPosition;
            pos.y = BaseHeight;
            rect.anchoredPosition = pos;
            moveTimer += Time.deltaTime;
        }
        else
        {
            return true;
        }
        return false;
    }

    // 射撃（Boss3のIceBlockと同じ）
    private bool ShootAttack()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        float startTime = 1f;     // 最初の攻撃開始時刻
        float interval = 2f;       // 一定間隔
        int attackCount = 4;       // 回数
        float attackDur = 1f;      // 表示継続時間
        float endWait = 2f;        // 終了後の待機時間

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
            SetSprite(normalSprite1);
        }
        else if (!attacking)
        {
            SetSprite(normalSprite_attack_idle);
        }

        if (moveTimer > startTime + interval * (attackCount - 1) + attackDur + endWait)
            return true;
        return false;
    }

    // ミサイル（Boss3のIceFallと同じ）
    private bool MissileAttack()
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
                var iceObj = Instantiate(boss4_Missile, ice.transform.position, Quaternion.identity, Reference.Instance.stageRect);
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
            SetSprite(normalSprite1);
        }

        if (moveTimer > 8f)
        {
            return true;
        }
        return false;
    }

    // ミサイル上空発射（Boss3のFireBoalと同じ）
    private bool MissileAirAttack()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        float startTime = 1f;     // 最初の攻撃開始時刻
        float interval = 1.5f;     // 一定間隔
        int attackCount = 5;       // 回数
        float attackDur = 1f;      // 表示継続時間
        float endWait = 2f;        // 終了後の待機時間

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
            SetSprite(normalSprite1);
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
        var bullet = Instantiate(boss4_Shoot, bulletPointIce.transform.position, Quaternion.identity, Reference.Instance.stageRect);
    }

    private void FireBall(int index)
    {
        SoundManager.Instance.Play("fire");
        var point = bulletFireBallPoints[index];
        var fire = Instantiate(boss4_MissileAir, point.transform.position, Quaternion.identity, Reference.Instance.stageRect);
    }

    public override void TakeDamage(int damage, bool breakAttack, string soundName)
    {
        if (isDead) { return; }

        image.sprite = damageSprite;
        damageWaitTime = 0.5f;

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
