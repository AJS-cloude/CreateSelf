using UnityEngine;
using System.Collections.Generic;

namespace TheTower.Game
{
    /// <summary>
    /// 타워 자동 공격: 사거리 내 적을 향해 발사체를 쏘고, 적중 시 데미지.
    /// </summary>
    public class TowerAttack : MonoBehaviour
    {
        [Tooltip("타워 중심. 없으면 TowerCenter 자동 탐색")]
        [SerializeField] Transform towerRoot;

        float nextAttackTime;

        void Start()
        {
            if (towerRoot == null)
            {
                var go = GameObject.Find("TowerCenter");
                if (go != null) towerRoot = go.transform;
                else
                {
                    var g = new GameObject("TowerCenter");
                    g.transform.position = BattleState.TowerSpawnPosition;
                    towerRoot = g.transform;
                }
            }
        }

        void Update()
        {
            var s = BattleState.Instance;
            if (s == null) return;
            // 자료 기반: 공격속도 99 max, 발사 주기 = 1/APS
            float interval = s.TowerAttackInterval;
            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + interval;

            float range = s.TowerRange;
            var enemies = new List<Enemy>();
            foreach (var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                if (e.IsDead) continue;
                float d = Vector3.Distance(towerRoot.position, e.transform.position);
                if (d <= range) enemies.Add(e);
            }
            if (enemies.Count == 0) return;
            var target = enemies[0];
            float dmg = (float)s.Damage;
            SpawnProjectile(target, dmg);
        }

        void SpawnProjectile(Enemy target, float damage)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Projectile";
            go.transform.rotation = Quaternion.Euler(0, 0, 0);
            go.transform.localScale = Vector3.one * BattleState.WorldScale * 0.8f;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = new Color(1f, 0.9f, 0.2f) };
            var p = go.AddComponent<Projectile>();
            p.Fire(towerRoot, target, damage);
        }
    }
}
