using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {
	// Referenced from http://answers.unity3d.com/questions/960413/editor-window-how-to-center-a-window.html
	public static class EditorWindowUtility {
		public static System.Type[] GetAllDerivedTypes (AppDomain appDomain, Type type) {
			var result = new List<Type>();
			var assemblies = appDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				var types = assembly.GetTypes();
				foreach (var t in types) {
					if (t.IsSubclassOf(type))
						result.Add(t);
				}
			}
			return result.ToArray();
		}

		public static Rect GetEditorMainWindowPos () {
			var containerWinType = GetAllDerivedTypes(AppDomain.CurrentDomain, typeof(ScriptableObject)).Where(t => t.Name == "ContainerWindow").FirstOrDefault();
			if (containerWinType == null)
				throw new System.MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
			var showModeField = containerWinType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var positionProperty = containerWinType.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			if (showModeField == null || positionProperty == null)
				throw new System.MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
			var windows = Resources.FindObjectsOfTypeAll(containerWinType);
			foreach (var win in windows) {
				var showmode = (int)showModeField.GetValue(win);
				if (showmode == 4) {
					var pos = (Rect)positionProperty.GetValue(win, null);
					return pos;
				}
			}
			throw new System.NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
		}

		public static Rect GetCenterPosition (Vector2 windowSize) {
			var main = GetEditorMainWindowPos();
			float w = (main.width - windowSize.x) * 0.5f;
			float h = (main.height - windowSize.y) * 0.5f;
			return new Rect(main.x + w, main.y + h, 0, 0);
		}
	}
}
