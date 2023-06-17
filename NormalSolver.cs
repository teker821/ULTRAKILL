using System;
using System.Collections.Generic;
using UnityEngine;

public static class NormalSolver
{
	private struct VertexKey
	{
		private readonly long _x;

		private readonly long _y;

		private readonly long _z;

		private const int Tolerance = 100000;

		private const long FNV32Init = 2166136261L;

		private const long FNV32Prime = 16777619L;

		public VertexKey(Vector3 position)
		{
			_x = (long)Mathf.Round(position.x * 100000f);
			_y = (long)Mathf.Round(position.y * 100000f);
			_z = (long)Mathf.Round(position.z * 100000f);
		}

		public override bool Equals(object obj)
		{
			VertexKey vertexKey = (VertexKey)obj;
			if (_x == vertexKey._x && _y == vertexKey._y)
			{
				return _z == vertexKey._z;
			}
			return false;
		}

		public override int GetHashCode()
		{
			long num = 2166136261L;
			num ^= _x;
			num *= 16777619;
			num ^= _y;
			num *= 16777619;
			num ^= _z;
			return (num * 16777619).GetHashCode();
		}
	}

	private struct VertexEntry
	{
		public int MeshIndex;

		public int TriangleIndex;

		public int VertexIndex;

		public VertexEntry(int meshIndex, int triIndex, int vertIndex)
		{
			MeshIndex = meshIndex;
			TriangleIndex = triIndex;
			VertexIndex = vertIndex;
		}
	}

	public static void RecalculateNormals(this Mesh mesh, float angle)
	{
		float num = Mathf.Cos(angle * ((float)Math.PI / 180f));
		Vector3[] vertices = mesh.vertices;
		Vector3[] array = new Vector3[vertices.Length];
		Vector3[][] array2 = new Vector3[mesh.subMeshCount][];
		Dictionary<VertexKey, List<VertexEntry>> dictionary = new Dictionary<VertexKey, List<VertexEntry>>(vertices.Length);
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			int[] triangles = mesh.GetTriangles(i);
			array2[i] = new Vector3[triangles.Length / 3];
			for (int j = 0; j < triangles.Length; j += 3)
			{
				int num2 = triangles[j];
				int num3 = triangles[j + 1];
				int num4 = triangles[j + 2];
				Vector3 lhs = vertices[num3] - vertices[num2];
				Vector3 rhs = vertices[num4] - vertices[num2];
				Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
				int num5 = j / 3;
				array2[i][num5] = normalized;
				VertexKey key = new VertexKey(vertices[num2]);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = new List<VertexEntry>(4);
					dictionary.Add(key, value);
				}
				value.Add(new VertexEntry(i, num5, num2));
				key = new VertexKey(vertices[num3]);
				if (!dictionary.TryGetValue(key, out value))
				{
					value = new List<VertexEntry>();
					dictionary.Add(key, value);
				}
				value.Add(new VertexEntry(i, num5, num3));
				key = new VertexKey(vertices[num4]);
				if (!dictionary.TryGetValue(key, out value))
				{
					value = new List<VertexEntry>();
					dictionary.Add(key, value);
				}
				value.Add(new VertexEntry(i, num5, num4));
			}
		}
		foreach (List<VertexEntry> value2 in dictionary.Values)
		{
			for (int k = 0; k < value2.Count; k++)
			{
				Vector3 vector = default(Vector3);
				VertexEntry vertexEntry = value2[k];
				for (int l = 0; l < value2.Count; l++)
				{
					VertexEntry vertexEntry2 = value2[l];
					if (vertexEntry.VertexIndex == vertexEntry2.VertexIndex)
					{
						vector += array2[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
					}
					else if (Vector3.Dot(array2[vertexEntry.MeshIndex][vertexEntry.TriangleIndex], array2[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex]) >= num)
					{
						vector += array2[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
					}
				}
				array[vertexEntry.VertexIndex] = vector.normalized;
			}
		}
		mesh.normals = array;
	}
}
