using UnityEngine;

public class Footsteps : MonoBehaviour
{
	public GameObject footstep;

	public void Footstep()
	{
		Object.Instantiate(footstep, base.transform.position, base.transform.rotation);
	}
}
