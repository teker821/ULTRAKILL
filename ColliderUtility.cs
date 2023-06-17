using System.Collections.Generic;
using NewBlood.Rendering;
using ULTRAKILL.Cheats;
using UnityEngine;

public static class ColliderUtility
{
	private static readonly List<Vector3> s_Vertices = new List<Vector3>();

	private static readonly List<int> s_Triangles = new List<int>();

	private static Triangle<Vector3> GetWorldTriangle(Transform collider, int index)
	{
		Vector3 position = s_Vertices[s_Triangles[3 * index]];
		Vector3 position2 = s_Vertices[s_Triangles[3 * index + 1]];
		Vector3 position3 = s_Vertices[s_Triangles[3 * index + 2]];
		return new Triangle<Vector3>(collider.TransformPoint(position), collider.TransformPoint(position2), collider.TransformPoint(position3));
	}

	private static Plane GetWorldTrianglePlane(Transform collider, int index)
	{
		return GetWorldTrianglePlane(GetWorldTriangle(collider, index));
	}

	private static Plane GetWorldTrianglePlane(Triangle<Vector3> source)
	{
		return new Plane(source.Index0, source.Index1, source.Index2);
	}

	private static bool InTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
	{
		Vector3 vector = b - a;
		Vector3 vector2 = c - a;
		Vector3 rhs = p - a;
		float num = Vector3.Dot(vector, vector);
		float num2 = Vector3.Dot(vector, vector2);
		float num3 = Vector3.Dot(vector, rhs);
		float num4 = Vector3.Dot(vector2, vector2);
		float num5 = Vector3.Dot(vector2, rhs);
		float num6 = 1f / (num * num4 - num2 * num2);
		float num7 = (num4 * num3 - num2 * num5) * num6;
		float num8 = (num * num5 - num2 * num3) * num6;
		if (num7 >= 0f && num8 >= 0f)
		{
			return num7 + num8 < 1f;
		}
		return false;
	}

	public static Vector3 FindClosestPoint(Collider collider, Vector3 position)
	{
		return FindClosestPoint(collider, position, ignoreVerticalTriangles: false);
	}

	public static Vector3 FindClosestPoint(Collider collider, Vector3 position, bool ignoreVerticalTriangles)
	{
		if (NonConvexJumpDebug.Active)
		{
			NonConvexJumpDebug.Reset();
		}
		if (collider is MeshCollider meshCollider && !meshCollider.convex)
		{
			Mesh sharedMesh = meshCollider.sharedMesh;
			sharedMesh.GetVertices(s_Vertices);
			Vector3 vector = Vector3.zero;
			float num = float.PositiveInfinity;
			for (int i = 0; i < sharedMesh.subMeshCount; i++)
			{
				sharedMesh.GetTriangles(s_Triangles, i);
				int j = 0;
				for (int num2 = s_Triangles.Count / 3; j < num2; j++)
				{
					Triangle<Vector3> worldTriangle = GetWorldTriangle(meshCollider.transform, j);
					Plane worldTrianglePlane = GetWorldTrianglePlane(worldTriangle);
					Vector3 vector2 = worldTrianglePlane.ClosestPointOnPlane(position);
					float num3 = Mathf.Abs(worldTrianglePlane.GetDistanceToPoint(position));
					if (!ignoreVerticalTriangles || (!(worldTrianglePlane.normal == Vector3.up) && !(worldTrianglePlane.normal == Vector3.down)))
					{
						bool flag = InTriangle(worldTriangle.Index0, worldTriangle.Index1, worldTriangle.Index2, vector2);
						if (NonConvexJumpDebug.Active)
						{
							NonConvexJumpDebug.CreateTri(worldTrianglePlane, worldTriangle, flag ? new Color(0f, 0f, 1f) : new Color(0f, 0f, Random.Range(0.1f, 0.4f)));
						}
						if (flag && ((i == 0 && j == 0) || num3 < num))
						{
							vector = vector2;
							num = num3;
						}
					}
				}
			}
			if (NonConvexJumpDebug.Active)
			{
				NonConvexJumpDebug.CreateBall(Color.green, vector);
			}
			return vector;
		}
		return collider.ClosestPoint(position);
	}
}
