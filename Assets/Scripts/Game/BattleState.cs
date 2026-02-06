using UnityEngine;

namespace TheTower.Game
{
    /// <summary>
    /// 한 판(런) 동안의 배틀 상태. 라운드 시작 시 리셋, 종료 시 GameData에 반영.
    /// </summary>
    public class BattleState
    {
        public static BattleState Instance { get; private set; }

        /// <summary> 인게임 현금 (업그레이드에 사용) </summary>
        public double Cash { get; set; } = 0;

        /// <summary> 현재 웨이브 </summary>
        public int Wave { get; set; } = 1;

        /// <summary> 타워 현재 체력 </summary>
        public double TowerHealth { get; set; } = 100;

        /// <summary> 타워 최대 체력 </summary>
        public double TowerMaxHealth { get; set; } = 100;

        /// <summary> 타워 공격력 (기본 데미지) </summary>
        public double Damage { get; set; } = 10;

        /// <summary> 초당 체력 재생 </summary>
        public double HealthRegenPerSec { get; set; } = 1;

        /// <summary> 게임 속도 배율 (1, 1.5, 2, ...) </summary>
        public float GameSpeed { get; set; } = 1f;

        /// <summary> 인게임 업그레이드 레벨 (공격) </summary>
        public int UpgradeDamage { get; set; } = 0;
        public int UpgradeAttackSpeed { get; set; } = 0;
        public int UpgradeCriticalChance { get; set; } = 0;
        public int UpgradeRange { get; set; } = 0;

        /// <summary> 인게임 업그레이드 레벨 (방어) </summary>
        public int UpgradeHealth { get; set; } = 0;
        public int UpgradeHealthRegen { get; set; } = 0;
        public int UpgradeDefensePercent { get; set; } = 0;

        /// <summary> 인게임 업그레이드 레벨 (유틸) </summary>
        public int UpgradeCashBonus { get; set; } = 0;
        public int UpgradeCashPerWave { get; set; } = 0;

        /// <summary> 월드 전체 스케일 (1/5) </summary>
        public const float WorldScale = 0.2f;

        private const double BaseDamage = 10;
        private const double BaseMaxHp = 100;
        private const double BaseRegenPerSec = 1;

        /// <summary> 타워(캐릭터) 기본 소환 위치 (Y축 0.5 올림) </summary>
        public static readonly Vector3 TowerSpawnPosition = new Vector3(0f, 0.5f, 0f);

        /// <summary> 타워 공격 사거리 (기본 5 + 업그레이드, 스케일 적용) </summary>
        public float TowerRange => (5f + UpgradeRange * 0.4f) * WorldScale;

        /// <summary> 자료: 공격속도 99 max = 발사 속도. 초당 발사 횟수(APS), 레벨당 +4%. </summary>
        public float TowerAttacksPerSecond => 2f * (1f + UpgradeAttackSpeed * 0.04f);

        /// <summary> 자료 기반 발사 주기(초). TowerAttacksPerSecond 역수. </summary>
        public float TowerAttackInterval => 1f / Mathf.Max(0.05f, TowerAttacksPerSecond);

        /// <summary> 배틀 시작 시 호출. 로비(워크샵) 레벨을 반영한 초기 상태로 설정. </summary>
        public static void ResetForNewRun()
        {
            int d = GameData.WorkshopDamage;
            int a = GameData.WorkshopAttackSpeed;
            int h = GameData.WorkshopHealth;
            int r = GameData.WorkshopHealthRegen;
            int def = GameData.WorkshopDefensePercent;
            int cb = GameData.WorkshopCashBonus;
            int cw = GameData.WorkshopCashPerWave;

            Instance = new BattleState
            {
                Cash = 0,
                Wave = 1,
                GameSpeed = 1f,
                UpgradeDamage = d,
                UpgradeAttackSpeed = a,
                UpgradeCriticalChance = GameData.WorkshopCriticalChance,
                UpgradeRange = GameData.WorkshopRange,
                UpgradeHealth = h,
                UpgradeHealthRegen = r,
                UpgradeDefensePercent = def,
                UpgradeCashBonus = cb,
                UpgradeCashPerWave = cw,
                Damage = BaseDamage + d * 2,
                TowerMaxHealth = BaseMaxHp + h * 10,
                HealthRegenPerSec = BaseRegenPerSec + r * 0.5,
                TowerHealth = BaseMaxHp + h * 10
            };
        }

        public static void EnsureExists()
        {
            if (Instance == null) ResetForNewRun();
        }

        /// <summary> 업그레이드 ID별 현재 레벨. UpgradeItemInfo.Id와 일치. 배틀 UI·비용 표시용. </summary>
        public int GetUpgradeLevel(string upgradeId)
        {
            if (string.IsNullOrEmpty(upgradeId)) return 0;
            switch (upgradeId.ToUpperInvariant())
            {
                case "DAMAGE": return UpgradeDamage;
                case "ATTACKSPEED": return UpgradeAttackSpeed;
                case "CRITICALCHANCE": return UpgradeCriticalChance;
                case "RANGE": return UpgradeRange;
                case "HEALTH": return UpgradeHealth;
                case "REGEN": return UpgradeHealthRegen;
                case "DEFENSE": return UpgradeDefensePercent;
                case "CASHBONUS": return UpgradeCashBonus;
                case "CASHPERWAVE": return UpgradeCashPerWave;
                default: return 0;
            }
        }
    }
}
