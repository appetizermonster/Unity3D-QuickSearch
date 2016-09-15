using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class AssetSearchableElement : ISearchableElement {
		private readonly float priority_ = 1f;

		private readonly string assetPath_ = null;
		private readonly string assetFilename_ = null;
		private readonly string assetExt_ = null;

		private readonly Texture2D resIcon_ = null;

		public AssetSearchableElement (string assetPath) {
			assetPath_ = assetPath;
			assetFilename_ = Path.GetFileName(assetPath);
			assetExt_ = Path.GetExtension(assetPath).ToLowerInvariant();

			if (assetExt_.Length > 0)
				assetExt_ = assetExt_.Substring(1);

			if (Directory.Exists(assetPath_))
				assetExt_ = "folder";

			if (assetExt_ == "png" || assetExt_ == "psd" || assetExt_ == "jpg")
				resIcon_ = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath_);
		}

		string ISearchableElement.PrimaryContents {
			get {
				return assetFilename_;
			}
		}

		string ISearchableElement.SecondaryContents {
			get {
				return assetPath_;
			}
		}

		float ISearchableElement.Priority {
			get {
				return priority_;
			}
		}

		Texture2D ISearchableElement.Icon {
			get {
				if (resIcon_ != null)
					return resIcon_;
				return AssetIcons.GetIcon(assetExt_);
			}
		}

		string ISearchableElement.Title {
			get {
				return Path.GetFileNameWithoutExtension(assetFilename_);
			}
		}

		string ISearchableElement.Description {
			get {
				return assetPath_;
			}
		}

		void ISearchableElement.Select () {
			_Select();
		}

		void ISearchableElement.Execute () {
			_Select();
		}

		private void _Select () {
			var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath_);
			Selection.activeObject = obj;
			EditorGUIUtility.PingObject(obj);
		}
	}
}
