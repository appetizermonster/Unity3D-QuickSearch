using System.Collections;
using UnityEditor;
using UnityEngine;

namespace QuickSearch {

	public static class AssetIcons {

		// Referenced from http://answers.unity3d.com/questions/792118/what-are-the-editor-resources-by-name-for-editorgu.html
		public static Texture2D GetIcon (string assetExt) {
			switch (assetExt) {
				case "boo":
					return EditorGUIUtility.FindTexture("boo Script Icon");

				case "cginc":
					return EditorGUIUtility.FindTexture("CGProgram Icon");

				case "cs":
					return EditorGUIUtility.FindTexture("cs Script Icon");

				case "guiskin":
					return EditorGUIUtility.FindTexture("GUISkin Icon");

				case "js":
					return EditorGUIUtility.FindTexture("Js Script Icon");

				case "mat":
					return EditorGUIUtility.FindTexture("Material Icon");

				case "prefab":
					return EditorGUIUtility.FindTexture("PrefabNormal Icon");

				case "shader":
					return EditorGUIUtility.FindTexture("Shader Icon");

				case "txt":
					return EditorGUIUtility.FindTexture("TextAsset Icon");

				case "unity":
					return EditorGUIUtility.FindTexture("SceneAsset Icon");

				case "asset":
				case "prefs":
					return EditorGUIUtility.FindTexture("GameManager Icon");

				case "anim":
					return EditorGUIUtility.FindTexture("Animation Icon");

				case "meta":
					return EditorGUIUtility.FindTexture("MetaFile Icon");

				case "ttf":
				case "otf":
				case "fon":
				case "fnt":
					return EditorGUIUtility.FindTexture("Font Icon");

				case "aac":
				case "aif":
				case "aiff":
				case "au":
				case "mid":
				case "midi":
				case "mp3":
				case "mpa":
				case "ra":
				case "ram":
				case "wma":
				case "wav":
				case "wave":
				case "ogg":
					return EditorGUIUtility.FindTexture("AudioClip Icon");

				case "ai":
				case "apng":
				case "png":
				case "bmp":
				case "cdr":
				case "dib":
				case "eps":
				case "exif":
				case "gif":
				case "ico":
				case "icon":
				case "j":
				case "j2c":
				case "j2k":
				case "jas":
				case "jiff":
				case "jng":
				case "jp2":
				case "jpc":
				case "jpe":
				case "jpeg":
				case "jpf":
				case "jpg":
				case "jpw":
				case "jpx":
				case "jtf":
				case "mac":
				case "omf":
				case "qif":
				case "qti":
				case "qtif":
				case "tex":
				case "tfw":
				case "tga":
				case "tif":
				case "tiff":
				case "wmf":
				case "psd":
				case "exr":
					return EditorGUIUtility.FindTexture("Texture Icon");

				case "3df":
				case "3dm":
				case "3dmf":
				case "3ds":
				case "3dv":
				case "3dx":
				case "blend":
				case "c4d":
				case "lwo":
				case "lws":
				case "ma":
				case "max":
				case "mb":
				case "mesh":
				case "obj":
				case "vrl":
				case "wrl":
				case "wrz":
				case "fbx":
					return EditorGUIUtility.FindTexture("Mesh Icon");

				case "asf":
				case "asx":
				case "avi":
				case "dat":
				case "divx":
				case "dvx":
				case "mlv":
				case "m2l":
				case "m2t":
				case "m2ts":
				case "m2v":
				case "m4e":
				case "m4v":
				case "mjp":
				case "mov":
				case "movie":
				case "mp21":
				case "mp4":
				case "mpe":
				case "mpeg":
				case "mpg":
				case "mpv2":
				case "ogm":
				case "qt":
				case "rm":
				case "rmvb":
				case "wmw":
				case "xvid":
					return EditorGUIUtility.FindTexture("MovieTexture Icon");

				case "colors":
				case "gradients":
				case "curves":
				case "curvesnormalized":
				case "particlecurves":
				case "particlecurvessigned":
				case "particledoublecurves":
				case "particledoublecurvessigned":
					return EditorGUIUtility.FindTexture("ScriptableObject Icon");

				case "folder":
					return EditorGUIUtility.FindTexture("Folder Icon");
			}
			return EditorGUIUtility.FindTexture("DefaultAsset Icon");
		}
	}
}
