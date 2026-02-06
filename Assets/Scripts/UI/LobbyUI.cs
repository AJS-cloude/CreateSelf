using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TheTower.Game;

namespace TheTower.UI
{
    /// <summary>
    /// 로비 씬 UI. LobbyCanvas에 붙여서 사용.
    /// 씬에 LobbyRoot가 있으면 해당 UI를 바인딩하고, 없으면 런타임에 UI를 생성합니다.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        #region Serialized

        [Header("Optional: Battle scene name")]
        [SerializeField] string battleSceneName = "Battle";

        [Header("워크샵 행 프리팹 (선택)")]
        [Tooltip("지정 시 워크샵 탭의 각 항목을 이 프리팹으로 생성. 구조: Name, Buy(Text/Value/Cost)")]
        [SerializeField] GameObject workshopRowPrefab;

        #endregion

        #region UI References

        Text coinsText;
        Text gemsText;
        Text powerStonesText;
        Text coinBonusText;
        Text tierText;
        Text highestWaveText;
        Text tierCoinMultiplierText;
        Button battleButton;
        Button tierPrevButton;
        Button tierNextButton;
        Button settingsButton;

        // 화면 전환: Main 하위 Center(홈) / WorkshopPanel(워크샵)
        Transform mainTransform;
        GameObject centerPanel;
        GameObject workshopPanel;
        Text workshopTitleText;
        Transform workshopContentGrid;
        Button[] workshopCategoryTabs;
        int currentWorkshopTab;

        #endregion

        #region Constants

        static readonly Color DarkBg = new Color(0.15f, 0.12f, 0.25f);
        static readonly Color PanelBg = new Color(0.2f, 0.16f, 0.35f, 0.95f);
        static readonly Color AccentBlue = new Color(0.4f, 0.6f, 1f);
        static readonly Color AccentPurple = new Color(0.6f, 0.35f, 0.9f);

        /// <summary> 워크샵 탭별 제목 (공격/방어/유틸/카드) </summary>
        static readonly string[] WorkshopTabTitles = { "공격 업그레이드", "방어 업그레이드", "유틸리티 업그레이드", "카드 업그레이드" };

        #endregion

        #region Lifecycle

        void Awake()
        {
            GameData.Load();

            EnsureEventSystem();

            var root = transform.Find("LobbyRoot");
            if (root != null)
                BindExistingUI(root);
            else
                BuildUI();

            EnsureMainActiveAndShowHome();
        }

        #endregion

        #region Binding

