using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class MenuSearchableElement : ISearchableElement {
		private readonly string menuPath_ = null;
		private readonly string contents_ = null;
		private readonly string title_ = null;
		private readonly bool markExperimental_ = false;

		private readonly Action action_ = null;

		public MenuSearchableElement (string menuPath, Action action = null, bool markExperimental = false) {
			menuPath_ = menuPath;
			action_ = action;
			markExperimental_ = markExperimental;

			// Remove hot keys
			var sanitizedPath = SanitizeMenuPath(menuPath);
			contents_ = "Menu: " + sanitizedPath;

			// Parse title
			var titleSepIndex = sanitizedPath.LastIndexOf('/');
			if (titleSepIndex >= 0)
				title_ = sanitizedPath.Substring(titleSepIndex + 1);
			else
				title_ = sanitizedPath;
		}

		private static string SanitizeMenuPath (string menuPath) {
			var _path = menuPath;
			var ctrlIdx = _path.LastIndexOf("%");
			var shiftIdx = _path.LastIndexOf("#");
			var altIdx = _path.LastIndexOf("&");
			var underscoreIdx = _path.LastIndexOf("_");

			if (ctrlIdx > 0)
				_path = _path.Substring(0, ctrlIdx);
			else if (shiftIdx > 0)
				_path = _path.Substring(0, shiftIdx);
			else if (altIdx > 0)
				_path = _path.Substring(0, altIdx);
			else if (underscoreIdx > 0)
				_path = _path.Substring(0, underscoreIdx);

			return _path;
		}

		string ISearchableElement.PrimaryContents {
			get {
				return contents_;
			}
		}

		string ISearchableElement.SecondaryContents {
			get {
				return null;
			}
		}

		float ISearchableElement.Priority {
			get {
				return 1f;
			}
		}

		Texture2D ISearchableElement.Icon {
			get {
				return null;
			}
		}

		string ISearchableElement.Title {
			get {
				return title_;
			}
		}

		string ISearchableElement.Description {
			get {
				var desc = contents_;
				if (markExperimental_)
					desc += " (experimental)";
				return desc;
			}
		}

		void ISearchableElement.Select () {
		}

		void ISearchableElement.Execute () {
			if (action_ != null) {
				EditorApplication.delayCall += () => action_();
				return;
			}

			EditorApplication.ExecuteMenuItem(menuPath_);
		}
	}
}
