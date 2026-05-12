using System;
using UnityEngine;
using Verse;

namespace XmlExtensions.Setting
{
    internal class Slider : KeyedSettingContainer
    {
        public float min;
        public float max;
        public int decimals = 6;

        protected override float CalculateHeight(float width)
        {
            float height = 0f;

            if (label != null)
            {
                height += 22f;
            }

            height += SliderHeight();

            return height;
        }

        private static float SliderHeight()
        {
            if (Prefs.UIScale > 1f && Math.Abs(Prefs.UIScale / 2f - Mathf.Floor(Prefs.UIScale / 2f)) > float.Epsilon)
            {
                return 24f;
            }

            return 22f;
        }

        protected override void DrawSettingContents(Rect inRect)
        {
            float currFloat = float.Parse(SettingsManager.GetSetting(modId, key));
            float y = inRect.y;

            if (label != null)
            {
                Rect labelRect = new Rect(inRect.x, y, inRect.width, 22f);

                string substituted = label.TranslateIfTKeyAvailable(tKey)
                    .SubstituteVariable(key, currFloat.ToString());

                if (substituted.Contains("{defaultValue}"))
                {
                    substituted = substituted.SubstituteVariable("defaultValue", SettingsManager.GetDefaultValue(modId, key));
                }

                Widgets.Label(labelRect, substituted.SubstituteVariable("key", currFloat.ToString()));
                y += 22f;
            }

            Rect sliderRect = new Rect(inRect.x, y, inRect.width, SliderHeight());
            float newFloat = Widgets.HorizontalSlider(sliderRect, currFloat, min, max);

            SettingsManager.SetSetting(modId, key, Math.Round(newFloat, decimals).ToString());
        }
    }
}