        /// <summary> 씬에 배치된 LobbyRoot 하위 UI를 찾아 바인딩합니다. </summary>
        void BindExistingUI(Transform root)
        {
            // TopBar: Coins, Gems, PowerStones (이름으로 찾거나 자식 순서 0,1,2)
            var topBar = root.Find("TopBar");
            if (topBar != null)
            {
                coinsText = GetText(topBar, "Coins");
                gemsText = GetText(topBar, "Gems");
                powerStonesText = GetText(topBar, "PowerStones");
                if (coinsText == null && topBar.childCount >= 3)
                {
                    coinsText = topBar.GetChild(0).GetComponent<Text>();
                    gemsText = topBar.GetChild(1).GetComponent<Text>();
                    powerStonesText = topBar.GetChild(2).GetComponent<Text>();
                }
            }

            // Main 있으면 그 안에서 Center·WorkshopPanel 찾기, 없으면 root 직계에서 Center만
            mainTransform = root.Find("Main");
            var center = mainTransform != null ? mainTransform.Find("Center") : null;
            if (center == null) center = root.Find("Center");
            centerPanel = center != null ? center.gameObject : null;

            var settingsGo = (mainTransform != null ? mainTransform.Find("Settings") : null) ?? root.Find("Settings");
            if (settingsGo != null)
            {
                settingsButton = settingsGo.GetComponent<Button>();
                if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            }

            if (center != null)
            {
                // CoinBonusPanel 안의 값 텍스트 (Value 또는 두 번째 Text)
                var bonusPanel = center.Find("CoinBonusPanel");
                if (bonusPanel != null)
                {
                    var valueT = bonusPanel.Find("Value");
                    if (valueT != null)
                        coinBonusText = valueT.GetComponent<Text>();
                    else
                    {
                        var texts = bonusPanel.GetComponentsInChildren<Text>(true);
                        coinBonusText = texts != null && texts.Length >= 2 ? texts[1] : (texts != null && texts.Length > 0 ? texts[0] : null);
                    }
                }

                // DifficultyPanel: TierRow(Prev, Tier, Next), HighestWave, TierCoin
                var diffPanel = center.Find("DifficultyPanel");
                if (diffPanel != null)
                {
                    var tierRow = diffPanel.Find("TierRow");
                    if (tierRow != null)
                    {
                        var prevGo = tierRow.Find("Prev");
                        var nextGo = tierRow.Find("Next");
                        tierPrevButton = prevGo != null ? prevGo.GetComponent<Button>() : null;
                        tierNextButton = nextGo != null ? nextGo.GetComponent<Button>() : null;
                        tierText = GetText(tierRow, "Tier");
                    }
                    else
                    {
                        tierPrevButton = GetButton(diffPanel, "Prev");
                        tierNextButton = GetButton(diffPanel, "Next");
                        tierText = GetText(diffPanel, "Tier");
                    }
                    if (tierPrevButton != null) tierPrevButton.onClick.AddListener(OnTierPrev);
                    if (tierNextButton != null) tierNextButton.onClick.AddListener(OnTierNext);

                    highestWaveText = GetText(diffPanel, "HighestWave");
                    tierCoinMultiplierText = GetText(diffPanel, "TierCoin");
                }

                // Battle 버튼
                var battleGo = center.Find("Battle");
                if (battleGo != null)
                {
                    battleButton = battleGo.GetComponent<Button>();
                    if (battleButton != null) battleButton.onClick.AddListener(OnBattle);
                }
            }

            // WorkshopPanel: 씬에 있으면 사용 (LobbyRoot 직계 또는 Main 하위), 없으면 나중에 EnsureWorkshopPanel에서만 생성
            var wp = root.Find("WorkshopPanel") ?? mainTransform?.Find("WorkshopPanel");
            workshopPanel = wp != null ? wp.gameObject : null;
            if (workshopPanel != null)
            {
                workshopPanel.SetActive(false);
                workshopTitleText = GetText(workshopPanel.transform, "Title");
                // Content: ScrollRect > Viewport > Content 또는 패널 직계 Content
                var scrollRect = workshopPanel.transform.Find("ScrollRect");
                var viewport = scrollRect != null ? scrollRect.Find("Viewport") : null;
                workshopContentGrid = (viewport != null ? viewport.Find("Content") : null) ?? workshopPanel.transform.Find("Content");
                var tabsRow = workshopPanel.transform.Find("CategoryTabs");
                if (tabsRow != null)
                {
                    var tabBtns = tabsRow.GetComponentsInChildren<Button>(true);
                    workshopCategoryTabs = tabBtns;
                    for (int i = 0; i < tabBtns.Length && i < 4; i++)
                    {
                        int idx = i;
                        tabBtns[i].onClick.AddListener(() => OnWorkshopCategoryTab(idx));
                    }
                }
            }

            // BottomNav: 버튼 인덱스 0=홈, 1=워크샵, 2~4=추가 화면
            var bottomNav = root.Find("BottomNav");
            if (bottomNav != null)
            {
                var navBtns = bottomNav.GetComponentsInChildren<Button>(true);
                for (int i = 0; i < navBtns.Length; i++)
                {
                    int idx = i;
                    navBtns[i].onClick.AddListener(() => OnNavButton(idx));
                }
            }

            RefreshValues();
            EnsureMainActiveAndShowHome();
        }

        /// <summary> 로비 진입 시 Main을 켜고 홈(Center) 화면을 보여줍니다. </summary>
        void EnsureMainActiveAndShowHome()
        {
            if (mainTransform != null && !mainTransform.gameObject.activeSelf)
                mainTransform.gameObject.SetActive(true);
            ShowHome();
        }

        #endregion

        #region Helpers (Binding)

        static Text GetText(Transform parent, string name)
        {
            var t = parent?.Find(name);
            return t != null ? t.GetComponent<Text>() : null;
        }

