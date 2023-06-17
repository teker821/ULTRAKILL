using UnityEngine;

public class DeathMarker : MonoBehaviour
{
	public bool activated;

	private void OnEnable()
	{
		if (!activated)
		{
			activated = true;
			GetComponentInParent<ActivateNextWave>().AddDeadEnemy();
		}
	}
}
