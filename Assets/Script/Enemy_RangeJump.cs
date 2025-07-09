using UnityEngine;

/// <summary>
/// 敵キャラクターの挙動を制御するクラス。
/// 移動、攻撃、ダメージ処理、アニメーション、死亡処理などを担当する。
/// </summary>
public class Enemy_RangeJump : Enemy_Range, ICharacter
{
    protected override void Move()
    {
        JumpMove();

        // 着地判定
        if (IsGround)
        {
            // プレイヤーの位置に応じて移動方向・向きを決定
            LookPlayer();
            JumpEnd();
            float distanceToPlayer = GetPlayerDistance();
            if (distanceToPlayer > attackStartDistance)
            {
                transform.position += dir * Time.deltaTime;
            }
            else
            {
                TryJump();
                if (currentJumpVelocity == maxJumpVelocity)
                {
                    StartAttackFlg();
                }
            }
        }
        else
        {
            transform.position += dir * Time.deltaTime;
        }

    }


}
