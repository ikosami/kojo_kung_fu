using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour, ICharacter
{
    public GameObject GameObject => gameObject;
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
    [SerializeField] Sprite deadSprite;

    public RectTransform BodyColRect => bodyRange;


    [SerializeField] RectTransform bodyRange;
    [SerializeField] RectTransform attack1Range;
    [SerializeField] RectTransform attack2Range;
    [SerializeField] RectTransform attackJumpRange;

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

    public float blinkInterval = 0.25f; // 点滅間隔（秒）
    public int blinkCount = 6; // 点滅回数
    private float totalBlinkTime;
    [SerializeField] float initTimer = 2;
    [SerializeField] float startTimer = 1;
    int centerPos = 78;

    [SerializeField] bool isInitEffect = true;

    private bool isBackMove
    {
        get
        {
            return !Reference.Instance.isBoss && (Reference.Instance.stage.anchoredPosition.x <= 0 && Rect.anchoredPosition.x >= centerPos);
        }
    }

    private void Start()
    {
        if (isInitEffect)
        {
            totalBlinkTime = blinkInterval * blinkCount;
            startTimer = totalBlinkTime;
            image.enabled = false;
        }
        else
        {
            totalBlinkTime = 0;
            startTimer = 0.5f;
            initTimer = 0;
            image.enabled = true;
        }
    }
    void Update()
    {
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.IsGameOver) return;

        if (Input.GetKeyDown(KeyCode.B) && Input.GetKey(KeyCode.LeftShift))
        {
            MoveEnd();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Reference.Instance.isPause = !Reference.Instance.isPause;
        }
        if (Reference.Instance.isPause)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                SceneManager.LoadScene("LoadScene");
            }
        }

        if (Reference.Instance.isPause) return;

        if (initTimer > 0)
        {
            initTimer -= Time.deltaTime;
            if (initTimer <= 0)
            {
                SoundManager.Instance.Play("start");
            }
            else
            {
                return;
            }
        }
        if (startTimer > 0)
        {
            startTimer -= Time.deltaTime;
            if (startTimer <= 0)
            {
                Reference.Instance.bgm.gameObject.SetActive(true);
                image.enabled = true;
            }
            else
            {
                float elapsedTime = totalBlinkTime - startTimer;
                float normalizedTime = elapsedTime / totalBlinkTime;
                int blinkIndex = Mathf.FloorToInt(normalizedTime * blinkCount);
                image.enabled = blinkIndex % 2 == 0;
                return;
            }

        }

        if (Reference.Instance.IsGameOver) { return; }

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
            var enemyList = Util.GetEnemyList(bodyRange);
            bool isEnemyRight = false;
            foreach (var enemy in enemyList)
            {
                if (enemy.GameObject.transform.position.x > gameObject.transform.position.x)
                {
                    isEnemyRight = true;
                    break;
                }
            }
            if (isEnemyRight) return;

            if (isBackMove)
            {
                Reference.Instance.stage.transform.position -= moveSpeed * Time.deltaTime;

                var pos = Rect.anchoredPosition;
                pos.x = centerPos;
                Rect.anchoredPosition = pos;
            }
            else
            {
                transform.position += moveSpeed * Time.deltaTime;

                var pos = Rect.anchoredPosition;
                if (pos.x > 149)
                {
                    pos.x = 149;
                }
                Rect.anchoredPosition = pos;

                var stagePos = Reference.Instance.stage.anchoredPosition;
                stagePos.x = 0;
                Reference.Instance.stage.anchoredPosition = stagePos;
            }

            transform.localScale = new Vector3(1, 1, 1);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            var enemyList = Util.GetEnemyList(bodyRange);
            bool isEnemyLeft = false;
            foreach (var enemy in enemyList)
            {
                if (enemy.GameObject.transform.position.x < gameObject.transform.position.x)
                {
                    isEnemyLeft = true;
                    break;
                }
            }
            if (isEnemyLeft) return;

            if (isBackMove)
            {
                Reference.Instance.stage.transform.position += moveSpeed * Time.deltaTime;

                var pos = Rect.anchoredPosition;
                pos.x = centerPos;
                Rect.anchoredPosition = pos;
            }
            else
            {
                transform.position -= moveSpeed * Time.deltaTime;

                var pos = Rect.anchoredPosition;
                if (pos.x < 11)
                {
                    pos.x = 11;
                }
                Rect.anchoredPosition = pos;

                var stagePos = Reference.Instance.stage.anchoredPosition;
                stagePos.x = 0;
                Reference.Instance.stage.anchoredPosition = stagePos;
            }
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void HandleJump(ref Vector2 pos)
    {
        if (isJumping)
        {
            if ((Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.J)) && !isFalling && pos.y < maxJumpHeight)
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
            if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.K)))
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
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.J))
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
                    SoundManager.Instance.Play("attack");
                    var enemyList = Util.GetEnemyList(attackJumpRange);
                    foreach (var enemy in enemyList)
                    {
                        enemy.TakeDamage(2);
                    }
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
            SoundManager.Instance.Play("attack");
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
        Reference.Instance.IsGameOver = true;
        SoundManager.Instance.Play("damage");
        Reference.Instance.bgm.gameObject.SetActive(false);
        image.sprite = deadSprite;
        StartCoroutine(Dead());
    }

    private IEnumerator Dead()
    {
        yield return new WaitForSeconds(0.5f); // 少し待機
        SoundManager.Instance.Play("dead");
        Vector3 startPos = transform.position;
        float duration = 1.0f; // 飛び上がりから落下までの時間
        float gravity = -2000f; // 重力加速度
        float elapsed = 0f;

        // 初速度を計算（自由落下を考慮）
        float velocityY = 700;

        Vector3 velocity = new Vector3(0, velocityY, 0f); // X方向 & Y方向の初速度

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // X方向は一定速度で移動、Y方向は重力で加速度的に変化
            velocity.y += gravity * Time.deltaTime; // 重力の影響を加える

            // 現在の位置を更新
            transform.position += velocity * Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(1.5f); // 少し待機


        Reference.Instance.isGameOverEnd = true;
    }

    public void MoveEnd()
    {
        Reference.Instance.IsClear = true;

        StartCoroutine(MoveEndIE());
    }

    private IEnumerator MoveEndIE()
    {

        var pos = rect.anchoredPosition;
        if (isJumping)
        {
            while (pos.y > floorHeight)
            {
                currentJumpVelocity -= gravity * Time.deltaTime;
                pos.y += currentJumpVelocity * Time.deltaTime;
                rect.anchoredPosition = pos;
                yield return null;
            }
            pos.y = floorHeight;
            rect.anchoredPosition = pos;
            currentJumpVelocity = 0;
            isAttacking = false;
            spriteChangeTimer = 0;
            HandleNormalSpriteAnimation();
        }

        while (pos.x < 175)
        {
            transform.position += moveSpeed * Time.deltaTime;
            HandleNormalSpriteAnimation();
            pos = rect.anchoredPosition;
            yield return null;
        }

        SceneManager.LoadScene("BossScene");
    }
}
