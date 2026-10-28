#if UNITY_EDITOR
using System.IO;
using Game.Scripts.Shop;
using Game.Scripts.UI;
using Game.Scripts.UI.Shop;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Editor
{
    public static class ShopUiSetup
    {
        private const string ShopUiPrefabPath = "Assets/Game/Prefabs/UI/ShopUI.prefab";
        private const string CardPrefabPath = "Assets/Game/Prefabs/UI/ShopItemCard.prefab";
        private const string ShopIconsDir = "Assets/Game/Art/Shop";
        private const string IconSkinPath = ShopIconsDir + "/Icon_Skin.png";
        private const string IconProjectilePath = ShopIconsDir + "/Icon_Projectile.png";
        private const string IconHatPath = ShopIconsDir + "/Icon_Hat.png";
        private const string CartIconPath = "Assets/Hyper_Casual_UI/Sprites/Icons/shop.png";
        private const string CoinIconPath = "Assets/Hyper_Casual_UI/Sprites/Icons/coin.png";
        private const string CloseIconPath = "Assets/Game/Art/UI/KenneyRpg/iconCross_beige.png";

        [MenuItem("Tools/Shop/Build Shop UI Prefab And Wire Scenes")]
        public static void BuildAndWire()
        {
            EnsureShopIcons();
            UpdateStubDescriptions();
            AssignItemIcons();

            EnsureFolder("Assets/Game/Prefabs/UI");
            ShopItemView cardView = BuildCardPrefabAsset();

            GameObject root = BuildShopUiRoot(cardView);
            PrefabUtility.SaveAsPrefabAsset(root, ShopUiPrefabPath);
            Object.DestroyImmediate(root);

            WireLevelScene("Assets/Game/Scenes/Level_1.unity", ShopUiPrefabPath);
            WireLevelScene("Assets/Game/Scenes/Level_Tutorial.unity", ShopUiPrefabPath);
            AddCoinRewardToEnemies();

            AssetDatabase.SaveAssets();
            Debug.Log("[ShopUiSetup] Done");
        }

        [MenuItem("Tools/Shop/Update Stub Descriptions")]
        public static void UpdateStubDescriptions()
        {
            void Patch(string path, string id, CosmeticCategory cat, string nRu, string nEn, string nTr,
                string dRu, string dEn, string dTr, int price, bool iap, string iapId)
            {
                var item = AssetDatabase.LoadAssetAtPath<ShopItemDefinition>(path);
                if (item == null)
                    return;
                item.ApplyStubData(id, cat, nRu, nEn, nTr, dRu, dEn, dTr, price, iap, iapId);
                EditorUtility.SetDirty(item);
            }

            CharacterContentSetup.ConfigureCatalog();
            Patch("Assets/Game/Configs/Shop/Skin_Iap_Trial.asset", "iap_skin_item", CosmeticCategory.Skin,
                "Премиум скин", "Premium Skin", "Premium Skin",
                "Пробный скин за реальные деньги (Yandex).", "Trial skin for real money (Yandex).", "Trial skin for real money (Yandex).",
                0, true, "iap_skin_trial");
            Patch("Assets/Game/Configs/Shop/Projectile_Stub_01.asset", "proj_stub_01", CosmeticCategory.Projectile,
                "Снаряд-заглушка", "Stub Projectile", "Stub Projectile",
                "Пробный вид снаряда за монеты.", "Coin projectile stub.", "Coin projectile stub.",
                40, false, "");
            Patch("Assets/Game/Configs/Shop/Projectile_Iap_Trial.asset", "iap_proj_item", CosmeticCategory.Projectile,
                "Премиум снаряд", "Premium Projectile", "Premium Projectile",
                "Пробный снаряд за реалы (Yandex).", "Trial projectile IAP.", "Trial projectile IAP.",
                0, true, "iap_proj_trial");
            Patch("Assets/Game/Configs/Shop/Hat_Stub_01.asset", "hat_stub_01", CosmeticCategory.Hat,
                "Шапка-заглушка", "Stub Hat", "Stub Hat",
                "Пробная шапка за монеты.", "Coin hat stub.", "Coin hat stub.",
                30, false, "");
            Patch("Assets/Game/Configs/Shop/Hat_Iap_Trial.asset", "iap_hat_item", CosmeticCategory.Hat,
                "Премиум шапка", "Premium Hat", "Premium Hat",
                "Пробная шапка за реалы (Yandex).", "Trial hat IAP.", "Trial hat IAP.",
                0, true, "iap_hat_trial");

            AssetDatabase.SaveAssets();
        }

        private static void EnsureShopIcons()
        {
            EnsureFolder(ShopIconsDir);
            CopyAsSprite("Assets/Hyper_Casual_UI/Sprites/Icons/character.png", IconSkinPath);
            CopyAsSprite("Assets/Game/Art/UI/KenneyRpg/cursorSword_gold.png", IconProjectilePath);
            CopyAsSprite("Assets/Hyper_Casual_UI/Sprites/Icons/crown.png", IconHatPath);
        }

        private static void CopyAsSprite(string source, string dest)
        {
            if (File.Exists(source) == false)
            {
                Debug.LogWarning("[ShopUiSetup] Missing icon source: " + source);
                return;
            }

            File.Copy(source, dest, true);
            AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(dest) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 256;
            importer.SaveAndReimport();
        }

        private static void AssignItemIcons()
        {
            CharacterContentSetup.ConfigureCatalog();
            AssignIcon("Assets/Game/Configs/Shop/Skin_Iap_Trial.asset", IconSkinPath);
            AssignIcon("Assets/Game/Configs/Shop/Projectile_Stub_01.asset", IconProjectilePath);
            AssignIcon("Assets/Game/Configs/Shop/Projectile_Iap_Trial.asset", IconProjectilePath);
            AssignIcon("Assets/Game/Configs/Shop/Hat_Stub_01.asset", IconHatPath);
            AssignIcon("Assets/Game/Configs/Shop/Hat_Iap_Trial.asset", IconHatPath);
        }

        private static void AssignIcon(string itemPath, string spritePath)
        {
            var item = AssetDatabase.LoadAssetAtPath<ShopItemDefinition>(itemPath);
            if (item == null)
                return;

            string guid = AssetDatabase.AssetPathToGUID(spritePath);
            if (string.IsNullOrEmpty(guid))
                return;

            var so = new SerializedObject(item);
            SerializedProperty icon = so.FindProperty("_iconReference");
            icon.FindPropertyRelative("m_AssetGUID").stringValue = guid;
            icon.FindPropertyRelative("m_SubObjectName").stringValue = string.Empty;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        private static ElvenForgeUiTheme LoadTheme()
        {
            return ApplyElvenForgeUiTheme.EnsureThemeAsset();
        }

        private static void StyleTmp(TextMeshProUGUI tmp, ElvenForgeUiTheme theme, Color color, bool display = false)
        {
            if (theme == null)
                return;
            tmp.font = display && theme.DisplayFont != null ? theme.DisplayFont : theme.BodyFont;
            tmp.color = color;
        }

        private static void StyleImage(Image img, Sprite sprite, Color color, bool sliced = true)
        {
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            }

            img.color = color;
        }

        private static ShopItemView BuildCardPrefabAsset()
        {
            GameObject card = BuildCardGameObject();
            PrefabUtility.SaveAsPrefabAsset(card, CardPrefabPath);
            Object.DestroyImmediate(card);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            return prefab.GetComponent<ShopItemView>();
        }

        private static GameObject BuildCardGameObject()
        {
            var theme = LoadTheme();
            Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CoinIconPath);
            Sprite skinIcon = AssetDatabase.LoadAssetAtPath<Sprite>(IconSkinPath);
            Sprite projectileIcon = AssetDatabase.LoadAssetAtPath<Sprite>(IconProjectilePath);
            Sprite hatIcon = AssetDatabase.LoadAssetAtPath<Sprite>(IconHatPath);

            var card = new GameObject("ShopItemCard", typeof(RectTransform));
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(420f, 600f);

            card.AddComponent<Image>();
            StyleImage(card.GetComponent<Image>(), theme.PanelParchment, Color.white);

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 16, 16);
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var iconArea = CreateUiObject("IconArea", card.transform);
            var iconAreaLe = iconArea.AddComponent<LayoutElement>();
            iconAreaLe.preferredHeight = 180f;
            iconAreaLe.minHeight = 96f;
            iconAreaLe.flexibleHeight = 1f;
            iconArea.AddComponent<Image>();
            StyleImage(iconArea.GetComponent<Image>(), theme.PanelHud, Color.white);

            var placeholder = CreateUiObject("Placeholder", iconArea.transform);
            StretchFull(placeholder.GetComponent<RectTransform>());
            var placeholderImg = placeholder.AddComponent<Image>();
            placeholderImg.preserveAspect = true;
            placeholderImg.color = Color.white;
            if (skinIcon != null)
            {
                placeholderImg.sprite = skinIcon;
                placeholderImg.type = Image.Type.Simple;
            }

            var iconGo = CreateUiObject("Icon", iconArea.transform);
            StretchFull(iconGo.GetComponent<RectTransform>());
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.enabled = false;

            var titleGo = CreateUiObject("Title", card.transform);
            var titleLayout = titleGo.AddComponent<LayoutElement>();
            titleLayout.minHeight = titleLayout.preferredHeight = 44f;
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.fontSize = 26;
            title.alignment = TextAlignmentOptions.Center;
            title.text = "Title";
            title.fontStyle = FontStyles.Bold;
            title.enableWordWrapping = true;
            title.maxVisibleLines = 2;
            title.overflowMode = TextOverflowModes.Ellipsis;
            StyleTmp(title, theme, theme.TextDark, display: true);

            var statusGo = CreateUiObject("Status", card.transform);
            var statusLayout = statusGo.AddComponent<LayoutElement>();
            statusLayout.minHeight = statusLayout.preferredHeight = 24f;
            var status = statusGo.AddComponent<TextMeshProUGUI>();
            status.fontSize = 18;
            status.alignment = TextAlignmentOptions.Center;
            status.text = string.Empty;
            StyleTmp(status, theme, theme.Gold);

            var actionGo = CreateUiObject("BuyButton", card.transform);
            var actionLayout = actionGo.AddComponent<LayoutElement>();
            actionLayout.minHeight = actionLayout.preferredHeight = 56f;
            actionGo.AddComponent<Image>();
            StyleImage(actionGo.GetComponent<Image>(), theme.ButtonPrimary, Color.white);
            var actionBtn = actionGo.AddComponent<Button>();
            var actionH = actionGo.AddComponent<HorizontalLayoutGroup>();
            actionH.padding = new RectOffset(12, 12, 6, 6);
            actionH.spacing = 8f;
            actionH.childAlignment = TextAnchor.MiddleCenter;
            actionH.childControlWidth = true;
            actionH.childControlHeight = true;
            actionH.childForceExpandWidth = true;
            actionH.childForceExpandHeight = true;

            var actionLabelGo = CreateUiObject("ActionLabel", actionGo.transform);
            var actionLabel = actionLabelGo.AddComponent<TextMeshProUGUI>();
            actionLabel.text = "Купить";
            actionLabel.fontSize = 22;
            actionLabel.alignment = TextAlignmentOptions.MidlineLeft;
            StyleTmp(actionLabel, theme, theme.TextLight);

            var coinGo = CreateUiObject("CoinIcon", actionGo.transform);
            var coinLe = coinGo.AddComponent<LayoutElement>();
            coinLe.preferredWidth = 28f;
            coinLe.preferredHeight = 28f;
            coinLe.flexibleWidth = 0f;
            var coinImg = coinGo.AddComponent<Image>();
            coinImg.preserveAspect = true;
            if (coinSprite != null)
            {
                coinImg.sprite = coinSprite;
                coinImg.type = Image.Type.Simple;
            }

            var priceGo = CreateUiObject("Price", actionGo.transform);
            var priceLe = priceGo.AddComponent<LayoutElement>();
            priceLe.preferredWidth = 72f;
            priceLe.flexibleWidth = 0f;
            var price = priceGo.AddComponent<TextMeshProUGUI>();
            price.text = "0";
            price.fontSize = 22;
            price.alignment = TextAlignmentOptions.MidlineRight;
            price.fontStyle = FontStyles.Bold;
            StyleTmp(price, theme, theme.Ember);

            var view = card.AddComponent<ShopItemView>();
            SetPrivate(view, "_icon", iconImg);
            SetPrivate(view, "_iconPlaceholder", placeholderImg);
            SetPrivate(view, "_title", title);
            SetPrivate(view, "_status", status);
            SetPrivate(view, "_actionButton", actionBtn);
            SetPrivate(view, "_actionLabel", actionLabel);
            SetPrivate(view, "_priceLabel", price);
            SetPrivate(view, "_coinIcon", coinImg);
            SetPrivate(view, "_skinPlaceholder", skinIcon);
            SetPrivate(view, "_projectilePlaceholder", projectileIcon);
            SetPrivate(view, "_hatPlaceholder", hatIcon);

            return card;
        }

        private static GameObject BuildShopUiRoot(ShopItemView itemPrefab)
        {
            var theme = LoadTheme();
            Sprite cartSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CartIconPath);
            Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CoinIconPath);
            Sprite closeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CloseIconPath);

            var root = new GameObject("ShopUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var rootRt = root.GetComponent<RectTransform>();
            StretchFull(rootRt);

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1100;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;

            var hudCoin = CreateCoinChip("CoinCounter", root.transform, theme, coinSprite, new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(0.5f, 1f));
            var hudCoinTmp = hudCoin.GetComponentInChildren<TextMeshProUGUI>(true);
            var hudCoinCounter = hudCoin.AddComponent<CoinCounterUI>();
            SetPrivate(hudCoinCounter, "_label", hudCoinTmp);

            var btnGo = CreateUiObject("ShopButton", root.transform);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0f, 1f);
            btnRt.anchorMax = new Vector2(0f, 1f);
            btnRt.pivot = new Vector2(0f, 1f);
            btnRt.anchoredPosition = new Vector2(24f, -24f);
            btnRt.sizeDelta = new Vector2(96f, 96f);
            var btnImage = btnGo.AddComponent<Image>();
            StyleImage(btnImage, theme.ButtonIcon, Color.white);
            var openButton = btnGo.AddComponent<Button>();
            var cartGo = CreateUiObject("Icon", btnGo.transform);
            StretchFull(cartGo.GetComponent<RectTransform>());
            var cartImg = cartGo.AddComponent<Image>();
            cartImg.preserveAspect = true;
            cartImg.raycastTarget = false;
            if (cartSprite != null)
            {
                cartImg.sprite = cartSprite;
                cartImg.type = Image.Type.Simple;
            }

            var panelGo = CreateUiObject("ShopPanel", root.transform);
            StretchFull(panelGo.GetComponent<RectTransform>());
            panelGo.AddComponent<Image>().color = theme.DimOverlay;
            panelGo.SetActive(false);

            var window = CreateUiObject("Window", panelGo.transform);
            var windowRt = window.GetComponent<RectTransform>();
            windowRt.anchorMin = new Vector2(0.05f, 0.08f);
            windowRt.anchorMax = new Vector2(0.95f, 0.92f);
            windowRt.offsetMin = Vector2.zero;
            windowRt.offsetMax = Vector2.zero;
            windowRt.pivot = new Vector2(0.5f, 0.5f);
            window.AddComponent<Image>();
            StyleImage(window.GetComponent<Image>(), theme.PanelModal, Color.white);

            var header = CreateUiObject("Header", window.transform);
            var headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.anchoredPosition = Vector2.zero;
            headerRt.sizeDelta = new Vector2(0f, 88f);
            var headerLayout = header.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset(20, 16, 16, 8);
            headerLayout.spacing = 12f;
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            var titleGo = CreateUiObject("Title", header.transform);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.flexibleWidth = 1f;
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.text = "Магазин";
            title.fontSize = 40;
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.fontStyle = FontStyles.Bold;
            StyleTmp(title, theme, theme.TextLight, display: true);

            var panelCoin = CreateCoinChip("PanelCoinCounter", header.transform, theme, coinSprite, Vector2.zero,
                Vector2.one, Vector2.zero, new Vector2(0.5f, 0.5f));
            var panelCoinLe = panelCoin.GetComponent<LayoutElement>();
            if (panelCoinLe == null)
                panelCoinLe = panelCoin.AddComponent<LayoutElement>();
            panelCoinLe.preferredWidth = 200f;
            panelCoinLe.preferredHeight = 56f;
            panelCoinLe.flexibleWidth = 0f;
            var panelCoinTmp = panelCoin.GetComponentInChildren<TextMeshProUGUI>(true);
            var panelCoinCounter = panelCoin.AddComponent<CoinCounterUI>();
            SetPrivate(panelCoinCounter, "_label", panelCoinTmp);

            var closeGo = CreateUiObject("CloseButton", header.transform);
            var closeLe = closeGo.AddComponent<LayoutElement>();
            closeLe.preferredWidth = 56f;
            closeLe.preferredHeight = 56f;
            closeLe.flexibleWidth = 0f;
            closeGo.AddComponent<Image>();
            StyleImage(closeGo.GetComponent<Image>(), theme.ButtonIcon, Color.white);
            var closeBtn = closeGo.AddComponent<Button>();
            var closeIconGo = CreateUiObject("Icon", closeGo.transform);
            StretchFull(closeIconGo.GetComponent<RectTransform>());
            var closeImg = closeIconGo.AddComponent<Image>();
            closeImg.preserveAspect = true;
            closeImg.raycastTarget = false;
            if (closeSprite != null)
            {
                closeImg.sprite = closeSprite;
                closeImg.type = Image.Type.Simple;
            }

            var tabs = CreateUiObject("Tabs", window.transform);
            var tabsRt = tabs.GetComponent<RectTransform>();
            tabsRt.anchorMin = new Vector2(0f, 0f);
            tabsRt.anchorMax = new Vector2(1f, 0f);
            tabsRt.pivot = new Vector2(0.5f, 0f);
            tabsRt.anchoredPosition = Vector2.zero;
            tabsRt.sizeDelta = new Vector2(0f, 88f);
            tabs.AddComponent<Image>();
            StyleImage(tabs.GetComponent<Image>(), theme.PanelHud, Color.white);
            var tabLayout = tabs.AddComponent<HorizontalLayoutGroup>();
            tabLayout.padding = new RectOffset(16, 16, 14, 14);
            tabLayout.spacing = 12f;
            tabLayout.childAlignment = TextAnchor.MiddleCenter;
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;

            Button MakeTab(string name, string label, out Image image, out TextMeshProUGUI tmp)
            {
                var tab = CreateUiObject(name, tabs.transform);
                image = tab.AddComponent<Image>();
                StyleImage(image, theme.ButtonSecondary, Color.white);
                var b = tab.AddComponent<Button>();
                var l = CreateUiObject("Label", tab.transform);
                StretchFull(l.GetComponent<RectTransform>());
                tmp = l.AddComponent<TextMeshProUGUI>();
                tmp.text = label;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 22;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 16;
                tmp.fontSizeMax = 24;
                StyleTmp(tmp, theme, theme.TextLight);
                return b;
            }

            var tabSkins = MakeTab("TabSkins", "Скины", out Image tabSkinsImg, out TextMeshProUGUI tabSkinsLabel);
            StyleImage(tabSkinsImg, theme.ButtonPrimary, Color.white);
            var tabProj = MakeTab("TabProjectiles", "Снаряды", out Image tabProjImg, out TextMeshProUGUI tabProjLabel);
            var tabHats = MakeTab("TabHats", "Шапки", out Image tabHatsImg, out TextMeshProUGUI tabHatsLabel);
            tabProj.gameObject.SetActive(false);
            tabHats.gameObject.SetActive(false);

            var scrollGo = CreateUiObject("CardScroll", window.transform);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(8f, 88f);
            scrollRt.offsetMax = new Vector2(-8f, -88f);

            var viewport = CreateUiObject("Viewport", scrollGo.transform);
            StretchFull(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            viewport.AddComponent<RectMask2D>();
            var viewportRt = viewport.GetComponent<RectTransform>();

            var content = CreateUiObject("Content", viewport.transform);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = Vector2.zero;

            var grid = content.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.cellSize = new Vector2(420f, 600f);
            grid.spacing = new Vector2(16f, 16f);
            grid.padding = new RectOffset(20, 20, 20, 20);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;
            scroll.decelerationRate = 0.135f;
            scroll.scrollSensitivity = 40f;
            scroll.viewport = viewportRt;
            scroll.content = contentRt;

            var panel = panelGo.AddComponent<ShopPanel>();
            SetPrivate(panel, "_root", panelGo);
            SetPrivate(panel, "_closeButton", closeBtn);
            SetPrivate(panel, "_tabSkins", tabSkins);
            SetPrivate(panel, "_tabProjectiles", tabProj);
            SetPrivate(panel, "_tabHats", tabHats);
            SetPrivate(panel, "_tabSkinsImage", tabSkinsImg);
            SetPrivate(panel, "_tabProjectilesImage", tabProjImg);
            SetPrivate(panel, "_tabHatsImage", tabHatsImg);
            SetPrivate(panel, "_tabSkinsLabel", tabSkinsLabel);
            SetPrivate(panel, "_tabProjectilesLabel", tabProjLabel);
            SetPrivate(panel, "_tabHatsLabel", tabHatsLabel);
            SetPrivate(panel, "_title", title);
            SetPrivate(panel, "_itemsParent", contentRt);
            SetPrivate(panel, "_itemPrefab", itemPrefab);
            SetPrivate(panel, "_scrollRect", scroll);
            SetPrivate(panel, "_viewport", viewportRt);
            SetPrivate(panel, "_grid", grid);
            SetPrivate(panel, "_tabSelectedSprite", theme.ButtonPrimary);
            SetPrivate(panel, "_tabIdleSprite", theme.ButtonSecondary);
            SetPrivate(panel, "_hideWhileOpen", new[] { btnGo, hudCoin });

            var preStart = btnGo.AddComponent<PreStartShopButton>();
            SetPrivate(preStart, "_button", openButton);
            SetPrivate(preStart, "_shopPanel", panel);
            SetPrivate(preStart, "_root", btnGo);

            return root;
        }

        private static GameObject CreateCoinChip(string name, Transform parent, ElvenForgeUiTheme theme, Sprite coinSprite,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 pivot)
        {
            var chip = CreateUiObject(name, parent);
            var rt = chip.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(200f, 56f);
            chip.AddComponent<Image>();
            StyleImage(chip.GetComponent<Image>(), theme.ChipCoin, Color.white);

            var layout = chip.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 16, 8, 8);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            var iconGo = CreateUiObject("CoinIcon", chip.transform);
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 32f;
            iconLe.preferredHeight = 32f;
            iconLe.flexibleWidth = 0f;
            var icon = iconGo.AddComponent<Image>();
            icon.preserveAspect = true;
            if (coinSprite != null)
            {
                icon.sprite = coinSprite;
                icon.type = Image.Type.Simple;
            }

            var labelGo = CreateUiObject("Label", chip.transform);
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1f;
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.text = "0";
            StyleTmp(tmp, theme, theme.TextDark);
            return chip;
        }

        private static void WireLevelScene(string scenePath, string prefabPath)
        {
            if (File.Exists(scenePath) == false)
            {
                Debug.LogWarning("[ShopUiSetup] Scene missing: " + scenePath);
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var existing = GameObject.Find("ShopUI");
            if (existing != null)
                Object.DestroyImmediate(existing);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "ShopUI";

            var panel = instance.GetComponentInChildren<ShopPanel>(true);
            var preStart = instance.GetComponentInChildren<PreStartShopButton>(true);
            var coins = instance.GetComponentInChildren<CoinCounterUI>(true);

            var scopes = Object.FindObjectsOfType<Game.Scripts.DI.GameLifetimeScope>(true);
            if (scopes.Length > 0)
            {
                var so = new SerializedObject(scopes[0]);
                so.FindProperty("_shopPanel").objectReferenceValue = panel;
                so.FindProperty("_preStartShopButton").objectReferenceValue = preStart;
                so.FindProperty("_coinCounterUI").objectReferenceValue = coins;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(scopes[0]);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AddCoinRewardToEnemies()
        {
            string[] paths =
            {
                "Assets/Game/Prefabs/Enemys/EnemyGoblin.prefab",
                "Assets/Game/Prefabs/Enemys/EnemyOrc.prefab",
                "Assets/Game/Prefabs/Enemys/EnemyTroll.prefab"
            };

            foreach (string path in paths)
            {
                if (File.Exists(path) == false)
                    continue;

                var root = PrefabUtility.LoadPrefabContents(path);
                if (root.GetComponent<CoinReward>() == null)
                    root.AddComponent<CoinReward>();
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) == false && AssetDatabase.IsValidFolder(parent) == false)
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
        }

        private static void SetPrivate(object target, string field, object value)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var f = target.GetType().GetField(field, flags);
            if (f != null)
                f.SetValue(target, value);
            else
                Debug.LogWarning($"Field missing: {target.GetType().Name}.{field}");
        }
    }
}
#endif
