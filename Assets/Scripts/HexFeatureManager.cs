using UnityEngine;

public class HexFeatureManager : MonoBehaviour {

	public HexFeatureCollection[] urbanCollections, farmCollections, plantCollections;
	public HexMesh walls;

	Transform container;

	public void Clear () {
		if (container)
			Destroy (container.gameObject);
		container = new GameObject ("Features Container").transform;
		container.SetParent (transform, false);
		walls.Clear ();
	}

	public void Apply () {
		walls.Apply ();
	}

	public void AddFeature (HexCell cell, Vector3 pos) {
		HexHash hash = HexMetrics.SampleHashGrid (pos);
		Transform prefab = PickPrefab (urbanCollections, cell.UrbanLevel, hash.a, hash.d);
		Transform otherPrefab = PickPrefab (farmCollections, cell.FarmLevel, hash.b, hash.d);
		float usedHash = hash.a;
		if (prefab) {
			if (otherPrefab && hash.b < hash.a) {
				prefab = otherPrefab;
				usedHash = hash.b;
			}
		} else if (otherPrefab) {
			prefab = otherPrefab;
			usedHash = hash.b;
		}
		
		otherPrefab = PickPrefab (plantCollections, cell.PlantLevel, hash.c, hash.d);

		if (prefab) {
			if (otherPrefab && hash.c < usedHash)
				prefab = otherPrefab;
		} else if (otherPrefab)
			prefab = otherPrefab;
		else
			return;
			
		Transform instance = Instantiate (prefab);
		pos.y += instance.localScale.y * 0.5f;
		instance.localPosition = HexMetrics.Perturb (pos);
		instance.localRotation = Quaternion.Euler (0f, 360f * hash.e, 0f);
		instance.SetParent (container, false);
	}

	public void AddWall (HexGridChunk.EdgeVertices near, HexCell nearCell, HexGridChunk.EdgeVertices far, HexCell farCell) {
		if (nearCell.Walled != farCell.Walled) {
			AddWallSegment (near.v1, far.v1, near.v2, far.v2);
			AddWallSegment (near.v2, far.v2, near.v3, far.v3);
			AddWallSegment (near.v3, far.v3, near.v4, far.v4);
			AddWallSegment (near.v4, far.v4, near.v5, far.v5);
		}
	}

	public void AddWall (Vector3 c1, HexCell cell1, Vector3 c2, HexCell cell2, Vector3 c3, HexCell cell3) {
		if (cell1.Walled) {
			if (cell2.Walled) {
				if (!cell3.Walled)
					AddWallSegment (c3, cell3, c1, cell1, c2, cell2);
			} else if (cell3.Walled)
				AddWallSegment (c2, cell2, c3, cell3, c1, cell1);
			else
				AddWallSegment (c1, cell1, c2, cell2, c3, cell3);
		}
		else if (cell2.Walled) {
			if (cell3.Walled)
				AddWallSegment (c1, cell1, c2, cell2, c3, cell3);
			else
				AddWallSegment (c2, cell2, c3, cell3, c1, cell1);
		}
		else if (cell3.Walled)
			AddWallSegment (c3, cell3, c1, cell1, c2, cell2);
	}

	void AddWallSegment (Vector3 nearLeft, Vector3 farLeft, Vector3 nearRight, Vector3 farRight) {
		nearLeft = HexMetrics.Perturb (nearLeft);
		farLeft = HexMetrics.Perturb (farLeft);
		nearRight = HexMetrics.Perturb (nearRight);
		farRight = HexMetrics.Perturb (farRight);

		Vector3 left = HexMetrics.WallLerp (nearLeft, farLeft);
		Vector3 right = HexMetrics.WallLerp (nearRight, farRight);

		Vector3 leftThicknessOffset = HexMetrics.WallThicknessOffset (nearLeft, farLeft);
		Vector3 rightThicknessOfsset = HexMetrics.WallThicknessOffset (nearRight, farRight);
		Vector3 topLeftNear, topRightNear;

		Vector3 v1, v2, v3, v4;
		v1 = v3 = left - leftThicknessOffset;
		v2 = v4 = right - rightThicknessOfsset;
		v3.y = left.y + HexMetrics.wallHeight;
		v4.y = right.y + HexMetrics.wallHeight;
		walls.AddQuadUnperturbed (v1, v2, v3, v4);
		topLeftNear = v3;
		topRightNear = v4;
		v1 = v3 = left + leftThicknessOffset;
		v2 = v4 = right + rightThicknessOfsset;
		v3.y = left.y + HexMetrics.wallHeight;
		v4.y = right.y + HexMetrics.wallHeight;
		walls.AddQuadUnperturbed (v2, v1, v4, v3);
		walls.AddQuadUnperturbed (topLeftNear, topRightNear, v3, v4);
	}

	void AddWallSegment (Vector3 pivot, HexCell pivotCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
		AddWallSegment (pivot, left, pivot, right);
	}

	Transform PickPrefab (HexFeatureCollection[] collection, int level, float hash, float choice) {
		if (level > 0) {
			float[] thresholds = HexMetrics.GetFeatureThresholds (level - 1);
			for (int i = 0; i < thresholds.Length; i++)
				if (hash < thresholds [i])
					return collection [i].Pick(choice);
		}
		return null;
	}
}
