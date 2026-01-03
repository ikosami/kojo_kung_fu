using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss4 : Enemy
{
    [SerializeField] Sprite attackSprite1_1;  // 近接攻撃用
    [SerializeField] Sprite attackSprite1_2;  // 近接攻撃用
    [SerializeField] Sprite attackSprite2_1;  // 突進用
    [SerializeField] Sprite attackSprite2_2;  // 突進用
    [SerializeField] Sprite attackSprite3_1;  // スタンプ用
    [SerializeField] Sprite attackSprite3_2;  // スタンプ用
    [SerializeField] Sprite normalSprite_attack_idle;  // 射撃待機用
    [SerializeField] Sprite normalSprite_attack;       // 射撃攻撃用

    [SerializeField] Sprite normalSprite_attack_air_idle;
    [SerializeField] Sprite normalSprite_attack_air;       // 射撃攻撃用
    [SerializeField] Sprite missileSprite_attack;       // ミサイル攻撃用

    [SerializeField] RectTransform[] attackRange1;  // 近接攻撃用
    [SerializeField] RectTransform[] attackRangeDash;   // 突進用
    [SerializeField] RectTransform[] fallAttackRects;  // スタンプ落下用
    [SerializeField] Transform bulletPointIce;  // 射撃発射ポイント
    [SerializeField] Transform[] bulletMissileAirPoints;  // ミサイル上空発射用
    [SerializeField] Transform[] iceBulletPoints;  // ミサイル用
    [SerializeField] Transform[] bulletMissilePoints;  // ミサイル用
    [SerializeField] float[] bulletMissileHeight;  // ミサイル用

    [SerializeField] Boss4_Shoot boss4_Shoot;  // 射撃用
    [SerializeField] Boss4_Missile boss4_Missile;  // ミサイル用
    [SerializeField] Boss4_MissileAir boss4_MissileAir;  // ミサイル上空発射用
    [SerializeField] Boss4_ShockWave boss4_ShockWave;  // ショックウェーブ用
    [SerializeField] Transform boss4_ShockWavePoint;  // ショックウェーブ発射ポイント

    [SerializeField] int moveState = 0;

    [SerializeField] RectTransform deadImageRect;
    float moveTimer = 0;

    float attack2Speed = 0;
    bool isAttack2Stop = false;
    bool isDamageHit = false;
    List<Boss4_Missile> iceFallList = new List<Boss4_Missile>();

    bool isFirstAttack = true;  // 開幕攻撃かどうか
    int attackPattern = 2;  // 現在のパターン位置（②～⑥）
    int nextShootIndex = 0;  // 次の発射インデックス

    protected override void Start()
    {
        hp = maxHP;
        Reference.Instance.AddEnemy(this);
        moveTimer = 0;
        isFirstAttack = true;
        attackPattern = 2;

        // 初期位置を地面に設定
        var pos = rect.anchoredPosition;
        pos.y = BaseHeight;
        rect.anchoredPosition = pos;

        ChangeState(-1);  // 開幕スタンプ
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
            case -1:  // 待機・移動
                if (Idle())
                {
                    ChangeState(3);
                }
                break;
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

    private bool Idle()
    {
        moveTimer += Time.deltaTime;

        if (moveTimer < 1f)
        {
            return true;
        }
        return false;
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
        nextShootIndex = 0;  // 発射インデックスをリセット

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
                if (Random.Range(0, 2) == 0)
                {
                    ChangeState(5);  // ミサイル
                }
                else
                {
                    ChangeState(6);  // ミサイル上空発射
                }
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
                moveDir = moveSpeed;
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                moveDir = -moveSpeed;
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        // ボスを移動させる（Boss1、Boss2と同じくtransform.positionを使用）
        transform.position += moveDir * Time.deltaTime;
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
        float moveTime = 5;
        if (moveTimer == 0)
        {
            attack2Speed = 0;
            isAttack2Stop = false;
            isDamageHit = false;

            // プレイヤーの方向を確認してdirを設定
            var player = Reference.Instance.player;
            if (player.transform.position.x > transform.position.x)
            {
                moveDir = moveSpeed;
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                moveDir = -moveSpeed;
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else if (moveTimer < 1)
        {
            SetSprite(attackSprite2_1);
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
                    foreach (var range in attackRangeDash)
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

                transform.position += moveDir * attack2Speed * Time.deltaTime;

                var pos = rect.anchoredPosition;

                if (pos.x > 149 && moveDir.x > 0)
                {
                    pos.x = 149;
                    rect.anchoredPosition = pos;
                    moveTimer = moveTime - 1;
                    isAttack2Stop = true;
                    SoundManager.Instance.Play("boss_attack_2_2");
                }
                if (pos.x < 11 && moveDir.x < 0)
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

        moveTimer += Time.deltaTime;
        return false;
    }

    // スタンプ（Boss2のAttack3と同じ）
    private bool StampAttack()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        if (preTimer == 0 && moveTimer > 0)
        {
            // dirを更新（プレイヤーの方向に向ける）
            var player = Reference.Instance.player;
            if (player.transform.position.x > transform.position.x)
            {
                moveDir = moveSpeed;
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                moveDir = -moveSpeed;
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else if (moveTimer < 1f)
        {
            SetSprite(normalSprite1);
        }
        else if (moveTimer < 2f)
        {
            // 1秒を超えた最初のフレームで音を鳴らす
            if (preTimer < 1f)
            {
                SoundManager.Instance.Play("boss2_stamp");
            }
            SetSprite(attackSprite3_1);


            // ジャンプ: Y方向に上昇、X方向に移動（Boss2と同じ）
            transform.position += (moveDir + new Vector3(0, moveSpeed.x, 0)) * Time.deltaTime * 2.5f;
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
        }
        else if (moveTimer < 4f)
        {
            // 着地時の処理（最初のフレームのみ実行）
            if (preTimer < 2.5f)
            {
                var pos = rect.anchoredPosition;
                pos.y = BaseHeight;
                rect.anchoredPosition = pos;
                SoundManager.Instance.Play("boss2_fall");
                SpawnShockWave();
            }

            // 地面に固定
            var pos2 = rect.anchoredPosition;
            pos2.y = BaseHeight;
            rect.anchoredPosition = pos2;
        }
        else
        {
            return true;
        }
        return false;
    }

    // 射撃
    private bool ShootAttack()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        float prepareTime = 1f;        // 構え時間（1秒）
        float firstSetStart = 1f;       // 1セット目開始時刻（1秒後）
        float secondSetStart = 3f;      // 2セット目開始時刻（3秒後）
        float interval = 0.2f;         // 発射間隔（0.2秒、変更しない）
        int shotsPerSet = 4;           // 1セットあたりの発射数（4発）
        int totalShots = 8;            // 合計発射数（2セット × 4発）
        float attackAnimDur = 2f;      // 攻撃アニメーション表示時間（2秒）
        float endWait = 0.5f;          // 終了後の待機時間

        // 次の発射タイミングをチェック
        if (nextShootIndex < totalShots)
        {
            float t;
            if (nextShootIndex < shotsPerSet)
            {
                // 1セット目（0-3）：1秒、1.2秒、1.4秒、1.6秒
                t = firstSetStart + interval * nextShootIndex;
            }
            else
            {
                // 2セット目（4-7）：3秒、3.2秒、3.4秒、3.6秒
                int set2Index = nextShootIndex - shotsPerSet;
                t = secondSetStart + interval * set2Index;
            }

            if (moveTimer >= t && preTimer < t)
            {
                SpawnShootProjectile();
                nextShootIndex++;
            }
        }

        // スプライト制御
        // 構え中（0-1秒）
        if (moveTimer < prepareTime)
        {
            SetSprite(normalSprite_attack_idle);
        }
        // 1セット目攻撃アニメーション（1秒-3秒、2秒間）
        else if (moveTimer >= firstSetStart && moveTimer < firstSetStart + attackAnimDur)
        {
            SetSprite(normalSprite_attack);
        }
        // 1セット目と2セット目の間（3秒未満で1セット目終了後）
        else if (moveTimer >= firstSetStart + attackAnimDur && moveTimer < secondSetStart)
        {
            SetSprite(normalSprite_attack_idle);
        }
        // 2セット目攻撃アニメーション（3秒-5秒、2秒間）
        else if (moveTimer >= secondSetStart && moveTimer < secondSetStart + attackAnimDur)
        {
            SetSprite(normalSprite_attack);
        }
        // 全ての発射が終了し、攻撃スプライト表示も終了
        else if (moveTimer >= secondSetStart + attackAnimDur)
        {
            SetSprite(normalSprite1);
        }
        // その他（念のため）
        else
        {
            SetSprite(normalSprite_attack_idle);
        }

        // 終了判定（2セット目のアニメーション終了 + 待機時間）
        float totalTime = secondSetStart + attackAnimDur + endWait;
        if (moveTimer > totalTime)
            return true;
        return false;
    }

    // ミサイルを1発発射する
    private void SpawnMissile(int pointIndex)
    {
        if (pointIndex >= bulletMissilePoints.Length) return;

        SoundManager.Instance.Play("ice");

        var player = Reference.Instance.player;
        // プレイヤーの方向を判定（プレイヤーが左側にいる場合は-1、右側にいる場合は1）
        var direction = player.transform.position.x >= transform.position.x ? 1f : -1f;

        Transform missilePoint = bulletMissilePoints[pointIndex];
        var iceObj = Instantiate(boss4_Missile, missilePoint.transform.position, Quaternion.identity, Reference.Instance.stageRect);

        // 目標高さを設定
        if (pointIndex < bulletMissileHeight.Length)
        {
            iceObj.SetFloor(bulletMissileHeight[pointIndex]);
        }

        // プレイヤーが左側にいる場合は左向きに設定
        if (direction < 0)
        {
            iceObj.SetLeft();
        }

        iceFallList.Add(iceObj);
        SetSprite(missileSprite_attack);
    }

    // ミサイル（時間差で2発発射）
    private bool MissileAttack()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        // 構え（0秒）
        if (moveTimer < 1f)
        {
            if (preTimer == 0)
            {
                SetSprite(normalSprite2);
            }
        }
        // 1発目発射（1秒）
        else if (moveTimer >= 1f && preTimer < 1f)
        {
            SpawnMissile(0);
        }
        // 2発目発射（3秒）
        else if (moveTimer >= 3f && preTimer < 3f)
        {
            SpawnMissile(1);
        }
        // 通常スプライトに戻す（発射後すぐ）
        else if (moveTimer > 1.2f && moveTimer < 3f)
        {
            SetSprite(normalSprite2);
        }
        else if (moveTimer > 3.2f && moveTimer < 5f)
        {
            SetSprite(normalSprite2);
        }

        // 次の行動へ（5秒）
        if (moveTimer >= 5f)
        {
            iceFallList.Clear();
            return true;
        }
        return false;
    }

    // ミサイル上空発射
    private bool MissileAirAttack()
    {
        float preTimer = moveTimer;
        moveTimer += Time.deltaTime;

        float waitTime = 1f;        // 発射前の待機時間
        float attackDur = 0.5f;     // 攻撃スプライト表示時間（0.5秒後にidleに戻す）
        float idleWait = 1f;        // 発射待機スプライト表示時間
        float endWait = 2f;         // 終了後の待機時間

        if (moveTimer < waitTime)
        {
            // 発射待機
            SetSprite(normalSprite_attack_air_idle);
        }
        else if (moveTimer < waitTime + attackDur)
        {
            // 発射タイミング（最初のフレームのみ発射）
            if (preTimer < waitTime)
            {
                // 2発同時発射
                SpawnAerialMissile(0);
                SpawnAerialMissile(1);
            }
            SetSprite(normalSprite_attack_air);
        }
        else if (moveTimer < waitTime + attackDur + idleWait)
        {
            // 0.5秒後にidleスプライトに戻す
            SetSprite(normalSprite_attack_air_idle);
        }
        else if (moveTimer < waitTime + attackDur + idleWait + endWait)
        {
            // 通常ポーズ
            SetSprite(normalSprite1);
        }
        else
        {
            // 行動終了
            return true;
        }
        return false;
    }

    void SpawnShootProjectile()
    {
        SoundManager.Instance.Play("ice");
        var bullet = Instantiate(boss4_Shoot, bulletPointIce.transform.position, Quaternion.identity, Reference.Instance.stageRect);
        var player = Reference.Instance.player;
        var direction = player.transform.position.x >= transform.position.x ? 1f : -1f;
        var bossScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(bossScale.x) * direction, bossScale.y, bossScale.z);
        bullet.move = new Vector2(Mathf.Abs(bullet.move.x) * direction, bullet.move.y);
        bullet.fallMove = new Vector2(Mathf.Abs(bullet.fallMove.x) * direction, bullet.fallMove.y);
        var bulletScale = bullet.transform.localScale;
        bullet.transform.localScale = new Vector3(Mathf.Abs(bulletScale.x) * direction, bulletScale.y, bulletScale.z);
    }

    // ショックウェーブを生成（1つのクラスで左右を制御）
    void SpawnShockWave()
    {
        Instantiate(boss4_ShockWave, boss4_ShockWavePoint.position, Quaternion.identity, Reference.Instance.stageRect);
    }

    private void SpawnAerialMissile(int index)
    {
        SoundManager.Instance.Play("fire");
        var point = bulletMissileAirPoints[index];
        var fire = Instantiate(boss4_MissileAir, point.transform.position, Quaternion.identity, Reference.Instance.stageRect);
        var player = Reference.Instance.player;
        var direction = player.transform.position.x >= transform.position.x ? 1f : -1f;
        var bossScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(bossScale.x) * direction, bossScale.y, bossScale.z);
        fire.move = new Vector2(Mathf.Abs(fire.move.x) * direction, fire.move.y);
        var fireScale = fire.transform.localScale;
        fire.transform.localScale = new Vector3(Mathf.Abs(fireScale.x) * direction, fireScale.y, fireScale.z);
    }

    public override void TakeDamage(int damage, bool breakAttack, string soundName)
    {
        if (isDead) { return; }

        //image.sprite = damageSprite;
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
        deadImageRect.gameObject.SetActive(true);
        // deadImageRectの位置を現在の位置に設定
        deadImageRect.position = transform.position;

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
            Vector3 newPosition = deadImageRect.position + velocity * Time.deltaTime;
            deadImageRect.position = newPosition;

            yield return null;
        }

        Reference.Instance.StageComplete(BossNum);
        yield return new WaitForSeconds(2f); // 少し待機
        Reference.Instance.player.MoveEnd();
    }

    [SerializeField] int BossNum = 1;
}
