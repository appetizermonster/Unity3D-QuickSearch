using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
			IndexHiddenMenu("File/New Scene", () => EditorApplication.NewScene());
			IndexHiddenMenu("Edit/Preferences...", () => {
				var assembly = Assembly.GetAssembly(typeof(EditorWindow));
				var prefWindowType = assembly.GetType("UnityEditor.PreferencesWindow");
				var method = prefWindowType.GetMethod("ShowPreferencesWindow", BindingFlags.NonPublic | BindingFlags.Static);
				method.Invoke(null, null);
			});
		}

		private void IndexHiddenMenu (string menuPath, Action action = null) {
			elements_.Add(new MenuSearchableElement(menuPath, action));
		}

		List<ISearchableElement> ISearchIndexer.GetElements () {
			return elements_;
		}
	}
}
