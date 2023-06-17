using UnityEngine;

public class Shockwave : MonoBehaviour
{
	public float lifeTime;

	private void Start()
	{
		Invoke("TimeToDie", lifeTime);
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Breakable")
		{
			Breakable component = other.gameObject.GetComponent<Breakable>();
			if (component != null && component.weak)
			{
				component.Break();
			}
		}
	}

	private void TimeToDie()
	{
		Object.Destroy(this);
	}
}
