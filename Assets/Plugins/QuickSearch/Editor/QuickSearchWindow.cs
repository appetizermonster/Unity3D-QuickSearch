using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class QuickSearchWindow : EditorWindow {
		public static QuickSearchWindow Active { get; private set; }

		private const int VISIBLE_RESULTS = 7;
		public static readonly Vector2 WINDOW_SIZE = new Vector2(700, (VISIBLE_RESULTS + 1) * QuickSearchGUI.ELEM_HEIGHT + QuickSearchGUI.HEAD_HEIGHT);

		public event Action<string> OnQueryChanged = null;

		public event Action<ISearchableElement> OnSelect = null;

		public event Action<ISearchableElement> OnExecute = null;

		private readonly List<ISearchableElement> list_ = new List<ISearchableElement>();
		public List<ISearchableElement> List { get { return list_; } }

		private int selectedIndex_ = 0;
		public int SelectedIndex { get { return selectedIndex_; } }

		private QuickSearchGUI gui_ = null;

		private bool tryFocusQueryField_ = false;
		private bool trySelectAllQueryField_ = false;
		private bool dontRestoreSelection_ = false;

		private static string query_ = "";
		private string oldQuery_ = null;
		private UnityEngine.Object[] oldSelections_ = null;

		private void OnEnable () {
			gui_ = ScriptableObject.CreateInstance<QuickSearchGUI>();
			gui_.hideFlags = HideFlags.HideAndDontSave;

			minSize = WINDOW_SIZE;
			maxSize = WINDOW_SIZE;

			oldSelections_ = Selection.objects;

			Active = this;
		}

		private void OnDisable () {
			if (Active == this)
				Active = null;

			if (!dontRestoreSelection_)
				Selection.objects = oldSelections_;

			ScriptableObject.DestroyImmediate(gui_);
		}

		private void OnGUI () {
			ProcessKeyboardEvents();
			DrawGUI();
			ProcessTryFocusQueryField();
		}

		private void DrawGUI () {
			gui_.StartGUI(new Rect(0, 0, WINDOW_SIZE.x, WINDOW_SIZE.y));
			gui_.DrawHead();

			GUI.SetNextControlName("queryField");
			query_ = gui_.DrawQueryField(query_);

			ProcessTrySelectAll();

			if (oldQuery_ != query_) {
				if (OnQueryChanged != null)
					OnQueryChanged(query_);
				oldQuery_ = query_;
			}

			if (List.Count > 0) {
				var begin = 0;
				var end = List.Count;

				if (selectedIndex_ >= VISIBLE_RESULTS) {
					begin = (int)(selectedIndex_ / VISIBLE_RESULTS) * VISIBLE_RESULTS;
					end = begin + VISIBLE_RESULTS;
				}

				end = Mathf.Min(List.Count, end);

				for (var i = begin; i < end; ++i) {
					var element = list_[i];
					var isSelected = (selectedIndex_ == i);

					gui_.DrawSearchElement(element, isSelected);
				}
			} else {
				// no results
				gui_.DrawEmpty();
			}
		}

		private void ProcessKeyboardEvents () {
			var evt = Event.current;
			var keyCode = evt.keyCode;

			// Ctrl + A Hook
			if (evt.control && keyCode == KeyCode.A) {
				trySelectAllQueryField_ = true;
				evt.Use();
				return;
			}

			// KeyDown
			if (evt.type != EventType.KeyDown)
				return;

			if (keyCode == KeyCode.DownArrow) {
				SetSelectedIndex(SelectedIndex + 1);
				evt.Use();
			} else if (keyCode == KeyCode.UpArrow) {
				SetSelectedIndex(SelectedIndex - 1);
				evt.Use();
			} else if (keyCode == KeyCode.Tab) {
				EmitSelect();
				evt.Use();
			} else if (keyCode == KeyCode.Return) {
				EmitExecute();
				evt.Use();
			} else if (keyCode == KeyCode.Escape) {
				Escape();
				evt.Use();
			}
		}

		private void ProcessTryFocusQueryField () {
			if (!tryFocusQueryField_)
				return;

			EditorGUI.FocusTextInControl("queryField");
			tryFocusQueryField_ = false;
		}

		private void ProcessTrySelectAll () {
			if (!trySelectAllQueryField_)
				return;

			var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
			if (textEditor != null) {
				textEditor.SelectAll();
			}
			trySelectAllQueryField_ = false;
		}

		public void Escape () {
			if (query_.Length > 0) {
				query_ = "";
				return;
			}

			Close();
		}

		public void CloseWithoutRestoreSelection () {
			dontRestoreSelection_ = true;
			Close();
		}

		private void EmitSelect () {
			if (selectedIndex_ >= List.Count)
				return;
			if (List.Count <= 0)
				return;

			var selectedElement = List[selectedIndex_];
			if (OnSelect != null)
				OnSelect(selectedElement);
		}

		private void EmitExecute () {
			if (selectedIndex_ >= List.Count)
				return;
			if (List.Count <= 0)
				return;

			var selectedElement = List[selectedIndex_];
			if (OnExecute != null)
				OnExecute(selectedElement);
		}

		public void SetSelectedIndex (int idx) {
			selectedIndex_ = Mathf.Clamp(idx, 0, List.Count - 1);
		}

		public void FocusOnQueryField () {
			tryFocusQueryField_ = true;
		}
	}
}
