using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CombineSpeedlineFrames
{
	[MenuItem("Tools/Combine Selected Speedline Frames")]
	private static void CombineSelectedFrames()
	{
		Object[] selected = Selection.objects;

		Texture2D[] textures = selected
			.Select(obj => AssetDatabase.GetAssetPath(obj))
			.Where(path => !string.IsNullOrEmpty(path))
			.Select(path => AssetDatabase.LoadAssetAtPath<Texture2D>(path))
			.Where(tex => tex != null)
			.OrderBy(tex => tex.name)
			.ToArray();

		if (textures.Length == 0)
		{
			Debug.LogError("No textures selected.");
			return;
		}

		// Make sure all textures are readable
		for (int i = 0; i < textures.Length; i++)
		{
			string path = AssetDatabase.GetAssetPath(textures[i]);
			TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

			if (importer != null && !importer.isReadable)
			{
				Debug.LogError($"Texture '{textures[i].name}' is not readable. Enable Read/Write in import settings first.");
				return;
			}
		}

		// Find the biggest frame size
		int frameWidth = textures.Max(t => t.width);
		int frameHeight = textures.Max(t => t.height);

		int sheetWidth = frameWidth * textures.Length;
		int sheetHeight = frameHeight;

		Texture2D sheet = new Texture2D(sheetWidth, sheetHeight, TextureFormat.RGBA32, false);

		// Fill whole sheet with transparent pixels first
		Color[] clearPixels = Enumerable.Repeat(new Color(0, 0, 0, 0), sheetWidth * sheetHeight).ToArray();
		sheet.SetPixels(clearPixels);

		for (int i = 0; i < textures.Length; i++)
		{
			Texture2D tex = textures[i];
			Color[] pixels = tex.GetPixels();

			int offsetXInCell = (frameWidth - tex.width) / 2;
			int offsetYInCell = (frameHeight - tex.height) / 2;

			int destX = i * frameWidth + offsetXInCell;
			int destY = offsetYInCell;

			sheet.SetPixels(destX, destY, tex.width, tex.height, pixels);
		}

		sheet.Apply();

		string outputPath = "Assets/Speedlines_Sheet.png";
		File.WriteAllBytes(outputPath, sheet.EncodeToPNG());

		AssetDatabase.Refresh();

		TextureImporter sheetImporter = AssetImporter.GetAtPath(outputPath) as TextureImporter;
		if (sheetImporter != null)
		{
			sheetImporter.textureType = TextureImporterType.Sprite;
			sheetImporter.spriteImportMode = SpriteImportMode.Single;
			sheetImporter.alphaIsTransparency = true;
			sheetImporter.isReadable = true;
			sheetImporter.SaveAndReimport();
		}

		Debug.Log($"Created spritesheet with padded frames: {outputPath}");
	}
}