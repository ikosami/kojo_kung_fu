using System;
using UnityEngine;

/// <summary>
/// 敵キャラクターの挙動を制御するクラス。
/// 移動、攻撃、ダメージ処理、アニメーション、死亡処理などを担当する。
/// </summary>
public class Enemy_Nozupas : Enemy
{
    [SerializeField] RectTransform[] attackRects;

    public NozupasSprite[] nozupasSprites;
    [SerializeField] FallItem armorObj;
    [SerializeField] Transform armorPoint;
    bool isArmor = true;

    bool chargeAttack = false;
    float chargeAttackTime = 0f;

    [SerializeField] bool isChargeAttack = false;

    public override bool CanLook
    {
        get
        {
            return !isAttack;
        }
    }

    void Armor()
    {
        var armor = Instantiate(armorObj, armorPoint.transform.position, Quaternion.identity, Reference.Instance.stageRect);
        armor.transform.localScale = transform.localScale;

        var player = Reference.Instance.player;
        if (player.transform.position.x < transform.position.x)
        {
            armor.fallPower.x *= -1;
        }
        isArmor = false;
    }

    public override void TakeDamage(int damage, bool breakAttack, string soundName = "")
    {

        //効かない
        if (isArmor)
        {
            if (breakAttack)
            {
                Armor();
                moveSpeed *= 1.2f;
                isAttack = false;
                base.TakeDamage(0, breakAttack, soundName);
                return;
            }
            else
            {
                SoundManager.Instance.Play("nozu_mukou");
                return;
            }
        }
        isAttack = false;


        base.TakeDamage(damage, breakAttack, soundName);
    }

    NozupasSprite NozupasSprite
    {
        get
        {
            if (isArmor)
            {
                return nozupasSprites[0];
            }
            else
            {
                return nozupasSprites[1];
            }
        }
    }

    protected override void HandleAttack()
    {
        if (!isAttack) { return; }

        attackTime += Time.deltaTime;

        if (isChargeAttack)
        {
            if (attackTime < 1f)
            {
                // 攻撃前の溜め
                HandleNormalSpriteAnimation2();
                chargeAttackTime = 2;
            }
            else if (attackTime < 4.0f - 0.1f)
            {
                transform.position += moveDir * Time.deltaTime;

                chargeAttackTime += Time.deltaTime;
                if (chargeAttackTime > 0.15f)
                {
                    chargeAttack = !chargeAttack;
                    chargeAttackTime = 0f;
                    SetSprite(chargeAttack ? NozupasSprite.attackSprite1 : NozupasSprite.attackWaitSprite);

                    // 攻撃発動
                    if (chargeAttack)
                    {

                        SoundManager.Instance.Play("enemy_attack");
                        if (Util.IsHitPlayer(attackRange))
                        {
                            Reference.Instance.player.TakeDamage(1);
                        }
                        isAttackDamage = false;
                    }
                }

            }
            else if (attackTime < 5f)
            {
                // 攻撃後の戻り
                SetSprite(NozupasSprite.normalSprite1);
            }
            else
            {
                // 攻撃終了
                isAttack = false;
                spriteChangeTimer = 0;
            }
        }
        else
        {
            if (attackTime < 1f)
            {
                // 攻撃前の溜め
                SetSprite(NozupasSprite.attackWaitSprite);
            }
            else if (attackTime < 2f)
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
                SetSprite(NozupasSprite.attackSprite1);
            }
            else if (attackTime < 2.5f)
            {
                // 攻撃後の戻り
                SetSprite(NozupasSprite.normalSprite1);
            }
            else
            {
                // 攻撃終了
                isAttack = false;
                spriteChangeTimer = 0;
            }
        }
    }


    protected override void Update()
    {
        base.Update();

    }

    protected override void HandleNormalSpriteAnimation()
    {
        if (isAttack) { return; }
        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer >= spriteChangeInterval)
        {
            spriteChangeTimer -= spriteChangeInterval;
            isNormalSprite = !isNormalSprite;
        }
        image.sprite = isNormalSprite ? NozupasSprite.normalSprite1 : NozupasSprite.normalSprite2;
    }


    /// <summary>
    /// 通常時のスプライトアニメーション（点滅）の処理。
    /// </summary>
    private void HandleNormalSpriteAnimation2()
    {

        var changeTime = 0.01f;
        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer >= changeTime)
        {
            spriteChangeTimer -= changeTime;
            isNormalSprite = !isNormalSprite;
        }
        image.sprite = isNormalSprite ? NozupasSprite.attackWaitSprite : NozupasSprite.chargeSprite;
    }

}


[Serializable]
public class NozupasSprite
{
    public Sprite normalSprite1;
    public Sprite normalSprite2;
    public Sprite attackWaitSprite;
    public Sprite attackSprite1;
    public Sprite chargeSprite;
}