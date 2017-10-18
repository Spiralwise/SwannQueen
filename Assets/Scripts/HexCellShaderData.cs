using UnityEngine;

public class HexCellShaderData : MonoBehaviour {

	Texture2D cellTexture;
	Color32[] cellTextureData;

	public void Initialize (int x, int y) {
		if (cellTexture)
			cellTexture.Resize (x, y);
		else {
			cellTexture = new Texture2D (x, y, TextureFormat.RGBA32, false, true);
			cellTexture.filterMode = FilterMode.Point;
			cellTexture.wrapMode = TextureWrapMode.Clamp;
			Shader.SetGlobalTexture ("_HexCellData", cellTexture);
		}
		Shader.SetGlobalVector ("_HexCellData_TexelSize", new Vector4 (1f / x, 1f / y, x, y));
		if (cellTextureData == null || cellTextureData.Length != x * y)
			cellTextureData = new Color32[x * y];
		else
			for (int i = 0; i < cellTextureData.Length; i++)
				cellTextureData [i] = new Color32 (0, 0, 0, 0);
		enabled = true;
	}

	public void RefreshVisibility (HexCell cell) {
		cellTextureData [cell.Index].r = cell.IsVisible ? (byte)255 : (byte)0;
		enabled = true;
	}

	public void RefreshTerrain (HexCell cell) {
		cellTextureData [cell.Index].a = (byte)cell.TerrainTypeIndex;
		enabled = true;
	}

	void LateUpdate () {
		cellTexture.SetPixels32 (cellTextureData);
		cellTexture.Apply ();
		enabled = false;
	}
}
