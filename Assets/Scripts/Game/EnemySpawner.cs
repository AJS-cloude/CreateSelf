using UnityEngine;
using System.Collections.Generic;

namespace TheTower.Game
{
    /// <summary>
    /// 배틀 씬에서 웨이브별로 적 스폰. 전원 처치 시 다음 웨이브 시작 + 현금/동전 보너스.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("필수")]
        [Tooltip("타워(중앙) Transform. 없으면 (0,0,0) 사용")]
        [SerializeField] Transform towerRoot;
        [Tooltip("적 프리팹. 없으면 런타임에 큐브로 생성")]
        [SerializeField] GameObject enemyPrefab;

        [Header("스폰 위치")]
        [Tooltip("타워에서 이 거리만큼 떨어진 원 위에서 스폰 (월드 스케일 1/5 적용됨)")]
        [SerializeField] float spawnRadius = 10f;

        float nextSpawnTime;
        int toSpawnThisWave;
        int spawnedThisWave;
        readonly List<Enemy> alive = new List<Enemy>();

        void Start()
        {
            if (towerRoot == null)
            {
                var existing = GameObject.Find("TowerCenter");
                if (existing != null)
                {
                    towerRoot = existing.transform;
                    AddTowerVisual(towerRoot);
                }
                else
                {
                    var go = new GameObject("TowerCenter");
                    go.transform.position = BattleState.TowerSpawnPosition;
                    towerRoot = go.transform;
                    AddTowerVisual(towerRoot);
                }
            }
            StartWave();
        }

        void Update()
        {
            if (BattleState.Instance == null) return;

            // 이번 웨이브 스폰
            if (spawnedThisWave < toSpawnThisWave && Time.time >= nextSpawnTime)
            {
                SpawnOne();
                spawnedThisWave++;
                nextSpawnTime = Time.time + EnemyConfig.GetSpawnInterval(BattleState.Instance.Wave);
            }

            // 타워 도달 체크 (간단히 거리로, 월드 스케일 적용)
            float reachDist = 0.8f * BattleState.WorldScale;
            for (int i = alive.Count - 1; i >= 0; i--)
            {
                var e = alive[i];
                if (e == null) { alive.RemoveAt(i); continue; }
                float d = Vector3.Distance(e.transform.position, towerRoot.position);
                if (d <= reachDist)
                {
                    e.ApplyDamageToTower();
                    e.OnDeath -= OnEnemyDeath;
                    alive.RemoveAt(i);
                    Destroy(e.gameObject);
                }
            }

            // 전원 처치 + 전원 스폰 완료 → 다음 웨이브
            if (alive.Count == 0 && spawnedThisWave >= toSpawnThisWave && toSpawnThisWave > 0)
            {
                CompleteWave();
            }
        }

        void StartWave()
        {
            var s = BattleState.Instance;
            if (s == null) return;
            toSpawnThisWave = EnemyConfig.GetSpawnCount(s.Wave);
            spawnedThisWave = 0;
            nextSpawnTime = Time.time;
        }

        void SpawnOne()
        {
            var s = BattleState.Instance;
            if (s == null) return;
            int tier = GameData.CurrentTier;
            int wave = s.Wave;
            bool isBoss = EnemyConfig.IsBossWave(wave);

            GameObject go;
            if (enemyPrefab != null)
                go = Instantiate(enemyPrefab);
            else
                go = CreateDefaultEnemy(isBoss);

            go.name = isBoss ? "Boss" : "Enemy";
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = spawnRadius * BattleState.WorldScale;
            go.transform.position = towerRoot.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            float scaleMult = isBoss ? 1.5f : 1f;
            go.transform.localScale = Vector3.one * BattleState.WorldScale * scaleMult;

            var enemy = go.GetComponent<Enemy>();
            if (enemy == null) enemy = go.AddComponent<Enemy>();
            enemy.Init(wave, tier, towerRoot, isBoss);
            enemy.OnDeath += OnEnemyDeath;
            alive.Add(enemy);
        }

        void OnEnemyDeath(Enemy e)
        {
            alive.Remove(e);
        }

        void CompleteWave()
        {
            var s = BattleState.Instance;
            if (s == null) return;
            // 참고: 웨이브 완료 시 현금/동전 지급
            s.Cash += 5 + s.UpgradeCashPerWave * 2;  // 웨이브 완료 보너스
            GameData.Coins += (int)(2 * GameData.CoinBonusMultiplier); // 동전 보너스
            s.Wave++;
            StartWave();
        }

        static GameObject CreateDefaultEnemy(bool isBoss)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.transform.rotation = Quaternion.Euler(0, 0, 0);
            var r = go.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = isBoss ? new Color(0.6f, 0.1f, 0.5f) : new Color(0.9f, 0.2f, 0.2f) };
            return go;
        }

        static void AddTowerVisual(Transform parent)
        {
            if (parent.Find("TowerVisual") != null) return;
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "TowerVisual";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one * BattleState.WorldScale;
            go.transform.rotation = Quaternion.Euler(0, 0, 0);
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = new Color(0.2f, 0.5f, 1f) };
        }
    }
}
