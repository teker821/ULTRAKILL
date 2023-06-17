using System.Collections.Generic;
using NewBlood.Rendering;
using UnityEngine;

namespace ULTRAKILL.Cheats;

public class NonConvexJumpDebug : ICheat
{
	private static NonConvexJumpDebug _lastInstance;

	private bool active;

	private List<GameObject> _debugObjects = new List<GameObject>();

	public static bool Active
	{
		get
		{
			if (_lastInstance != null)
			{
				return _lastInstance.active;
			}
			return false;
		}
	}

	public string LongName => "Non Convex Jump Debug";

	public string Identifier => "ultrakill.debug.non-convex-jump-debug";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => null;

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable()
	{
		active = true;
		_lastInstance = this;
	}

	public void Disable()
	{
		active = false;
		Reset();
	}

	public void Update()
	{
	}

	public static void Reset()
	{
		if (_lastInstance == null)
		{
			return;
		}
		foreach (GameObject debugObject in _lastInstance._debugObjects)
		{
			Object.Destroy(debugObject);
		}
		_lastInstance._debugObjects.Clear();
	}

	public static void CreateBall(Color color, Vector3 position, float size = 1f)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Object.DestroyImmediate(gameObject.GetComponent<Collider>());
		if (_lastInstance == null)
		{
			_lastInstance = new NonConvexJumpDebug();
		}
		_lastInstance._debugObjects.Add(gameObject);
		gameObject.name = "jump debug ball";
		gameObject.transform.position = position;
		gameObject.transform.localScale = Vector3.one * size;
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		Shader shader = Shader.Find("Unlit/Color");
		component.material = new Material(shader);
		component.material.color = color;
		gameObject.AddComponent<RemoveOnTime>().time = 3f;
	}

	public static void CreateTri(Plane plane, Triangle<Vector3> triangle, Color color)
	{
		GameObject gameObject = new GameObject("jump debug tri");
		_lastInstance._debugObjects.Add(gameObject);
		gameObject.transform.position += plane.normal * 0.02f;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		gameObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Color"))
		{
			color = color
		};
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[3] { triangle.Index0, triangle.Index1, triangle.Index2 };
		mesh.triangles = new int[3] { 0, 1, 2 };
		meshFilter.sharedMesh = mesh;
		gameObject.AddComponent<RemoveOnTime>().time = 3f;
	}
}
