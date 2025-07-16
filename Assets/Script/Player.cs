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

    [SerializeField] Sprite slidingSprite;
    bool isSliding = false;
    float slidingTimer = 0f;
    float slidingStopTime = 0;
    float slidingWaitTimer = 0;

    public RectTransform BodyColRect => isSliding ? bodySlidingRange : bodyRange;

    [SerializeField] RectTransform bodySlidingRange;
    [SerializeField] RectTransform attackSlidingRange;

    [SerializeField] RectTransform bodyRange;
    [SerializeField] RectTransform attack1Range;
    [SerializeField] RectTransform attack2Range;
    [SerializeField] RectTransform attackJumpRange;

    [SerializeField] float jumpSpeed = 3f;
    [SerializeField] float maxJumpVelocity = 6f;
    [SerializeField] float gravity = 0.2f;

    [SerializeField] float damageStopTime = 0.2f;
    [SerializeField] float invincibleTime = 0.5f;
    float damageTimer = 9999;
    public bool isInvincible => damageTimer < invincibleTime;


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
            return !Reference.Instance.isBoss && (Reference.Instance.stageRect.anchoredPosition.x <= 0 && Rect.anchoredPosition.x >= centerPos);
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

        //デバッグクリア
        if (Input.GetKeyDown(KeyCode.B) && Input.GetKey(KeyCode.LeftShift))
        {
            MoveEnd();
        }

        //ポーズ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Reference.Instance.isPause = !Reference.Instance.isPause;
        }

        //タイトルに戻る
        if (Reference.Instance.isPause)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                SceneManager.LoadScene("LoadScene");
            }
        }

        if (Reference.Instance.isPause) return;

        //出現待ち
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
        //出現中の点滅
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

        damageTimer += Time.deltaTime;
        if (damageTimer < damageStopTime) return;

        Move();
        var pos = rect.anchoredPosition;
        HandleJump(ref pos);
        rect.anchoredPosition = pos;
        HandleAttack();

    }



    private void Move()
    {
        if (isSliding)
        {
            if (slidingStopTime <= 0)
            {
                var enemyList = Util.GetEnemyList(attackSlidingRange);
                foreach (var enemy in enemyList)
                {
                    enemy.TakeDamage(3);
                    slidingStopTime = Mathf.Min(0.2f, slidingTimer);
                    slidingTimer = 0;
                    break;
                }
                MovePos(moveSpeed * transform.localScale.x * 1.5f);
            }
            slidingTimer -= Time.deltaTime;
            slidingStopTime -= Time.deltaTime;
            if (slidingTimer <= 0f && slidingStopTime <= 0f)
            {
                isSliding = false;
                // スライディング終了後のスプライトに戻す
                HandleNormalSpriteAnimation();
                slidingWaitTimer = 0.5f;
            }
            return;
        }
        else
        {
            slidingWaitTimer -= Time.deltaTime;
        }

        if (isAttacking && !isJumping)
        {
            return;
        }

        //右移動
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

            transform.localScale = new Vector3(1, 1, 1);
            MovePos(moveSpeed * transform.localScale.x);

            if (Reference.Instance.isDojo)
            {
                var pos = rect.anchoredPosition;
                if (pos.x > 140)
                {
                    MoveEnd();
                }
            }
        }

        //左移動
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            //敵が左側にいるかどうか
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

            transform.localScale = new Vector3(-1, 1, 1);
            MovePos(moveSpeed * transform.localScale.x);
        }
    }

    private void MovePos(Vector3 move)
    {
        if (isBackMove)
        {
            Reference.Instance.stageRect.transform.position -= move * Time.deltaTime;

            var pos = Rect.anchoredPosition;
            pos.x = centerPos;
            Rect.anchoredPosition = pos;
        }
        else
        {

            transform.position += move * Time.deltaTime;

            var pos = Rect.anchoredPosition;
            if (pos.x < 11)
            {
                pos.x = 11;
            }
            if (pos.x > 149)
            {
                pos.x = 149;
            }
            Rect.anchoredPosition = pos;

            var stagePos = Reference.Instance.stageRect.anchoredPosition;
            stagePos.x = 0;
            Reference.Instance.stageRect.anchoredPosition = stagePos;
        }
    }

    private void HandleJump(ref Vector2 pos)
    {
        if (isJumping)
        {
            if (IsAttackInput && !isFalling && pos.y < maxJumpHeight)
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
        else if (isSliding)
        {
            //スライティング中はジャンプできない
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
    public virtual bool IsGround
    {
        get
        {
            var pos = rect.anchoredPosition;
            return pos.y <= floorHeight;
        }
    }

    bool IsAttackInput => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.J);

    private void HandleAttack()
    {
        //スライティング中は追加で攻撃できない
        if (isSliding)
        {
            return;
        }

        if (IsAttackInput)
        {
            // 連打防止（前回の攻撃から一定時間経過しないと次の攻撃ができない）
            if (Time.time < lastAttackTime + attackCooldown)
            {
                return;
            }

            if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && Reference.Instance.StageNum >= 2 && IsGround)
            {
                if (slidingWaitTimer > 0)
                {
                    return;
                }

                isSliding = true;
                image.sprite = slidingSprite;
                slidingTimer = 1f;
                slidingStopTime = 0;
                SoundManager.Instance.Play("sliding");

            }
            else if (isJumping)
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
                        enemy.TakeDamage(3);
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
                enemy.TakeDamage(2);
            }
        }
        else
        {
            SoundManager.Instance.Play("attack");
            var enemyList = Util.GetEnemyList(attack2Range);
            foreach (var enemy in enemyList)
            {
                enemy.TakeDamage(3);
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

    public void TakeDamage(int damage, string soundName = "")
    {
        if (damageTimer < invincibleTime)
        {
            return;
        }

        if (soundName != string.Empty)
            SoundManager.Instance.Play(soundName);

        SaveDataManager.Hp--;
        Reference.Instance.UpdateStateView();
        SaveDataManager.NoDamage = false;

        if (SaveDataManager.Hp <= 0)
        {
            Reference.Instance.IsGameOver = true;
            SoundManager.Instance.Play("damage");
            Reference.Instance.bgm.gameObject.SetActive(false);
            image.sprite = deadSprite;
            StartCoroutine(Dead());
        }
        else
        {
            isSliding = false;
            damageTimer = 0;
            image.sprite = deadSprite;
        }
        return;
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

        SaveDataManager.Life--;
        if (SaveDataManager.Life <= -1)
        {
            Reference.Instance.isGameOverEnd = true;
        }
        else
        {
            Reference.Instance.uiController.SetLife();


            SaveDataManager.Hp = SaveDataManager.HpMax;
            Reference.Instance.uiController.SetHp(SaveDataManager.HpMax);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void MoveEnd()
    {
        Reference.Instance.IsClear = true;

        StartCoroutine(MoveEndIE());
    }

    private IEnumerator MoveEndIE()
    {

        transform.localScale = new Vector3(1, 1, 1);
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

        if (!Reference.Instance.isDojo)
        {
            SceneManager.LoadScene("BossScene");
        }
        else
        {
            SaveDataManager.Dojo = 0;
            SceneManager.LoadScene("GameScene");
        }

    }
}
