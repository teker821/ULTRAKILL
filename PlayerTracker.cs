using UnityEngine;

public class PlayerTracker : MonoSingleton<PlayerTracker>
{
	public PlayerType playerType;

	private bool initialized;

	private NewMovement nmov;

	private CameraController cc;

	public GameObject platformerPlayerPrefab;

	[HideInInspector]
	public GameObject currentPlatformerPlayerPrefab;

	[HideInInspector]
	public PlatformerMovement pmov;

	private Transform player;

	private Transform target;

	private Rigidbody playerRb;

	[HideInInspector]
	public bool levelStarted;

	private bool startAsPlatformer;

	public PlatformerCameraType cameraType;

	public GameObject[] platformerFailSafes;

	private void Start()
	{
		if (!initialized)
		{
			Initialize();
		}
	}

	public Transform GetPlayer()
	{
		if (!initialized)
		{
			Initialize();
		}
		return player;
	}

	public Transform GetTarget()
	{
		if (!initialized)
		{
			Initialize();
		}
		return target;
	}

	public Rigidbody GetRigidbody()
	{
		if (!initialized)
		{
			Initialize();
		}
		return playerRb;
	}

	public Vector3 PredictPlayerPosition(float time)
	{
		Vector3 vector = GetPlayerVelocity() * time;
		if (Physics.Raycast(playerRb.position, vector, out var hitInfo, vector.magnitude, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			return hitInfo.point;
		}
		return playerRb.position + vector;
	}

	public Vector3 GetPlayerVelocity()
	{
		if (!initialized)
		{
			Initialize();
		}
		Vector3 velocity = playerRb.velocity;
		if (MonoSingleton<NewMovement>.Instance.boost && !MonoSingleton<NewMovement>.Instance.sliding)
		{
			velocity /= 3f;
		}
		if ((bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
		{
			velocity += MonoSingleton<NewMovement>.Instance.ridingRocket.rb.velocity;
		}
		if (MonoSingleton<PlayerMovementParenting>.Instance != null)
		{
			Vector3 currentDelta = MonoSingleton<PlayerMovementParenting>.Instance.currentDelta;
			currentDelta *= 60f;
			velocity += currentDelta;
		}
		return velocity;
	}

	public bool GetOnGround()
	{
		if (!initialized)
		{
			Initialize();
		}
		if (playerType != 0 || !MonoSingleton<NewMovement>.Instance.gc.onGround)
		{
			if (playerType == PlayerType.Platformer)
			{
				return MonoSingleton<PlatformerMovement>.Instance.groundCheck.onGround;
			}
			return false;
		}
		return true;
	}

	public void ChangeToPlatformer()
	{
		ChangeToPlatformer(ignorePreviousRotation: false);
	}

	public void ChangeToPlatformer(bool ignorePreviousRotation = false)
	{
		if (!initialized)
		{
			Initialize();
		}
		if (!pmov || !nmov || !currentPlatformerPlayerPrefab)
		{
			return;
		}
		if (cameraType == PlatformerCameraType.PlayerControlled)
		{
			pmov.freeCamera = true;
		}
		else
		{
			pmov.freeCamera = false;
		}
		if (!levelStarted)
		{
			startAsPlatformer = true;
		}
		else
		{
			if (playerType == PlayerType.Platformer)
			{
				return;
			}
			if (cameraType == PlatformerCameraType.PlayerControlled && !ignorePreviousRotation)
			{
				pmov.ResetCamera(MonoSingleton<CameraController>.Instance.rotationY, MonoSingleton<CameraController>.Instance.rotationX + 20f);
			}
			playerType = PlayerType.Platformer;
			currentPlatformerPlayerPrefab.SetActive(value: true);
			pmov.gameObject.SetActive(value: true);
			ChangeTargetParent(player, pmov.transform, Vector3.up * 2.5f);
			ChangeTargetParent(target, pmov.transform, Vector3.up * 2.5f);
			nmov.gameObject.SetActive(value: false);
			currentPlatformerPlayerPrefab.transform.position = nmov.transform.position - Vector3.up * 1.5f;
			pmov.transform.position = currentPlatformerPlayerPrefab.transform.position;
			pmov.platformerCamera.transform.localPosition = Vector3.up * 2.5f;
			pmov.playerModel.transform.rotation = nmov.transform.rotation;
			if ((bool)pmov.rb)
			{
				playerRb = pmov.rb;
			}
			else
			{
				playerRb = pmov.GetComponent<Rigidbody>();
			}
			pmov.CheckItem();
			playerRb.velocity = nmov.rb.velocity;
			MonoSingleton<PostProcessV2_Handler>.Instance?.ChangeCamera(hudless: true);
			GameObject[] array = platformerFailSafes;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: true);
				}
			}
		}
	}

