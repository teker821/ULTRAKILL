using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sandbox;

public class SandboxSpawnableInstance : MonoBehaviour
{
	public SpawnableObject sourceObject;

	public GameObject attachedParticles;

	public Collider collider;

	[NonSerialized]
	public bool alwaysFrozen;

	[NonSerialized]
	public Rigidbody rigidbody;

	public bool frozen;

	public bool uniformSize
	{
		get
		{
			if (base.transform.localScale.x == base.transform.localScale.y)
			{
				return base.transform.localScale.y == base.transform.localScale.z;
			}
			return false;
		}
	}

	public float defaultSize { get; private set; }

	public Vector3 normalizedSize => base.transform.localScale / defaultSize;

	public virtual void Awake()
	{
		defaultSize = base.transform.localScale.x;
		rigidbody = GetComponent<Rigidbody>();
		if (collider == null)
		{
			collider = GetComponent<Collider>();
		}
		if (collider == null)
		{
			collider = base.transform.GetComponentInChildren<Collider>();
		}
		SandboxPropPart[] componentsInChildren = GetComponentsInChildren<SandboxPropPart>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].parent = this;
		}
	}

	public virtual void SetSize(Vector3 size)
	{
		base.transform.localScale = size * defaultSize;
	}

	public void SetSizeUniform(float size)
	{
		SetSize(Vector3.one * size);
	}

	public void BaseSave(ref SavedGeneric saveObject)
	{
		if (saveObject != null)
		{
			saveObject.ObjectIdentifier = sourceObject.identifier;
			saveObject.Position = new SavedVector3(base.transform.position);
			saveObject.Rotation = new SavedQuaternion(base.transform.rotation);
			saveObject.Scale = new SavedVector3(normalizedSize);
			if (saveObject is SavedPhysical savedPhysical)
			{
				savedPhysical.Kinematic = frozen;
			}
		}
		else
		{
			saveObject = new SavedGeneric
			{
				ObjectIdentifier = sourceObject.identifier,
				Position = new SavedVector3(base.transform.position),
				Rotation = new SavedQuaternion(base.transform.rotation),
				Scale = new SavedVector3(normalizedSize)
			};
		}
		IAlter[] componentsInChildren = GetComponentsInChildren<IAlter>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		saveObject.Data = new SavedAlterData[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			IAlter obj = componentsInChildren[i];
			List<SavedAlterOption> list = new List<SavedAlterOption>();
			if (obj is IAlterOptions<bool> alterOptions)
			{
				list.AddRange(alterOptions.options.Select((AlterOption<bool> b) => new SavedAlterOption
				{
					BoolValue = b.value,
					Key = b.key
				}));
			}
			if (obj is IAlterOptions<float> alterOptions2)
			{
				list.AddRange(alterOptions2.options.Select((AlterOption<float> b) => new SavedAlterOption
				{
					FloatValue = b.value,
					Key = b.key
				}));
			}
			saveObject.Data[i] = new SavedAlterData
			{
				Key = componentsInChildren[i].alterKey,
				Options = list.ToArray()
			};
		}
	}

	public virtual void Pause(bool freeze = true)
	{
		if (freeze)
		{
			frozen = true;
		}
	}

	public virtual void Resume()
	{
		frozen = false;
	}
}
