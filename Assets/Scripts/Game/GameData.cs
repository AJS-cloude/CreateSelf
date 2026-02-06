using UnityEngine;

namespace TheTower.Game
{
    /// <summary>
    /// 로비/배틀 공용 데이터. 워크샵 레벨 저장 및 배틀 시작 시 BattleState 반영.
    /// 추후 세이브·로드 연동 시 이 클래스를 직렬화 대상으로 사용.
    /// </summary>
    public static class GameData
    {
        #region Currency & Progress

        /// <summary> 동전 (로비 워크샵·진행용). 로비/배틀 공용, 저장·로드 연동. </summary>
        public static long Coins { get; set; }

        /// <summary> 보석. 로비/배틀 공용, 저장·로드 연동. </summary>
        public static int Gems { get; set; }

        /// <summary> 파워스톤. 로비/배틀 공용, 저장·로드 연동. </summary>
        public static int PowerStones { get; set; }

        /// <summary> 현재 선택된 난이도 티어 (1 ~ MaxTier) </summary>
        public static int CurrentTier { get; set; } = 1;

        /// <summary> 현재 티어에서 달성한 최고 웨이브 </summary>
        public static int HighestWave { get; set; }

        /// <summary> 코인 보너스 배율 (티어/업그레이드 반영) </summary>
        public static float CoinBonusMultiplier => GetTierCoinMultiplier(CurrentTier);

        public static int MaxTier => 21;

        /// <summary> 티어별 코인 배수 (나무위키 기준, 1~14 구간) </summary>
        public static float GetTierCoinMultiplier(int tier)
        {
            if (tier <= 0) return 1f;
            const int TierOneBasedIndex = 1;
            int index = Mathf.Clamp(tier - TierOneBasedIndex, 0, TierCoinMultipliers.Length - 1);
            return TierCoinMultipliers[index];
        }

        private static readonly float[] TierCoinMultipliers =
            { 1f, 1.8f, 2.6f, 3.4f, 4.2f, 5f, 5.8f, 6.6f, 7.5f, 8.7f, 10.3f, 12.2f, 14.7f, 17.6f };

        const string PrefsKeyCoins = "TheTower_Coins";
        const string PrefsKeyGems = "TheTower_Gems";
        const string PrefsKeyPowerStones = "TheTower_PowerStones";
        const string PrefsKeyTier = "TheTower_CurrentTier";
        const string PrefsKeyHighestWave = "TheTower_HighestWave";

        /// <summary> 저장된 재화·진행도를 불러옵니다. 씬 진입 시(로비/배틀) 호출. 없으면 0 유지. </summary>
        public static void Load()
        {
            if (PlayerPrefs.HasKey(PrefsKeyCoins)) Coins = long.TryParse(PlayerPrefs.GetString(PrefsKeyCoins), out var c) ? c : 0;
            if (PlayerPrefs.HasKey(PrefsKeyGems)) Gems = PlayerPrefs.GetInt(PrefsKeyGems, 0);
            if (PlayerPrefs.HasKey(PrefsKeyPowerStones)) PowerStones = PlayerPrefs.GetInt(PrefsKeyPowerStones, 0);
            if (PlayerPrefs.HasKey(PrefsKeyTier)) CurrentTier = Mathf.Clamp(PlayerPrefs.GetInt(PrefsKeyTier), 1, MaxTier);
            if (PlayerPrefs.HasKey(PrefsKeyHighestWave)) HighestWave = Mathf.Max(0, PlayerPrefs.GetInt(PrefsKeyHighestWave));
            LoadWorkshopLevels();
        }

        /// <summary> 재화·진행도·워크샵 레벨을 저장합니다. 씬 이탈·종료 시 호출. </summary>
        public static void Save()
        {
            PlayerPrefs.SetString(PrefsKeyCoins, Coins.ToString());
            PlayerPrefs.SetInt(PrefsKeyGems, Gems);
            PlayerPrefs.SetInt(PrefsKeyPowerStones, PowerStones);
            PlayerPrefs.SetInt(PrefsKeyTier, CurrentTier);
            PlayerPrefs.SetInt(PrefsKeyHighestWave, HighestWave);
            SaveWorkshopLevels();
            PlayerPrefs.Save();
        }

