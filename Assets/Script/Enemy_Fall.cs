using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵キャラクターの挙動を制御するクラス。
/// 移動、攻撃、ダメージ処理、アニメーション、死亡処理などを担当する。
/// </summary>
public class Enemy_Fall : Enemy, ICharacter
{

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
            transform.position += (dir + new Vector3(0, -100, 0)) * Time.deltaTime;
            if (IsGround)
            {
                attackTime = 1f; // 攻撃時間をリセット
            }
            else
            {

                attackTime = 0.5f; // 攻撃時間をリセット       
            }

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
}
