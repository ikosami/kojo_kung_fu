using UnityEngine;

/// <summary>
/// 敵キャラクターの挙動を制御するクラス。
/// 静止状態でダメージを受け、倒される機能を提供する。
/// </summary>
public class Enemy_Obj : Enemy, ICharacter
{
    [SerializeField] RectTransform[] attackRects;

    public override bool CanLook
    {
        get
        {
            return !isAttack;
        }
    }

    /// <summary>
    /// 初期位置を設定しない（初期高さを調整しない）
    /// </summary>
    protected override void SetInitPos()
    {
        // 初期高さを調整しない
    }

    /// <summary>
    /// 移動処理を無効化（静止状態を維持）
    /// </summary>
    protected override void Move()
    {
        // 移動しない（静止状態を維持）
    }

    protected override void HandleAttack()
    {
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
            // 攻撃発動（移動なし）
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
