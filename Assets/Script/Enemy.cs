using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

/// <summary>
/// 敵キャラクターの挙動を制御するクラス。
/// 移動、攻撃、ダメージ処理、アニメーション、死亡処理などを担当する。
/// </summary>
public class Enemy : MonoBehaviour, ICharacter
{
    // ICharacterインターフェース実装：自身のGameObjectを返す
    public GameObject GameObject => gameObject;

    [SerializeField] protected RectTransform rect; // 敵のUI座標
    // ICharacterインターフェース実装：自身のRectTransformを返す
    public RectTransform Rect => rect;

    [SerializeField] protected Image image; // 敵の画像コンポーネント
    [Space]
    [SerializeField] protected Sprite normalSprite1; // 通常時スプライト1
    [SerializeField] protected Sprite normalSprite2; // 通常時スプライト2

    [SerializeField] protected Sprite attackSprite1; // 攻撃時スプライト
    protected float spriteChangeTimer = 0f; // スプライト切り替え用タイマー
    [SerializeField] protected float spriteChangeInterval = 0.5f; // スプライト切り替え間隔
    protected bool isNormalSprite = true; // 現在のスプライトがnormalSprite1かどうか

    [SerializeField] Sprite damageSprite; // ダメージ時スプライト

    // ICharacterインターフェース実装：当たり判定用のRectTransform
    public RectTransform BodyColRect => bodyRange;

    [SerializeField] protected RectTransform attackRange;   // 攻撃判定範囲
    [SerializeField] protected RectTransform attackRange2;  // ジャンプ攻撃用範囲
    [SerializeField] protected RectTransform bodyRange;     // 本体当たり判定範囲

    [SerializeField] protected Vector3 moveSpeed = new Vector3(0.4f, 0, 0); // 移動速度

    // 攻撃状態フラグ
    protected bool isAttack = false;
    [SerializeField] protected float attackTime = 0; // 攻撃経過時間

    [SerializeField] int hpMax = 3; // 最大HP
    [SerializeField] int hp = 3;    // 現在HP
                                    // 死亡判定
    protected bool isDead { get { return hp <= 0; } }
    protected float damageWaitTime = 0; // ダメージ演出中の待機時間

    [SerializeField] protected bool isJumpEnemy = false; // ジャンプする敵かどうか
    protected float currentJumpVelocity = 0f;    // 現在のジャンプ速度
    [SerializeField] protected float maxJumpVelocity = 6f; // ジャンプ初速度
    [SerializeField] protected float gravity = 0.2f;       // 重力加速度
    [SerializeField] protected int floorHeight = 10;       // 床の高さ
    [SerializeField] protected float jumpWaitTime = 2;     // ジャンプ間隔
    [SerializeField] protected float jumpTimer = 0;        // ジャンプ用タイマー

    [Header("攻撃発生の距離")]
    [SerializeField] protected float attackStartDistance = 50f;
    [SerializeField] protected float backDistance = -1f;
    protected Vector3 dir; // 移動方向

    public float BaseHeight = -73;

    /// <summary>
    /// 初期化処理。HPを最大値にし、敵リストに自身を追加。
    /// </summary>
    void Start()
    {
        hp = hpMax;
        Reference.Instance.enemyList.Add(this);
        var pos = rect.anchoredPosition;
        pos.y = BaseHeight;
        rect.anchoredPosition = pos;
    }

    /// <summary>
    /// 毎フレーム呼ばれる更新処理。
    /// ゲームの状態やダメージ演出、移動・攻撃・アニメーションを制御。
    /// </summary>
    protected virtual void Update()
    {
        // ゲームクリア・ポーズ・ゲームオーバー時は処理しない
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        // 死亡時は何もしない
        if (isDead) { return; }

        // ダメージ演出中はスプライトをダメージ用にし、一定時間後に戻す
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

        // 移動処理
        Move();

        // 攻撃処理
        HandleAttack();

        // 通常時アニメーション処理
        HandleNormalSpriteAnimation();
    }

    protected bool isAttackDamage = false; // 攻撃ダメージ判定用フラグ

