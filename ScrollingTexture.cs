using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScrollingTexture : MonoBehaviour
{
	private static MaterialPropertyBlock _propertyBlock;

	public float scrollSpeedX;

	public float scrollSpeedY;

	private Dictionary<int, int[]> texturePropertyStNames;

	private Material[] sharedMaterials;

	private MeshRenderer mr;

	private Vector2 offset;

	public bool scrollAttachedObjects;

	public Vector3 force;

	public bool relativeDirection;

	public List<Transform> attachedObjects = new List<Transform>();

	[HideInInspector]
	public Bounds bounds;

	[HideInInspector]
	public bool valuesSet;

	[HideInInspector]
	public List<GameObject> cleanUp = new List<GameObject>();

	[HideInInspector]
	public List<WaterDryTracker> specialScrollers = new List<WaterDryTracker>();

	private void Start()
	{
		if (_propertyBlock == null)
		{
			_propertyBlock = new MaterialPropertyBlock();
		}
		mr = GetComponent<MeshRenderer>();
		texturePropertyStNames = new Dictionary<int, int[]>();
		sharedMaterials = mr.sharedMaterials;
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			Material material = sharedMaterials[i];
			List<string> list = material.GetTexturePropertyNames().ToList();
			for (int j = 0; j < list.Count; j++)
			{
				list[j] += "_ST";
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!material.HasProperty(list[num]))
				{
					list.RemoveAt(num);
				}
			}
			int[] array = new int[list.Count];
			for (int k = 0; k < array.Length; k++)
			{
				array[k] = Shader.PropertyToID(list[k]);
			}
			texturePropertyStNames[i] = array;
		}
		if (!scrollAttachedObjects || valuesSet)
		{
			return;
		}
		valuesSet = true;
		MonoSingleton<ComponentsDatabase>.Instance.scrollers.Add(base.transform);
		Rigidbody component2;
		if (TryGetComponent<Collider>(out var component))
		{
			bounds = component.bounds;
		}
		else if (TryGetComponent<Rigidbody>(out component2))
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			bool flag = false;
			for (int l = 0; l < componentsInChildren.Length; l++)
			{
				if (componentsInChildren[l].attachedRigidbody == component2)
				{
					if (!flag)
					{
						bounds = componentsInChildren[l].bounds;
						flag = true;
					}
					else
					{
						bounds.Encapsulate(componentsInChildren[l].bounds);
					}
				}
			}
		}
		Invoke("SlowUpdate", 5f);
	}

	private void SlowUpdate()
	{
		foreach (GameObject item in cleanUp)
		{
			Object.Destroy(item);
		}
		cleanUp.Clear();
		Invoke("SlowUpdate", 5f);
	}

	private void OnDestroy()
	{
		if ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(base.transform))
		{
			MonoSingleton<ComponentsDatabase>.Instance.scrollers.Remove(base.transform);
		}
	}

	private void Update()
	{
		offset += new Vector2(scrollSpeedX * Time.deltaTime, scrollSpeedY * Time.deltaTime);
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			mr.GetPropertyBlock(_propertyBlock, i);
			int[] array = texturePropertyStNames[i];
			Material material = sharedMaterials[i];
			for (int j = 0; j < array.Length; j++)
			{
				Vector4 vector = material.GetVector(array[j]);
				vector.z = offset.x;
				vector.w = offset.y;
				_propertyBlock.SetVector(array[j], vector);
			}
			mr.SetPropertyBlock(_propertyBlock, i);
		}
		if (!scrollAttachedObjects || attachedObjects.Count <= 0)
		{
			return;
		}
		Vector3 vector2 = force;
		if (relativeDirection)
		{
			vector2 = new Vector3(force.x * base.transform.forward.x, force.y * base.transform.forward.y, force.z * base.transform.forward.z);
		}
		for (int num = attachedObjects.Count - 1; num >= 0; num--)
		{
			if (attachedObjects[num] != null)
			{
				attachedObjects[num].position = attachedObjects[num].position + vector2 * Time.deltaTime;
				int num2 = -1;
				if (specialScrollers.Count != 0)
				{
					for (int num3 = specialScrollers.Count - 1; num3 >= 0; num3--)
					{
						if (specialScrollers[num3].transform == null)
						{
							specialScrollers.RemoveAt(num3);
						}
						else if (specialScrollers[num3].transform == attachedObjects[num])
						{
							num2 = num3;
							break;
						}
					}
				}
				if ((num2 < 0 && Vector3.Distance(attachedObjects[num].position, bounds.ClosestPoint(attachedObjects[num].position)) > 1f) || (num2 >= 0 && Vector3.Distance(attachedObjects[num].position + specialScrollers[num2].closestPosition, bounds.ClosestPoint(attachedObjects[num].position + specialScrollers[num2].closestPosition)) > 1f))
				{
					if (num2 >= 0)
					{
						specialScrollers.RemoveAt(num2);
					}
					cleanUp.Add(attachedObjects[num].gameObject);
					attachedObjects[num].gameObject.SetActive(value: false);
					attachedObjects.RemoveAt(num);
				}
			}
			else
			{
				attachedObjects.RemoveAt(num);
			}
		}
	}
}
