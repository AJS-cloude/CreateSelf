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

        [Header("Canvas Scaler (씬에 이미 있으면 덮어쓰지 않음)")]
        [Tooltip("안드로이드 세로: 1080x1920. 가로 게임이면 1920x1080")]
        [SerializeField] Vector2 referenceResolution = new Vector2(1080, 1920);
        [Tooltip("세로 화면: 0.5 또는 1 권장. 0=가로 기준, 1=세로 기준")]
        [SerializeField] [Range(0f, 1f)] float matchWidthOrHeight = 0.5f;

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

        // 업그레이드 패널: 공격/방어/유틸 탭 + 탭별 항목
        GameObject attackContent;
        GameObject defenseContent;
        GameObject utilityContent;

        Text damageValueText;
        Text damageCostText;
        Button damageBuyButton;
        Text attackSpeedValueText;
        Text attackSpeedCostText;
        Button attackSpeedBuyButton;

        Text healthValueText;
        Text healthCostText;
        Button healthBuyButton;
        Text regenValueText;
        Text regenCostText;
        Button regenBuyButton;
        Text defenseValueText;
        Text defenseCostText;
        Button defenseBuyButton;

        Text cashBonusValueText;
        Text cashBonusCostText;
        Button cashBonusBuyButton;
        Text cashPerWaveValueText;
        Text cashPerWaveCostText;
        Button cashPerWaveBuyButton;

        Image healthBarFill;

        static readonly Color PanelBg = new Color(0.2f, 0.16f, 0.35f, 0.95f);
        static readonly Color TabActive = new Color(0.25f, 0.2f, 0.45f, 1f);

        void Awake()
        {
            BattleState.EnsureExists();
            BattleState.ResetForNewRun();
            Time.timeScale = BattleState.Instance.GameSpeed;
            EnsureEventSystem();
            EnsureCanvas();
            BuildUI();
            EnsureBattleRunner();
        }

        void EnsureBattleRunner()
        {
            if (FindFirstObjectByType<EnemySpawner>() == null)
            {
                var go = new GameObject("EnemySpawner");
                go.AddComponent<EnemySpawner>();
            }
            if (FindFirstObjectByType<TowerAttack>() == null)
            {
                var go = new GameObject("TowerAttack");
                go.AddComponent<TowerAttack>();
            }
            if (FindFirstObjectByType<TowerRangeIndicator>() == null)
            {
                var go = new GameObject("TowerRangeIndicator");
                go.AddComponent<TowerRangeIndicator>();
            }
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
            bool weAddedScaler = (scaler == null);
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            if (weAddedScaler)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = referenceResolution;
                scaler.matchWidthOrHeight = matchWidthOrHeight;
            }
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        }

        void BuildUI()
        {
            Transform root = null;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == "BattleRoot")
                {
                    root = transform.GetChild(i);
                    break;
                }
            }
            if (root != null)
            {
                BindFromScene(root);
                RefreshValues();
                return;
            }

            var rootGo = new GameObject("BattleRoot");
            rootGo.transform.SetParent(transform, false);
            var rootRect = rootGo.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            root = rootGo.transform;

            BuildTopBar(root);
            BuildGameArea(root);
            BuildStatsBar(root);
            BuildUpgradePanel(root);
            RefreshValues();
        }

        /// <summary>
        /// 씬에 배치된 BattleRoot 하위 UI(TopBar, StatsBar, UpgradePanel/UpgradeGrid 등)를 찾아 바인딩.
        /// UpgradeGrid 안에는 UPGRADESITEM(Label x3 + BuyDamage / BuyAttackSpeed) 구조를 기대합니다.
        /// </summary>
        void BindFromScene(Transform battleRoot)
        {
            var topBar = battleRoot.Find("TopBar");
            if (topBar != null)
            {
                BindTopBar(topBar);
            }

            var statsBar = battleRoot.Find("StatsBar");
            if (statsBar != null)
            {
                BindStatsBar(statsBar);
            }

            var upgradePanel = battleRoot.Find("UpgradePanel");
            if (upgradePanel != null)
            {
                EnsureUpgradeTabsAndContents(upgradePanel);
                var ac = upgradePanel.Find("AttackContent");
                var dc = upgradePanel.Find("DefenseContent");
                var uc = upgradePanel.Find("UtilityContent");
                if (ac != null) { attackContent = ac.gameObject; var g = ac.Find("UpgradeGrid"); if (g != null) BindUpgradeGrid(g); }
                if (dc != null) { defenseContent = dc.gameObject; defenseContent.SetActive(false); var g = dc.Find("UpgradeGrid"); if (g != null) BindUpgradeGrid(g); }
                if (uc != null) { utilityContent = uc.gameObject; utilityContent.SetActive(false); var g = uc.Find("UpgradeGrid"); if (g != null) BindUpgradeGrid(g); }
            }
        }

        void EnsureUpgradeTabsAndContents(Transform upgradePanel)
        {
            var header = upgradePanel.Find("Header");
            if (header == null) return;
            if (header.Find("TabAttack") != null) return;

            var tabAttack = CreateButton(header, "TabAttack", "공격", 18);
            SetRect(tabAttack.GetComponent<RectTransform>(), 0, 0.5f, 10, 0, 70, 36);
            tabAttack.GetComponent<Image>().color = TabActive;
            tabAttack.GetComponent<Button>().onClick.AddListener(() => OnTab(0));
            var tabDefense = CreateButton(header, "TabDefense", "방어", 18);
            SetRect(tabDefense.GetComponent<RectTransform>(), 0, 0.5f, 90, 0, 70, 36);
            tabDefense.GetComponent<Button>().onClick.AddListener(() => OnTab(1));
            var tabUtility = CreateButton(header, "TabUtility", "유틸", 18);
            SetRect(tabUtility.GetComponent<RectTransform>(), 0, 0.5f, 170, 0, 70, 36);
            tabUtility.GetComponent<Button>().onClick.AddListener(() => OnTab(2));

            if (upgradePanel.Find("AttackContent") == null)
            {
                var acGo = CreateContentPanel(upgradePanel, "AttackContent");
                attackContent = acGo;
                var ag = AddGridToContent(attackContent.transform);
                EnsureDefaultUpgradeItems(ag.transform);
            }
            if (upgradePanel.Find("DefenseContent") == null)
            {
                defenseContent = CreateContentPanel(upgradePanel, "DefenseContent").gameObject;
                defenseContent.SetActive(false);
                var dg = AddGridToContent(defenseContent.transform);
                EnsureDefenseUpgradeItems(dg.transform);
            }
            if (upgradePanel.Find("UtilityContent") == null)
            {
                utilityContent = CreateContentPanel(upgradePanel, "UtilityContent").gameObject;
                utilityContent.SetActive(false);
                var ug = AddGridToContent(utilityContent.transform);
                EnsureUtilityUpgradeItems(ug.transform);
            }
        }

        /// <summary> UpgradeGrid가 비어 있으면 처음부터 데미지·공격속도 두 항목을 생성. </summary>
        void EnsureDefaultUpgradeItems(Transform upgradeGrid)
        {
            if (upgradeGrid.childCount > 0) return;

            var s = BattleState.Instance;
            int dCost = s != null ? UpgradeConfig.GetDamageCost(s.UpgradeDamage) : 10;
            int aCost = s != null ? UpgradeConfig.GetAttackSpeedCost(s.UpgradeAttackSpeed) : 5;

            // 데미지 항목
            var item1 = new GameObject("UPGRADESITEM");
            item1.transform.SetParent(upgradeGrid, false);
            var r1 = item1.AddComponent<RectTransform>();
            r1.sizeDelta = new Vector2(280, 48);
            AddLabel(item1.transform, UpgradeConfig.Damage, 18, 0, 0.5f, 5, 0, 70, 22);
            AddLabel(item1.transform, s != null ? s.Damage.ToString("F0") : "10", 18, 0.25f, 0.5f, 0, 0, 50, 22);
            AddLabel(item1.transform, "$" + dCost, 16, 0.45f, 0.5f, 0, 0, 50, 20);
            var btn1 = CreateButton(item1.transform, "BuyDamage", "$", 16);
            SetRect(btn1.GetComponent<RectTransform>(), 0.85f, 0.5f, 0, 0, 44, 36);

            // 공격속도 항목
            var item2 = new GameObject("UPGRADESITEM");
            item2.transform.SetParent(upgradeGrid, false);
            var r2 = item2.AddComponent<RectTransform>();
            r2.sizeDelta = new Vector2(280, 48);
            AddLabel(item2.transform, UpgradeConfig.AttackSpeed, 18, 0, 0.5f, 5, 0, 80, 22);
            AddLabel(item2.transform, s != null ? s.TowerAttacksPerSecond.ToString("F2") : "2.00", 18, 0.3f, 0.5f, 0, 0, 50, 22);
            AddLabel(item2.transform, "$" + aCost, 16, 0.5f, 0.5f, 0, 0, 50, 20);
            var btn2 = CreateButton(item2.transform, "BuyAttackSpeed", "$", 16);
            SetRect(btn2.GetComponent<RectTransform>(), 0.85f, 0.5f, 0, 0, 44, 36);
        }

        void EnsureDefenseUpgradeItems(Transform grid)
        {
            var s = BattleState.Instance;
            var item1 = new GameObject("UPGRADESITEM");
            item1.transform.SetParent(grid, false);
            item1.AddComponent<RectTransform>().sizeDelta = new Vector2(280, 48);
            AddLabel(item1.transform, UpgradeConfig.Health, 18, 0, 0.5f, 5, 0, 70, 22);
            AddLabel(item1.transform, s != null ? s.TowerMaxHealth.ToString("F0") : "100", 18, 0.25f, 0.5f, 0, 0, 50, 22);
            AddLabel(item1.transform, "$" + (s != null ? UpgradeConfig.GetHealthCost(s.UpgradeHealth) : 10), 16, 0.45f, 0.5f, 0, 0, 50, 20);
            var b1 = CreateButton(item1.transform, "BuyHealth", "$", 16);
            SetRect(b1.GetComponent<RectTransform>(), 0.85f, 0.5f, 0, 0, 44, 36);
            var item2 = new GameObject("UPGRADESITEM");
            item2.transform.SetParent(grid, false);
            item2.AddComponent<RectTransform>().sizeDelta = new Vector2(280, 48);
            AddLabel(item2.transform, UpgradeConfig.HealthRegen, 18, 0, 0.5f, 5, 0, 80, 22);
            AddLabel(item2.transform, s != null ? s.HealthRegenPerSec.ToString("F2") + "/s" : "1.00/s", 18, 0.3f, 0.5f, 0, 0, 60, 22);
            AddLabel(item2.transform, "$" + (s != null ? UpgradeConfig.GetHealthRegenCost(s.UpgradeHealthRegen) : 5), 16, 0.5f, 0.5f, 0, 0, 50, 20);
            var b2 = CreateButton(item2.transform, "BuyRegen", "$", 16);
            SetRect(b2.GetComponent<RectTransform>(), 0.85f, 0.5f, 0, 0, 44, 36);
            var item3 = new GameObject("UPGRADESITEM");
            item3.transform.SetParent(grid, false);
            item3.AddComponent<RectTransform>().sizeDelta = new Vector2(280, 48);
            AddLabel(item3.transform, UpgradeConfig.DefensePercent, 18, 0, 0.5f, 5, 0, 80, 22);
            AddLabel(item3.transform, s != null ? (s.UpgradeDefensePercent * 0.5f).ToString("F1") + "%" : "0%", 18, 0.3f, 0.5f, 0, 0, 50, 22);
            AddLabel(item3.transform, "$" + (s != null ? UpgradeConfig.GetDefensePercentCost(s.UpgradeDefensePercent) : 6), 16, 0.5f, 0.5f, 0, 0, 50, 20);
            var b3 = CreateButton(item3.transform, "BuyDefense", "$", 16);
            SetRect(b3.GetComponent<RectTransform>(), 0.85f, 0.5f, 0, 0, 44, 36);
        }

        void EnsureUtilityUpgradeItems(Transform grid)
        {
            var s = BattleState.Instance;
            var item1 = new GameObject("UPGRADESITEM");
            item1.transform.SetParent(grid, false);
            item1.AddComponent<RectTransform>().sizeDelta = new Vector2(280, 48);
            AddLabel(item1.transform, UpgradeConfig.CashBonus, 18, 0, 0.5f, 5, 0, 90, 22);
            AddLabel(item1.transform, s != null ? "x" + (1f + s.UpgradeCashBonus * 0.05f).ToString("F2") : "x1.00", 18, 0.3f, 0.5f, 0, 0, 50, 22);
            AddLabel(item1.transform, "$" + (s != null ? UpgradeConfig.GetCashBonusCost(s.UpgradeCashBonus) : 4), 16, 0.5f, 0.5f, 0, 0, 50, 20);
            var b1 = CreateButton(item1.transform, "BuyCashBonus", "$", 16);
            SetRect(b1.GetComponent<RectTransform>(), 0.85f, 0.5f, 0, 0, 44, 36);
            var item2 = new GameObject("UPGRADESITEM");
            item2.transform.SetParent(grid, false);
            item2.AddComponent<RectTransform>().sizeDelta = new Vector2(280, 48);
            AddLabel(item2.transform, UpgradeConfig.CashPerWave, 18, 0, 0.5f, 5, 0, 90, 22);
            AddLabel(item2.transform, s != null ? "+" + (5 + s.UpgradeCashPerWave * 2) : "+5", 18, 0.3f, 0.5f, 0, 0, 50, 22);
            AddLabel(item2.transform, "$" + (s != null ? UpgradeConfig.GetCashPerWaveCost(s.UpgradeCashPerWave) : 5), 16, 0.5f, 0.5f, 0, 0, 50, 20);
            var b2 = CreateButton(item2.transform, "BuyCashPerWave", "$", 16);
            SetRect(b2.GetComponent<RectTransform>(), 0.85f, 0.5f, 0, 0, 44, 36);
        }

        void BindTopBar(Transform topBar)
        {
            backButton = topBar.Find("Back")?.GetComponent<Button>();
            if (backButton == null)
                backButton = topBar.GetComponentInChildren<Button>();
            if (backButton != null)
                backButton.onClick.AddListener(OnBack);

            var texts = topBar.GetComponentsInChildren<Text>(true);
            if (texts.Length >= 1) cashText = texts[0];
            if (texts.Length >= 2) coinsText = texts[1];
            if (texts.Length >= 3) gemsText = texts[2];
        }

        void BindStatsBar(Transform statsBar)
        {
            damageText = FindComponentByName<Text>(statsBar, "DamageText", "Damage");
            regenText = FindComponentByName<Text>(statsBar, "RegenText", "Regen");
            healthBarText = FindComponentByName<Text>(statsBar, "HealthBarText", "HealthText");
            waveTierText = FindComponentByName<Text>(statsBar, "WaveTierText", "Wave");
            speedText = FindComponentByName<Text>(statsBar, "SpeedText", "Speed");

            var healthPanel = statsBar.Find("HealthPanel");
            if (healthPanel != null)
            {
                var fill = healthPanel.Find("Fill");
                if (fill != null)
                    healthBarFill = fill.GetComponent<Image>();
                if (healthBarFill == null)
                    healthBarFill = healthPanel.GetComponentInChildren<Image>();
            }

            speedDownButton = FindComponentByName<Button>(statsBar, "SpeedDown");
            speedUpButton = FindComponentByName<Button>(statsBar, "SpeedUp");
            if (speedDownButton != null) speedDownButton.onClick.AddListener(OnSpeedDown);
            if (speedUpButton != null) speedUpButton.onClick.AddListener(OnSpeedUp);
        }

        void BindUpgradeGrid(Transform upgradeGrid)
        {
            for (int i = 0; i < upgradeGrid.childCount; i++)
            {
                var item = upgradeGrid.GetChild(i);
                var labels = item.GetComponentsInChildren<Text>(true);
                var btn = item.GetComponentInChildren<Button>(true);
                if (btn == null || labels.Length < 3) continue;

                string btnName = btn.name.ToUpperInvariant();
                if (labels.Length > 0)
                    labels[0].text = UpgradeConfig.GetDisplayName(btn.name);
                if (btnName.Contains("DAMAGE") && !btnName.Contains("ATTACKSPEED"))
                {
                    damageValueText = labels[1];
                    damageCostText = labels[2];
                    damageBuyButton = btn;
                    damageBuyButton.onClick.AddListener(OnBuyDamage);
                }
                else if (btnName.Contains("ATTACKSPEED") || btnName.Contains("ATTACK_SPEED"))
                {
                    attackSpeedValueText = labels[1];
                    attackSpeedCostText = labels[2];
                    attackSpeedBuyButton = btn;
                    attackSpeedBuyButton.onClick.AddListener(OnBuyAttackSpeed);
                }
                else if (btnName.Contains("BUYHEALTH") && !btnName.Contains("REGEN"))
                {
                    healthValueText = labels[1];
                    healthCostText = labels[2];
                    healthBuyButton = btn;
                    healthBuyButton.onClick.AddListener(OnBuyHealth);
                }
                else if (btnName.Contains("BUYREGEN") || (btnName.Contains("HEALTH") && btnName.Contains("REGEN")))
                {
                    regenValueText = labels[1];
                    regenCostText = labels[2];
                    regenBuyButton = btn;
                    regenBuyButton.onClick.AddListener(OnBuyHealthRegen);
                }
                else if (btnName.Contains("BUYDEFENSE") || btnName.Contains("DEFENSE"))
                {
                    defenseValueText = labels[1];
                    defenseCostText = labels[2];
                    defenseBuyButton = btn;
                    defenseBuyButton.onClick.AddListener(OnBuyDefensePercent);
                }
                else if (btnName.Contains("CASHBONUS"))
                {
                    cashBonusValueText = labels[1];
                    cashBonusCostText = labels[2];
                    cashBonusBuyButton = btn;
                    cashBonusBuyButton.onClick.AddListener(OnBuyCashBonus);
                }
                else if (btnName.Contains("CASHPERWAVE") || btnName.Contains("CASH_PER"))
                {
                    cashPerWaveValueText = labels[1];
                    cashPerWaveCostText = labels[2];
                    cashPerWaveBuyButton = btn;
                    cashPerWaveBuyButton.onClick.AddListener(OnBuyCashPerWave);
                }
            }
        }

        static T FindComponentByName<T>(Transform parent, params string[] names) where T : Component
        {
            foreach (var name in names)
            {
                var t = parent.Find(name);
                if (t != null)
                {
                    var c = t.GetComponent<T>();
                    if (c != null) return c;
                }
            }
            return parent.GetComponentInChildren<T>(true);
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
            gemsText = AddLabel(top.transform, "G " + GameData.Gems, 28, 0, 0.5f, 350, 0, 100, 40);

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

            // 자료: 공격 / 방어 / 유틸리티 탭 버튼
            var tabAttack = CreateButton(header.transform, "TabAttack", "공격", 18);
            SetRect(tabAttack.GetComponent<RectTransform>(), 0, 0.5f, 10, 0, 70, 36);
            tabAttack.GetComponent<Image>().color = TabActive;
            tabAttack.GetComponent<Button>().onClick.AddListener(() => OnTab(0));

            var tabDefense = CreateButton(header.transform, "TabDefense", "방어", 18);
            SetRect(tabDefense.GetComponent<RectTransform>(), 0, 0.5f, 90, 0, 70, 36);
            tabDefense.GetComponent<Button>().onClick.AddListener(() => OnTab(1));

            var tabUtility = CreateButton(header.transform, "TabUtility", "유틸", 18);
            SetRect(tabUtility.GetComponent<RectTransform>(), 0, 0.5f, 170, 0, 70, 36);
            tabUtility.GetComponent<Button>().onClick.AddListener(() => OnTab(2));

            // 공격 탭 콘텐츠
            attackContent = CreateContentPanel(panel.transform, "AttackContent");
            var attackGrid = AddGridToContent(attackContent.transform);
            AddLabel(attackGrid.transform, UpgradeConfig.Damage, 20, 0, 1, 10, -10, 120, 28);
            damageValueText = AddLabel(attackGrid.transform, "10", 20, 0, 1, 10, -42, 100, 24);
            damageCostText = AddLabel(attackGrid.transform, "$10", 18, 0, 1, 10, -68, 80, 22);
            damageBuyButton = CreateButton(attackGrid.transform, "BuyDamage", "$", 18).GetComponent<Button>();
            SetRect(damageBuyButton.GetComponent<RectTransform>(), 0, 1, 100, -38, 60, 50);
            damageBuyButton.onClick.AddListener(OnBuyDamage);
            AddLabel(attackGrid.transform, UpgradeConfig.AttackSpeed, 20, 0.33f, 1, 10, -100, 140, 28);
            attackSpeedValueText = AddLabel(attackGrid.transform, "2.00", 20, 0.33f, 1, 10, -132, 80, 24);
            attackSpeedCostText = AddLabel(attackGrid.transform, "$5", 18, 0.33f, 1, 10, -158, 60, 22);
            attackSpeedBuyButton = CreateButton(attackGrid.transform, "BuyAttackSpeed", "$", 18).GetComponent<Button>();
            SetRect(attackSpeedBuyButton.GetComponent<RectTransform>(), 0.33f, 1, 100, -128, 60, 50);
            attackSpeedBuyButton.onClick.AddListener(OnBuyAttackSpeed);

            // 방어 탭 콘텐츠
            defenseContent = CreateContentPanel(panel.transform, "DefenseContent");
            defenseContent.SetActive(false);
            var defenseGrid = AddGridToContent(defenseContent.transform);
            AddLabel(defenseGrid.transform, UpgradeConfig.Health, 20, 0, 1, 10, -10, 80, 28);
            healthValueText = AddLabel(defenseGrid.transform, "100", 20, 0, 1, 10, -42, 80, 24);
            healthCostText = AddLabel(defenseGrid.transform, "$10", 18, 0, 1, 10, -68, 60, 22);
            healthBuyButton = CreateButton(defenseGrid.transform, "BuyHealth", "$", 18).GetComponent<Button>();
            SetRect(healthBuyButton.GetComponent<RectTransform>(), 0, 1, 100, -38, 60, 50);
            healthBuyButton.onClick.AddListener(OnBuyHealth);
            AddLabel(defenseGrid.transform, UpgradeConfig.HealthRegen, 20, 0.33f, 1, 10, -100, 100, 28);
            regenValueText = AddLabel(defenseGrid.transform, "1.00/s", 20, 0.33f, 1, 10, -132, 80, 24);
            regenCostText = AddLabel(defenseGrid.transform, "$5", 18, 0.33f, 1, 10, -158, 60, 22);
            regenBuyButton = CreateButton(defenseGrid.transform, "BuyRegen", "$", 18).GetComponent<Button>();
            SetRect(regenBuyButton.GetComponent<RectTransform>(), 0.33f, 1, 100, -128, 60, 50);
            regenBuyButton.onClick.AddListener(OnBuyHealthRegen);
            AddLabel(defenseGrid.transform, UpgradeConfig.DefensePercent, 20, 0.66f, 1, 10, -190, 80, 28);
            defenseValueText = AddLabel(defenseGrid.transform, "0%", 20, 0.66f, 1, 10, -222, 60, 24);
            defenseCostText = AddLabel(defenseGrid.transform, "$6", 18, 0.66f, 1, 10, -248, 50, 22);
            defenseBuyButton = CreateButton(defenseGrid.transform, "BuyDefense", "$", 18).GetComponent<Button>();
            SetRect(defenseBuyButton.GetComponent<RectTransform>(), 0.66f, 1, 100, -218, 60, 50);
            defenseBuyButton.onClick.AddListener(OnBuyDefensePercent);

            // 유틸리티 탭 콘텐츠
            utilityContent = CreateContentPanel(panel.transform, "UtilityContent");
            utilityContent.SetActive(false);
            var utilityGrid = AddGridToContent(utilityContent.transform);
            AddLabel(utilityGrid.transform, UpgradeConfig.CashBonus, 20, 0, 1, 10, -10, 120, 28);
            cashBonusValueText = AddLabel(utilityGrid.transform, "x1.0", 20, 0, 1, 10, -42, 80, 24);
            cashBonusCostText = AddLabel(utilityGrid.transform, "$4", 18, 0, 1, 10, -68, 60, 22);
            cashBonusBuyButton = CreateButton(utilityGrid.transform, "BuyCashBonus", "$", 18).GetComponent<Button>();
            SetRect(cashBonusBuyButton.GetComponent<RectTransform>(), 0, 1, 100, -38, 60, 50);
            cashBonusBuyButton.onClick.AddListener(OnBuyCashBonus);
            AddLabel(utilityGrid.transform, UpgradeConfig.CashPerWave, 20, 0.33f, 1, 10, -100, 120, 28);
            cashPerWaveValueText = AddLabel(utilityGrid.transform, "+5", 20, 0.33f, 1, 10, -132, 80, 24);
            cashPerWaveCostText = AddLabel(utilityGrid.transform, "$5", 18, 0.33f, 1, 10, -158, 60, 22);
            cashPerWaveBuyButton = CreateButton(utilityGrid.transform, "BuyCashPerWave", "$", 18).GetComponent<Button>();
            SetRect(cashPerWaveBuyButton.GetComponent<RectTransform>(), 0.33f, 1, 100, -128, 60, 50);
            cashPerWaveBuyButton.onClick.AddListener(OnBuyCashPerWave);
        }

        GameObject CreateContentPanel(Transform panel, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(panel, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, -50);
            return go;
        }

        GameObject AddGridToContent(Transform content)
        {
            var grid = new GameObject("UpgradeGrid");
            grid.transform.SetParent(content, false);
            var gridR = grid.AddComponent<RectTransform>();
            gridR.anchorMin = Vector2.zero;
            gridR.anchorMax = Vector2.one;
            gridR.offsetMin = new Vector2(10, 10);
            gridR.offsetMax = new Vector2(-10, -10);
            return grid;
        }

        void OnTab(int index)
        {
            if (attackContent != null) attackContent.SetActive(index == 0);
            if (defenseContent != null) defenseContent.SetActive(index == 1);
            if (utilityContent != null) utilityContent.SetActive(index == 2);
        }

        void RefreshValues()
        {
            var s = BattleState.Instance;
            if (s == null) return;

            if (cashText) cashText.text = "$ " + s.Cash.ToString("F0");
            if (coinsText) coinsText.text = "C " + GameData.Coins;
            if (gemsText) gemsText.text = "G " + GameData.Gems;
            if (damageText) damageText.text = s.Damage.ToString("F0");
            if (regenText) regenText.text = s.HealthRegenPerSec.ToString("F2") + "/s";
            if (healthBarFill) healthBarFill.fillAmount = (float)(s.TowerHealth / s.TowerMaxHealth);
            if (healthBarText) healthBarText.text = s.TowerHealth.ToString("F0") + " / " + s.TowerMaxHealth.ToString("F0");
            if (waveTierText) waveTierText.text = "Tier " + GameData.CurrentTier + "  Wave " + s.Wave;
            if (speedText) speedText.text = "x" + s.GameSpeed.ToString("F1");

            if (damageValueText) damageValueText.text = s.Damage.ToString("F0");
            if (damageCostText) damageCostText.text = "$" + UpgradeConfig.GetDamageCost(s.UpgradeDamage);
            if (attackSpeedValueText) attackSpeedValueText.text = s.TowerAttacksPerSecond.ToString("F2");
            if (attackSpeedCostText) attackSpeedCostText.text = "$" + UpgradeConfig.GetAttackSpeedCost(s.UpgradeAttackSpeed);

            if (healthValueText) healthValueText.text = s.TowerMaxHealth.ToString("F0");
            if (healthCostText) healthCostText.text = "$" + UpgradeConfig.GetHealthCost(s.UpgradeHealth);
            if (regenValueText) regenValueText.text = s.HealthRegenPerSec.ToString("F2") + "/s";
            if (regenCostText) regenCostText.text = "$" + UpgradeConfig.GetHealthRegenCost(s.UpgradeHealthRegen);
            if (defenseValueText) defenseValueText.text = (s.UpgradeDefensePercent * 0.5f).ToString("F1") + "%";
            if (defenseCostText) defenseCostText.text = "$" + UpgradeConfig.GetDefensePercentCost(s.UpgradeDefensePercent);

            if (cashBonusValueText) cashBonusValueText.text = "x" + (1f + s.UpgradeCashBonus * 0.05f).ToString("F2");
            if (cashBonusCostText) cashBonusCostText.text = "$" + UpgradeConfig.GetCashBonusCost(s.UpgradeCashBonus);
            if (cashPerWaveValueText) cashPerWaveValueText.text = "+" + (5 + s.UpgradeCashPerWave * 2);
            if (cashPerWaveCostText) cashPerWaveCostText.text = "$" + UpgradeConfig.GetCashPerWaveCost(s.UpgradeCashPerWave);
        }

        int GetDamageCost() => UpgradeConfig.GetDamageCost(BattleState.Instance.UpgradeDamage);
        int GetAttackSpeedCost() => UpgradeConfig.GetAttackSpeedCost(BattleState.Instance.UpgradeAttackSpeed);

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

        void OnBuyHealth()
        {
            var s = BattleState.Instance;
            int cost = UpgradeConfig.GetHealthCost(s.UpgradeHealth);
            if (s.Cash < cost) return;
            s.Cash -= cost;
            s.UpgradeHealth++;
            s.TowerMaxHealth += 20;
            s.TowerHealth += 20;
        }

        void OnBuyHealthRegen()
        {
            var s = BattleState.Instance;
            int cost = UpgradeConfig.GetHealthRegenCost(s.UpgradeHealthRegen);
            if (s.Cash < cost) return;
            s.Cash -= cost;
            s.UpgradeHealthRegen++;
            s.HealthRegenPerSec += 0.15;
        }

        void OnBuyDefensePercent()
        {
            var s = BattleState.Instance;
            int cost = UpgradeConfig.GetDefensePercentCost(s.UpgradeDefensePercent);
            if (s.Cash < cost) return;
            s.Cash -= cost;
            s.UpgradeDefensePercent++;
        }

        void OnBuyCashBonus()
        {
            var s = BattleState.Instance;
            int cost = UpgradeConfig.GetCashBonusCost(s.UpgradeCashBonus);
            if (s.Cash < cost) return;
            s.Cash -= cost;
            s.UpgradeCashBonus++;
        }

        void OnBuyCashPerWave()
        {
            var s = BattleState.Instance;
            int cost = UpgradeConfig.GetCashPerWaveCost(s.UpgradeCashPerWave);
            if (s.Cash < cost) return;
            s.Cash -= cost;
            s.UpgradeCashPerWave++;
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
