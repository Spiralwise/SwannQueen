using System.Collections.Generic;
using UnityEngine;

public class HexCellShaderData : MonoBehaviour {

	const float transitionSpeed = 255f;

	Texture2D cellTexture;
	Color32[] cellTextureData;

	List<HexCell> transitioningCells = new List<HexCell>();

	public bool ImmediateMode {
		get;
		set;
	}

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
		transitioningCells.Clear ();
		enabled = true;
	}

	public void RefreshVisibility (HexCell cell) {
		int index = cell.Index;
		if (ImmediateMode) {
			cellTextureData [index].r = cell.IsVisible ? (byte)255 : (byte)0;
			cellTextureData [index].g = cell.IsExplored ? (byte)255 : (byte)0;
		}
		else {
			transitioningCells.Add (cell);
		}
		enabled = true;
	}

	public void RefreshTerrain (HexCell cell) {
		cellTextureData [cell.Index].a = (byte)cell.TerrainTypeIndex;
		enabled = true;
	}

	void LateUpdate () {
		int delta = (int)(Time.deltaTime * transitionSpeed);
		if (delta == 0)
			delta = 1;
		for (int i = 0; i < transitioningCells.Count; i++)
			if (!UpdateCellData (transitioningCells [i], delta)) {
				transitioningCells [i--] = transitioningCells [transitioningCells.Count - 1];
				transitioningCells.RemoveAt (transitioningCells.Count - 1);
			}
		cellTexture.SetPixels32 (cellTextureData);
		cellTexture.Apply ();
		enabled = transitioningCells.Count > 0;
	}

	bool UpdateCellData (HexCell cell, int delta) {
		int index = cell.Index;
		Color32 data = cellTextureData [index];
		bool stillUpdating = false;
		if (cell.IsExplored && data.g < 255) {
			stillUpdating = true;
			int t = data.g + delta;
			data.g = t >= 255 ? (byte)255 : (byte)t;
		}
		if (cell.IsVisible) {
			if (data.r < 255) {
				stillUpdating = true;
				int t = data.r + delta;
				data.r = t >= 255 ? (byte)255 : (byte)t;
			}
		}
		else if (data.r > 0) {
			stillUpdating = true;
			int t = data.r - delta;
			data.r = t < 0 ? (byte)0 : (byte)t;
		}
		cellTextureData [index] = data;
		return stillUpdating;
	}
}
