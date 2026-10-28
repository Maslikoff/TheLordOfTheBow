using TMPro;
using UnityEngine;

namespace Game.Scripts.UI
{
    [CreateAssetMenu(menuName = "Game/UI/Elven Forge UI Theme", fileName = "ElvenForgeUiTheme")]
    public class ElvenForgeUiTheme : ScriptableObject
    {
        [Header("Fonts")]
        public TMP_FontAsset DisplayFont;
        public TMP_FontAsset BodyFont;

        [Header("Sprites (9-slice)")]
        public Sprite PanelModal;
        public Sprite PanelParchment;
        public Sprite PanelHud;
        public Sprite ButtonPrimary;
        public Sprite ButtonSecondary;
        public Sprite ButtonDanger;
        public Sprite ButtonReward;
        public Sprite ButtonIcon;
        public Sprite ChipCoin;
        public Sprite SliderTrack;
        public Sprite SliderFill;
        public Sprite SliderFillGold;

        [Header("Surfaces")]
        public Color Stone = Hex("2A2118");
        public Color StoneMid = Hex("3D3228");
        public Color StoneLight = Hex("5A4A3A");
        public Color Parchment = Hex("E8DCC8");
        public Color DimOverlay = new Color(0f, 0f, 0f, 0.55f);

        [Header("Accents")]
        public Color Gold = Hex("C9A227");
        public Color Mithril = Hex("B8C4D0");
        public Color Teal = Hex("5B8FA8");
        public Color Ember = Hex("D4A574");
        public Color Moss = Hex("6B8F5A");
        public Color Health = Hex("7CB86A");
        public Color Forest = Hex("3D5A32");
        public Color Danger = Hex("8B4A3A");
        public Color Reward = Hex("C9A227");

        [Header("Text")]
        public Color TextLight = Hex("F5EFE0");
        public Color TextMuted = Hex("A89880");
        public Color TextDark = Hex("2A241C");

        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex.TrimStart('#'), out var c))
                return c;
            return Color.magenta;
        }
    }
}
