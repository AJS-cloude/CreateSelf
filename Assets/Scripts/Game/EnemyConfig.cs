using UnityEngine;

namespace TheTower.Game
{
    /// <summary>
    /// 자료 기반: 스테이지(티어)·웨이브별 일반 적/보스 체력·공격력·보상.
    /// 스테이지 = 티어(등급), 101웨이브 달성 시 다음 스테이지 해금. 보스는 N웨이브마다 1마리.
    /// </summary>
    public static class EnemyConfig
    {
        // 자료: 등급별 101웨이브 도달 시 다음 등급 해금
        public const int WavesPerStage = 101;
        // 자료: 보스 주기적 등장 (예: 10웨이브마다)
        public const int BossWaveInterval = 10;

        // 일반 적 기본값 (웨이브1, 티어1 기준)
        const float BaseHp = 15f;
        const float BaseDamageToTower = 2f;
        const float BaseMoveSpeed = 2.5f;
        const float BaseCashReward = 3f;

        // 보스 배수 (자료: 보스 체력/공격력 훨씬 높음)
        const float BossHpMultiplier = 12f;
        const float BossDamageMultiplier = 4f;
        const float BossCashMultiplier = 5f;
        const float BossMoveSpeedMultiplier = 0.7f;

        /// <summary> 이 웨이브가 보스 웨이브인지 (10, 20, 30...) </summary>
        public static bool IsBossWave(int wave)
        {
            return wave > 0 && wave % BossWaveInterval == 0;
        }

        /// <summary> 적 최대 체력 (스테이지·웨이브에 따라 성장) </summary>
        public static float GetMaxHp(int wave, int tier, bool isBoss)
        {
            float normal = GetMaxHpNormal(wave, tier);
            return isBoss ? normal * BossHpMultiplier : normal;
        }

        static float GetMaxHpNormal(int wave, int tier)
        {
            float waveFactor = 1f + (wave - 1) * 0.08f;
            float tierFactor = 1f + (tier - 1) * 0.25f;
            return Mathf.Max(1f, BaseHp * waveFactor * tierFactor);
        }

        /// <summary> 타워에게 주는 피해 (자료: 받는 피해 = 적 공격력 × (1 - 방어율) - 절대방어) </summary>
        public static float GetDamageToTower(int wave, int tier, bool isBoss)
        {
            float normal = GetDamageToTowerNormal(wave, tier);
            return isBoss ? normal * BossDamageMultiplier : normal;
        }

        static float GetDamageToTowerNormal(int wave, int tier)
        {
            float waveFactor = 1f + (wave - 1) * 0.06f;
            float tierFactor = 1f + (tier - 1) * 0.2f;
            return Mathf.Max(0.5f, BaseDamageToTower * waveFactor * tierFactor);
        }

        /// <summary> 이동 속도 (월드 스케일 적용, 보스는 약간 느림) </summary>
        public static float GetMoveSpeed(int wave, int tier, bool isBoss)
        {
            float waveFactor = 1f + (wave - 1) * 0.01f;
            float mult = isBoss ? BossMoveSpeedMultiplier : 1f;
            return BaseMoveSpeed * waveFactor * mult * BattleState.WorldScale;
        }

        /// <summary> 처치 시 주는 현금 (자료: 현금 보너스 업그레이드로 배율 적용) </summary>
        public static float GetCashReward(int wave, int tier, bool isBoss)
        {
            float normal = GetCashRewardNormal(wave, tier);
            return isBoss ? normal * BossCashMultiplier : normal;
        }

        static float GetCashRewardNormal(int wave, int tier)
        {
            float waveFactor = 1f + wave * 0.04f;
            float tierFactor = GameData.GetTierCoinMultiplier(tier) / Mathf.Max(1f, GameData.GetTierCoinMultiplier(1));
            return Mathf.Max(0.5f, BaseCashReward * waveFactor * tierFactor);
        }

        /// <summary> 이 웨이브에 스폰할 적 수. 보스 웨이브면 1마리. </summary>
        public static int GetSpawnCount(int wave)
        {
            if (IsBossWave(wave)) return 1;
            return Mathf.Clamp(3 + wave / 2, 3, 25);
        }

        /// <summary> 스폰 간격(초). 보스 웨이브는 한 번만 스폰. </summary>
        public static float GetSpawnInterval(int wave)
        {
            if (IsBossWave(wave)) return 999f;
            return Mathf.Max(0.3f, 0.8f - wave * 0.01f);
        }

        /// <summary> 자료: 반사 데미지 - 일반 적 1%, 보스 50% (보스 3방 버티면 확정 처치) </summary>
        public static float GetReflectDamagePercent(bool isBoss) => isBoss ? 0.5f : 0.01f;
    }
}
