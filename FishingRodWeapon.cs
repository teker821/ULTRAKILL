using UnityEngine;

public class FishingRodWeapon : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private FishingRodTarget targetPrefab;

	[SerializeField]
	private FishBait baitPrefab;

	[SerializeField]
	private Transform rodTip;

	[SerializeField]
	private ItemIdentifier fishPickupTemplate;

	public AudioSource pullSound;

	private FishingRodTarget targetingCircle;

	private FishBait spawnedBaitCon;

	private FishingRodState state;

	private float selectedPower;

	private bool climaxed;

	private static readonly int Set = Animator.StringToHash("Set");

	private static readonly int Throw = Animator.StringToHash("Throw");

	private bool baitThrown;

	private float distanceAfterThrow;

	private bool fishHooked;

	private FishDB currentFishPool;

	private Water currentWater;

	private FishDescriptor hookedFishe;

	private static readonly int Pull = Animator.StringToHash("Pull");

	private static readonly int Idle = Animator.StringToHash("Idle");

	private float fishTolerance = 0.5f;

	private float fishDesirePosition = 0.25f;

	private float playerProvidedPosition;

	private float playerPositionVelocity;

	private TimeSince timeSinceBaitInWater;

	private TimeSince timeSinceAction;

	private bool noFishErrorDisplayed;

	public static float suggestedDistanceMulti = 1f;

	public static float minDistanceMulti = 1f;

	private float bottomBound => fishDesirePosition + fishTolerance / 2f;

	private float topBound => fishDesirePosition - fishTolerance / 2f;

	private bool struggleSatisfied
	{
		get
		{
			if (playerProvidedPosition < bottomBound)
			{
				return playerProvidedPosition > topBound;
			}
			return false;
		}
	}

	private Vector3 approximateTargetPosition => MonoSingleton<NewMovement>.Instance.transform.position + (MonoSingleton<NewMovement>.Instance.transform.forward * 10f * minDistanceMulti + MonoSingleton<NewMovement>.Instance.transform.forward * 35f * selectedPower) * suggestedDistanceMulti - Vector3.up * 1.9f;

	public void ThrowBaitEvent()
	{
		if (spawnedBaitCon == null)
		{
			spawnedBaitCon = Object.Instantiate(baitPrefab, rodTip.position, Quaternion.identity, rodTip);
			spawnedBaitCon.landed = false;
			spawnedBaitCon.ThrowStart(targetingCircle.transform.position, rodTip, this);
		}
	}

	private void Awake()
	{
		suggestedDistanceMulti = 1f;
		timeSinceAction = 0f;
	}

	private void OnEnable()
	{
		ResetFishing();
		MonoSingleton<FishingHUD>.Instance.ShowHUD();
	}

	public static GameObject CreateFishPickup(ItemIdentifier template, FishObject fish, bool grab, bool unlock = true)
	{
		if (unlock)
		{
			MonoSingleton<FishManager>.Instance.UnlockFish(fish);
		}
		if (grab)
		{
			if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
			{
				Object.Destroy(MonoSingleton<FistControl>.Instance.heldObject.gameObject);
			}
			MonoSingleton<FistControl>.Instance.currentPunch.ResetHeldState();
		}
		ItemIdentifier itemIdentifier;
		if (fish.customPickup != null)
		{
			itemIdentifier = Object.Instantiate(fish.customPickup);
			if (!itemIdentifier.GetComponent<FishObjectReference>())
			{
				itemIdentifier.gameObject.AddComponent<FishObjectReference>().fishObject = fish;
			}
		}
		else
		{
			itemIdentifier = Object.Instantiate(template);
			itemIdentifier.gameObject.AddComponent<FishObjectReference>().fishObject = fish;
			Transform obj = itemIdentifier.transform.GetChild(0).transform;
			Vector3 localPosition = obj.localPosition;
			Quaternion localRotation = obj.localRotation;
			Vector3 localScale = obj.localScale;
			Object.Destroy(obj.gameObject);
			GameObject obj2 = fish.InstantiateDumb();
			obj2.transform.SetParent(itemIdentifier.transform);
			obj2.transform.localPosition = localPosition;
			obj2.transform.localRotation = localRotation;
			obj2.transform.localScale = localScale;
		}
		if (grab)
		{
			MonoSingleton<FistControl>.Instance.currentPunch.ForceHold(itemIdentifier);
		}
		return itemIdentifier.gameObject;
	}

	public void FishCaughtAndGrabbed()
	{
		animator.SetTrigger(Idle);
		MonoSingleton<FishingHUD>.Instance.ShowFishCaught(show: true, hookedFishe.fish);
		CreateFishPickup(fishPickupTemplate, hookedFishe.fish, grab: true);
		ResetFishing();
	}

	private void ResetFishing()
	{
		state = FishingRodState.ReadyToThrow;
		if ((bool)spawnedBaitCon)
		{
			spawnedBaitCon.Dispose();
			Object.Destroy(spawnedBaitCon.gameObject);
		}
		if ((bool)targetingCircle)
		{
			Object.Destroy(targetingCircle.gameObject);
		}
		selectedPower = 0f;
		climaxed = false;
		baitThrown = false;
		animator.ResetTrigger(Idle);
		animator.ResetTrigger(Throw);
		fishHooked = false;
		timeSinceAction = 0f;
		noFishErrorDisplayed = false;
		MonoSingleton<FishingHUD>.Instance.SetFishHooked(hooked: false);
	}

	private void OnGUI()
	{
	}

	private void Update()
	{
		if (GameStateManager.Instance.PlayerInputLocked || MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo())
		{
			return;
		}
		if ((float)MonoSingleton<FishingHUD>.Instance.timeSinceFishCaught >= 1f && (MonoSingleton<InputManager>.Instance.InputSource.Punch.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame))
		{
			MonoSingleton<FishingHUD>.Instance.ShowFishCaught(show: false);
		}
		MonoSingleton<FishingHUD>.Instance.SetState(state);
		switch (state)
		{
		case FishingRodState.ReadyToThrow:
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && (float)timeSinceAction > 0.1f)
			{
				MonoSingleton<FishingHUD>.Instance.SetPowerMeter(0f, canFish: false);
				selectedPower = 0f;
				climaxed = false;
				fishHooked = false;
				baitThrown = false;
				state = FishingRodState.SelectingPower;
				targetingCircle = Object.Instantiate(targetPrefab, approximateTargetPosition, Quaternion.identity);
				timeSinceAction = 0f;
			}
			break;
		case FishingRodState.SelectingPower:
		{
			selectedPower += (Time.deltaTime * 0.4f + selectedPower * 0.01f) * (climaxed ? (-0.5f) : 1f);
			if (selectedPower > 1f)
			{
				selectedPower = 1f;
				climaxed = true;
			}
			if (selectedPower < 0.1f)
			{
				climaxed = false;
			}
			Vector3 vector = approximateTargetPosition;
			bool flag = false;
			if (Physics.Raycast(vector + Vector3.up * 3f, Vector3.down, out var hitInfo, 30f))
			{
				vector = hitInfo.point;
				if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
				{
					if (hitInfo.collider.TryGetComponent<Water>(out var component) && (bool)component.fishDB)
					{
						currentFishPool = component.fishDB;
						currentWater = component;
						flag = true;
						if ((bool)component.overrideFishingPoint)
						{
							vector = component.overrideFishingPoint.position;
						}
					}
					else
					{
						currentFishPool = null;
						currentWater = null;
						flag = false;
					}
				}
				else
				{
					currentFishPool = null;
					currentWater = null;
					flag = false;
				}
			}
			else
			{
				currentFishPool = null;
				currentWater = null;
				flag = false;
			}
			MonoSingleton<FishingHUD>.Instance.SetPowerMeter(selectedPower, flag);
			if (flag)
			{
				targetingCircle.transform.position = vector + Vector3.up * 0.5f;
				targetingCircle.SetState(isGood: true, Vector3.Distance(hitInfo.point, MonoSingleton<NewMovement>.Instance.transform.position));
				targetingCircle.waterNameText.text = currentFishPool.fullName;
				targetingCircle.waterNameText.color = currentFishPool.symbolColor;
			}
			else
			{
				targetingCircle.transform.position = vector + Vector3.up * 0.5f;
				targetingCircle.SetState(isGood: false, Vector3.Distance(hitInfo.point, MonoSingleton<NewMovement>.Instance.transform.position));
				targetingCircle.waterNameText.text = "";
			}
			targetingCircle.transform.forward = MonoSingleton<NewMovement>.Instance.transform.forward;
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasCanceledThisFrame && (float)timeSinceAction > 0.1f)
			{
				if (flag)
				{
					targetingCircle.GetComponent<Animator>().SetTrigger(Set);
					animator.ResetTrigger(Throw);
					state = FishingRodState.Throwing;
					timeSinceAction = 0f;
				}
				else
				{
					ResetFishing();
				}
			}
			break;
		}
		case FishingRodState.Throwing:
			targetingCircle.transform.forward = MonoSingleton<NewMovement>.Instance.transform.forward;
			fishHooked = false;
			if (!baitThrown)
			{
				baitThrown = true;
				animator.SetTrigger(Throw);
			}
			if ((bool)spawnedBaitCon && spawnedBaitCon.landed)
			{
				state = FishingRodState.WaitingForFish;
				timeSinceBaitInWater = 0f;
				distanceAfterThrow = Vector3.Distance(MonoSingleton<NewMovement>.Instance.transform.position, spawnedBaitCon.baitPoint.position);
				Object.Destroy(targetingCircle.gameObject);
			}
			break;
		case FishingRodState.WaitingForFish:
			baitThrown = false;
			if (Vector3.Distance(MonoSingleton<NewMovement>.Instance.transform.position, spawnedBaitCon.baitPoint.position) > distanceAfterThrow + 30f)
			{
				Object.Destroy(spawnedBaitCon.gameObject);
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Fishing interrupted");
				ResetFishing();
				break;
			}
			if (!fishHooked && Random.value < 0.002f + (float)timeSinceBaitInWater * 0.01f)
			{
				hookedFishe = currentFishPool.GetRandomFish(currentWater.attractFish);
				if (hookedFishe == null)
				{
					if (!noFishErrorDisplayed)
					{
						noFishErrorDisplayed = true;
						MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Nothing seems to be biting here...");
					}
					break;
				}
				currentWater.attractFish = null;
				fishHooked = true;
				MonoSingleton<FishingHUD>.Instance.SetFishHooked(hooked: true);
				spawnedBaitCon.FishHooked();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed || MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed)
			{
				animator.SetTrigger(Pull);
				if (fishHooked)
				{
					MonoSingleton<FishingHUD>.Instance.SetFishHooked(hooked: false);
					state = FishingRodState.FishStruggle;
					spawnedBaitCon.CatchFish(hookedFishe.fish);
				}
				else
				{
					Object.Destroy(spawnedBaitCon.gameObject);
					animator.SetTrigger(Idle);
					animator.ResetTrigger(Throw);
					animator.Play(Idle);
					ResetFishing();
				}
			}
			break;
		case FishingRodState.FishStruggle:
			fishDesirePosition = Mathf.PerlinNoise(Time.time * 0.3f, 0f);
			fishTolerance = 0.1f + 0.4f * Mathf.PerlinNoise(Time.time * 0.4f, 0f);
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
			{
				playerPositionVelocity += 1.9f * Time.deltaTime;
				animator.SetTrigger(Pull);
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed)
			{
				playerPositionVelocity -= 1.9f * Time.deltaTime;
				animator.SetTrigger(Pull);
			}
			else
			{
				playerPositionVelocity *= 1f - 2f * Time.deltaTime;
			}
			playerProvidedPosition += playerPositionVelocity * Time.deltaTime;
			if (playerProvidedPosition > 1f)
			{
				playerProvidedPosition = 1f;
				playerPositionVelocity = 0f - playerPositionVelocity;
			}
			if (playerProvidedPosition < 0f)
			{
				playerProvidedPosition = 0f;
				playerPositionVelocity = 0f - playerPositionVelocity;
			}
			MonoSingleton<FishingHUD>.Instance.SetPlayerStrugglePosition(playerProvidedPosition);
			MonoSingleton<FishingHUD>.Instance.SetStruggleSatisfied(struggleSatisfied);
			MonoSingleton<FishingHUD>.Instance.SetFishDesire(Mathf.Clamp01(topBound), Mathf.Clamp01(bottomBound));
			spawnedBaitCon.allowedToProgress = struggleSatisfied;
			MonoSingleton<FishingHUD>.Instance.SetStruggleProgress(spawnedBaitCon.flyProgress, hookedFishe.fish.blockedIcon, hookedFishe.fish.icon);
			break;
		}
	}
}
