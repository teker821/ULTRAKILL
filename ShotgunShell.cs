using UnityEngine;

public class ShotgunShell : MonoBehaviour
{
	private bool hitGround;

	private void Start()
	{
		Invoke("TurnGib", 0.2f);
		Invoke("Remove", 2f);
	}

	private void TurnGib()
	{
		GetComponent<Collider>().enabled = true;
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 9;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((!hitGround && collision.gameObject.layer == 8) || collision.gameObject.layer == 24)
		{
			hitGround = true;
			AudioSource component = GetComponent<AudioSource>();
			component.pitch = Random.Range(0.85f, 1.15f);
			component.Play();
		}
	}

	private void Remove()
	{
		if (!hitGround || base.transform.position.magnitude > 1000f)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Object.Destroy(GetComponent<Rigidbody>());
		}
	}
}
