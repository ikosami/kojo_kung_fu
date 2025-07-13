using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Boss : MonoBehaviour, ICharacter
{
    public GameObject GameObject => gameObject;
    [SerializeField] RectTransform rect;
    public RectTransform Rect => rect;
    [SerializeField] Image image;
    [SerializeField] Sprite normalSprite1;
    [SerializeField] Sprite normalSprite2;
    [SerializeField] Sprite attackSprite1_1;
    [SerializeField] Sprite attackSprite1_2;
    [SerializeField] Sprite attackSprite2_1;
    [SerializeField] Sprite attackSprite2_2;
    private float spriteChangeTimer = 0f;
    [SerializeField] private float spriteChangeInterval = 0.5f;
    private bool isNormalSprite = true;

    [SerializeField] Sprite damageSprite;
    public RectTransform BodyColRect => bodyRange;
    [SerializeField] RectTransform[] attackRange1;
    [SerializeField] RectTransform[] attackRange2;
    [SerializeField] RectTransform bodyRange;

    [SerializeField] Vector3 moveSpeed = new Vector3(0.4f, 0, 0);

    //攻撃しようとして止まっている
    bool isAttack = false;
    [SerializeField] float attackTime = 0;

    [SerializeField] int hpMax = 3;
    [SerializeField] int hp = 3;
    bool isDead { get { return hp <= 0; } }

    private float currentJumpVelocity = 0f;
    [SerializeField] float maxJumpVelocity = 6f;
    [SerializeField] float gravity = 0.2f;
    [SerializeField] int floorHeight = 10;

    int attakKind = 0;
    int attackState = 1;
    float attack2Speed = 0;
    bool isAttack2Stop = false;
    Vector3 dir;

    void Start()
    {
        hp = hpMax;
        Reference.Instance.enemyList.Add(this);
        RandomAttackKind();
    }

    void RandomAttackKind()
    {
        attakKind = Random.Range(0, 2);
    }

    void Update()
    {
        if (Reference.Instance.IsClear) return;
        if (Reference.Instance.isPause) return;
        if (Reference.Instance.IsGameOver) { return; }

        if (isDead) { return; }

        Move();
        HandleAttack();
        HandleNormalSpriteAnimation();

    }

    bool isAttackDamage = false;
    private void HandleAttack()
    {
        if (!isAttack) { return; }


        switch (attakKind)
        {
            case 0:
                Attack1();
                break;
            case 1:
                Attack2();
                break;
        }

    }

    private void Attack1()
    {
        attackTime += Time.deltaTime;

        if (attackTime < 1f)
        {
            if (image.sprite != attackSprite1_1)
                image.sprite = attackSprite1_1;
        }
        else if (attackTime < 2f)
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
            if (image.sprite != attackSprite1_2)
                image.sprite = attackSprite1_2;
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
                RandomAttackKind();
            }
            else if (attackState == 2)
            {
                attackState = 3;
                //2連続で腕攻撃だったら次は突進
                attakKind = 1;
            }
            isAttack = false;
            spriteChangeTimer = 0;
        }
    }
    private void Attack2()
    {
        attackTime += Time.deltaTime;

        float moveTime = 5;

        if (attackTime < 1)
        {
            if (image.sprite != attackSprite2_1)
                image.sprite = attackSprite2_1;
            attack2Speed = 0;
            isAttack2Stop = false;
        }
        else if (attackTime < moveTime)
        {
            if (isAttackDamage)
            {
                SoundManager.Instance.Play("boss_attack_2_1");
                isAttackDamage = false;
            }
            if (image.sprite != attackSprite2_2)
                image.sprite = attackSprite2_2;

            if (!isAttack2Stop)
            {
                foreach (var range in attackRange2)
                {
                    if (Util.IsHitPlayer(range))
                    {
                        Reference.Instance.player.TakeDamage(1);
                        SoundManager.Instance.Play("boss_attack_2_2");
                        break;
                    }
                }

                attack2Speed += Time.deltaTime * 3;
                transform.position += dir * attack2Speed * Time.deltaTime;


                var pos = Rect.anchoredPosition;

                if (pos.x > 149 && dir.x > 0)
                {
                    pos.x = 149;
                    attackTime = moveTime - 1;
                    isAttack2Stop = true;
                    SoundManager.Instance.Play("boss_attack_2_2");
                }
                if (pos.x < 11 && dir.x < 0)
                {
                    pos.x = 11;
                    attackTime = moveTime - 1;
                    isAttack2Stop = true;
                    SoundManager.Instance.Play("boss_attack_2_2");
                }
            }

        }
        else if (attackTime < 6)
        {
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
        }
        else
        {
            attackState = 1;
            attakKind = 0;

            isAttack = false;
            spriteChangeTimer = 0;
        }
    }


    /// <summary>
    /// ボスの移動を制御する関数
    /// </summary>
    private void Move()
    {
        var pos = rect.anchoredPosition;

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
            }
            return;
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

        // プレイヤーが攻撃範囲に入った場合、攻撃を開始する
        if (Mathf.Abs(player.transform.position.x - transform.position.x) < 120)
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

    public void TakeDamage(int damage)
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

        yield return new WaitForSeconds(2f); // 少し待機
        //Reference.Instance.completePanel.SetActive(true);
        //SoundManager.Instance.Play("stage_clear");


        //yield return new WaitForSeconds(4f); // 少し待機

        SaveDataManager.Dojo = BossNum;
        SaveDataManager.NextStage();
        Reference.Instance.player.MoveEnd();
    }

    [SerializeField] int BossNum = 1;



}
