using System;
using UnityEngine;

public class FishBait : MonoBehaviour
{
	public Transform baitPoint;

	[SerializeField]
	private LineRenderer lineRenderer;

	[SerializeField]
	private GameObject splashPrefab;

	[SerializeField]
	private GameObject fishHookedPrefab;

	private GameObject fishHookedSpawned;

	private Transform initialParent;

	private Vector3 landTarget;

	private FishingRodWeapon sourceWeapon;

	public bool landed = true;

	public float flyProgress;

	private float fishPullVelocity;

	private float overrideLastMile = -1f;

	public bool allowedToProgress;

	private bool returnToRod;

	private bool outOfWater;

	private Transform spawnedFish;

	private void Update()
	{
		if (landed && !returnToRod)
		{
			UpdateLineRenderer();
			return;
		}
		if (returnToRod)
		{
			if (allowedToProgress || outOfWater)
			{
				fishPullVelocity += 0.3f * Time.deltaTime;
			}
			else
			{
				fishPullVelocity *= 1f - 0.4f * Time.deltaTime;
			}
			if (fishPullVelocity > 1f)
			{
				fishPullVelocity = 1f;
			}
			if (fishPullVelocity < 0f)
			{
				fishPullVelocity = 0f;
			}
			flyProgress += Time.deltaTime * 0.9f * fishPullVelocity;
			if (fishPullVelocity > 0.1f)
			{
				if (!sourceWeapon.pullSound.isPlaying)
				{
					sourceWeapon.pullSound.Play();
				}
				sourceWeapon.pullSound.pitch = Mathf.Abs(0.7f + fishPullVelocity * 2f);
			}
			else
			{
				sourceWeapon.pullSound.Stop();
			}
			ReturnAnim();
		}
		else
		{
			flyProgress += Time.deltaTime;
			ThrowAnim();
		}
		UpdateLineRenderer();
	}

	private void ThrowAnim()
	{
		if (flyProgress >= 1f)
		{
			flyProgress = 1f;
			UnityEngine.Object.Instantiate(splashPrefab, baitPoint.position + Vector3.down * 0.3f, Quaternion.Euler(-90f, 0f, 0f));
			landed = true;
		}
		float num = Mathf.Sin(flyProgress * (float)Math.PI) * 20f;
		baitPoint.position = Vector3.Lerp(baitPoint.position, landTarget, flyProgress);
		baitPoint.position = new Vector3(baitPoint.position.x, baitPoint.position.y + num, baitPoint.position.z);
	}

	private void ReturnAnim()
	{
		if (flyProgress >= 1f)
		{
			flyProgress = 1f;
			sourceWeapon.FishCaughtAndGrabbed();
			sourceWeapon.pullSound.Stop();
			UnityEngine.Object.Destroy(spawnedFish.gameObject);
			MonoSingleton<LeaderboardController>.Instance.SubmitFishSize(SteamController.FishSizeMulti);
		}
		else
		{
			Vector3 forward = initialParent.position - baitPoint.position;
			spawnedFish.rotation = Quaternion.LookRotation(forward);
			float num = ((overrideLastMile > 0f) ? overrideLastMile : 0.95f);
			float t = (flyProgress - num) / (1f - num);
			float a = landTarget.y - 1f;
			baitPoint.position = Vector3.Lerp(landTarget, initialParent.position, flyProgress);
			RaycastHit hitInfo;
			bool flag = Physics.Raycast(baitPoint.position + Vector3.up * 2f, Vector3.down, out hitInfo, 10f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies));
			baitPoint.position = new Vector3(baitPoint.position.x, Mathf.Max(Mathf.Lerp(a, initialParent.position.y, t), flag ? hitInfo.point.y : float.NegativeInfinity), baitPoint.position.z);
		}
	}

	private void UpdateLineRenderer()
	{
		Camera cam = MonoSingleton<CameraController>.Instance.cam;
		Vector3 position = MonoSingleton<PostProcessV2_Handler>.Instance.hudCam.WorldToScreenPoint(base.transform.position);
		position = cam.ScreenToWorldPoint(position);
		position = lineRenderer.transform.InverseTransformPoint(position);
		lineRenderer.SetPosition(0, position);
		Vector3 position2 = baitPoint.position;
		Vector3 position3 = lineRenderer.transform.InverseTransformPoint(position2);
		lineRenderer.SetPosition(1, position3);
	}

	public void ThrowStart(Vector3 targetWorldPosition, Transform inPar, FishingRodWeapon srcWpn)
	{
		flyProgress = 0f;
		landTarget = targetWorldPosition;
		initialParent = inPar;
		sourceWeapon = srcWpn;
		baitPoint.SetParent(null);
	}

	public void FishHooked()
	{
		fishHookedSpawned = UnityEngine.Object.Instantiate(fishHookedPrefab, baitPoint.position + Vector3.up * 3f, Quaternion.identity);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(baitPoint.gameObject);
		if ((bool)fishHookedSpawned)
		{
			UnityEngine.Object.Destroy(fishHookedSpawned);
		}
	}

	public void CatchFish(FishObject fish)
	{
		if (!returnToRod)
		{
			returnToRod = true;
			UnityEngine.Object.Destroy(fishHookedSpawned);
			flyProgress = 0f;
			spawnedFish = fish.InstantiateWorld(baitPoint.position).transform;
			spawnedFish.SetParent(baitPoint);
		}
	}

	public void OutOfWater()
	{
		if (returnToRod && !outOfWater)
		{
			outOfWater = true;
			overrideLastMile = flyProgress;
			MonoSingleton<FishingHUD>.Instance.ShowOutOfWater();
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (returnToRod && other.gameObject.layer == 4)
		{
			if (Physics.Raycast(baitPoint.position, Vector3.down, out var hitInfo, 6f) && hitInfo.collider.gameObject.layer == 4)
			{
				Debug.Log("We're above water, ignore trigger exit");
				return;
			}
			Debug.Log("out of water since trigger exit");
			OutOfWater();
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (returnToRod)
		{
			LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
			if ((int)layerMask == ((int)layerMask | (1 << collision.gameObject.layer)))
			{
				Debug.LogError("touched env!!!");
				OutOfWater();
			}
		}
	}
}
