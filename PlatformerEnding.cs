using UnityEngine;

public class PlatformerEnding : MonoBehaviour
{
	[SerializeField]
	private Transform teleportEffect;

	[SerializeField]
	private UltrakillEvent onActivate;

	private bool activated;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player" && other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject)
		{
			Activate();
		}
	}

	private void Update()
	{
		if (activated)
		{
			teleportEffect.localScale = Vector3.MoveTowards(teleportEffect.localScale, Vector3.up * teleportEffect.localScale.y, Time.deltaTime * 2.5f);
			if (teleportEffect.localScale.x == 0f && teleportEffect.localScale.z == 0f)
			{
				Done();
			}
		}
	}

	public void Activate()
	{
		MonoSingleton<PlatformerMovement>.Instance.transform.parent.GetComponentInChildren<Canvas>().gameObject.SetActive(value: false);
		MonoSingleton<PlatformerMovement>.Instance.gameObject.SetActive(value: false);
		GameObject obj = Object.Instantiate(MonoSingleton<PlatformerMovement>.Instance.gameObject, MonoSingleton<PlatformerMovement>.Instance.transform.position, MonoSingleton<PlatformerMovement>.Instance.transform.rotation, teleportEffect);
		SandboxUtils.StripForPreview(obj.transform);
		obj.SetActive(value: true);
		Invoke("DelayedActivateEffect", 0.5f);
	}

	private void DelayedActivateEffect()
	{
		activated = true;
		onActivate.Invoke();
	}

	public void Done()
	{
		activated = false;
	}
}
