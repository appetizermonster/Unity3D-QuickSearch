using UnityEngine;
using UnityEditor;
using System.Collections;

namespace QuickSearch
{

    public static class QuickSearchController
    {
        private static QuickSearchWindow window_ = null;

        [MenuItem("Window/QuickSearch %,")]
        private static void OpenQuickSearch()
        {
            window_ = EditorWindow.GetWindow<QuickSearchWindow>();
            window_.OnQueryChanged += OnQueryChanged;
            window_.OnSelect += OnSelect;

            window_.Show();
            window_.FocusOnQueryField();
        }

        private static void OnQueryChanged(string query)
        {
            var engine = QuickSearchEngine.Instance;
            var searchedElements = engine.FindElements(query);

            window_.List.Clear();
            window_.List.AddRange(searchedElements);
            window_.SetSelectedIndex(0);
        }

        private static void OnSelect(ISearchableElement element)
        {
            element.Select();
        }
    }
}