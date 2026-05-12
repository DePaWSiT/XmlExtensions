using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace XmlExtensions.Setting
{
    public class CollapsibleSettings : SettingContainer
    {
        public GameFont headerFont = GameFont.Small;
        public State defaultState = State.Closed;
        public Anchor anchor = Anchor.Left;
        public List<SettingContainer> settings;

        public enum Anchor
        {
            Left,
            Middle
        }

        public enum State
        {
            Open,
            Closed
        }

        private float headerHeight = 0f;
        private float labelPadding = 0f;
        private float buttonSize = 0f;
        private State state = State.Closed;
        private static Texture2D revealOpenTexture;

        private static Texture2D RevealOpenTexture
        {
            get
            {
                revealOpenTexture ??= RotatedClockwise(TexButton.Reveal);
                return revealOpenTexture;
            }
        }

        protected override bool Init()
        {
            searchType = SearchType.SearchAllAndHighlight;
            if (headerFont == GameFont.Medium)
            {
                labelPadding = 3;
                headerHeight = 29 + 2 * labelPadding;
                buttonSize = 29;
            }
            else if (headerFont == GameFont.Small)
            {
                labelPadding = 2;
                headerHeight = 22 + 2 * labelPadding;
                buttonSize = 22;
            }
            else
            {
                labelPadding = 1;
                headerHeight = 18 + 2 * labelPadding;
                buttonSize = 18;
            }
            if (!InitializeContainers(settings)) { return false; }
            return true;
        }

        protected override bool PreOpen()
        {
            state = defaultState;
            return true;
        }

        protected override float CalculateHeight(float width)
        {
            float height = headerHeight;
            if (state == State.Open)
                height += CalculateHeightSettingsList(width, settings);
            return height;
        }

        protected override void DrawSettingContents(Rect inRect)
        {
            Verse.Text.Font = headerFont;

            Rect headerRect = inRect.TopPartPixels(headerHeight);
            Rect headerRectInner = headerRect.ContractedBy(0f, labelPadding);

            // Draw header
            Widgets.DrawBoxSolid(headerRect, new(1f, 1f, 1f, 0.05f));
            headerRectInner.SplitVertically(buttonSize, out Rect buttonRect, out Rect labelRect);
            buttonRect.y -= 1;
            if (anchor == Anchor.Left) 
            {
                Widgets.Label(labelRect.TrimLeftPartPixels(6f), label.TranslateIfTKeyAvailable(tKey)); 
            }
            else
            {
                Verse.Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(headerRectInner, label.TranslateIfTKeyAvailable(tKey));
                Verse.Text.Anchor = TextAnchor.UpperLeft;
            }
            DrawRevealIcon(buttonRect, state == State.Open);

            Verse.Text.Font = GameFont.Small;

            // Draw highlight and tooltip
            Widgets.DrawHighlightIfMouseover(headerRect);
            if (!tooltip.NullOrEmpty()) { TooltipHandler.TipRegion(headerRect, tooltip.TranslateIfTKeyAvailable(tKeyTip)); }

            // Toggle settings
            if (Widgets.ButtonInvisible(headerRect)) { state = state == State.Open ? State.Closed : State.Open; }

            // Draw settings
            if (state == State.Open) { DrawSettingsList(inRect.TrimTopPartPixels(headerHeight), settings); }
        }
        

        

        private static void DrawRevealIcon(Rect rect, bool open)
        {
            GUI.DrawTexture(rect, open ? RevealOpenTexture : TexButton.Reveal);
        }

        private static Texture2D RotatedClockwise(Texture texture)
        {
            RenderTexture previous = RenderTexture.active;

            RenderTexture rt = RenderTexture.GetTemporary(
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.ARGB32
            );

            Graphics.Blit(texture, rt);
            RenderTexture.active = rt;

            Texture2D source = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            source.ReadPixels(new Rect(0f, 0f, texture.width, texture.height), 0, 0);
            source.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            Texture2D rotated = new Texture2D(source.height, source.width, TextureFormat.ARGB32, false);

            Color[] pixels = source.GetPixels();
            Color[] rotatedPixels = new Color[pixels.Length];

            int width = source.width;
            int height = source.height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rotatedPixels[(width - x - 1) * height + y] = pixels[y * width + x];
                }
            }

            rotated.SetPixels(rotatedPixels);
            rotated.Apply();

            Object.Destroy(source);
            return rotated;
        }

        protected override void DrawFilterBox(Rect inRect)
        {
            Rect headerRect = inRect.TopPartPixels(headerHeight);
            FilterBox(headerRect);
        }
    }
}