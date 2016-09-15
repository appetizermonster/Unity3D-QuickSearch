using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	internal sealed class QuickSearchGUI : ScriptableObject {
		public const float WIDTH = 700f;
		public const float HEAD_HEIGHT = 20f;
		public const float ELEM_HEIGHT = 45F;

		[SerializeField]
		private GUISkin guiSkin;

		[SerializeField]
		private Texture2D bg;

		[SerializeField]
		private Texture2D empty;

		[SerializeField]
		private Texture2D searchIcon;

		private Rect lastRect_;
		private Rect size_;

		public void StartGUI (Rect size) {
			lastRect_ = new Rect();
			size_ = size;

			GUI.skin = guiSkin;
			GUI.DrawTexture(size, bg);
		}

		public void DrawHead () {
			var titleRect = new Rect(0f, 0f, WIDTH, HEAD_HEIGHT);
			var titleStyle = guiSkin.GetStyle("head_title");

			GUI.Label(titleRect, "QuickSearch", titleStyle);

			var rightStyle = guiSkin.GetStyle("head_right");
			GUI.Label(titleRect, "<color=#333>tab</color> to peek, <color=#333>enter</color> to select", rightStyle);

			lastRect_ = titleRect;
		}

		public void DrawEmpty () {
			var emptyWidth = empty.width;
			var emptyHeight = empty.height;

			var center = new Rect((size_.width - emptyWidth) * 0.5f, 165f, emptyWidth, emptyHeight);

			GUI.DrawTexture(center, empty);
		}

		public string DrawQueryField (string query) {
			var rect = new Rect(0f, lastRect_.yMax, WIDTH, ELEM_HEIGHT);
			lastRect_ = rect;

			var newQuery = GUI.TextField(rect, query);
			var iconRect = new Rect(rect.x + 10f, rect.yMin + 11f, 23f, 23f);
			GUI.DrawTexture(iconRect, searchIcon);

			return newQuery;
		}

		public void DrawSearchElement (ISearchableElement element, bool selected,
										Action<ISearchableElement> onMouseDown,
										Action<ISearchableElement> onDrag) {
			var bgRect = new Rect(0f, lastRect_.yMax, WIDTH, ELEM_HEIGHT);
			var bgStyle = guiSkin.GetStyle(selected ? "elem_bg_selected" : "elem_bg_normal");

			GUI.Box(bgRect, new GUIContent(), bgStyle);
			lastRect_ = bgRect;

			if (element.Icon != null) {
				var iconRect = new Rect(bgRect.x + 5f, bgRect.y + 9f, 26f, 26f);
				GUI.DrawTexture(iconRect, element.Icon, ScaleMode.ScaleToFit);
			}

			var titleRect = new Rect(bgRect.x + 35f, bgRect.y + 2f, WIDTH, 25f);
			var titleStyle = guiSkin.GetStyle(selected ? "title_selected" : "title_normal");
			GUI.Label(titleRect, element.Title, titleStyle);

			var descRect = new Rect(bgRect.x + 35f, bgRect.y + 23f, WIDTH, 25f);
			var descStyle = guiSkin.GetStyle("desc_normal");
			GUI.Label(descRect, element.Description, descStyle);

			var evt = Event.current;
			if (bgRect.Contains(Event.current.mousePosition)) {
				if (evt.type == EventType.MouseDown && onMouseDown != null) {
					onMouseDown(element);
					evt.Use();
				} else if (evt.type == EventType.MouseDrag && onDrag != null) {
					onDrag(element);
					evt.Use();
				}
			}
		}
	}
}
