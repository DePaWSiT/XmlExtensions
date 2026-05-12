using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace XmlExtensions.Setting
{
    internal class TabView : SettingContainer
    {
        public class Tab
        {
            public string label;
            public string tKey;
            public List<SettingContainer> settings = new List<SettingContainer>();
        }

        public List<Tab> tabs;
        public int rows = 1;
        public float? maxTabWidth = null; // Obsolete
        public int defaultTab = 1;

        private List<TabRecord> tabRecords;
        private int selectedTab = 0;
        private float tabHeight = 31; // Game-defined constant

        protected override bool Init()
        {
            if (defaultTab <= 0)
            {
                Error("<defaultTab> must be at least 1");
                return false;
            }
            if (defaultTab > tabs.Count)
            {
                Error("<defaultTab> must be at most " + tabs.Count.ToString());
                return false;
            }
            searchType = SearchType.SearchAllAndHighlight;
            rows = Mathf.Min(rows, tabs.Count);
            selectedTab = defaultTab - 1;
            addDefaultSpacing = false;
            if (tabs != null)
            {
                foreach (Tab tab in tabs)
                {
                    if (!InitializeContainers(tab.settings, tab.label))
                    {
                        return false;
                    }
                }
            }

            tabRecords = [];
            for (int i = 0; i < tabs.Count; i++)
            {
                int t = i;
                TabRecord temp = new(tabs[t].label, delegate () { selectedTab = t; }, () => selectedTab == t);
                tabRecords.Add(temp);
            }
            return true;
        }

        protected override float CalculateHeight(float width)
        {
            return CalculateHeightSettingsList(width, tabs[selectedTab].settings) + rows*((int)tabHeight);
        }

        protected override void DrawSettingContents(Rect inRect)
        {
            inRect.yMin += rows*tabHeight;
            TabDrawer.DrawTabs(inRect, tabRecords, rows, null);
            DrawSettingsList(inRect, tabs[selectedTab].settings);
        }

        protected override void DrawFilterBox(Rect inRect)
        {
            const float maxTabWidth = 200f;
            const float tabHorizontalOverlap = 10f;

            Rect baseRect = inRect;
            baseRect.yMin += rows * tabHeight;

            int tabsDrawn = 0;

            for (int r = 0; r < rows; r++)
            {
                int tabsThisRow = r == 0
                    ? tabs.Count - (rows - 1) * Mathf.FloorToInt((float)tabs.Count / rows)
                    : Mathf.FloorToInt((float)tabs.Count / rows);

                float tabWidth = (baseRect.width + (tabsThisRow - 1) * tabHorizontalOverlap) / tabsThisRow;
                tabWidth = Mathf.Min(tabWidth, maxTabWidth);

                float rowY = baseRect.y - tabHeight;

                for (int c = 0; c < tabsThisRow; c++)
                {
                    int tabIndex = tabsDrawn + c;

                    if (containedFiltered[tabs[tabIndex].settings])
                    {
                        Rect tabRect = new Rect(
                            baseRect.x + c * (tabWidth - tabHorizontalOverlap),
                            rowY,
                            tabWidth,
                            tabHeight
                        );

                        FilterBox(tabRect);
                    }
                }

                baseRect.yMin += 31f;
                tabsDrawn += tabsThisRow;
            }
        }

        protected override bool PreOpen()
        {
            selectedTab = defaultTab - 1;
            return true;
        }
    }
}