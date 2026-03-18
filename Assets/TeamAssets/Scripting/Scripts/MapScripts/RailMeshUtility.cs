using UnityEngine;

namespace Group26.Player.Movement
{
	public static class RailMeshUtility
	{
		public static void BuildTubeMesh(
			Mesh mesh,
			Vector3[] centers,
			Vector3[] forwards,
			Vector3[] ups,
			float radius,
			int sides)
		{
			if (mesh == null || centers == null || forwards == null || ups == null)
				return;

			if (centers.Length < 2 || forwards.Length != centers.Length || ups.Length != centers.Length)
				return;

			sides = Mathf.Max(3, sides);
			radius = Mathf.Max(0.001f, radius);

			int ringCount = centers.Length;
			int vertexCount = ringCount * sides;
			int triangleCount = (ringCount - 1) * sides * 2;

			Vector3[] vertices = new Vector3[vertexCount];
			Vector3[] normals = new Vector3[vertexCount];
			Vector2[] uvs = new Vector2[vertexCount];
			int[] triangles = new int[triangleCount * 3];

			float accumulatedLength = 0f;
			float[] vCoords = new float[ringCount];
			vCoords[0] = 0f;

			for (int i = 1; i < ringCount; i++)
			{
				accumulatedLength += Vector3.Distance(centers[i - 1], centers[i]);
				vCoords[i] = accumulatedLength;
			}

			if (accumulatedLength <= 0.0001f)
				accumulatedLength = 1f;

			for (int ring = 0; ring < ringCount; ring++)
			{
				Vector3 forward = forwards[ring].normalized;
				if (forward.sqrMagnitude < 0.0001f)
					forward = Vector3.forward;

				Vector3 up = ups[ring].normalized;
				if (up.sqrMagnitude < 0.0001f)
					up = Vector3.up;

				Vector3 right = Vector3.Cross(up, forward).normalized;
				if (right.sqrMagnitude < 0.0001f)
					right = Vector3.right;

				up = Vector3.Cross(forward, right).normalized;

				for (int side = 0; side < sides; side++)
				{
					float angle = (side / (float)sides) * Mathf.PI * 2f;
					float cos = Mathf.Cos(angle);
					float sin = Mathf.Sin(angle);

					Vector3 normal = (right * cos) + (up * sin);
					int index = ring * sides + side;

					vertices[index] = centers[ring] + normal * radius;
					normals[index] = normal.normalized;
					uvs[index] = new Vector2(side / (float)sides, vCoords[ring] / accumulatedLength);
				}
			}

			int triIndex = 0;

			for (int ring = 0; ring < ringCount - 1; ring++)
			{
				int currentRing = ring * sides;
				int nextRing = (ring + 1) * sides;

				for (int side = 0; side < sides; side++)
				{
					int nextSide = (side + 1) % sides;

					int a = currentRing + side;
					int b = currentRing + nextSide;
					int c = nextRing + side;
					int d = nextRing + nextSide;

					triangles[triIndex++] = a;
					triangles[triIndex++] = c;
					triangles[triIndex++] = b;

					triangles[triIndex++] = b;
					triangles[triIndex++] = c;
					triangles[triIndex++] = d;
				}
			}

			mesh.Clear();
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();
		}
	}
}