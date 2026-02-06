using System;
using System.Collections.Generic;

namespace TheTower.Game
{
    /// <summary>
    /// 워크샵 업그레이드 항목 정보 정리 (Docs/Workshop_UpgradeItems_ForPrefabs.md 기준).
    /// 표시명, 버튼/ID 이름, Max Lv, 탭. 로비/배틀 UI·레벨 연동용.
    /// </summary>
    public static class UpgradeItemInfo
    {
        public enum Tab { Attack, Defense, Utility, Card }

        /// <summary> 항목 하나: ID(버튼명에서 Buy 제거), 표시명, 버튼이름, 최대레벨, 탭 </summary>
        public readonly struct Entry
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly string ButtonName;
            public readonly int MaxLevel;
            public readonly Tab UpgradeTab;

            public Entry(string id, string displayName, string buttonName, int maxLevel, Tab tab)
            {
                Id = id;
                DisplayName = displayName;
                ButtonName = buttonName;
                MaxLevel = maxLevel;
                UpgradeTab = tab;
            }
        }

        static readonly Entry[] AllEntries = new[]
        {
            // 공격
            new Entry("Damage", "데미지", "BuyDamage", 6000, Tab.Attack),
            new Entry("AttackSpeed", "공격속도", "BuyAttackSpeed", 99, Tab.Attack),
            new Entry("CriticalChance", "치명타율", "BuyCriticalChance", 79, Tab.Attack),
            new Entry("CriticalMultiplier", "치명타 계수", "BuyCriticalMultiplier", 150, Tab.Attack),
            new Entry("Range", "범위", "BuyRange", 200, Tab.Attack),
            new Entry("DamagePerMeter", "데미지/미터", "BuyDamagePerMeter", 200, Tab.Attack),
            new Entry("MultishotChance", "멀티샷 확률", "BuyMultishotChance", 99, Tab.Attack),
            new Entry("MultishotTargets", "멀티샷 표적", "BuyMultishotTargets", 7, Tab.Attack),
            new Entry("RapidFireChance", "고속발사 확률", "BuyRapidFireChance", 85, Tab.Attack),
            new Entry("RapidFireDuration", "고속발사 지속시간", "BuyRapidFireDuration", 99, Tab.Attack),
            new Entry("BounceChance", "바운스샷 확률", "BuyBounceChance", 85, Tab.Attack),
            new Entry("BounceTargets", "바운스샷 표적", "BuyBounceTargets", 7, Tab.Attack),
            new Entry("BounceRange", "바운스샷 범위", "BuyBounceRange", 60, Tab.Attack),
            new Entry("SuperCritChance", "슈퍼치명타 확률", "BuySuperCritChance", 100, Tab.Attack),
            new Entry("SuperCritAmp", "슈퍼치명타 증폭", "BuySuperCritAmp", 120, Tab.Attack),
            // 방어
            new Entry("Health", "체력", "BuyHealth", 6000, Tab.Defense),
            new Entry("Regen", "체력 재생", "BuyRegen", 6000, Tab.Defense),
            new Entry("Defense", "방어율%", "BuyDefense", 99, Tab.Defense),
            new Entry("AbsoluteDefense", "절대 방어", "BuyAbsoluteDefense", 5000, Tab.Defense),
            new Entry("ReflectDamage", "반사 데미지", "BuyReflectDamage", 99, Tab.Defense),
            new Entry("Lifesteal", "흡혈", "BuyLifesteal", 80, Tab.Defense),
            new Entry("KnockbackChance", "넉백 확률", "BuyKnockbackChance", 80, Tab.Defense),
            new Entry("KnockbackStrength", "넉백 강도", "BuyKnockbackStrength", 40, Tab.Defense),
            new Entry("OrbSpeed", "오브 속도", "BuyOrbSpeed", 38, Tab.Defense),
            new Entry("OrbCount", "오브", "BuyOrbCount", 4, Tab.Defense),
            new Entry("ShockwaveSize", "충격파 크기", "BuyShockwaveSize", 35, Tab.Defense),
            new Entry("ShockwaveInterval", "충격파 주기", "BuyShockwaveInterval", 40, Tab.Defense),
            new Entry("MineChance", "지뢰 확률", "BuyMineChance", 50, Tab.Defense),
            new Entry("MineDamage", "지뢰 피해", "BuyMineDamage", 200, Tab.Defense),
            new Entry("MineRadius", "지뢰 반경", "BuyMineRadius", 50, Tab.Defense),
            new Entry("DeathDenial", "죽음의 거부", "BuyDeathDenial", 75, Tab.Defense),
            new Entry("WallHealth", "벽 체력", "BuyWallHealth", 1800, Tab.Defense),
            new Entry("WallRebuild", "벽 재건", "BuyWallRebuild", 300, Tab.Defense),
            // 유틸
            new Entry("CashBonus", "현금 보너스", "BuyCashBonus", 149, Tab.Utility),
            new Entry("CashPerWave", "현금/웨이브", "BuyCashPerWave", 149, Tab.Utility),
            new Entry("CoinPerKill", "동전/킬 보너스", "BuyCoinPerKill", 149, Tab.Utility),
            new Entry("CoinPerWave", "동전/웨이브", "BuyCoinPerWave", 149, Tab.Utility),
            new Entry("FreeAttackUpgrade", "무료 공격업그레이드", "BuyFreeAttackUpgrade", 99, Tab.Utility),
            new Entry("FreeDefenseUpgrade", "무료 방어업그레이드", "BuyFreeDefenseUpgrade", 99, Tab.Utility),
            new Entry("FreeUtilityUpgrade", "무료 유틸리티업그레이드", "BuyFreeUtilityUpgrade", 99, Tab.Utility),
            new Entry("InterestPerWave", "이자/웨이브", "BuyInterestPerWave", 99, Tab.Utility),
            new Entry("HealAmount", "회복량", "BuyHealAmount", 300, Tab.Utility),
            new Entry("MaxHeal", "최대 회복", "BuyMaxHeal", 500, Tab.Utility),
            new Entry("PackageChance", "패키지 확률", "BuyPackageChance", 60, Tab.Utility),
            new Entry("EnemyAtkSkip", "적 공격력 레벨 건너뛰기", "BuyEnemyAtkSkip", 699, Tab.Utility),
            new Entry("EnemyHpSkip", "적 체력 레벨 건너뛰기", "BuyEnemyHpSkip", 699, Tab.Utility),
        };

