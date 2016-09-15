using System.Collections;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public static class QuickSearchController {
		private static QuickSearchWindow window_ = null;

		[MenuItem("Window/QuickSearch %,")]
		private static void OpenQuickSearch () {
			window_ = EditorWindow.CreateInstance<QuickSearchWindow>();
			window_.OnQueryChanged += OnQueryChanged;
			window_.OnSelect += OnSelect;
			window_.OnExecute += OnExecute;

			var centerPos = EditorWindowUtility.GetCenterPosition(QuickSearchWindow.WINDOW_SIZE);
			window_.position = centerPos;
			window_.ShowAsDropDown(centerPos, QuickSearchWindow.WINDOW_SIZE);
			window_.FocusOnQueryField();

			QuickSearchEngine.Instance.EmitOpen();
		}

		private static void OnQueryChanged (string query) {
			var engine = QuickSearchEngine.Instance;
			var searchedElements = engine.FindElements(query);

			window_.List.Clear();
			window_.List.AddRange(searchedElements);
			window_.SetSelectedIndex(0);
		}

		private static void OnSelect (ISearchableElement element) {
			element.Select();

			EditorApplication.delayCall += () => {
				if (window_ != null)
					window_.Focus();
			};
		}

		private static void OnExecute (ISearchableElement element) {
			if (window_ != null)
				window_.CloseWithoutRestoreSelection();

			element.Execute();
		}
	}
}