        static Button GetButton(Transform parent, string name)
        {
            var t = parent?.Find(name);
            return t != null ? t.GetComponent<Button>() : null;
        }

        #endregion

        #region EventSystem & Build

        void EnsureEventSystem()
        {
            // 씬에 EventSystem이 이미 있으면 건드리지 않음 (직접 추가한 설정 유지)
            if (FindFirstObjectByType<EventSystem>() != null)
                return;
            // 없을 때만 생성
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            if (!UseInputSystemUI(go))
                go.AddComponent<StandaloneInputModule>();
        }

        /// <summary>
        /// Input System 패키지가 있으면 InputSystemUIInputModule 추가. (EventSystem 없을 때만 생성하는 경우 사용)
        /// </summary>
        public static bool UseInputSystemUI(GameObject eventSystemGo)
        {
            var newModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem")
                ?? GetTypeFromAssemblies("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            if (newModuleType == null) return false;
            if (eventSystemGo.GetComponent(newModuleType) == null)
                eventSystemGo.AddComponent(newModuleType);
            return true;
        }

        static Type GetTypeFromAssemblies(string typeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(typeName);
                if (t != null) return t;
            }
            return null;
        }

        void BuildUI()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                gameObject.AddComponent<CanvasScaler>();
                gameObject.AddComponent<GraphicRaycaster>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

            var root = new GameObject("LobbyRoot");
            root.transform.SetParent(transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var bg = CreatePanel(root.transform, "Background", DarkBg);
            SetFullRect(bg.GetComponent<RectTransform>(), 0, 0, 0, 0);
            BuildTopBar(root.transform);

            var mainGo = new GameObject("Main");
            mainGo.transform.SetParent(root.transform, false);
            var mainR = mainGo.AddComponent<RectTransform>();
            mainR.anchorMin = Vector2.zero;
            mainR.anchorMax = Vector2.one;
            mainR.offsetMin = new Vector2(0, 100);
            mainR.offsetMax = new Vector2(0, -100);
            mainTransform = mainGo.transform;
            BuildCenter(mainTransform);
            centerPanel = mainTransform.Find("Center")?.gameObject;

            BuildBottomNav(root.transform);
            var bottomNav = root.transform.Find("BottomNav");
            if (bottomNav != null)
            {
                var navBtns = bottomNav.GetComponentsInChildren<Button>(true);
                for (int i = 0; i < navBtns.Length; i++)
                {
                    int idx = i;
                    navBtns[i].onClick.AddListener(() => OnNavButton(idx));
                }
            }
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
            r.sizeDelta = new Vector2(0, 120);

            var coins = CreateLabel(top.transform, "Coins", "C " + FormatBigNumber(GameData.Coins), 36);
            SetRect(coins.GetComponent<RectTransform>(), 0, 0.5f, 40, 0, 200, 50);
            coinsText = coins;

            var gems = CreateLabel(top.transform, "Gems", "💎 " + GameData.Gems, 36);
            SetRect(gems.GetComponent<RectTransform>(), 0, 0.5f, 220, 0, 150, 50);
            gemsText = gems;

            var ps = CreateLabel(top.transform, "PowerStones", "▲ " + GameData.PowerStones, 36);
            SetRect(ps.GetComponent<RectTransform>(), 0, 0.5f, 380, 0, 120, 50);
            powerStonesText = ps;

            var settingsGo = CreateButton(top.transform, "Settings", "⚙", 32);
            settingsButton = settingsGo.GetComponent<Button>();
            var sr = settingsGo.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(1, 0.5f);
            sr.anchorMax = new Vector2(1, 0.5f);
            sr.pivot = new Vector2(1, 0.5f);
            sr.anchoredPosition = new Vector2(-40, 0);
            sr.sizeDelta = new Vector2(80, 80);
            settingsButton.onClick.AddListener(OnSettings);
        }

        void BuildCenter(Transform parent)
        {
            var center = new GameObject("Center");
            center.transform.SetParent(parent, false);
            var r = center.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0, 20);
            r.sizeDelta = new Vector2(700, 900);

            var title = CreateLabel(center.transform, "Title", "THE TOWER", 72);
            SetRect(title.GetComponent<RectTransform>(), 0.5f, 1, 0, -20, 600, 100);
            title.color = Color.white;

