using UnityEngine;

namespace TheTower.Game
{
    /// <summary>
    /// 자료 기반: 인게임 업그레이드 표시명·비용. 공격/방어/유틸리티 3계열.
    /// </summary>
    public static class UpgradeConfig
    {
        #region Tab Display Names

        public const string TabAttack = "공격 업그레이드";
        /// <summary> 자료: 방어 탭 표시명 </summary>
        public const string TabDefense = "방어 업그레이드";
        /// <summary> 자료: 유틸리티 탭 표시명 </summary>
        public const string TabUtility = "유틸리티 업그레이드";

        #endregion

        #region Upgrade Display Names (3.1~3.3)

        public const string Damage = "데미지";
        public const string AttackSpeed = "공격속도";
        public const string CriticalChance = "치명타율";
        public const string CriticalMultiplier = "치명타 계수";
        public const string Range = "범위";

        // 자료 3.2 방어
        public const string Health = "체력";
        public const string HealthRegen = "체력 재생";
        public const string DefensePercent = "방어율%";
        public const string AbsoluteDefense = "절대 방어";

        // 자료 3.3 유틸리티
        public const string CashBonus = "현금 보너스";
        public const string CashPerWave = "현금/웨이브";
        public const string CoinPerKill = "동전/킬 보너스";
        public const string CoinPerWave = "동전/웨이브";

        #endregion

        #region Cost Formulas

        private const int DefaultCostBase = 10;
        private const int DefaultCostPerLevel = 5;

        /// <summary> 버튼/키 이름으로 표시명 반환 (씬 바인딩 시 제목 라벨용) </summary>
        public static string GetDisplayName(string buttonOrKey)
        {
            if (string.IsNullOrEmpty(buttonOrKey)) return "";
            string k = buttonOrKey.ToUpperInvariant();
            if (k.Contains("DAMAGE") && !k.Contains("ATTACKSPEED")) return Damage;
            if (k.Contains("ATTACKSPEED") || k.Contains("ATTACK_SPEED")) return AttackSpeed;
            if (k.Contains("CRITICAL") && k.Contains("CHANCE")) return CriticalChance;
            if (k.Contains("RANGE")) return Range;
            if (k.Contains("HEALTH") && !k.Contains("REGEN")) return Health;
            if (k.Contains("REGEN") || k.Contains("HEALTHREGEN")) return HealthRegen;
            if (k.Contains("DEFENSE")) return DefensePercent;
            if (k.Contains("CASHBONUS")) return CashBonus;
            if (k.Contains("CASHPERWAVE") || k.Contains("CASH_PER")) return CashPerWave;
            if (k.Contains("COIN")) return CoinPerKill;
            return buttonOrKey;
        }

        /// <summary> 자료 기반 비용: 데미지 (기본 10 + 레벨당 5) </summary>
        public static int GetDamageCost(int level) => 10 + level * 5;
        /// <summary> 자료 기반 비용: 공격속도 (기본 5 + 레벨당 3) </summary>
        public static int GetAttackSpeedCost(int level) => 5 + level * 3;
        /// <summary> 자료 기반 비용: 범위 </summary>
        public static int GetRangeCost(int level) => 8 + level * 4;
        /// <summary> 자료 기반 비용: 체력 </summary>
        public static int GetHealthCost(int level) => 10 + level * 5;
        /// <summary> 자료 기반 비용: 체력 재생 </summary>
        public static int GetHealthRegenCost(int level) => 5 + level * 3;
        /// <summary> 자료 기반 비용: 방어율% </summary>
        public static int GetDefensePercentCost(int level) => 6 + level * 3;
        /// <summary> 자료 기반 비용: 현금 보너스 </summary>
        public static int GetCashBonusCost(int level) => 4 + level * 2;
        /// <summary> 자료 기반 비용: 현금/웨이브 </summary>
        public static int GetCashPerWaveCost(int level) => 5 + level * 2;

        /// <summary> 업그레이드 ID별 코인 비용 (로비 워크샵용). 미정의 ID는 10 + 레벨×5. </summary>
        public static int GetCostForUpgrade(string upgradeId, int level)
        {
            if (string.IsNullOrEmpty(upgradeId)) return 0;
            switch (upgradeId.ToUpperInvariant())
            {
                case "DAMAGE": return GetDamageCost(level);
                case "ATTACKSPEED": return GetAttackSpeedCost(level);
                case "RANGE": return GetRangeCost(level);
                case "HEALTH": return GetHealthCost(level);
                case "REGEN": return GetHealthRegenCost(level);
                case "DEFENSE": return GetDefensePercentCost(level);
                case "CASHBONUS": return GetCashBonusCost(level);
                case "CASHPERWAVE": return GetCashPerWaveCost(level);
                default: return DefaultCostBase + level * DefaultCostPerLevel;
            }
        }

        #endregion
    }
}
