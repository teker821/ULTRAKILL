using UnityEngine;

public class FogEnabler : MonoBehaviour
{
	public bool disable;

	public bool oneTime;

	private bool activated;

	private void OnEnable()
	{
		if (!oneTime || !activated)
		{
			activated = true;
			RenderSettings.fog = !disable;
		}
	}
}
