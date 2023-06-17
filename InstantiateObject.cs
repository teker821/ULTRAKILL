using System.Collections.Generic;
using UnityEngine;

public class InstantiateObject : MonoBehaviour
{
	[SerializeField]
	private bool instantiateOnEnable = true;

	[SerializeField]
	private GameObject source;

	[SerializeField]
	private InstantiateObjectMode mode;

	[SerializeField]
	private bool removePreviousOnInstantiate = true;

	[SerializeField]
	private bool reParent = true;

	[SerializeField]
	private bool useOwnPosition = true;

	[SerializeField]
	private bool useOwnRotation = true;

	[SerializeField]
	private bool combineRotations;

	private List<GameObject> createdObjects = new List<GameObject>();

	private void OnEnable()
	{
		if (instantiateOnEnable)
		{
			Instantiate();
		}
	}

	public void Instantiate()
	{
		if (removePreviousOnInstantiate)
		{
			foreach (GameObject createdObject in createdObjects)
			{
				Object.Destroy(createdObject);
			}
			createdObjects.Clear();
		}
		GameObject gameObject = Object.Instantiate(source);
		if (useOwnPosition)
		{
			gameObject.transform.position = base.transform.position;
		}
		if (useOwnRotation)
		{
			if (combineRotations)
			{
				gameObject.transform.rotation *= base.transform.rotation;
			}
			else
			{
				gameObject.transform.rotation = base.transform.rotation;
			}
		}
		if (reParent)
		{
			gameObject.transform.SetParent(base.transform);
			if (useOwnPosition)
			{
				gameObject.transform.localPosition = Vector3.zero;
			}
			if (useOwnRotation)
			{
				gameObject.transform.localRotation = Quaternion.identity;
			}
		}
		createdObjects.Add(gameObject);
		switch (mode)
		{
		case InstantiateObjectMode.ForceDisable:
			gameObject.SetActive(value: false);
			break;
		case InstantiateObjectMode.ForceEnable:
			gameObject.SetActive(value: true);
			break;
		}
	}
}