	public void ChangeToFPS()
	{
		if (!initialized)
		{
			Initialize();
		}
		if (!pmov || !nmov || !currentPlatformerPlayerPrefab)
		{
			return;
		}
		if (!levelStarted)
		{
			startAsPlatformer = false;
		}
		else
		{
			if (playerType == PlayerType.FPS)
			{
				return;
			}
			playerType = PlayerType.FPS;
			nmov.transform.position = pmov.transform.position + Vector3.up * 1.5f;
			currentPlatformerPlayerPrefab.SetActive(value: false);
			playerRb = nmov.rb;
			nmov.gameObject.SetActive(value: true);
			ChangeTargetParent(player, nmov.transform);
			ChangeTargetParent(target, cc.transform);
			pmov.gameObject.SetActive(value: false);
			MonoSingleton<PostProcessV2_Handler>.Instance?.ChangeCamera(hudless: false);
			if (pmov.freeCamera)
			{
				MonoSingleton<CameraController>.Instance.ResetCamera(pmov.rotationY, pmov.rotationX - 20f);
			}
			else
			{
				MonoSingleton<CameraController>.Instance.ResetCamera(pmov.playerModel.transform.rotation.eulerAngles.y);
			}
			if ((bool)pmov.rb)
			{
				nmov.rb.velocity = pmov.rb.velocity;
			}
			GameObject[] array = platformerFailSafes;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: false);
				}
			}
		}
	}

	private void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		nmov = MonoSingleton<NewMovement>.Instance;
		cc = MonoSingleton<CameraController>.Instance;
		if (!nmov || !cc)
		{
			return;
		}
		Camera camera = null;
		if ((bool)cc && (bool)cc.cam)
		{
			camera = cc.cam;
		}
		else if ((bool)cc)
		{
			camera = cc.GetComponent<Camera>();
		}
		if (playerType == PlayerType.Platformer && !levelStarted)
		{
			startAsPlatformer = true;
			playerType = PlayerType.FPS;
		}
		GameObject gameObject = new GameObject();
		player = gameObject.transform;
		ChangeTargetParent(player, nmov.transform);
		if ((bool)nmov.rb)
		{
			playerRb = nmov.rb;
		}
		else
		{
			playerRb = nmov.GetComponent<Rigidbody>();
		}
		GameObject gameObject2 = new GameObject();
		target = gameObject2.transform;
		ChangeTargetParent(target, cc.transform);
		if (!pmov && !(player == null) && !(platformerPlayerPrefab == null))
		{
			currentPlatformerPlayerPrefab = Object.Instantiate(platformerPlayerPrefab, player.position, Quaternion.identity);
			pmov = currentPlatformerPlayerPrefab.GetComponentInChildren<PlatformerMovement>(includeInactive: true);
			if ((bool)camera)
			{
				currentPlatformerPlayerPrefab.GetComponentInChildren<Camera>(includeInactive: true).clearFlags = camera.clearFlags;
			}
			currentPlatformerPlayerPrefab.GetComponentInChildren<UnderwaterController>(includeInactive: true).overlay = MonoSingleton<UnderwaterController>.Instance.overlay;
		}
	}

	private void ChangeTargetParent(Transform toMove, Transform newParent, Vector3 offset = default(Vector3))
	{
		toMove.position = newParent.position + offset;
		toMove.SetParent(newParent);
	}

	public void CheckPlayerType()
	{
		if (playerType == PlayerType.FPS && (!MonoSingleton<NewMovement>.Instance || !MonoSingleton<NewMovement>.Instance.gameObject.activeInHierarchy))
		{
			ChangeToFPS();
		}
		else if (playerType == PlayerType.Platformer && (!currentPlatformerPlayerPrefab || !currentPlatformerPlayerPrefab.gameObject.activeInHierarchy))
		{
			ChangeToPlatformer();
		}
	}

	public void LevelStart()
	{
		if (levelStarted)
		{
			return;
		}
		levelStarted = true;
		if (startAsPlatformer)
		{
			ChangeToPlatformer(pmov.freeCamera);
		}
		else
		{
			if (playerType != 0)
			{
				return;
			}
			GameObject[] array = platformerFailSafes;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: false);
				}
			}
		}
	}
}
