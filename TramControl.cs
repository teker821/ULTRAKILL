using UnityEngine;

public class TramControl : MonoBehaviour
{
	[SerializeField]
	private Tram targetTram;

	[Space]
	[SerializeField]
	private GameObject clickSound;

	[SerializeField]
	private GameObject clickFailSound;

	public void SpeedUp()
	{
		Object.Instantiate(targetTram.SpeedUp(1) ? clickSound : clickFailSound, base.transform.position, Quaternion.identity);
	}

	public void SpeedDown()
	{
		Object.Instantiate(targetTram.SpeedDown(1) ? clickSound : clickFailSound, base.transform.position, Quaternion.identity);
	}
}
