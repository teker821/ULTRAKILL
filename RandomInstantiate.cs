using System.Collections.Generic;
using UnityEngine;

public class RandomInstantiate : RandomBase<RandomGameObjectEntry>
{
	public bool removePreviousOnRandomize = true;

	[SerializeField]
	private InstantiateObjectMode mode;

	public bool reParent = true;

	public bool useOwnPosition = true;

	public bool useOwnRotation = true;

	private List<GameObject> createdObjects = new List<GameObject>();

	public override void PerformTheAction(RandomEntry entry)
	{
		GameObject gameObject = Object.Instantiate(((RandomGameObjectEntry)entry).targetObject);
		if (useOwnPosition)
		{
			gameObject.transform.position = base.transform.position;
		}
		if (useOwnRotation)
		{
			gameObject.transform.rotation = base.transform.rotation;
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

	public override void RandomizeWithCount(int count)
	{
		if (removePreviousOnRandomize)
		{
			foreach (GameObject createdObject in createdObjects)
			{
				Object.Destroy(createdObject);
			}
			createdObjects.Clear();
		}
		base.RandomizeWithCount(count);
	}
}
