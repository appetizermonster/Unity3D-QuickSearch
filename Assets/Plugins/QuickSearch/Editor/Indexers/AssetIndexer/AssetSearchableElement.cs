using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public sealed class AssetSearchableElement : ISearchableElement {
		private readonly float priority_ = 1f;

		private readonly bool isFolder_ = false;
		private readonly bool isTexture_ = false;

		private readonly string assetPath_ = null;
		private readonly string assetFilename_ = null;
		private readonly string assetExt_ = null;

		public AssetSearchableElement (string assetPath) {
			assetPath_ = assetPath;
			assetFilename_ = Path.GetFileName(assetPath);
			assetExt_ = Path.GetExtension(assetPath).ToLowerInvariant();

			if (assetExt_.Length > 0)
				assetExt_ = assetExt_.Substring(1);

			if (Directory.Exists(assetPath_)) {
				isFolder_ = true;
				assetExt_ = "folder";
			} else {
				isTexture_ = (
					assetExt_ == "png" || assetExt_ == "psd"
					|| assetExt_ == "jpg" || assetExt_ == "tga" || assetExt_ == "tif"
					|| assetExt_ == "bmp"
				);
			}
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
				if (isTexture_) {
					var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath_);
					if (texture != null)
						return texture;
				}
				return AssetIcons.GetIcon(assetExt_);
			}
		}

		string ISearchableElement.Title {
			get {
				if (isFolder_)
					return assetFilename_;
				return Path.GetFileNameWithoutExtension(assetFilename_);
			}
		}

		string ISearchableElement.Description {
			get {
				return assetPath_;
			}
		}

		bool ISearchableElement.SupportDrag {
			get {
				return true;
			}
		}

		UnityEngine.Object ISearchableElement.DragObject {
			get {
				return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath_);
			}
		}

		void ISearchableElement.Select () {
			_Select(false);
		}

		void ISearchableElement.Execute () {
			_Select(true);
		}

		private void _Select (bool focus) {
			var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath_);
			Selection.activeObject = obj;
			EditorGUIUtility.PingObject(obj);

			if (focus)
				EditorUtility.FocusProjectWindow();
		}
	}
}
