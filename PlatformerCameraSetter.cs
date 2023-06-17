using UnityEngine;

public class PlatformerCameraSetter : MonoBehaviour
{
	public Vector3 position = new Vector3(0f, 7f, -5.5f);

	public Vector3 rotation = new Vector3(20f, 0f, 0f);

	private void OnTriggerEnter(Collider other)
	{
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && !(MonoSingleton<PlatformerMovement>.Instance == null) && other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject)
		{
			MonoSingleton<PlatformerMovement>.Instance.cameraTargets.Add(new CameraTargetInfo(position, rotation, base.gameObject));
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.Platformer || !(other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject) || MonoSingleton<PlatformerMovement>.Instance.cameraTargets.Count <= 0)
		{
			return;
		}
		for (int num = MonoSingleton<PlatformerMovement>.Instance.cameraTargets.Count - 1; num >= 0; num--)
		{
			if (MonoSingleton<PlatformerMovement>.Instance.cameraTargets[num] != null && MonoSingleton<PlatformerMovement>.Instance.cameraTargets[num].caller == base.gameObject)
			{
				MonoSingleton<PlatformerMovement>.Instance.cameraTargets.RemoveAt(num);
				break;
			}
		}
	}
}
