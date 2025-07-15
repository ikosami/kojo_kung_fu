using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵キャラクターの挙動を制御するクラス。
/// 移動、攻撃、ダメージ処理、アニメーション、死亡処理などを担当する。
/// </summary>
public class Enemy_Range : Enemy, ICharacter
{
    [SerializeField] Bullet bulletPrefab;
    [SerializeField] Transform bulletPoint;

    [SerializeField] protected Sprite attackWaitSprite;
    protected override void HandleAttack()
    {
        //
        if (!isAttack) { return; }

        attackTime += Time.deltaTime;

        if (attackTime < 0.5f)
        {
            // 攻撃前の溜め
            if (image.sprite != attackWaitSprite)
                image.sprite = attackWaitSprite;
        }
        else if (attackTime < 1f)
        {
            // 攻撃発動
            if (isAttackDamage)
            {
                SoundManager.Instance.Play("throwing");

                var shuri = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.identity, Reference.Instance.stageRect);
                shuri.move.x *= transform.localScale.x;
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
