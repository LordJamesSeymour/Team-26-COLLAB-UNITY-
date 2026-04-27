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
			int sides,
			float tileEveryUnits = 50f)
		{
			if (mesh == null || centers == null || forwards == null || ups == null)
				return;

			if (centers.Length < 2 || forwards.Length != centers.Length || ups.Length != centers.Length)
				return;

			sides = Mathf.Max(3, sides);
			radius = Mathf.Max(0.001f, radius);
			tileEveryUnits = Mathf.Max(0.001f, tileEveryUnits);

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

			float circumference = 2f * Mathf.PI * radius;

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

					float aroundDistance = (side / (float)sides) * circumference;
					float alongDistance = vCoords[ring];

					uvs[index] = new Vector2(
						aroundDistance / tileEveryUnits,
						alongDistance / tileEveryUnits);
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

		public static void BuildRibbonMesh(
			Mesh mesh,
			Vector3[] edgeA,
			Vector3[] edgeB,
			bool doubleSided,
			bool rotateUVs90 = false,
			bool flipU = false,
			bool flipV = false,
			float tileEveryUnitsAlong = 50f,
			float tileEveryUnitsAcross = 50f,
			bool reverseAcrossUVDirection = false)
		{
			if (mesh == null || edgeA == null || edgeB == null)
				return;

			if (edgeA.Length < 2 || edgeB.Length != edgeA.Length)
				return;

			tileEveryUnitsAlong = Mathf.Max(0.001f, tileEveryUnitsAlong);
			tileEveryUnitsAcross = Mathf.Max(0.001f, tileEveryUnitsAcross);

			int count = edgeA.Length;
			int vertexCount = count * 2;
			int quadCount = count - 1;
			int triangleCount = quadCount * 2;
			if (doubleSided)
				triangleCount *= 2;

			Vector3[] vertices = new Vector3[vertexCount];
			Vector2[] uvs = new Vector2[vertexCount];
			int[] triangles = new int[triangleCount * 3];

			float accumulatedLength = 0f;
			float[] alongCoords = new float[count];
			alongCoords[0] = 0f;

			for (int i = 1; i < count; i++)
			{
				Vector3 prevCenter = (edgeA[i - 1] + edgeB[i - 1]) * 0.5f;
				Vector3 currentCenter = (edgeA[i] + edgeB[i]) * 0.5f;
				accumulatedLength += Vector3.Distance(prevCenter, currentCenter);
				alongCoords[i] = accumulatedLength;
			}

			for (int i = 0; i < count; i++)
			{
				int a = i * 2;
				int b = a + 1;

				vertices[a] = edgeA[i];
				vertices[b] = edgeB[i];

				float along = alongCoords[i] / tileEveryUnitsAlong;
				float across = Vector3.Distance(edgeA[i], edgeB[i]) / tileEveryUnitsAcross;

				float aU, aV, bU, bV;

				if (rotateUVs90)
				{
					aU = along;
					bU = along;

					float v0 = 0f;
					float v1 = across;

					bool invertAcross = reverseAcrossUVDirection ^ flipV;
					if (invertAcross)
					{
						float temp = v0;
						v0 = v1;
						v1 = temp;
					}

					aV = v0;
					bV = v1;
				}
				else
				{
					float u0 = 0f;
					float u1 = across;

					if (flipU)
					{
						float temp = u0;
						u0 = u1;
						u1 = temp;
					}

					aU = u0;
					bU = u1;

					aV = along;
					bV = along;

					if (flipV)
					{
						aV = -aV;
						bV = -bV;
					}
				}

				uvs[a] = new Vector2(aU, aV);
				uvs[b] = new Vector2(bU, bV);
			}

			int tri = 0;
			for (int i = 0; i < count - 1; i++)
			{
				int a0 = i * 2;
				int b0 = a0 + 1;
				int a1 = a0 + 2;
				int b1 = a1 + 1;

				triangles[tri++] = a0;
				triangles[tri++] = a1;
				triangles[tri++] = b0;

				triangles[tri++] = b0;
				triangles[tri++] = a1;
				triangles[tri++] = b1;

				if (doubleSided)
				{
					triangles[tri++] = b0;
					triangles[tri++] = a1;
					triangles[tri++] = a0;

					triangles[tri++] = b1;
					triangles[tri++] = a1;
					triangles[tri++] = b0;
				}
			}

			mesh.Clear();
			mesh.vertices = vertices;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}
	}
}