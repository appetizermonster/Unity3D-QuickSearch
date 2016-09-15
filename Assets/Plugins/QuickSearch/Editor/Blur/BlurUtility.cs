using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

// Referenced from https://medium.com/@jimfleming/translucent-editor-windows-in-unity3d-b21778c04de9
namespace QuickSearch {

	public sealed class BlurOptions {
		public Color tint = Color.white;
		public float tinting = 0.8f;
		public float blurSize = 4f;
		public int passes = 8;
	}

	public static class BlurUtility {

		public static Texture BlurTexture (Texture sourceTexture, BlurOptions options) {
			var blurMaterial = new Material(Shader.Find("Hidden/QuickSearch-Blur"));
			blurMaterial.SetColor("_Tint", options.tint);
			blurMaterial.SetFloat("_Tinting", options.tinting);
			blurMaterial.SetFloat("_BlurSize", options.blurSize);

			var destTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0);
			destTexture.Create();

			var active = RenderTexture.active;
			try {
				var tempA = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height);
				var tempB = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height);

				for (int i = 0; i < options.passes; i++) {
					if (i == 0) {
						Graphics.Blit(sourceTexture, tempA, blurMaterial, 0);
					} else {
						Graphics.Blit(tempB, tempA, blurMaterial, 0);
					}
					Graphics.Blit(tempA, tempB, blurMaterial, 1);
				}

				Graphics.Blit(tempB, destTexture, blurMaterial, 2);

				RenderTexture.ReleaseTemporary(tempA);
				RenderTexture.ReleaseTemporary(tempB);
			} catch (Exception e) {
				Debug.LogException(e);
			} finally {
				RenderTexture.active = active; // Restore
			}

			Material.DestroyImmediate(blurMaterial);

			return destTexture;
		}
	}
}
