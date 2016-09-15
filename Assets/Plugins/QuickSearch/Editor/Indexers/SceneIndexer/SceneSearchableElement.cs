using System.Collections;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class SceneSearchableElement : ISearchableElement {
		private readonly GameObject gameObject_ = null;
		private readonly string name_ = null;
		private readonly string desc_ = null;

		public SceneSearchableElement (GameObject gameObject) {
			gameObject_ = gameObject;
			name_ = gameObject.name;
			desc_ = "Scene: " + name_ + (gameObject.activeInHierarchy ? " (active)" : " (inactive)");
		}

		string ISearchableElement.PrimaryContents {
			get {
				return "Scene: " + name_;
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

		void ISearchableElement.Select () {
			_Select();
		}

		void ISearchableElement.Execute () {
			_Select();
		}

		private void _Select () {
			Selection.activeGameObject = gameObject_;
			EditorGUIUtility.PingObject(gameObject_);
		}
	}
}
