using UnityEngine;

public class EnrageEffect : MonoBehaviour
{
	public GameObject endSound;

	public bool noParticle;

	[HideInInspector]
	public bool activated;

	private void Start()
	{
		if (!activated)
		{
			activated = true;
			CameraController instance = MonoSingleton<CameraController>.Instance;
			MonoSingleton<StyleHUD>.Instance.AddPoints(250, "ultrakill.enraged");
			instance.CameraShake(1f);
		}
	}

	private void OnDestroy()
	{
		if (!noParticle && base.gameObject.scene.isLoaded)
		{
			Object.Instantiate(endSound, base.transform.position, base.transform.rotation);
		}
	}
}
