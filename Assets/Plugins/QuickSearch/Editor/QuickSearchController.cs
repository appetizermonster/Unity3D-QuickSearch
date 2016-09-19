using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace QuickSearch {

	public static class QuickSearchController {
		private static QuickSearchWindow window_ = null;

		[MenuItem("Window/Toggle QuickSearch %,")]
		private static void ToggleQuickSearch () {
			if (QuickSearchWindow.Active != null) {
				QuickSearchWindow.Active.Close();
				return;
			}

			window_ = EditorWindow.CreateInstance<QuickSearchWindow>();

			window_.OnClose += OnClose;
			window_.OnQueryChanged += OnQueryChanged;
			window_.OnSelect += OnSelect;
			window_.OnExecute += OnExecute;

			var windowSize = QuickSearchWindow.WINDOW_SIZE;
			var windowRect = EditorWindowUtility.GetCenterRect(windowSize);

			window_.position = windowRect;
			window_.RefreshBlurBackground();
			window_.ShowPopup();

			window_.Focus();
			window_.FocusOnQueryField();

			QuickSearchEngine.Instance.NotifyOpen();
			QuickSearchEngine.Instance.OnResultUpdate += Worker_OnResultUpdate;
		}

		private static void OnClose () {
			QuickSearchEngine.Instance.OnResultUpdate -= Worker_OnResultUpdate;
		}

		private static void OnQueryChanged (string query) {
			QuickSearchEngine.Instance.RequestFindElements(query);
		}

		private static List<ISearchableElement> fetchedResult_ = new List<ISearchableElement>();
		private static void Worker_OnResultUpdate () {
			if (window_ == null)
				return;

			QuickSearchEngine.Instance.GetLastResult(fetchedResult_);

			window_.List.Clear();
			window_.List.AddRange(fetchedResult_);
			window_.SetSelectedIndex(0);
			window_.Repaint();
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
