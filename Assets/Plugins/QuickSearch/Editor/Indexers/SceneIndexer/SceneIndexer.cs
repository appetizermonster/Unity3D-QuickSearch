using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class SceneIndexer : SearchIndexerBase {
		private readonly List<ISearchableElement> elements_ = new List<ISearchableElement>(100);

		private readonly List<Transform> tempTransforms_ = new List<Transform>(100);

		protected override void OnOpen () {
			elements_.Clear();

			var rootObjs = GetSceneRoots();
			for (var i = 0; i < rootObjs.Count; ++i) {
				var obj = rootObjs[i];
				obj.GetComponentsInChildren<Transform>(true, tempTransforms_);

				for (var j = 0; j < tempTransforms_.Count; ++j) {
					var tr = tempTransforms_[j];
					if (tr == null)
						continue;

					var element = new SceneSearchableElement(tr.gameObject);
					elements_.Add(element);
				}
			}
		}

		private static List<GameObject> GetSceneRoots () {
			var prop = new HierarchyProperty(HierarchyType.GameObjects);
			var expanded = new int[0];
			var roots = new List<GameObject>();

			while (prop.Next(expanded)) {
				var go = prop.pptrValue as GameObject;
				if (go == null)
					continue;
				roots.Add(go);
			}

			return roots;
		}

		protected override List<ISearchableElement> GetElements () {
			return elements_;
		}
	}
}
