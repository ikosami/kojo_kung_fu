using System;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, ICharacter
{
    [SerializeField] int floorHeight = 10;
    [SerializeField] float maxJumpHeight = 300f;
    [SerializeField] RectTransform rect;
    public RectTransform Rect => rect;

    [SerializeField] Image image;
    [SerializeField] Sprite normalSprite1;
    [SerializeField] Sprite normalSprite2;
    private float spriteChangeTimer = 0f;
    [SerializeField] private float spriteChangeInterval = 0.5f;
    private bool isNormalSprite = true;


    [SerializeField] Sprite attackSprite1;
    [SerializeField] Sprite attackSprite2;
    [SerializeField] Sprite attackSprite3;
    [SerializeField] Sprite jumpSprite;
    [SerializeField] RectTransform attack1Range;
    [SerializeField] RectTransform attack2Range;

    [SerializeField] float jumpSpeed = 3f;
    [SerializeField] float maxJumpVelocity = 6f;
    [SerializeField] float gravity = 0.2f;

    [SerializeField] Vector3 moveSpeed = new Vector3(0.1f, 0, 0);

    private bool isJumping = false;
    private bool isFalling = false;
    private float currentJumpVelocity = 0f;

    private bool isAttacking = false;
    private int attackStep = 0;
    private float lastAttackTime = -1f;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackTimeout = 0.5f; // 連続攻撃の入力受付時間
    [SerializeField] private float attackCooldown = 0.3f; // 連打防止のクールダウン
    private bool nextAttackQueued = false; // 次の攻撃を受け付けるフラグ


    private void Start()
    {
        SoundManager.Instance.Play("start");

    }
    void Update()
    {

        Move();
        var pos = rect.anchoredPosition;
        HandleJump(ref pos);
        rect.anchoredPosition = pos;
        HandleAttack();

    }

    private void Move()
    {
        if (isAttacking && !isJumping)
        {
            return;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            if (!Reference.Instance.isBoss)
                Reference.Instance.stage.transform.position -= moveSpeed * Time.deltaTime;
            else
                transform.position -= moveSpeed * Time.deltaTime;

            transform.localScale = new Vector3(1, 1, 1);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            if (!Reference.Instance.isBoss)
                Reference.Instance.stage.transform.position += moveSpeed * Time.deltaTime;
            else
                transform.position += moveSpeed * Time.deltaTime;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void HandleJump(ref Vector2 pos)
    {
        if (isJumping)
        {
            if (Input.GetKey(KeyCode.W) && !isFalling && pos.y < maxJumpHeight)
            {
                currentJumpVelocity = Mathf.Min(currentJumpVelocity + 0.2f, maxJumpVelocity);
                pos.y += currentJumpVelocity * Time.deltaTime;
            }
            else
            {
                isFalling = true;
            }

            if (isFalling)
            {
                currentJumpVelocity -= gravity * Time.deltaTime;
                pos.y += currentJumpVelocity * Time.deltaTime;
            }

            if (pos.y <= floorHeight)
            {
                pos.y = floorHeight;
                isJumping = false;
                isFalling = false;
                currentJumpVelocity = 0;
                isAttacking = false;
                attackStep = 0;
                spriteChangeTimer = 0;
                HandleNormalSpriteAnimation();
            }
            else
            {
                image.sprite = isAttacking ? attackSprite3 : jumpSprite;
                return;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (!isJumping)
                {
                    SoundManager.Instance.Play("jump");
                }

                isJumping = true;
                isFalling = false;
                currentJumpVelocity = jumpSpeed;
            }

            if (!isAttacking)
            {
                HandleNormalSpriteAnimation();
            }
        }
    }

    private void HandleNormalSpriteAnimation()
    {
        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer >= spriteChangeInterval)
        {
            spriteChangeTimer -= spriteChangeInterval;
            isNormalSprite = !isNormalSprite;
        }
        image.sprite = isNormalSprite ? normalSprite1 : normalSprite2;
    }

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 連打防止（前回の攻撃から一定時間経過しないと次の攻撃ができない）
            if (Time.time < lastAttackTime + attackCooldown)
            {
                return;
            }

            if (isJumping)
            {
                if (!isAttacking)
                {
                    //ジャンプ攻撃
                    isAttacking = true;
                    image.sprite = attackSprite3;
                    SoundManager.Instance.Play("attack2");
                }
            }
            else
            {
                if (isAttacking)
                {
                    // すでに攻撃中なら、次の攻撃を予約
                    nextAttackQueued = true;
                }
                else
                {
                    attackStep = 0;
                    //攻撃中でなければ、攻撃を開始
                    Attack();
                    isAttacking = true;
                }
            }
        }

        if (isAttacking && !isJumping)
        {

            // 攻撃が完了し、次の攻撃が予約されていれば続行
            if (Time.time >= lastAttackTime + attackTimeout)
            {
                if (nextAttackQueued && attackStep < 3)
                {
                    Attack();
                }
                else
                {
                    spriteChangeTimer = 0;
                    isAttacking = false;
                    attackStep = 0;
                }
                nextAttackQueued = false; // 予約をリセット
            }

            if (Time.time >= lastAttackTime + attackDuration)
            {
                spriteChangeTimer = 0;
                HandleNormalSpriteAnimation();
            }
        }
    }

    private void Attack()
    {
        if (attackStep < 2)
        {
            SoundManager.Instance.Play("attack");
            var enemyList = Util.GetEnemyList(attack1Range);
            foreach (var enemy in enemyList)
            {
                enemy.TakeDamage(1);
            }
        }
        else
        {
            SoundManager.Instance.Play("attack2");
            var enemyList = Util.GetEnemyList(attack2Range);
            foreach (var enemy in enemyList)
            {
                enemy.TakeDamage(2);
            }
        }
        ChangeAttackSprite();
        lastAttackTime = Time.time;

    }

    void ChangeAttackSprite()
    {
        attackStep++;
        if (attackStep == 1)
            image.sprite = attackSprite1;
        else if (attackStep == 2)
            image.sprite = attackSprite1;
        else if (attackStep == 3)
            image.sprite = attackSprite2;
    }

    public void TakeDamage(int damage)
    {
    }
}
