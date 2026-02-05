using UnityEngine;

namespace TheTower.Game
{
    /// <summary>
    /// 플레이어블 캐릭터(타워) 중심으로 공격 가능 거리를 반지름으로 하는 원을 LineRenderer로 그림.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TowerRangeIndicator : MonoBehaviour
    {
        [Tooltip("타워 중심. 비어 있으면 씬에서 TowerCenter 검색")]
        [SerializeField] Transform towerRoot;

        [Header("라인 설정")]
        [SerializeField] int segments = 64;
        [SerializeField] float lineWidth = 0.02f;
        [SerializeField] Color lineColor = new Color(0.3f, 0.6f, 1f, 0.8f);

        LineRenderer line;

        void Start()
        {
            if (towerRoot == null)
            {
                var go = GameObject.Find("TowerCenter");
                if (go != null) towerRoot = go.transform;
            }
            line = GetComponent<LineRenderer>();
            SetupLine();
        }

        void LateUpdate()
        {
            if (towerRoot == null)
            {
                var go = GameObject.Find("TowerCenter");
                if (go != null) towerRoot = go.transform;
            }
            if (towerRoot == null || BattleState.Instance == null) return;

            float range = BattleState.Instance.TowerRange;
            line.positionCount = segments + 1;
            Vector3 center = towerRoot.position;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * (2f * Mathf.PI / segments);
                float x = center.x + Mathf.Cos(angle) * range;
                float y = center.y + Mathf.Sin(angle) * range;
                line.SetPosition(i, new Vector3(x, y, center.z));
            }
        }

        void SetupLine()
        {
            line.useWorldSpace = true;
            line.loop = true;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.material = new Material(Shader.Find("Sprites/Default")) { color = lineColor };
        }
    }
}
