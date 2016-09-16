using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class QuickSearchWindow : EditorWindow {
		public static QuickSearchWindow Active { get; private set; }

		private const int VISIBLE_RESULTS = 7;
		private const float WINDOW_MOVE_DELTA = 20f;

		public static readonly Vector2 WINDOW_SIZE = new Vector2(700, (VISIBLE_RESULTS + 1) * QuickSearchGUI.ELEM_HEIGHT + QuickSearchGUI.HEAD_HEIGHT);

		public event Action<string> OnQueryChanged = null;

		public event Action<ISearchableElement> OnSelect = null;

		public event Action<ISearchableElement> OnExecute = null;

		private readonly List<ISearchableElement> list_ = new List<ISearchableElement>();
		public List<ISearchableElement> List { get { return list_; } }

		private int selectedIndex_ = 0;
		public int SelectedIndex { get { return selectedIndex_; } }

		private QuickSearchGUI gui_ = null;
		private Texture backgroundTexture_ = null;

		private bool tryFocusQueryField_ = false;
		private bool trySelectAllQueryField_ = false;
		private bool dontRestoreSelection_ = false;

		private static string query_ = "";
		private string oldQuery_ = null;
		private bool isDragging_ = false;

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

			DestroyBackgroundTexture();

			if (!dontRestoreSelection_)
				Selection.objects = oldSelections_;

			ScriptableObject.DestroyImmediate(gui_);
		}

		public void RefreshBlurBackground () {
			DestroyBackgroundTexture();

			var windowRect = this.position;
			var position = windowRect.position;
			var width = (int)windowRect.width;
			var height = (int)windowRect.height;

			if (width <= 0 || height <= 0)
				return;

			var pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(position, width, height);
			var baseTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
			baseTex.hideFlags = HideFlags.DontSave;
			baseTex.SetPixels(pixels);
			baseTex.Apply();

			var blurOptions = new BlurOptions();
			var blurTexture = BlurUtility.BlurTexture(baseTex, blurOptions);

			Texture2D.DestroyImmediate(baseTex);
			backgroundTexture_ = blurTexture;
		}

		private void DestroyBackgroundTexture () {
			if (backgroundTexture_ != null)
				Texture.DestroyImmediate(backgroundTexture_);
			backgroundTexture_ = null;
		}

		private void Update () {
			WatchDragEnd();

			if (EditorWindow.focusedWindow != this && !isDragging_)
				Close();
		}

		private void OnGUI () {
			wantsMouseMove = true;

			ProcessKeyboardEvents();
			ProcessWheelEvents();
			DrawGUI();
			ProcessTryFocusQueryField();

			if (Event.current.type == EventType.MouseMove)
				Repaint();
		}

		private void DrawGUI () {
			gui_.StartGUI(new Rect(0, 0, WINDOW_SIZE.x, WINDOW_SIZE.y), backgroundTexture_);
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

					gui_.DrawSearchElement(element, isSelected, OnElementClick, OnElementDrag);
				}
			} else {
				// no results
				gui_.DrawEmpty();
			}
		}

		private void OnElementClick (ISearchableElement elem) {
			var idx = List.IndexOf(elem);
			if (idx < 0)
				return;

			SetSelectedIndex(idx);
			Repaint();
		}

		private void OnElementDrag (ISearchableElement elem) {
			if (!elem.SupportDrag || elem.DragObject == null)
				return;

			DragAndDrop.PrepareStartDrag();
			DragAndDrop.objectReferences = new[] { elem.DragObject };
			DragAndDrop.StartDrag("Dragging Object");

			isDragging_ = true;
		}

		private void WatchDragEnd () {
			if (!isDragging_)
				return;
			if (DragAndDrop.objectReferences.Length > 0)
				return;

			// End of drag
			isDragging_ = false;
			Focus();
		}

		private void ProcessWheelEvents () {
			var evt = Event.current;
			if (evt.type != EventType.ScrollWheel)
				return;

			var delta = evt.delta;
			if (delta.y > 0)
				SetSelectedIndex(SelectedIndex + 1);
			else
				SetSelectedIndex(SelectedIndex - 1);
			evt.Use();
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

			var consumeEvent = true;
			if (evt.alt && keyCode == KeyCode.LeftArrow)
				MoveWindow(new Vector2(-WINDOW_MOVE_DELTA, 0));
			else if (evt.alt && keyCode == KeyCode.RightArrow)
				MoveWindow(new Vector2(WINDOW_MOVE_DELTA, 0));
			else if (evt.alt && keyCode == KeyCode.UpArrow)
				MoveWindow(new Vector2(0, -WINDOW_MOVE_DELTA));
			else if (evt.alt && keyCode == KeyCode.DownArrow)
				MoveWindow(new Vector2(0, WINDOW_MOVE_DELTA));
			else if (keyCode == KeyCode.DownArrow)
				SetSelectedIndex(SelectedIndex + 1);
			else if (keyCode == KeyCode.UpArrow)
				SetSelectedIndex(SelectedIndex - 1);
			else if (keyCode == KeyCode.Tab)
				EmitSelect();
			else if (keyCode == KeyCode.Return)
				EmitExecute();
			else if (keyCode == KeyCode.Escape)
				Escape();
			else
				consumeEvent = false;

			if (consumeEvent)
				evt.Use();
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

		public void MoveWindow (Vector2 delta) {
			var rect = position;
			var pos = rect.position;
			pos += delta;

			rect.position = pos;
			position = rect;

			DestroyBackgroundTexture();
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
