using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TheTower.Game;

namespace TheTower.UI
{
    /// <summary>
    /// 배틀(게임) 씬 UI. 상단 재화, 중앙 타워/범위, 스탯/웨이브/속도, 하단 업그레이드 패널.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class BattleUI : MonoBehaviour
    {
        [Header("Optional: Lobby scene name")]
        [SerializeField] string lobbySceneName = "Lobby";

        Text cashText;
        Text coinsText;
        Text gemsText;
        Text damageText;
        Text regenText;
        Text healthBarText;
        Text waveTierText;
        Text speedText;
        Button speedDownButton;
        Button speedUpButton;
        Button backButton;

        // 업그레이드 패널 (공격 탭)
        Text damageValueText;
        Text damageCostText;
        Button damageBuyButton;
        Text attackSpeedValueText;
        Text attackSpeedCostText;
        Button attackSpeedBuyButton;
        Image healthBarFill;

        static readonly Color PanelBg = new Color(0.2f, 0.16f, 0.35f, 0.95f);

        void Awake()
        {
            BattleState.EnsureExists();
            BattleState.ResetForNewRun();
            Time.timeScale = BattleState.Instance.GameSpeed;
            EnsureEventSystem();
            EnsureCanvas();
            BuildUI();
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (BattleState.Instance == null) return;
            var s = BattleState.Instance;
            // 간단 체력 재생 (실제로는 적/타워 로직에서 처리)
            s.TowerHealth = Mathf.Min((float)(s.TowerHealth + s.HealthRegenPerSec * Time.deltaTime), (float)s.TowerMaxHealth);
            RefreshValues();
        }

        void EnsureEventSystem()
        {
            var es = FindFirstObjectByType<EventSystem>();
            if (es != null)
            {
                LobbyUI.UseInputSystemUI(es.gameObject);
                return;
            }
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            if (!LobbyUI.UseInputSystemUI(go))
                go.AddComponent<StandaloneInputModule>();
        }

        void EnsureCanvas()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        }

        void BuildUI()
        {
            var root = new GameObject("BattleRoot");
            root.transform.SetParent(transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;

            BuildTopBar(root.transform);
            BuildGameArea(root.transform);
            BuildStatsBar(root.transform);
            BuildUpgradePanel(root.transform);
            RefreshValues();
        }

        void BuildTopBar(Transform parent)
        {
            var top = new GameObject("TopBar");
            top.transform.SetParent(parent, false);
            var r = top.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0, 1);
            r.anchorMax = new Vector2(1, 1);
            r.pivot = new Vector2(0.5f, 1);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = new Vector2(0, 100);

            cashText = AddLabel(top.transform, "$ 0", 32, 0, 0.5f, 30, 0, 180, 45);
            coinsText = AddLabel(top.transform, "C " + GameData.Coins, 28, 0, 0.5f, 220, 0, 120, 40);
            gemsText = AddLabel(top.transform, "💎 " + GameData.Gems, 28, 0, 0.5f, 350, 0, 100, 40);

            var back = CreateButton(top.transform, "Back", "←", 28);
            var backR = back.GetComponent<RectTransform>();
            backR.anchorMin = new Vector2(1, 0.5f);
            backR.anchorMax = new Vector2(1, 0.5f);
            backR.pivot = new Vector2(1, 0.5f);
            backR.anchoredPosition = new Vector2(-20, 0);
            backR.sizeDelta = new Vector2(70, 50);
            backButton = back.GetComponent<Button>();
            backButton.onClick.AddListener(OnBack);
        }

        void BuildGameArea(Transform parent)
        {
            var area = new GameObject("GameArea");
            area.transform.SetParent(parent, false);
            var r = area.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0, 80);
            r.sizeDelta = new Vector2(400, 400);