            var hexPanel = CreatePanel(center.transform, "HexPlaceholder", new Color(0.3f, 0.5f, 0.9f, 0.3f));
            SetRect(hexPanel.GetComponent<RectTransform>(), 0.5f, 0.5f, 0, -80, 200, 200);

            var bonusPanel = CreatePanel(center.transform, "CoinBonusPanel", PanelBg);
            SetRect(bonusPanel.GetComponent<RectTransform>(), 0.5f, 0.5f, 0, -320, 400, 80);
            CreateLabel(bonusPanel.transform, "Label", "Total Coin Bonus", 28);
            var bonusValue = CreateLabel(bonusPanel.transform, "Value", "C x1.00", 32);
            bonusValue.alignment = TextAnchor.MiddleCenter;
            coinBonusText = bonusValue;

            var diffPanel = CreatePanel(center.transform, "DifficultyPanel", PanelBg);
            SetRect(diffPanel.GetComponent<RectTransform>(), 0.5f, 0.5f, 0, -420, 400, 140);
            var diffTitle = CreateLabel(diffPanel.transform, "Label", "Difficulty", 28);
            diffTitle.alignment = TextAnchor.MiddleCenter;

            var tierRow = new GameObject("TierRow");
            tierRow.transform.SetParent(diffPanel.transform, false);
            var tierRowR = tierRow.AddComponent<RectTransform>();
            tierRowR.anchorMin = new Vector2(0.5f, 0.5f);
            tierRowR.anchorMax = new Vector2(0.5f, 0.5f);
            tierRowR.pivot = new Vector2(0.5f, 0.5f);
            tierRowR.anchoredPosition = new Vector2(0, 5);
            tierRowR.sizeDelta = new Vector2(360, 44);
            tierPrevButton = CreateButton(tierRow.transform, "Prev", "<", 36).GetComponent<Button>();
            tierText = CreateLabel(tierRow.transform, "Tier", "Tier 1", 32);
            tierNextButton = CreateButton(tierRow.transform, "Next", ">", 36).GetComponent<Button>();
            tierPrevButton.onClick.AddListener(OnTierPrev);
            tierNextButton.onClick.AddListener(OnTierNext);
            highestWaveText = CreateLabel(diffPanel.transform, "HighestWave", "Highest Wave: 104", 24);
            highestWaveText.alignment = TextAnchor.MiddleCenter;
            tierCoinMultiplierText = CreateLabel(diffPanel.transform, "TierCoin", "C x1.0", 22);
            tierCoinMultiplierText.alignment = TextAnchor.MiddleCenter;

