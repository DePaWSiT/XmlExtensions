using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using XmlExtensions.Action;

namespace XmlExtensions.Setting
{
    internal class Text : SettingContainer
    {
        public string text;
        public GameFont font = GameFont.Small;
        public Anchor anchor = Anchor.Left;
        public List<string> keys;
        public string xpath;
        public List<ActionContainer> actions;

        public enum Anchor
        {
            Left = TextAnchor.UpperLeft,
            Middle = TextAnchor.UpperCenter,
            Right = TextAnchor.UpperRight
        }

        private float cachedHeight;
        private Vector2 cachedSize;

        protected override float CalculateHeight(float width)
        {
            Verse.Text.Font = font;
            Verse.Text.Anchor = (TextAnchor)anchor;

            string str = text.TranslateIfTKeyAvailable(tKey);
            cachedText = SubstituteVariables(str);

            float rawHeight = Verse.Text.CalcHeight(cachedText, width);

            if (IsFractionalUIScale())
            {
                cachedHeight = Mathf.Ceil(rawHeight + 2f);
            }
            else
            {
                cachedHeight = Mathf.Ceil(rawHeight);
            }

            cachedSize = Verse.Text.CalcSize(cachedText);
            cachedSize.x = Math.Min(cachedSize.x, width);

            Verse.Text.Font = GameFont.Small;
            Verse.Text.Anchor = TextAnchor.UpperLeft;

            return cachedHeight;
        }

        private static bool IsFractionalUIScale()
        {
            return Math.Abs(Prefs.UIScale - Mathf.Round(Prefs.UIScale)) > 0.001f;
        }

        protected override void DrawSettingContents(Rect inRect)
        {//M: 29 S: 22 T:18
            Verse.Text.Font = font;
            Verse.Text.Anchor = (TextAnchor)anchor;
            if (!tooltip.NullOrEmpty())
            {
                string tooltipLabel = tooltip.TranslateIfTKeyAvailable(tKeyTip);
                tooltipLabel = SubstituteVariables(tooltipLabel);
                TooltipHandler.TipRegion(inRect, tooltipLabel);
            }
            Rect alignedRect;
            if (anchor == Anchor.Left)
            {
                alignedRect = inRect.LeftPartPixels(cachedSize.x);
            }
            else if (anchor == Anchor.Right)
            {
                alignedRect = inRect.RightPartPixels(cachedSize.x);
            }
            else
            {
                alignedRect = inRect.MiddlePartPixels(cachedSize.x, cachedHeight);
            }
            if (actions != null)
            {
                Widgets.DrawHighlightIfMouseover(alignedRect);
                if (Widgets.ButtonInvisible(alignedRect))
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    int i = 0;
                    foreach (ActionContainer action in actions)
                    {
                        i++;
                        if (!action.DoAction())
                        {
                            Error("Failed action at index=" + i.ToString());
                            ErrorManager.PrintErrors();
                            break;
                        }
                    }
                }
            }
            Rect drawRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Widgets.Label(drawRect, cachedText);
            Verse.Text.Font = GameFont.Small;
            Verse.Text.Anchor = TextAnchor.UpperLeft;
        }

        protected override bool Init()
        {
            if (xpath != null)
            {
                text = PatchManager.XmlDocs.MainDocument.SelectSingleNode(xpath).InnerText;
            }
            string str = text.TranslateIfTKeyAvailable(tKey);
            cachedText = SubstituteVariables(str);
            return true;
        }

        private string SubstituteVariables(string inString)
        {
            if (keys != null)
            {
                foreach (string key in keys)
                    inString = inString.SubstituteVariable(key, SettingsManager.GetSetting(modId, key));
                if (keys.Count > 0 && inString.Contains("{defaultValue}"))
                    inString = inString.SubstituteVariable("defaultValue", SettingsManager.GetDefaultValue(modId, keys[0]));
            }
            if (key != null)
            {
                inString = inString.SubstituteVariable("key", SettingsManager.GetSetting(modId, key));
                if (inString.Contains("{defaultValue}"))
                    inString = inString.SubstituteVariable("defaultValue", SettingsManager.GetDefaultValue(modId, key));
            }
            return inString;
        }
    }
}