using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Boss2 : Enemy, ICharacter
{
    [SerializeField] Sprite attackSprite2_1;
    [SerializeField] Sprite attackSprite2_2;
    [SerializeField] Sprite attackSprite3_1;
    [SerializeField] Sprite attackSprite3_2;

    [SerializeField] Bullet bulletPrefab;
    [SerializeField] Transform bulletPoint;

    [SerializeField] RectTransform[] fallAttackRects;

    [SerializeField] int attackKind = 0;
    [SerializeField] int attackState = 1;

    protected override void Start()
    {
        hp = hpMax;
        Reference.Instance.enemyList.Add(this);

        attackState = 1;
        SetAttackKind();
    }

    protected override void Update()
    {
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        if (isDead) { return; }

        Move();
        HandleAttack();
        HandleNormalSpriteAnimation();

    }

    protected override void HandleAttack()
    {
        if (!isAttack) { return; }

        switch (attackKind)
        {
            case 1:
                Attack1();
                break;
            case 2:
                Attack2();
                break;
            case 3:
                Attack3();
                break;
        }
    }


    private void Attack1()
    {
        attackTime += Time.deltaTime;

        if (attackTime < 1f)
        {
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
        }
        else if (attackTime < 2f)
        {
            if (isAttackDamage)
            {
                if (Util.IsHitPlayer(attackRange))
                {
                    Reference.Instance.player.TakeDamage(1);
                }

                SoundManager.Instance.Play("boss_attack_1");
                isAttackDamage = false;
            }
            if (image.sprite != attackSprite1)
                image.sprite = attackSprite1;
        }
        else if (attackTime < 3f)
        {
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
        }
        else
        {
            if (attackState == 1)
            {
                attackState = 2;
            }
            SetAttackKind();
            isAttack = false;
            spriteChangeTimer = 0;
        }
    }
    private void Attack2()
    {
        attackTime += Time.deltaTime;

        if (attackTime < 1f)
        {
            // 攻撃前の溜め
            if (image.sprite != attackSprite2_1)
                image.sprite = attackSprite2_1;
        }
        else if (attackTime < 2f)
        {
            // 攻撃発動
            if (isAttackDamage)
            {
                SoundManager.Instance.Play("throwing");

                var bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.identity, Reference.Instance.stageRect);
                bullet.move.x *= transform.localScale.x;

                var scale = bullet.transform.localScale;
                scale.x = transform.localScale.x;
                bullet.transform.localScale = scale;
                isAttackDamage = false;
            }
            if (image.sprite != attackSprite2_2)
                image.sprite = attackSprite2_2;
        }
        else if (attackTime < 3f)
        {
            // 攻撃後の戻り
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
        }
        else
        {

            if (attackState == 1)//接近してパンチ
            {
                attackState = 2;//ジャンプしてスタンプ攻撃
            }
            else if (attackState == 4)
            {
                Vector3 diff = Reference.Instance.player.transform.position - transform.position;

                if (diff.x * dir.x > 0)
                {
                    // 正面
                    attackState = 5;
                }
                else if (diff.x * dir.x < 0)
                {
                    attackState = 6;
                }
            }
            else if (attackState == 5)
            {
                attackState = 6;
            }
            SetAttackKind();


            // 攻撃終了
            isAttack = false;
            spriteChangeTimer = 0;
        }
    }
    private void Attack3()
    {

        if (attackTime < 1f)
        {
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
            attackTime += Time.deltaTime;
            if (attackTime >= 1)
            {
                SoundManager.Instance.Play("boss2_stamp");
            }
        }
        else if (attackTime < 2f)
        {
            if (image.sprite != attackSprite3_1)
                image.sprite = attackSprite3_1;

            transform.position += (dir + new Vector3(0, moveSpeed.x, 0)) * Time.deltaTime * 2.5f; // 突進の移動速度を上げる

            attackTime += Time.deltaTime;
        }
        else if (attackTime < 2.5f)
        {
            if (image.sprite != attackSprite3_2)
                image.sprite = attackSprite3_2;

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

            transform.position += new Vector3(0, -moveSpeed.x, 0) * Time.deltaTime * 5f; // 突進の移動速度を上げる

            attackTime += Time.deltaTime;
            if (attackTime >= 2.5)
            {
                var pos = rect.anchoredPosition;
                pos.y = BaseHeight;
                rect.anchoredPosition = pos;
                SoundManager.Instance.Play("boss2_fall");
            }
        }
        else if (attackTime < 4f)
        {
            attackTime += Time.deltaTime;
        }
        else
        {
            if (attackState == 2)
            {
                attackState = Random.Range(3, 5);
            }
            else if (attackState == 3)//もう一度スタンプ攻撃
            {
                attackState = 4;
            }
            else if (attackState == 4)
            {
                attackState = 5;
            }
            if (attackState == 6)
            {
                attackState = Random.Range(1, 5);
            }
            SetAttackKind();

            isAttack = false;
            spriteChangeTimer = 0;
        }
    }

    public void SetAttackKind()
    {
        switch (attackState)
        {
            case 1:
                attackKind = 1;
                break;
            case 2:
                attackKind = 3;
                break;
            case 3:
                attackKind = 3;
                break;
            case 4:
                attackKind = 2;
                break;
            case 5:
                attackKind = 2;
                break;
            case 6:
                attackKind = 3;
                break;
        }

    }

    bool isFirstFall = true;

    /// <summary>
    /// ボスの移動を制御する関数
    /// </summary>
    protected override void Move()
    {
        var pos = rect.anchoredPosition;

        if (isFirstFall)
        {
            // ボスが空中にいる場合、重力を適用して落下させる
            if (pos.y > floorHeight)
            {
                currentJumpVelocity -= gravity * Time.deltaTime;
                pos.y += currentJumpVelocity * Time.deltaTime;
                rect.anchoredPosition = pos;

                // ボスが地面に到達した場合、位置を修正し、ジャンプ速度をリセットする
                if (pos.y <= floorHeight)
                {
                    pos.y = floorHeight;
                    rect.anchoredPosition = pos;
                    currentJumpVelocity = 0;
                    isFirstFall = false;
                }
                return;
            }
        }

        // 攻撃中は移動しない
        if (isAttack) { return; }

        var player = Reference.Instance.player;

        // ボスが地面にいる場合、プレイヤーの位置に応じて移動方向を決定する
        if (pos.y <= floorHeight)
        {
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

        // ボスを移動させる
        transform.position += dir * Time.deltaTime;

        int[] range = new int[] { 0, 100, 999, 300 };
        // プレイヤーが攻撃範囲に入った場合、攻撃を開始する
        if (Mathf.Abs(player.transform.position.x - transform.position.x) < range[attackKind])
        {
            isAttack = true;
            isAttackDamage = true;
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

    public override void TakeDamage(int damage, string soundName)
    {
        Debug.LogError("Boss2 TakeDamage " + damage + " " + soundName);

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
        float duration = 1.0f; // 飛び上がりから落下までの時間
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