    /// <summary>
    /// 攻撃アニメーションと攻撃判定の処理。
    /// </summary>
    protected virtual void HandleAttack()
    {
        //
        if (!isAttack) { return; }

        attackTime += Time.deltaTime;

        if (attackTime < 0.5f)
        {
            // 攻撃前の溜め
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
        }
        else if (attackTime < 1f)
        {
            // 攻撃発動
            if (isAttackDamage)
            {
                SoundManager.Instance.Play("enemy_attack");
                if (Util.IsHitPlayer(attackRange))
                {
                    Reference.Instance.player.TakeDamage(1);
                }
                isAttackDamage = false;
            }
            if (image.sprite != attackSprite1)
                image.sprite = attackSprite1;
        }
        else if (attackTime < 1.5f)
        {
            // 攻撃後の戻り
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
        }
        else
        {
            // 攻撃終了
            isAttack = false;
            spriteChangeTimer = 0;
        }
    }

    public virtual bool CanLook
    {
        get
        {
            return IsGround;
        }
    }
    public virtual bool IsGround
    {
        get
        {
            var pos = rect.anchoredPosition;
            return pos.y <= floorHeight;
        }
    }

    /// <summary>
    /// 移動・ジャンプ・攻撃開始判定の処理。
    /// </summary>
    protected virtual void Move()
    {
        // ジャンプ中は方向転換しない
        if (CanLook)
        {
            // プレイヤーの位置に応じて移動方向・向きを決定
            LookPlayer();
        }

        // ジャンプ敵の場合のジャンプ処理
        if (isJumpEnemy)
        {
            JumpMove();


            // 着地判定
            if (IsGround)
            {
                JumpEnd();
                TryJump();
                return;
            }
            else
            {
                // ジャンプ攻撃判定
                if (Util.IsHitPlayer(attackRange2))
                {
                    Reference.Instance.player.TakeDamage(1);
                }
            }
        }



        // 攻撃中は移動しない
        if (isAttack) { return; }

        // 後退処理
        var isBacking = CheckBackMove();

        if (isBacking)
        {
            // 向きは変えずに後退
            transform.position -= dir * Time.deltaTime;
        }
        else
        {
            // 通常移動
            transform.position += dir * Time.deltaTime;
        }

        // 後退中(攻撃判定より後ろ)でなければ攻撃判定
        if (!isJumpEnemy && !isBacking)
        {
            //一定以上近い
            float distanceToPlayer = GetPlayerDistance();
            if (distanceToPlayer < attackStartDistance)
            {
                StartAttackFlg();
            }
        }
    }

    protected void StartAttackFlg()
    {
        isAttack = true;
        isAttackDamage = true;
        attackTime = 0;
    }

    protected void JumpEnd()
    {
        var pos = rect.anchoredPosition;
        pos.y = floorHeight;
        rect.anchoredPosition = pos;
        currentJumpVelocity = 0;
    }

    protected void TryJump()
    {
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0)
        {
            jumpTimer = jumpWaitTime;
            currentJumpVelocity = maxJumpVelocity;
        }
    }

    protected virtual void JumpMove()
    {
        currentJumpVelocity -= gravity * Time.deltaTime;

        var pos = rect.anchoredPosition;
        pos.y += currentJumpVelocity * Time.deltaTime;
        rect.anchoredPosition = pos;
    }

    public float GetPlayerDistance()
    {
        var player = Reference.Instance.player;
        float distanceToPlayer = Mathf.Abs(player.transform.position.x - transform.position.x);
        return distanceToPlayer;
    }


    public bool CheckBackMove()
    {
        var player = Reference.Instance.player;
        float distanceToPlayer = Mathf.Abs(player.transform.position.x - transform.position.x);

        bool isBacking = (backDistance > 0 && distanceToPlayer <= backDistance);
        return isBacking;
    }

    protected void LookPlayer()
    {
        var pos = rect.anchoredPosition;
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
    }

    /// <summary>
    /// 通常時のスプライトアニメーション（点滅）の処理。
    /// </summary>
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

    /// <summary>
    /// ダメージを受けた際の処理。HP減少・死亡判定・ダメージ演出。
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public void TakeDamage(int damage)
    {
        if (isDead) { return; }

        var player = Reference.Instance.player;
        // プレイヤーの位置に応じて向きを変更
        if (player.transform.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        image.sprite = damageSprite;
        damageWaitTime = 0.5f;

        hp -= damage;
        if (hp <= 0)
        {
            SoundManager.Instance.Play("attack2");
            Reference.Instance.AddScore(800);
            StartCoroutine(Dead());
        }
    }

    /// <summary>
    /// 死亡時のアニメーションと非表示処理。
    /// </summary>
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
            // ポーズ・ゲームオーバー中は停止
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

        yield return new WaitForSeconds(0.5f); // 少し待機
        gameObject.SetActive(false); // オブジェクトを非表示にする
    }
}