        static Dictionary<string, Entry> _byId;
        static Dictionary<string, Entry> ById => _byId ??= BuildById();

        static Dictionary<string, Entry> BuildById()
        {
            var d = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in AllEntries)
                d[e.Id] = e;
            return d;
        }

        /// <summary> 전체 항목 (탭 순: 공격 → 방어 → 유틸) </summary>
        public static IReadOnlyList<Entry> All => AllEntries;

        /// <summary> ID로 항목 조회 (대소문자 무시) </summary>
        public static bool TryGet(string id, out Entry entry) => ById.TryGetValue(id ?? "", out entry);

        private const string ButtonNamePrefix = "Buy";

        /// <summary> 버튼 이름(예: BuyDamage)으로 항목 조회 </summary>
        public static bool TryGetByButtonName(string buttonName, out Entry entry)
        {
            entry = default;
            if (string.IsNullOrEmpty(buttonName)) return false;
            string id = buttonName.StartsWith(ButtonNamePrefix, StringComparison.OrdinalIgnoreCase)
                ? buttonName.Substring(ButtonNamePrefix.Length)
                : buttonName;
            return !string.IsNullOrEmpty(id) && TryGet(id, out entry);
        }

        /// <summary> 탭별 항목만 반환 </summary>
        public static IEnumerable<Entry> GetByTab(Tab tab)
        {
            foreach (var e in AllEntries)
                if (e.UpgradeTab == tab) yield return e;
        }
    }
}