            var battleGo = CreateButton(center.transform, "Battle", "BATTLE", 42);
            battleButton = battleGo.GetComponent<Button>();
            var battleR = battleGo.GetComponent<RectTransform>();
            battleR.anchorMin = new Vector2(0.5f, 0);
            battleR.anchorMax = new Vector2(0.5f, 0);
            battleR.pivot = new Vector2(0.5f, 0);
            battleR.anchoredPosition = new Vector2(0, 80);
            battleR.sizeDelta = new Vector2(400, 90);
            var battleColors = battleButton.colors;
            battleColors.normalColor = AccentPurple;
            battleColors.highlightedColor = AccentPurple * 1.2f;
            battleColors.pressedColor = AccentPurple * 0.8f;
            battleButton.colors = battleColors;
            battleButton.onClick.AddListener(OnBattle);
        }

        void BuildBottomNav(Transform parent)
        {
            var nav = new GameObject("BottomNav");
            nav.transform.SetParent(parent, false);
            var r = nav.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0, 0);
            r.anchorMax = new Vector2(1, 0);
            r.pivot = new Vector2(0.5f, 0);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = new Vector2(0, 100);
            var layout = nav.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 20;
            layout.padding = new RectOffset(40, 10, 10, 10);
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            CreateNavButton(nav.transform, "⚔", AccentBlue);
            CreateNavButton(nav.transform, "🔨", PanelBg);
            CreateNavButton(nav.transform, "📦", PanelBg);
            CreateNavButton(nav.transform, "🧪", PanelBg);
            CreateNavButton(nav.transform, "🛒", PanelBg);
        }

        #endregion

        #region Layout Helpers

        static void SetRect(RectTransform r, float ax, float ay, float px, float py, float w, float h)
        {
            r.anchorMin = new Vector2(ax, ay);
            r.anchorMax = new Vector2(ax, ay);
            r.pivot = new Vector2(ax, ay);
            r.anchoredPosition = new Vector2(px, py);
            r.sizeDelta = new Vector2(w, h);
        }

        GameObject CreateNavButton(Transform parent, string label, Color normalColor)
        {
            var go = CreateButton(parent, "Nav_" + label, label, 28);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 70);
            var btn = go.GetComponent<Button>();
            var c = btn.colors;
            c.normalColor = normalColor;
            c.highlightedColor = normalColor * 1.2f;
            btn.colors = c;
            return go;
        }

        #endregion

        #region Event Handlers

        void OnBattle()
        {
            if (string.IsNullOrEmpty(battleSceneName) || !Application.CanStreamedLevelBeLoaded(battleSceneName))
            {
                Debug.Log("[Lobby] Battle scene not found. Create a scene named '" + battleSceneName + "' and add it to Build Settings.");
                return;
            }
            GameData.Save();
            SceneManager.LoadScene(battleSceneName);
        }

        void OnApplicationQuit()
        {
            GameData.Save();
        }

        void OnTierPrev()
        {
            GameData.CurrentTier = Mathf.Clamp(GameData.CurrentTier - 1, 1, GameData.MaxTier);
            RefreshValues();
        }

        void OnTierNext()
        {
            GameData.CurrentTier = Mathf.Clamp(GameData.CurrentTier + 1, 1, GameData.MaxTier);
            RefreshValues();
        }

        void OnSettings()
        {
            Debug.Log("[Lobby] Settings pressed.");
        }

        void OnNavButton(int index)
        {
            if (index == 0) ShowHome();
            else if (index == 1) ShowWorkshop();
            else ShowHome();
        }

        void ShowHome()
        {
            if (centerPanel != null) centerPanel.SetActive(true);
            if (workshopPanel != null) workshopPanel.SetActive(false);
        }

        void ShowWorkshop()
        {
            if (centerPanel != null) centerPanel.SetActive(false);
            EnsureWorkshopPanel();
            if (workshopPanel == null) return;

            workshopPanel.SetActive(true);
            // 유저의 현재 업그레이드 레벨(GameData)을 반영해 탭별 아이템 생성 및 제목 설정
            RefreshWorkshopTab(currentWorkshopTab);
        }

        #endregion

        #region Workshop

        void EnsureWorkshopPanel()
        {
            if (workshopPanel != null || mainTransform == null) return;

            workshopPanel = new GameObject("WorkshopPanel");
            workshopPanel.transform.SetParent(mainTransform, false);
            var panelRect = workshopPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImg = workshopPanel.AddComponent<Image>();
            panelImg.color = DarkBg;

            var titleGo = CreateLabel(workshopPanel.transform, "Title", "공격 업그레이드", 36);
            var titleR = titleGo.GetComponent<RectTransform>();
            titleR.anchorMin = new Vector2(0, 1);
            titleR.anchorMax = new Vector2(1, 1);
            titleR.pivot = new Vector2(0.5f, 1);
            titleR.anchoredPosition = new Vector2(0, -20);
            titleR.sizeDelta = new Vector2(0, 60);
            titleGo.alignment = TextAnchor.MiddleCenter;
            workshopTitleText = titleGo;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(workshopPanel.transform, false);
            var contentR = contentGo.AddComponent<RectTransform>();
            contentR.anchorMin = new Vector2(0, 0);
            contentR.anchorMax = new Vector2(1, 1);
            contentR.offsetMin = new Vector2(20, 120);
            contentR.offsetMax = new Vector2(-20, 100);
            var grid = contentGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(320, 56);
            grid.spacing = new Vector2(12, 8);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            workshopContentGrid = contentGo.transform;

            var tabsRow = new GameObject("CategoryTabs");
            tabsRow.transform.SetParent(workshopPanel.transform, false);
            var tabsR = tabsRow.AddComponent<RectTransform>();
            tabsR.anchorMin = new Vector2(0, 0);
            tabsR.anchorMax = new Vector2(1, 0);
            tabsR.pivot = new Vector2(0.5f, 0);
            tabsR.anchoredPosition = new Vector2(0, 10);
            tabsR.sizeDelta = new Vector2(0, 56);
            var h = tabsRow.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleCenter;
            h.spacing = 16;
            h.padding = new RectOffset(20, 0, 0, 0);
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;

            string[] tabLabels = { "공격", "방어", "유틸", "카드" };
            workshopCategoryTabs = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                var btnGo = CreateButton(tabsRow.transform, "Tab_" + i, tabLabels[i], 26);
                btnGo.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 44);
                var btn = btnGo.GetComponent<Button>();
                var c = btn.colors;
                c.normalColor = PanelBg;
                c.highlightedColor = AccentBlue * 0.8f;
                btn.colors = c;
                workshopCategoryTabs[i] = btn;
                int idx = i;
                btn.onClick.AddListener(() => OnWorkshopCategoryTab(idx));
            }

            workshopPanel.SetActive(false);
        }

        void OnWorkshopCategoryTab(int index)
        {
            currentWorkshopTab = Mathf.Clamp(index, 0, 3);
            RefreshWorkshopTab(currentWorkshopTab);
        }

        void RefreshWorkshopTab(int tabIndex)
        {
            int clampedIndex = Mathf.Clamp(tabIndex, 0, WorkshopTabTitles.Length - 1);
            if (workshopTitleText != null)
                workshopTitleText.text = WorkshopTabTitles[clampedIndex];

            if (workshopContentGrid == null) return;
            for (int i = workshopContentGrid.childCount - 1; i >= 0; i--)
                Destroy(workshopContentGrid.GetChild(i).gameObject);

            var tab = (UpgradeItemInfo.Tab)clampedIndex;
            foreach (var entry in UpgradeItemInfo.GetByTab(tab))
            {
                int level = GameData.GetWorkshopLevel(entry.Id);
                int cost = UpgradeConfig.GetCostForUpgrade(entry.Id, level);
                string valueStr = level.ToString();
                if (level >= entry.MaxLevel) valueStr = "Max";
                string costStr = level >= entry.MaxLevel ? "최대" : "C " + FormatBigNumber(cost);
                GameObject row = workshopRowPrefab != null
                    ? CreateWorkshopRowFromPrefab(entry.DisplayName, valueStr, costStr, entry.Id, cost, entry.MaxLevel)
                    : CreateWorkshopRow(entry.DisplayName, valueStr, costStr, entry.Id, cost, entry.MaxLevel);
                row.transform.SetParent(workshopContentGrid, false);
            }

            RefreshValues();
        }

        GameObject CreateWorkshopRow(string displayName, string valueStr, string costStr, string upgradeId, int cost, int maxLevel)
        {
            int currentLevel = GameData.GetWorkshopLevel(upgradeId);
            var row = new GameObject("WorkshopRow");
            row.AddComponent<RectTransform>();
            var img = row.AddComponent<Image>();
            img.color = PanelBg;

            // WorkshopRow 직계: Name
            var nameText = CreateLabel(row.transform, "Name", displayName, 22);
            SetRect(nameText.GetComponent<RectTransform>(), 0, 0.5f, 10, 0, 140, 24);

            // Buy 컨테이너 (Text, Value, Cost 자식)
            var buyGo = new GameObject("Buy");
            buyGo.transform.SetParent(row.transform, false);
            var buyRect = buyGo.AddComponent<RectTransform>();
            buyRect.anchorMin = new Vector2(0.35f, 0);
            buyRect.anchorMax = new Vector2(1f, 1f);
            buyRect.offsetMin = new Vector2(8, 4);
            buyRect.offsetMax = new Vector2(-8, -4);
            var buyImg = buyGo.AddComponent<Image>();
            buyImg.color = PanelBg;
            var buyBtn = buyGo.AddComponent<Button>();
            buyBtn.interactable = currentLevel < maxLevel && GameData.Coins >= cost;
            buyBtn.onClick.AddListener(() =>
            {
                if (GameData.TryBuyWorkshopLevel(upgradeId, cost, maxLevel))
                {
                    RefreshValues();
                    RefreshWorkshopTab(currentWorkshopTab);
                }
            });

            var buyLabel = CreateLabel(buyGo.transform, "Text", currentLevel >= maxLevel ? "최대" : "C", 18);
            SetRect(buyLabel.GetComponent<RectTransform>(), 0, 0.5f, 8, 0, 36, 22);
            var valueText = CreateLabel(buyGo.transform, "Value", valueStr, 20);
            SetRect(valueText.GetComponent<RectTransform>(), 0.25f, 0.5f, 0, 0, 80, 22);
            var costText = CreateLabel(buyGo.transform, "Cost", costStr, 18);
            SetRect(costText.GetComponent<RectTransform>(), 0.6f, 0.5f, 0, 0, 90, 20);

            return row;
        }

        /// <summary> WorkshopRow 프리팹 인스턴스 생성. Name, Buy(Text/Value/Cost) 설정 및 구매 버튼 연결. </summary>
        GameObject CreateWorkshopRowFromPrefab(string displayName, string valueStr, string costStr, string upgradeId, int cost, int maxLevel)
        {
            int currentLevel = GameData.GetWorkshopLevel(upgradeId);
            var row = Instantiate(workshopRowPrefab);
            row.name = "WorkshopRow";

            var nameT = row.transform.Find("Name");
            if (nameT != null)
            {
                var t = nameT.GetComponent<Text>();
                if (t != null) t.text = displayName;
            }

            var buyT = row.transform.Find("Buy");
            if (buyT != null)
            {
                var buyBtn = buyT.GetComponent<Button>();
                if (buyBtn != null)
                {
                    buyBtn.interactable = currentLevel < maxLevel && GameData.Coins >= cost;
                    buyBtn.onClick.AddListener(() =>
                    {
                        if (GameData.TryBuyWorkshopLevel(upgradeId, cost, maxLevel))
                        {
                            RefreshValues();
                            RefreshWorkshopTab(currentWorkshopTab);
                        }
                    });
                }

                var textT = buyT.Find("Text");
                if (textT != null)
                {
                    var t = textT.GetComponent<Text>();
                    if (t != null) t.text = currentLevel >= maxLevel ? "최대" : "C";
                }
                var valueT = buyT.Find("Value");
                if (valueT != null)
                {
                    var t = valueT.GetComponent<Text>();
                    if (t != null) t.text = valueStr;
                }
                var costT = buyT.Find("Cost");
                if (costT != null)
                {
                    var t = costT.GetComponent<Text>();
                    if (t != null) t.text = costStr;
                }
            }

            return row;
        }

        #endregion

        #region Refresh & Formatting

        void RefreshValues()
        {
            if (coinsText) coinsText.text = "C " + FormatBigNumber(GameData.Coins);
            if (gemsText) gemsText.text = "💎 " + GameData.Gems;
            if (powerStonesText) powerStonesText.text = "▲ " + GameData.PowerStones;
            if (coinBonusText) coinBonusText.text = "C x" + GameData.CoinBonusMultiplier.ToString("F2");
            if (tierText) tierText.text = "Tier " + GameData.CurrentTier;
            if (highestWaveText) highestWaveText.text = "Highest Wave: " + GameData.HighestWave;
            if (tierCoinMultiplierText) tierCoinMultiplierText.text = "C x" + GameData.GetTierCoinMultiplier(GameData.CurrentTier).ToString("F1");
        }

        static string FormatBigNumber(long n)
        {
            if (n >= 1_000_000_000_000) return (n / 1_000_000_000_000f).ToString("F1") + "T";
            if (n >= 1_000_000_000) return (n / 1_000_000_000f).ToString("F1") + "B";
            if (n >= 1_000_000) return (n / 1_000_000f).ToString("F1") + "M";
            if (n >= 1_000) return (n / 1_000f).ToString("F1") + "K";
            return n.ToString();
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

        static Text CreateLabel(Transform parent, string name, string content, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            return text;
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
            SetFullRect(textRect, 5, 5, 5, 5);
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = fontSize;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            return go;
        }

        #endregion

        static void SetFullRect(RectTransform r, float left, float bottom, float right, float top)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(left, bottom);
            r.offsetMax = new Vector2(-right, -top);
        }
    }
}
