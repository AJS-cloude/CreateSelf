using UnityEngine;

namespace TheTower.Game
{
    /// <summary>
    /// 로비/배틀 공용 데이터. 나중에 세이브·로드 연동.
    /// </summary>
    public static class GameData
    {
        public static long Coins { get; set; } = 164;
        public static int Gems { get; set; } = 263;
        public static int PowerStones { get; set; } = 0;

        /// <summary> 현재 선택된 난이도 티어 (1~21) </summary>
        public static int CurrentTier { get; set; } = 1;

        /// <summary> 현재 티어에서 달성한 최고 웨이브 </summary>
        public static int HighestWave { get; set; } = 104;

        /// <summary> 코인 보너스 배율 (티어/업그레이드 반영) </summary>
        public static float CoinBonusMultiplier => GetTierCoinMultiplier(CurrentTier);

        /// <summary> 티어별 코인 배수 (나무위키 기준) </summary>
        public static float GetTierCoinMultiplier(int tier)
        {
            if (tier <= 0) return 1f;
            // 1~14: 1.0, 1.8, 2.6, 3.4, 4.2, 5.0, 5.8, 6.6, 7.5, 8.7, 10.3, 12.2, 14.7, 17.6
            float[] multipliers = { 1f, 1.8f, 2.6f, 3.4f, 4.2f, 5f, 5.8f, 6.6f, 7.5f, 8.7f, 10.3f, 12.2f, 14.7f, 17.6f };
            int index = Mathf.Clamp(tier - 1, 0, multipliers.Length - 1);
            return multipliers[index];
        }

        public static int MaxTier => 21;
    }
}
