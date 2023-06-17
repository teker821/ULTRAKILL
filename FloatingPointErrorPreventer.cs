using UnityEngine;

public class FloatingPointErrorPreventer : MonoBehaviour
{
	private bool deactivated;

	private Vector3 startPos;

	public float checkFrequency = 3f;

	private void Start()
	{
		startPos = base.transform.position;
		SlowCheck();
	}

	private void SlowCheck()
	{
		if (!deactivated)
		{
			Invoke("SlowCheck", checkFrequency);
		}
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (base.transform.position.y > 0f && base.transform.position.y < 10000f)
		{
			position.y = startPos.y;
		}
		if (!(Vector3.Distance(position, startPos) > 5000f))
		{
			return;
		}
		deactivated = true;
		CharacterJoint[] componentsInChildren = GetComponentsInChildren<CharacterJoint>();
		Rigidbody[] componentsInChildren2 = GetComponentsInChildren<Rigidbody>();
		CharacterJoint[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i]);
		}
		Rigidbody[] array2 = componentsInChildren2;
		foreach (Rigidbody rigidbody in array2)
		{
			if (rigidbody != null)
			{
				rigidbody.useGravity = false;
				rigidbody.isKinematic = true;
			}
		}
		base.gameObject.SetActive(value: false);
		base.transform.position = new Vector3(-100f, -100f, -100f);
		base.transform.localScale = Vector3.zero;
		Object.Destroy(this);
	}
}
