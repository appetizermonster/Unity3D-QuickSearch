using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class SceneIndexer : SearchIndexerBase {
		private readonly List<ISearchableElement> elements_ = new List<ISearchableElement>(100);

		protected override void OnOpen () {
			elements_.Clear();

			var allGameObjects = GameObject.FindObjectsOfType<GameObject>();
			for (var i = 0; i < allGameObjects.Length; ++i) {
				var element = new SceneSearchableElement(allGameObjects[i]);
				elements_.Add(element);
			}
		}

		protected override List<ISearchableElement> GetElements () {
			return elements_;
		}
	}
}
