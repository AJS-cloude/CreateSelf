using UnityEngine;
using System;

namespace TheTower.Game
{
    /// <summary>
    /// 웨이브/티어 기준 스탯을 가지며 타워 쪽으로 이동. 타워 공격에 피해를 받고, 처치 시 현금 지급.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        public float MaxHp { get; private set; }
        public float Hp { get; private set; }
        public float DamageToTower { get; private set; }
        public float CashReward { get; private set; }
        /// <summary> 자료: 보스는 반사 데미지 50%, 일반 1%. 보스 3방 버티면 확정 처치 </summary>
        public bool IsBoss { get; private set; }

        Transform tower;
        float moveSpeed;
        bool dead;

        /// <summary> 처치 시 (현금 지급 후) 호출 </summary>
        public event Action<Enemy> OnDeath;

        public void Init(int wave, int tier, Transform towerRoot, bool isBoss = false)
        {
            tower = towerRoot;
            IsBoss = isBoss;
            MaxHp = EnemyConfig.GetMaxHp(wave, tier, isBoss);
            Hp = MaxHp;
            DamageToTower = EnemyConfig.GetDamageToTower(wave, tier, isBoss);
            moveSpeed = EnemyConfig.GetMoveSpeed(wave, tier, isBoss);
            CashReward = EnemyConfig.GetCashReward(wave, tier, isBoss);
            dead = false;
        }

        void Update()
        {
            if (dead || tower == null) return;
            Vector3 dir = (tower.position - transform.position).normalized;
            dir.z = 0;
            transform.position += dir * (moveSpeed * Time.deltaTime);
        }

        public void TakeDamage(float amount)
        {
            if (dead) return;
            Hp -= amount;
            if (Hp <= 0)
                Die();
        }

        void Die()
        {
            if (dead) return;
            dead = true;
            if (BattleState.Instance != null)
                BattleState.Instance.Cash += CashReward;
            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }

        /// <summary> 타워에 도달했을 때 호출. 타워에게 피해 적용 후, 자료 기반 반사 데미지(일반 1%/보스 50%) 적용. </summary>
        public void ApplyDamageToTower()
        {
            if (BattleState.Instance == null) return;
            double dmg = DamageToTower;
            double defPct = BattleState.Instance.UpgradeDefensePercent * 0.005; // 1레벨당 0.5%
            double defAbs = BattleState.Instance.UpgradeDefensePercent * 0;  // TODO: 절대방어 업그레이드 추가 시
            dmg = Math.Max(0, dmg * (1 - defPct) - defAbs);
            BattleState.Instance.TowerHealth -= dmg;
            if (BattleState.Instance.TowerHealth <= 0)
                BattleState.Instance.TowerHealth = 0;

            // 자료: 반사 데미지 - 적이 타워 공격 시 적 최대체력 1% 피해 (보스 50%). 보스 3방 버티면 확정 처치
            float reflectPct = EnemyConfig.GetReflectDamagePercent(IsBoss);
            TakeDamage(MaxHp * reflectPct);
        }

        public bool IsDead => dead;
    }
}
