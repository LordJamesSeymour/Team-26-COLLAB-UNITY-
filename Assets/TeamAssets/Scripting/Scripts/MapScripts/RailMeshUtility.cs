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

		public static void BuildRibbonMesh(
			Mesh mesh,
			Vector3[] edgeA,
			Vector3[] edgeB,
			bool doubleSided)
		{
			if (mesh == null || edgeA == null || edgeB == null)
				return;

			if (edgeA.Length < 2 || edgeB.Length != edgeA.Length)
				return;

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
			float[] vCoords = new float[count];
			vCoords[0] = 0f;

			for (int i = 1; i < count; i++)
			{
				Vector3 prevCenter = (edgeA[i - 1] + edgeB[i - 1]) * 0.5f;
				Vector3 currentCenter = (edgeA[i] + edgeB[i]) * 0.5f;
				accumulatedLength += Vector3.Distance(prevCenter, currentCenter);
				vCoords[i] = accumulatedLength;
			}

			if (accumulatedLength <= 0.0001f)
				accumulatedLength = 1f;

			for (int i = 0; i < count; i++)
			{
				int a = i * 2;
				int b = a + 1;

				vertices[a] = edgeA[i];
				vertices[b] = edgeB[i];

				float v = vCoords[i] / accumulatedLength;
				uvs[a] = new Vector2(0f, v);
				uvs[b] = new Vector2(1f, v);
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

		public static void BuildRibbonMeshForVisualizerWall(
			Mesh mesh,
			Vector3[] edgeTop,
			Vector3[] edgeBottom,
			bool doubleSided,
			bool flipFacing,
			float waveformRepeatEveryWorldUnits,
			float detailRepeatEveryWorldUnits,
			float averageHeightForUv,
			float verticalUvScale)
		{
			if (mesh == null || edgeTop == null || edgeBottom == null)
				return;

			if (edgeTop.Length < 2 || edgeBottom.Length != edgeTop.Length)
				return;

			waveformRepeatEveryWorldUnits = Mathf.Max(0.01f, waveformRepeatEveryWorldUnits);
			detailRepeatEveryWorldUnits = Mathf.Max(0.01f, detailRepeatEveryWorldUnits);
			verticalUvScale = Mathf.Max(0.01f, verticalUvScale);

			int count = edgeTop.Length;
			int vertexCount = count * 2;
			int quadCount = count - 1;
			int triangleCount = quadCount * 2;
			if (doubleSided)
				triangleCount *= 2;

			Vector3[] vertices = new Vector3[vertexCount];
			Vector2[] uv0 = new Vector2[vertexCount];
			Vector2[] uv1 = new Vector2[vertexCount];
			int[] triangles = new int[triangleCount * 3];

			float accumulatedLength = 0f;
			float[] lengths = new float[count];
			lengths[0] = 0f;

			for (int i = 1; i < count; i++)
			{
				Vector3 prevCenter = (edgeTop[i - 1] + edgeBottom[i - 1]) * 0.5f;
				Vector3 currentCenter = (edgeTop[i] + edgeBottom[i]) * 0.5f;
				accumulatedLength += Vector3.Distance(prevCenter, currentCenter);
				lengths[i] = accumulatedLength;
			}

			for (int i = 0; i < count; i++)
			{
				int topIndex = i * 2;
				int bottomIndex = topIndex + 1;

				vertices[topIndex] = edgeTop[i];
				vertices[bottomIndex] = edgeBottom[i];

				float columnHeight = Vector3.Distance(edgeTop[i], edgeBottom[i]);

				float topV;
				if (averageHeightForUv > 0.0001f)
					topV = (columnHeight / averageHeightForUv) * verticalUvScale;
				else
					topV = 1f * verticalUvScale;

				float uWave = lengths[i] / waveformRepeatEveryWorldUnits;
				float uDetail = lengths[i] / detailRepeatEveryWorldUnits;

				uv0[topIndex] = new Vector2(uWave, topV);
				uv0[bottomIndex] = new Vector2(uWave, 0f);

				uv1[topIndex] = new Vector2(uDetail, topV);
				uv1[bottomIndex] = new Vector2(uDetail, 0f);
			}

			int tri = 0;
			for (int i = 0; i < count - 1; i++)
			{
				int top0 = i * 2;
				int bottom0 = top0 + 1;
				int top1 = top0 + 2;
				int bottom1 = top1 + 1;

				if (!flipFacing)
				{
					triangles[tri++] = top0;
					triangles[tri++] = top1;
					triangles[tri++] = bottom0;

					triangles[tri++] = bottom0;
					triangles[tri++] = top1;
					triangles[tri++] = bottom1;
				}
				else
				{
					triangles[tri++] = bottom0;
					triangles[tri++] = top1;
					triangles[tri++] = top0;

					triangles[tri++] = bottom1;
					triangles[tri++] = top1;
					triangles[tri++] = bottom0;
				}

				if (doubleSided)
				{
					if (!flipFacing)
					{
						triangles[tri++] = bottom0;
						triangles[tri++] = top1;
						triangles[tri++] = top0;

						triangles[tri++] = bottom1;
						triangles[tri++] = top1;
						triangles[tri++] = bottom0;
					}
					else
					{
						triangles[tri++] = top0;
						triangles[tri++] = top1;
						triangles[tri++] = bottom0;

						triangles[tri++] = bottom0;
						triangles[tri++] = top1;
						triangles[tri++] = bottom1;
					}
				}
			}

			mesh.Clear();
			mesh.vertices = vertices;
			mesh.uv = uv0;
			mesh.uv2 = uv1;
			mesh.triangles = triangles;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}
	}
}