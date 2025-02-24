using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] Transform stage;
    [SerializeField] bool isBoss = false;
    public const int SizeX = 160;
    public const int SizeY = 120;

    [SerializeField] int floorHeight = 10;
    [SerializeField] float maxJumpHeight = 300f;
    [SerializeField] RectTransform rect;

    [SerializeField] Image image;
    [SerializeField] Sprite normalSprite1;
    [SerializeField] Sprite normalSprite2;

    [SerializeField] Sprite attackSprite1;
    [SerializeField] Sprite attackSprite2;
    [SerializeField] Sprite attackSprite3;
    [SerializeField] Sprite jumpSprite;

    [SerializeField] float jumpSpeed = 3f;
    [SerializeField] float maxJumpVelocity = 6f;
    [SerializeField] float gravity = 0.2f;

    [SerializeField] Vector3 moveSpeed = new Vector3(0.1f, 0, 0);

    private bool isJumping = false;
    private bool isFalling = false;
    private float currentJumpVelocity = 0f;

    private float spriteChangeTimer = 0f;
    [SerializeField] private float spriteChangeInterval = 0.5f;
    private bool isNormalSprite1 = true;

    private bool isAttacking = false;
    [SerializeField] private float attackDuration = 0.2f;
    private int attackStep = 0;
    [SerializeField] private float attackTimeout = 0.5f; // �A���U���̓��͎�t����
    private float lastAttackTime = -1f;

    [SerializeField] private float attackCooldown = 0.3f; // �A�Ŗh�~�̃N�[���_�E��
    [SerializeField] private float attackEndDelay = 0.1f; // �U���I����ɒʏ��Ԃɖ߂鎞��

    void Update()
    {
        var pos = rect.anchoredPosition;

        Move();
        HandleJump(ref pos);
        HandleAttack();

        rect.anchoredPosition = pos;
    }

    private void Move()
    {
        if (isAttacking && !isJumping)
        {
            return;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            if (!isBoss)
                stage.transform.position -= moveSpeed;
            else
                transform.position -= moveSpeed;

            transform.localScale = new Vector3(1, 1, 1);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            if (!isBoss)
                stage.transform.position += moveSpeed;
            else
                transform.position += moveSpeed;
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
                pos.y += currentJumpVelocity;
            }
            else
            {
                isFalling = true;
            }

            if (isFalling)
            {
                currentJumpVelocity -= gravity;
                pos.y += currentJumpVelocity;
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
            isNormalSprite1 = !isNormalSprite1;
        }
        image.sprite = isNormalSprite1 ? normalSprite1 : normalSprite2;
    }

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // �A�Ŗh�~�i�O��̍U�������莞�Ԍo�߂��Ȃ��Ǝ��̍U�����ł��Ȃ��j
            if (Time.time < lastAttackTime + attackCooldown)
            {
                return;
            }

            if (isJumping)
            {
                isAttacking = true;
                image.sprite = attackSprite3;
            }
            else
            {
                if (Time.time - lastAttackTime <= attackTimeout && attackStep < 3)
                {
                    attackStep++;
                }
                else
                {
                    attackStep = 1;
                }

                lastAttackTime = Time.time;
                isAttacking = true;
            }
        }

        if (isAttacking && !isJumping)
        {
            spriteChangeTimer = 0;

            if (attackStep == 1)
                image.sprite = attackSprite1;
            else if (attackStep == 2)
                image.sprite = attackSprite1;
            else if (attackStep == 3)
                image.sprite = attackSprite2;

            // �U�����������A�w�莞�Ԍo�ߌ�ɒʏ��Ԃɖ߂�
            if (Time.time >= lastAttackTime + attackDuration)
            {
                HandleNormalSpriteAnimation();
            }
            if (Time.time > lastAttackTime + attackCooldown)
            {
                isAttacking = false;
            }
        }
    }
}
