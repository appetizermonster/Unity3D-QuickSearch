using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace QuickSearch
{
    public sealed class QuickSearchWindow : EditorWindow
    {
        private const int VISIBLE_RESULTS = 6;
        private static readonly Vector2 WINDOW_SIZE = new Vector2(700, (VISIBLE_RESULTS + 1) * 45);

        public event Action<string> OnQueryChanged = null;
        public event Action<ISearchableElement> OnSelect = null;

        private readonly List<ISearchableElement> list_ = new List<ISearchableElement>();
        public List<ISearchableElement> List { get { return list_; } }

        private int selectedIndex_ = 0;
        public int SelectedIndex { get { return selectedIndex_; } }

        private string query_ = "";
        private string oldQuery_ = null;
        private bool focusQueryField_ = false;

        private QuickSearchGUI gui_ = null;

        private void OnEnable()
        {
            gui_ = ScriptableObject.CreateInstance<QuickSearchGUI>();

            minSize = WINDOW_SIZE;
            maxSize = WINDOW_SIZE;
        }

        private void OnGUI()
        {
            ProcessKeyboardEvents();
            DrawGUI();
            ProcessEvents();
        }

        private void DrawGUI()
        {
            gui_.StartGUI();

            GUI.SetNextControlName("queryField");
            query_ = gui_.DrawQueryField(query_);

            if (oldQuery_ != query_)
            {
                if (OnQueryChanged != null)
                    OnQueryChanged(query_);
                oldQuery_ = query_;
            }

            if (List.Count > 0) {
                var begin = 0;
                var end = List.Count;

                if (selectedIndex_ >= VISIBLE_RESULTS)
                {
                    begin = (int)(selectedIndex_ / VISIBLE_RESULTS) * VISIBLE_RESULTS;
                    end = begin + VISIBLE_RESULTS;
                }

                end = Mathf.Min(List.Count, end);

                for (var i = begin; i < end; ++i)
                {
                    var element = list_[i];
                    var isSelected = (selectedIndex_ == i);

                    gui_.DrawSearchElement(element, isSelected);
                }
            } else
            {
                // no results
            }
        }

        private void ProcessKeyboardEvents()
        {
            var evt = Event.current;
            var keyCode = evt.keyCode;
            if (evt.type != EventType.KeyDown)
                return;

            if (keyCode == KeyCode.DownArrow)
            {
                SetSelectedIndex(SelectedIndex + 1);
                evt.Use();
            }
            else if (keyCode == KeyCode.UpArrow)
            {
                SetSelectedIndex(SelectedIndex - 1);
                evt.Use();
            }
            else if (keyCode == KeyCode.Escape)
            {
                Escape();
                evt.Use();
            }
            else if (keyCode == KeyCode.Return)
            {
                CallSelect();
                evt.Use();
            }
        }

        private void ProcessEvents()
        {
            if (focusQueryField_)
            {
                EditorGUI.FocusTextInControl("queryField");
                focusQueryField_ = false;
            }
        }

        public void Escape()
        {
            if (query_.Length > 0)
            {
                query_ = "";
                return;
            }
            Close();
        }

        private void CallSelect()
        {
            if (selectedIndex_ >= List.Count)
                return;
            if (List.Count <= 0)
                return;

            var selectedElement = List[selectedIndex_];
            if (OnSelect != null)
                OnSelect(selectedElement);
            Close();
        }

        public void SetSelectedIndex(int idx)
        {
            selectedIndex_ = Mathf.Clamp(idx, 0, List.Count - 1);
        }

        public void FocusOnQueryField()
        {
            focusQueryField_ = true;
        }
    }
}