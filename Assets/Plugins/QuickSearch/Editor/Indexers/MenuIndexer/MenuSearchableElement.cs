using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
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

		private static readonly Regex removeCtrlRegex_ = new Regex(@" ?(%[^% ]+)$");
		private static readonly Regex removeShiftRegex_ = new Regex(@" ?(#[^# ]+)$");
		private static readonly Regex removeAltRegex_ = new Regex(@" ?(&[^& ]+)$");
		private static readonly Regex removeUnderscoreIdx_ = new Regex(@" ?(_[^_ ]+)$");

		private static string SanitizeMenuPath (string menuPath) {
			var _path = menuPath;
			var hotkeyIdx = 5000;

			var ctrl = removeCtrlRegex_.Match(menuPath);
			if (ctrl.Success)
				hotkeyIdx = Mathf.Min(ctrl.Index, hotkeyIdx);

			var shift = removeShiftRegex_.Match(menuPath);
			if (shift.Success)
				hotkeyIdx = Mathf.Min(shift.Index, hotkeyIdx);

			var alt = removeAltRegex_.Match(menuPath);
			if (alt.Success)
				hotkeyIdx = Mathf.Min(alt.Index, hotkeyIdx);

			var underscore = removeUnderscoreIdx_.Match(menuPath);
			if (underscore.Success)
				hotkeyIdx = Mathf.Min(underscore.Index, hotkeyIdx);

			var hotkeyExists = (hotkeyIdx < 5000);
			if (hotkeyExists)
				_path = _path.Substring(0, hotkeyIdx);

			return _path;
		}

		string ISearchableElement.PrimaryContents {
			get {
				return menuPath_;
            }
		}

		string ISearchableElement.SecondaryContents {
			get {
				return contents_;
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

		bool ISearchableElement.SupportDrag {
			get {
				return false;
			}
		}

		UnityEngine.Object ISearchableElement.DragObject {
			get {
				return null;
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
