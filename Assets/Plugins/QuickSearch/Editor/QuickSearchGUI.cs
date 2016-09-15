using UnityEngine;
using UnityEditor;
using System.Collections;

namespace QuickSearch
{

    internal sealed class QuickSearchGUI : ScriptableObject
    {
        [SerializeField]
        private GUISkin guiSkin;

        public int elemHeight = 45;
        public int width = 700;

        private Rect lastRect_ = new Rect();

        public void StartGUI()
        {
            lastRect_ = new Rect();

            GUI.skin = guiSkin;
        }

        public string DrawQueryField(string query)
        {
            var rect = new Rect(0f, lastRect_.yMax, width, elemHeight);
            lastRect_ = rect;

            return GUI.TextField(rect, query);
        }

        // Colors
        private readonly Color elemBgNormal = new Color(1f, 1f, 1f, 1f);
        private readonly Color elemBgSelected = new Color(0.5f, 0.5f, 1f, 1f);

        public void DrawSearchElement(ISearchableElement element, bool selected)
        {
            var rect = new Rect(0f, lastRect_.yMax, width, elemHeight);
            lastRect_ = rect;

            GUI.Label(rect, element.Title);
        }
    }
}