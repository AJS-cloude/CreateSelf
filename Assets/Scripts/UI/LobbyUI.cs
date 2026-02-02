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
        [Header("Optional: Battle scene name")]
        [SerializeField] string battleSceneName = "Battle";

        // 씬에 배치된 UI 참조 (이름으로 찾거나 Inspector 할당)
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

        static readonly Color DarkBg = new Color(0.15f, 0.12f, 0.25f);
        static readonly Color PanelBg = new Color(0.2f, 0.16f, 0.35f, 0.95f);
        static readonly Color AccentBlue = new Color(0.4f, 0.6f, 1f);
        static readonly Color AccentPurple = new Color(0.6f, 0.35f, 0.9f);

        void Awake()
        {
            EnsureEventSystem();

            var root = transform.Find("LobbyRoot");
            if (root != null)
                BindExistingUI(root);
            else
                BuildUI();
        }

        /// <summary>
        /// 씬에 배치된 LobbyRoot 하위 UI를 찾아 바인딩합니다.
        /// </summary>
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

            // Settings (LobbyRoot 직계)
            var settingsGo = root.Find("Settings");
            if (settingsGo != null)
            {
                settingsButton = settingsGo.GetComponent<Button>();
                if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            }

            var center = root.Find("Center");
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

            RefreshValues();
        }

        static Text GetText(Transform parent, string name)
        {
            var t = parent.Find(name);
            return t != null ? t.GetComponent<Text>() : null;
        }

        static Button GetButton(Transform parent, string name)
        {
            var t = parent.Find(name);
            return t != null ? t.GetComponent<Button>() : null;
        }

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
            BuildCenter(root.transform);
            BuildBottomNav(root.transform);
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

        void OnBattle()
        {
            if (!string.IsNullOrEmpty(battleSceneName) && Application.CanStreamedLevelBeLoaded(battleSceneName))
                SceneManager.LoadScene(battleSceneName);
            else
                Debug.Log("[Lobby] Battle scene not found. Create a scene named '" + battleSceneName + "' and add it to Build Settings.");
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

        static void SetFullRect(RectTransform r, float left, float bottom, float right, float top)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(left, bottom);
            r.offsetMax = new Vector2(-right, -top);
        }
    }
}
