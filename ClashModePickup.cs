using UnityEngine;

public class ClashModePickup : MonoBehaviour
{
	private bool activated;

	[SerializeField]
	private GameObject dancer;

	private void OnTriggerEnter(Collider other)
	{
		if (!activated)
		{
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && (bool)MonoSingleton<PlatformerMovement>.Instance && other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject)
			{
				activated = true;
				Activate();
			}
			else if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS && (bool)MonoSingleton<NewMovement>.Instance && other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
			{
				MonoSingleton<PlayerTracker>.Instance.ChangeToPlatformer();
			}
		}
	}

	private void Activate()
	{
		MonoSingleton<PlatformerMovement>.Instance.gameObject.SetActive(value: false);
		MonoSingleton<PlatformerMovement>.Instance.SnapCamera(new Vector3(0f, 5f, -5.5f), new Vector3(20f, 0f, 0f));
		MonoSingleton<PlatformerMovement>.Instance.platformerCamera.position = dancer.transform.position + new Vector3(0f, 5f, -5.5f);
		dancer.SetActive(value: true);
		GameProgressSaver.SetClashModeUnlocked(unlocked: true);
		base.gameObject.SetActive(value: false);
	}
}
