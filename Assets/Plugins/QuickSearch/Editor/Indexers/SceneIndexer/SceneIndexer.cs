using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class SceneIndexer : ISearchIndexer {
		private readonly List<ISearchableElement> elements_ = new List<ISearchableElement>(100);

		void ISearchIndexer.OnStartup () {
		}

		void ISearchIndexer.OnOpen () {
			elements_.Clear();

			var allGameObjects = GameObject.FindObjectsOfType<GameObject>();
			for (var i = 0; i < allGameObjects.Length; ++i) {
				var element = new SceneSearchableElement(allGameObjects[i]);
				elements_.Add(element);
			}
		}

		void ISearchIndexer.OnQuery (string query) {
		}

		List<ISearchableElement> ISearchIndexer.GetElements () {
			return elements_;
		}
	}
}
