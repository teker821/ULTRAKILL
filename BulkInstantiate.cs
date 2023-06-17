using UnityEngine;

public class BulkInstantiate : MonoBehaviour
{
	[SerializeField]
	private int count = 1;

	[SerializeField]
	private bool instantiateOnEnable;

	[SerializeField]
	private bool instantiateOnStart = true;

	[SerializeField]
	private GameObject source;

	[SerializeField]
	private InstantiateObjectMode mode;

	private void OnEnable()
	{
		if (instantiateOnEnable)
		{
			Instantiate();
		}
	}

	private void Start()
	{
		if (instantiateOnStart)
		{
			Instantiate();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(base.transform.position, base.transform.lossyScale);
	}

	public void Instantiate()
	{
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = Object.Instantiate(source);
			Vector3 position = base.transform.position;
			Vector3 vector = base.transform.localScale / 2f;
			Vector3 position2 = new Vector3(Random.Range(position.x - vector.x, position.x + vector.x), Random.Range(position.y - vector.y, position.y + vector.y), Random.Range(position.z - vector.z, position.z + vector.z));
			gameObject.transform.position = position2;
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
}
