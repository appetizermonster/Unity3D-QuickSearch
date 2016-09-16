using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class AssetIndexerHook : AssetPostprocessor {

		private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (AssetIndexer.Active != null)
				AssetIndexer.Active.ReindexElements();
		}
	}

	public sealed class AssetIndexer : SearchIndexerBase {
		public static AssetIndexer Active { get; private set; }

		private static readonly Pool<AssetSearchableElement> elementPool_ = new Pool<AssetSearchableElement>(() => new AssetSearchableElement(), 500);
		private readonly List<ISearchableElement> elements_ = new List<ISearchableElement>(200);

		protected override void OnStartup () {
			ReindexElements();
			Active = this;
		}

		public void ReindexElements () {
			var thread = new Thread(CollectAssets);
			thread.Start(AssetDatabase.GetAllAssetPaths());
		}

		public void CollectAssets (object arg) {
			var assetPaths = (string[])arg;
			lock (elements_) {
				for (var i = 0; i < elements_.Count; ++i) {
					var element = elements_[i];
					elementPool_.Dealloc((AssetSearchableElement)element);
				}
				elements_.Clear();

				for (var i = 0; i < assetPaths.Length; ++i) {
					var assetPath = assetPaths[i];

					// Ignore non-project assets
					if (assetPath.StartsWith("Assets/") == false)
						continue;

					var assetElement = elementPool_.Alloc();
					assetElement.Setup(assetPath);
					elements_.Add(assetElement);
				}
			}
		}

		protected override List<ISearchableElement> GetElements () {
			lock (elements_) {
				return elements_;
			}
		}

		protected override bool IsActive () {
			return true;
		}
	}
}
