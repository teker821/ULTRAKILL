using UnityEngine;

public class SkyboxEnabler : MonoBehaviour
{
	public bool disable;

	public bool oneTime;

	private bool activated;

	private void OnEnable()
	{
		if (!oneTime || !activated)
		{
			activated = true;
			if (MonoSingleton<CameraController>.Instance.TryGetComponent<Camera>(out var component))
			{
				component.clearFlags = ((!disable) ? CameraClearFlags.Skybox : CameraClearFlags.Color);
			}
		}
	}
}