            var rangeCircle = CreatePanel(area.transform, "Range", new Color(0.3f, 0.5f, 0.9f, 0.2f));
            SetRect(rangeCircle.GetComponent<RectTransform>(), 0.5f, 0.5f, 0, 0, 350, 350);
            var tower = CreatePanel(area.transform, "Tower", new Color(0.4f, 0.6f, 1f, 0.8f));
            SetRect(tower.GetComponent<RectTransform>(), 0.5f, 0.5f, 0, 0, 80, 80);
        }

        void BuildStatsBar(Transform parent)
        {
            var bar = new GameObject("StatsBar");
            bar.transform.SetParent(parent, false);
            var r = bar.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0, 0.5f);
            r.anchorMax = new Vector2(1, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0, -120);
            r.sizeDelta = new Vector2(0, 80);

            damageText = AddLabel(bar.transform, "89", 24, 0, 0.5f, 20, 0, 80, 30);
            regenText = AddLabel(bar.transform, "1.14/s", 24, 0, 0.5f, 110, 0, 80, 30);
            var healthPanel = CreatePanel(bar.transform, "HealthPanel", PanelBg);
            var hr = healthPanel.GetComponent<RectTransform>();
            hr.anchorMin = new Vector2(0, 0.5f);
            hr.anchorMax = new Vector2(0, 0.5f);
            hr.pivot = new Vector2(0, 0.5f);
            hr.anchoredPosition = new Vector2(200, 0);
            hr.sizeDelta = new Vector2(200, 36);
            var fill = new GameObject("Fill");
            fill.transform.SetParent(healthPanel.transform, false);
            var fillR = fill.AddComponent<RectTransform>();
            fillR.anchorMin = Vector2.zero;
            fillR.anchorMax = Vector2.one;
            fillR.offsetMin = fillR.offsetMax = Vector2.zero;
            healthBarFill = fill.AddComponent<Image>();
            healthBarFill.color = new Color(0.2f, 0.8f, 0.3f);
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            healthBarFill.fillAmount = 1f;
            healthBarText = AddLabel(healthPanel.transform, "59 / 59", 18, 0.5f, 0.5f, 0, 0, 120, 24);
            healthBarText.alignment = TextAnchor.MiddleCenter;

            waveTierText = AddLabel(bar.transform, "Tier 1  Wave 1", 22, 0.5f, 0.5f, 0, 0, 180, 28);
            waveTierText.alignment = TextAnchor.MiddleCenter;

            speedDownButton = CreateButton(bar.transform, "SpeedDown", "-", 24).GetComponent<Button>();
            SetRect(speedDownButton.GetComponent<RectTransform>(), 1, 0.5f, -180, 0, 40, 40);
            speedDownButton.onClick.AddListener(OnSpeedDown);
            speedText = AddLabel(bar.transform, "x1.0", 22, 1, 0.5f, -130, 0, 50, 28);
            speedText.alignment = TextAnchor.MiddleCenter;
            speedUpButton = CreateButton(bar.transform, "SpeedUp", "+", 24).GetComponent<Button>();
            SetRect(speedUpButton.GetComponent<RectTransform>(), 1, 0.5f, -80, 0, 40, 40);
            speedUpButton.onClick.AddListener(OnSpeedUp);
        }

        void BuildUpgradePanel(Transform parent)
        {
            var panel = new GameObject("UpgradePanel");
            panel.transform.SetParent(parent, false);
            var r = panel.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0, 0);
            r.anchorMax = new Vector2(1, 0);
            r.pivot = new Vector2(0.5f, 0);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = new Vector2(0, 220);

            var header = CreatePanel(panel.transform, "Header", PanelBg);
            var headerR = header.GetComponent<RectTransform>();
            headerR.anchorMin = new Vector2(0, 1);
            headerR.anchorMax = new Vector2(1, 1);
            headerR.pivot = new Vector2(0.5f, 1);
            headerR.anchoredPosition = new Vector2(0, -5);
            headerR.sizeDelta = new Vector2(0, 44);
            var tabLabel = AddLabel(header.transform, "ATTACK UPGRADES", 24, 0.5f, 0.5f, 0, 0, 300, 36);
            tabLabel.alignment = TextAnchor.MiddleCenter;
            var x1Btn = CreateButton(header.transform, "x1", "x1", 20).GetComponent<Button>();
            SetRect(x1Btn.GetComponent<RectTransform>(), 1, 0.5f, -20, 0, 50, 36);

            var grid = new GameObject("UpgradeGrid");
            grid.transform.SetParent(panel.transform, false);
            var gridR = grid.AddComponent<RectTransform>();
            gridR.anchorMin = new Vector2(0, 0);
            gridR.anchorMax = new Vector2(1, 1);
            gridR.offsetMin = new Vector2(10, 10);
            gridR.offsetMax = new Vector2(-10, -55);

            // Damage 행
            AddLabel(grid.transform, "Damage", 20, 0, 1, 10, -10, 120, 28);
            damageValueText = AddLabel(grid.transform, "89", 20, 0, 1, 10, -42, 100, 24);
            damageCostText = AddLabel(grid.transform, "$10", 18, 0, 1, 10, -68, 80, 22);
            damageBuyButton = CreateButton(grid.transform, "BuyDamage", "$", 18).GetComponent<Button>();
            SetRect(damageBuyButton.GetComponent<RectTransform>(), 0, 1, 100, -38, 60, 50);
            damageBuyButton.onClick.AddListener(OnBuyDamage);

            // Attack Speed 행
            AddLabel(grid.transform, "Attack Speed", 20, 0.33f, 1, 10, -100, 140, 28);
            attackSpeedValueText = AddLabel(grid.transform, "2.41", 20, 0.33f, 1, 10, -132, 80, 24);
            attackSpeedCostText = AddLabel(grid.transform, "$5", 18, 0.33f, 1, 10, -158, 60, 22);
            attackSpeedBuyButton = CreateButton(grid.transform, "BuyAttackSpeed", "$", 18).GetComponent<Button>();
            SetRect(attackSpeedBuyButton.GetComponent<RectTransform>(), 0.33f, 1, 100, -128, 60, 50);
            attackSpeedBuyButton.onClick.AddListener(OnBuyAttackSpeed);
        }

        void RefreshValues()
        {
            var s = BattleState.Instance;
            if (s == null) return;

            if (cashText) cashText.text = "$ " + s.Cash.ToString("F0");
            if (coinsText) coinsText.text = "C " + GameData.Coins;
            if (gemsText) gemsText.text = "💎 " + GameData.Gems;
            if (damageText) damageText.text = s.Damage.ToString("F0");
            if (regenText) regenText.text = s.HealthRegenPerSec.ToString("F2") + "/s";
            if (healthBarFill) healthBarFill.fillAmount = (float)(s.TowerHealth / s.TowerMaxHealth);
            if (healthBarText) healthBarText.text = s.TowerHealth.ToString("F0") + " / " + s.TowerMaxHealth.ToString("F0");
            if (waveTierText) waveTierText.text = "Tier " + GameData.CurrentTier + "  Wave " + s.Wave;
            if (speedText) speedText.text = "x" + s.GameSpeed.ToString("F1");

            if (damageValueText) damageValueText.text = s.Damage.ToString("F0");
            if (damageCostText) damageCostText.text = "$" + GetDamageCost().ToString("F0");
            if (attackSpeedValueText) attackSpeedValueText.text = (2f + s.UpgradeAttackSpeed * 0.1f).ToString("F2");
            if (attackSpeedCostText) attackSpeedCostText.text = "$" + GetAttackSpeedCost().ToString("F0");
        }

        int GetDamageCost() => 10 + BattleState.Instance.UpgradeDamage * 5;
        int GetAttackSpeedCost() => 5 + BattleState.Instance.UpgradeAttackSpeed * 3;

        void OnBack()
        {
            if (!string.IsNullOrEmpty(lobbySceneName) && Application.CanStreamedLevelBeLoaded(lobbySceneName))
                SceneManager.LoadScene(lobbySceneName);
        }

        void OnSpeedDown()
        {
            var s = BattleState.Instance;
            if (s == null) return;
            s.GameSpeed = Mathf.Max(0.5f, s.GameSpeed - 0.5f);
            Time.timeScale = s.GameSpeed;
        }

        void OnSpeedUp()
        {
            var s = BattleState.Instance;
            if (s == null) return;
            s.GameSpeed = Mathf.Min(5f, s.GameSpeed + 0.5f);
            Time.timeScale = s.GameSpeed;
        }

        void OnBuyDamage()
        {
            var s = BattleState.Instance;
            int cost = GetDamageCost();
            if (s.Cash < cost) return;
            s.Cash -= cost;
            s.UpgradeDamage++;
            s.Damage += 15;
        }

        void OnBuyAttackSpeed()
        {
            var s = BattleState.Instance;
            int cost = GetAttackSpeedCost();
            if (s.Cash < cost) return;
            s.Cash -= cost;
            s.UpgradeAttackSpeed++;
            s.HealthRegenPerSec += 0.1;
        }

        static Text AddLabel(Transform parent, string content, int fontSize, float ax, float ay, float px, float py, float w, float h)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(ax, ay);
            rect.anchorMax = new Vector2(ax, ay);
            rect.pivot = new Vector2(ax, ay);
            rect.anchoredPosition = new Vector2(px, py);
            rect.sizeDelta = new Vector2(w, h);
            return text;
        }

        static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        static GameObject CreateButton(Transform parent, string name, string label, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = PanelBg;
            go.AddComponent<Button>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(2, 2);
            textRect.offsetMax = new Vector2(-2, -2);
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = fontSize;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            return go;
        }

        static void SetRect(RectTransform r, float ax, float ay, float px, float py, float w, float h)
        {
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(ax, ay);
            r.anchoredPosition = new Vector2(px, py);
            r.sizeDelta = new Vector2(w, h);
        }
    }
}