        static void LoadWorkshopLevels()
        {
            if (PlayerPrefs.HasKey("ws_Damage")) WorkshopDamage = PlayerPrefs.GetInt("ws_Damage", 0);
            if (PlayerPrefs.HasKey("ws_AttackSpeed")) WorkshopAttackSpeed = PlayerPrefs.GetInt("ws_AttackSpeed", 0);
            if (PlayerPrefs.HasKey("ws_Health")) WorkshopHealth = PlayerPrefs.GetInt("ws_Health", 0);
            if (PlayerPrefs.HasKey("ws_HealthRegen")) WorkshopHealthRegen = PlayerPrefs.GetInt("ws_HealthRegen", 0);
            if (PlayerPrefs.HasKey("ws_Defense")) WorkshopDefensePercent = PlayerPrefs.GetInt("ws_Defense", 0);
            if (PlayerPrefs.HasKey("ws_CashBonus")) WorkshopCashBonus = PlayerPrefs.GetInt("ws_CashBonus", 0);
            if (PlayerPrefs.HasKey("ws_CashPerWave")) WorkshopCashPerWave = PlayerPrefs.GetInt("ws_CashPerWave", 0);
            if (PlayerPrefs.HasKey("ws_CriticalChance")) WorkshopCriticalChance = PlayerPrefs.GetInt("ws_CriticalChance", 0);
            if (PlayerPrefs.HasKey("ws_Range")) WorkshopRange = PlayerPrefs.GetInt("ws_Range", 0);
        }

        static void SaveWorkshopLevels()
        {
            PlayerPrefs.SetInt("ws_Damage", WorkshopDamage);
            PlayerPrefs.SetInt("ws_AttackSpeed", WorkshopAttackSpeed);
            PlayerPrefs.SetInt("ws_Health", WorkshopHealth);
            PlayerPrefs.SetInt("ws_HealthRegen", WorkshopHealthRegen);
            PlayerPrefs.SetInt("ws_Defense", WorkshopDefensePercent);
            PlayerPrefs.SetInt("ws_CashBonus", WorkshopCashBonus);
            PlayerPrefs.SetInt("ws_CashPerWave", WorkshopCashPerWave);
            PlayerPrefs.SetInt("ws_CriticalChance", WorkshopCriticalChance);
            PlayerPrefs.SetInt("ws_Range", WorkshopRange);
        }

        #endregion

        #region Workshop (Permanent Upgrade) Levels

        public static int WorkshopDamage { get; set; }
        public static int WorkshopAttackSpeed { get; set; }
        public static int WorkshopCriticalChance { get; set; }
        public static int WorkshopRange { get; set; }
        public static int WorkshopHealth { get; set; }
        public static int WorkshopHealthRegen { get; set; }
        public static int WorkshopDefensePercent { get; set; }
        public static int WorkshopCashBonus { get; set; }
        public static int WorkshopCashPerWave { get; set; }

        /// <summary> 업그레이드 ID로 워크샵 레벨 조회. UpgradeItemInfo.Id와 일치해야 함. </summary>
        public static int GetWorkshopLevel(string upgradeId)
        {
            if (string.IsNullOrEmpty(upgradeId)) return 0;
            switch (upgradeId.ToUpperInvariant())
            {
                case "DAMAGE": return WorkshopDamage;
                case "ATTACKSPEED": return WorkshopAttackSpeed;
                case "CRITICALCHANCE": return WorkshopCriticalChance;
                case "RANGE": return WorkshopRange;
                case "HEALTH": return WorkshopHealth;
                case "REGEN": return WorkshopHealthRegen;
                case "DEFENSE": return WorkshopDefensePercent;
                case "CASHBONUS": return WorkshopCashBonus;
                case "CASHPERWAVE": return WorkshopCashPerWave;
                default: return 0;
            }
        }

        /// <summary> 로비에서 워크샵 업그레이드 구매 시 호출. ID와 레벨 설정. </summary>
        public static void SetWorkshopLevel(string upgradeId, int level)
        {
            if (string.IsNullOrEmpty(upgradeId)) return;
            level = Mathf.Max(0, level);
            switch (upgradeId.ToUpperInvariant())
            {
                case "DAMAGE": WorkshopDamage = level; break;
                case "ATTACKSPEED": WorkshopAttackSpeed = level; break;
                case "CRITICALCHANCE": WorkshopCriticalChance = level; break;
                case "RANGE": WorkshopRange = level; break;
                case "HEALTH": WorkshopHealth = level; break;
                case "REGEN": WorkshopHealthRegen = level; break;
                case "DEFENSE": WorkshopDefensePercent = level; break;
                case "CASHBONUS": WorkshopCashBonus = level; break;
                case "CASHPERWAVE": WorkshopCashPerWave = level; break;
            }
        }

        /// <summary> 로비에서 코인으로 워크샵 1레벨 구매 시도. 비용 부족·최대 레벨이면 false. </summary>
        public static bool TryBuyWorkshopLevel(string upgradeId, int costPerLevel, int maxLevel)
        {
            int current = GetWorkshopLevel(upgradeId);
            if (current >= maxLevel || costPerLevel <= 0 || Coins < costPerLevel)
                return false;
            Coins -= costPerLevel;
            SetWorkshopLevel(upgradeId, current + 1);
            return true;
        }

        #endregion
    }
}
