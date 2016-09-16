using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading;

namespace QuickSearch {

	public sealed class AssetIndexer : AssetPostprocessor, ISearchIndexer {
		private static AssetIndexer activeIndexer_ = null;

		private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (activeIndexer_ != null)
				activeIndexer_.ReindexElements();
		}

		private readonly List<ISearchableElement> elements_ = new List<ISearchableElement>(200);

		void ISearchIndexer.OnStartup () {
			ReindexElements();
			activeIndexer_ = this;
		}

		void ISearchIndexer.OnOpen () {
		}

		void ISearchIndexer.OnQuery (string query) {
		}

		public void ReindexElements () {
			var thread = new Thread(CollectAssets);
			thread.Start(AssetDatabase.GetAllAssetPaths());
		}

		public void CollectAssets (object arg) {
			var assetPaths = (string[])arg;
			lock (elements_) {
				var watch = System.Diagnostics.Stopwatch.StartNew();
				elements_.Clear();

				for (var i = 0; i < assetPaths.Length; ++i) {
					var assetPath = assetPaths[i];

					// Ignore non-project assets
					if (assetPath.StartsWith("Assets/") == false)
						continue;

					var assetElement = new AssetSearchableElement(assetPath);
					elements_.Add(assetElement);
				}
				watch.Stop();
				Debug.LogFormat("indexing complete: {0}ms", watch.ElapsedMilliseconds);
			}
		}

		List<ISearchableElement> ISearchIndexer.GetElements () {
			lock (elements_) {
				return elements_;
			}
		}
	}
}
