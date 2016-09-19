using System.Collections;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class SceneSearchableElement : ISearchableElement {
		private readonly GameObject gameObject_ = null;
		private readonly string name_ = null;
		private readonly string secondary_ = null;
		private readonly string desc_ = null;

		public SceneSearchableElement (GameObject gameObject) {
			gameObject_ = gameObject;
			name_ = gameObject.name;

			var path = GetFullPath(gameObject.transform);
			desc_ = "Scene: " + path + (gameObject.activeInHierarchy ? " (active)" : " (inactive)");
			secondary_ = "Scene:" + path;
		}

		private static string GetFullPath (Transform tr) {
			var sb = new StringBuilder();
			var parent = tr.parent;

			while (parent != null) {
				sb.Insert(0, '/');
				sb.Insert(0, parent.name);
				parent = parent.parent;
			}
			sb.Append(tr.name);
			return sb.ToString();
		}

		string ISearchableElement.PrimaryContents {
			get {
				return name_;
			}
		}

		string ISearchableElement.SecondaryContents {
			get {
				return secondary_;
			}
		}

		float ISearchableElement.Priority {
			get {
				return 1f;
			}
		}

		Texture2D ISearchableElement.Icon {
			get {
				return EditorGUIUtility.FindTexture("GameObject Icon");
			}
		}

		string ISearchableElement.Title {
			get {
				return name_;
			}
		}

		string ISearchableElement.Description {
			get {
				return desc_;
			}
		}

		bool ISearchableElement.SupportDrag {
			get {
				return true;
			}
		}

		UnityEngine.Object ISearchableElement.DragObject {
			get {
				return gameObject_;
			}
		}

		void ISearchableElement.Select () {
			_Select(false);
		}

		void ISearchableElement.Execute () {
			_Select(true);
		}

		private void _Select (bool focus) {
			Selection.activeGameObject = gameObject_;
			EditorGUIUtility.PingObject(gameObject_);

			if (focus) {
				// Set Focus on Hierarchy Window
				EditorApplication.ExecuteMenuItem("Window/Hierarchy");
			}
		}
	}
}
