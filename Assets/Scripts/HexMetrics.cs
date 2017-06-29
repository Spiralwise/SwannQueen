using UnityEngine;

public static class HexMetrics {

	// Hexagon metrics
	public const float outterRadius = 10.0f;
	public const float innerRadius = outterRadius * 0.866025404f;
	public const float solidFactor = 0.75f;
	public const float blendFactor = 1f - solidFactor;

	// Elevation metrics
	public const float elevationStep = 3f;
	public const int terracesPerSlope = 2;
	public const int terraceSteps = terracesPerSlope * 2 + 1;
	public const float horizontalTerraceStepSize = 1f / terraceSteps;
	public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

	static Vector3[] corners = {
		new Vector3 (0f, 0f, outterRadius),
		new Vector3 (innerRadius, 0f, 0.5f * outterRadius),
		new Vector3 (innerRadius, 0f, -0.5f * outterRadius),
		new Vector3 (0f, 0f, -outterRadius),
		new Vector3 (-innerRadius, 0f, -0.5f * outterRadius),
		new Vector3 (-innerRadius, 0f, 0.5f * outterRadius)
	};

	public static Vector3 GetFirstCorner (HexDirection direction) {
		return corners [(int)direction];
	}

	public static Vector3 GetSecondCorner (HexDirection direction) {
		return corners [((int)direction + 1) % corners.Length];
	}

	public static Vector3 GetFirstSolidCorner (HexDirection direction) {
		return corners [(int)direction] * solidFactor;
	}

	public static Vector3 GetSecondSolidCorner (HexDirection direction) {
		return corners [((int)direction + 1) % corners.Length] * solidFactor;
	}

	public static Vector3 GetBridge (HexDirection direction) {
		return (corners [(int)direction] + corners [((int)direction + 1) % corners.Length]) * blendFactor;
	}

	public static Vector3 TerraceLerp (Vector3 a, Vector3 b, int step) {
		float h = step * horizontalTerraceStepSize;
		a.x += (b.x - a.x) * h;
		a.z += (b.z - a.z) * h;
		float v = ((step + 1) / 2) * verticalTerraceStepSize;
		a.y += (b.y - a.y) * v;
		return a;
	}

	public static Color TerraceLerp (Color a, Color b, int step) {
		float h = step * horizontalTerraceStepSize;
		return Color.Lerp (a, b, h);
	}

	public static HexEdgeType GetEdgeType (int elevationA, int elevationB) {
		if (elevationA == elevationB)
			return HexEdgeType.Flat;
		else if (Mathf.Abs (elevationA - elevationB) == 1)
			return HexEdgeType.Slope;
		else
			return HexEdgeType.Cliff;
	}
}

public enum HexDirection {
	NE, E, SE, SW, W, NW
};

public static class HexDirectionExtensions {

	public static HexDirection Opposite (this HexDirection direction) {
		return (HexDirection) (((int)direction + 3) % 6);
	}

	public static HexDirection Previous (this HexDirection direction) {
		return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
	}

	public static HexDirection Next (this HexDirection direction) {
		return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
	}
}

public enum HexEdgeType {
	Flat, Slope, Cliff
}

[System.Serializable]
public struct HexCoordinates {

	[SerializeField]
	private int x, y;
	
	public int X { get { return x; } }

	public int Y { get { return y; } }

	public int Z { get { return -X - Y; } }

	public HexCoordinates (int x, int y) {
		this.x = x;
		this.y = y;
	}

	public static HexCoordinates FromOffsetCoordinates (int x, int y) {
		return new HexCoordinates (x - y/2, y);
	}

	public static HexCoordinates FromPosition (Vector3 v) {
		float foundX = v.x / (HexMetrics.innerRadius * 2f);
		float foundZ = -foundX;
		float offset = v.z / (HexMetrics.outterRadius * 3f);
		foundX -= offset;
		foundZ -= offset;
		int iX = Mathf.RoundToInt (foundX);
		int iZ = Mathf.RoundToInt (foundZ);
		int iY = Mathf.RoundToInt (-foundX - foundZ);
		if (iX + iY + iZ != 0) {
			float dX = Mathf.Abs (foundX - iX);
			float dZ = Mathf.Abs (foundZ - iZ);
			float dY = Mathf.Abs (-foundZ - foundZ - iY);
			if (dX > dZ && dX > dZ)
				iX = -iZ - iY;
			else if (dY > dZ) {
				iZ = -iX - iY;
			}
		}
		return new HexCoordinates (iX, iY);
	}

	public string toString () {
		return "(" + X.ToString () + ", " + Z.ToString () + ", " + Y.ToString () + ")";
	}

	public string ToStringOnSeparateLines () {
		return X.ToString () + "\n" + Z.ToString () + "\n" + Y.ToString ();
	}
}