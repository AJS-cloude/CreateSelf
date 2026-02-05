using UnityEngine;

namespace TheTower.Game
{
    /// <summary>
    /// 타워에서 적을 향해 날아가는 발사체. 도달 시 데미지 적용 후 제거.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        const float Speed = 8f; // 스케일 적용된 월드 기준

        Enemy target;
        float damage;
        bool hit;

        public void Fire(Transform from, Enemy to, float dmg)
        {
            transform.position = from.position;
            target = to;
            damage = dmg;
            hit = false;
        }

        void Update()
        {
            if (hit) return;
            if (target == null || target.IsDead)
            {
                Destroy(gameObject);
                return;
            }
            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.z = 0;
            float dist = toTarget.magnitude;
            float move = Speed * Time.deltaTime;
            if (move >= dist)
            {
                target.TakeDamage(damage);
                hit = true;
                Destroy(gameObject);
                return;
            }
            transform.position += toTarget.normalized * move;
        }
    }
}
