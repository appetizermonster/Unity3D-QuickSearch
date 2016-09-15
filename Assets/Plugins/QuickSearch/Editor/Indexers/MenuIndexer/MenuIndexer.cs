using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class MenuIndexer : ISearchIndexer {
		private readonly List<ISearchableElement> elements_ = new List<ISearchableElement>(100);

		void ISearchIndexer.OnStartup () {
			IndexMenus();
		}

		void ISearchIndexer.OnOpen () {
		}

		void ISearchIndexer.OnQuery (string query) {
		}

		private void IndexMenus () {
			elements_.Clear();

			IndexMenuItems();
			IndexHiddenMenus();
		}

		private void IndexMenuItems () {
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (var i = 0; i < assemblies.Length; ++i) {
				var assembly = assemblies[i];
				var assemblyName = assembly.FullName;

				// Skip system assembly
				if (assemblyName.StartsWith("System.") || assemblyName.StartsWith("Mono.") || assemblyName.StartsWith("mscorlib"))
					continue;
				if (assemblyName.StartsWith("nunit.") || assemblyName.StartsWith("I18N"))
					continue;

				var types = assembly.GetTypes();

				foreach (var type in types) {
					var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

					foreach (var method in methods) {
						var attribs = method.GetCustomAttributes(typeof(MenuItem), false);
						if (attribs.Length <= 0)
							continue;

						foreach (var attrib in attribs) {
							var menuItem = (MenuItem)attrib;
							if (menuItem.validate)
								continue;

							var menuPath = menuItem.menuItem;
							if (menuPath.StartsWith("internal:") || menuPath.StartsWith("CONTEXT/"))
								continue;

							var element = new MenuSearchableElement(menuPath);
							elements_.Add(element);
						}
					}
				}
			}
		}

		private void IndexHiddenMenus () {
#if UNITY_EDITOR_WIN
			// File
			IndexHiddenMenuForWindows("File/New Scene", 0xE100);
			IndexHiddenMenuForWindows("File/Open Scene", 0xE101);
			IndexHiddenMenuForWindows("File/Save Scene", 0xE103);
			IndexHiddenMenuForWindows("File/Save Scene as...", 0xE104);

			IndexHiddenMenuForWindows("File/New Project", 0x9C4A);
			IndexHiddenMenuForWindows("File/Open Project", 0x9C66);
			IndexHiddenMenuForWindows("File/Save Project", 0x9C69);

			IndexHiddenMenuForWindows("File/Build Settings...", 0x9C73);
			IndexHiddenMenuForWindows("File/Build & Run", 0x9C74);

			// Edit
			IndexHiddenMenuForWindows("Edit/Preferences...", 0x9C72);
			IndexHiddenMenuForWindows("Edit/Modules...", 0x9C77);

			// Assets
			IndexHiddenMenuForWindows("Assets/Refresh", 0x3CB9);
#endif
		}

#if UNITY_EDITOR_WIN
		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow ();

		[DllImport("User32.dll")]
		private static extern int SendMessage (IntPtr hWnd, int msg, int wParam, int lParam);

		private const int WM_COMMAND = 0x0111;

		private void IndexHiddenMenuForWindows (string menuPath, int command) {
			elements_.Add(new MenuSearchableElement(menuPath, () => SendCommandToEditorForWindows(command), true));
		}

		private void SendCommandToEditorForWindows (int command) {
			var activeWindow = GetActiveWindow();
			SendMessage(activeWindow, WM_COMMAND, command, 0);
		}
#endif

		List<ISearchableElement> ISearchIndexer.GetElements () {
			return elements_;
		}
	}
}
