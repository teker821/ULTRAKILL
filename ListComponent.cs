using System.Collections.Generic;
using UnityEngine;

public abstract class ListComponent<T> : MonoBehaviour where T : MonoBehaviour
{
	public static List<T> InstanceList = new List<T>();

	protected virtual void Awake()
	{
		InstanceList.Add(this as T);
	}

	protected virtual void OnDestroy()
	{
		InstanceList.Remove(this as T);
	}
}
