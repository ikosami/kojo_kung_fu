using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵キャラクターの挙動を制御するクラス。
/// 移動、攻撃、ダメージ処理、アニメーション、死亡処理などを担当する。
/// </summary>
public class Enemy_MolFire : Enemy, ICharacter
{
    [SerializeField] Sprite attackSprite2;
    float attackSpriteTimer = 0;

    protected override void HandleAttack()
    {
        //
        if (!isAttack) { return; }


        if (attackTime < 0.5f)
        {
            // 攻撃前の溜め
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;

            attackTime += Time.deltaTime;
            if (attackTime >= 0.5f)
            {
                currentJumpVelocity = maxJumpVelocity;
                attackTime = 0.51f;
                attackSpriteTimer = 0;
            }
        }
        else if (attackTime < 1f)
        {
            // 攻撃発動
            if (isAttackDamage)
            {
                if (Util.IsHitPlayer(attackRange))
                {
                    SoundManager.Instance.Play("enemy_attack");
                    Reference.Instance.player.TakeDamage(1);
                    isAttackDamage = false;
                    isAttackDamage = false;
                }
            }

            var spriteChangeTime = 0.3f;

            attackSpriteTimer += Time.deltaTime;

            if (attackSpriteTimer > spriteChangeTime)
            {
                attackSpriteTimer -= spriteChangeTime;
            }

            var sprite = attackSpriteTimer < spriteChangeTime / 2 ? attackSprite1 : attackSprite2;

            if (image.sprite != sprite)
                image.sprite = sprite;



            Jump();
        }
        else
        {
            // 攻撃後の戻り
            if (image.sprite != normalSprite1)
                image.sprite = normalSprite1;
            // 攻撃終了
            isAttack = false;
            spriteChangeTimer = 0;
        }
    }

    private void Jump()
    {
        JumpMove();

        // 着地判定
        if (IsGround)
        {
            // プレイヤーの位置に応じて移動方向・向きを決定
            LookPlayer();
            JumpEnd();
            attackTime = 1.1f;
        }
        else
        {
            transform.position += dir * Time.deltaTime;
        }
    }
}
