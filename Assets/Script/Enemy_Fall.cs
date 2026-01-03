using UnityEngine;

/// <summary>
/// 敵キャラクターの挙動を制御するクラス。
/// 移動、攻撃、ダメージ処理、アニメーション、死亡処理などを担当する。
/// </summary>
public class Enemy_Fall : Enemy
{
    public Sprite FalledSprite;
    public Sprite Climb1Sprite;
    public Sprite Climb2Sprite;

    [SerializeField] RectTransform[] attackRects;

    bool isClimb = false;

    public override bool CanLook
    {
        get
        {
            return !isAttack;
        }
    }

    protected override void HandleAttack()
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
            //斜め下に移動d
            transform.position += (moveDir + new Vector3(0, -moveSpeed.x, 0)) * Time.deltaTime * 2;
            // 攻撃発動
            if (isAttackDamage)
            {
                foreach (var attackRange in attackRects)
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
            if (IsGround)
            {
                attackTime = 1f; // 攻撃時間をリセット
            }
            else
            {

                attackTime = 0.5f; // 攻撃時間をリセット       
            }

            if (image.sprite != attackSprite1)
                image.sprite = attackSprite1;
        }
        else if (attackTime < 1.5f)
        {

            // 攻撃後の戻り
            if (image.sprite != FalledSprite)
                image.sprite = FalledSprite;
        }
        else if (rect.anchoredPosition.y < BaseHeight + 10)
        {
            transform.position += new Vector3(0, moveSpeed.x / 2, 0) * Time.deltaTime;
            isClimb = true;
        }
        else
        {
            var pos = rect.anchoredPosition;
            pos.y = BaseHeight;
            rect.anchoredPosition = pos;

            // 攻撃終了
            isAttack = false;
            isClimb = false;
            spriteChangeTimer = 0;

        }
    }

    protected override void Update()
    {
        base.Update();

        if (isClimb)
            HandleNormalSpriteAnimation2();
    }


    /// <summary>
    /// 通常時のスプライトアニメーション（点滅）の処理。
    /// </summary>
    private void HandleNormalSpriteAnimation2()
    {
        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer >= spriteChangeInterval)
        {
            spriteChangeTimer -= spriteChangeInterval;
            isNormalSprite = !isNormalSprite;
        }
        image.sprite = isNormalSprite ? Climb1Sprite : Climb2Sprite;
    }

}
