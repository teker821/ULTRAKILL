using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class StyleHUD : MonoSingleton<StyleHUD>
{
	public Image rankImage;

	public List<StyleRank> ranks;

	public bool showStyleMeter;

	public bool forceMeterOn;

	private int _rankIndex;

	public int maxReachedRank;

	private Queue<string> hudItemsQueue = new Queue<string>();

	private float currentMeter;

	private GameObject styleHud;

	private Slider styleSlider;

	private Text styleInfo;

	private float rankShaking;

	private Vector3 defaultPos;

	private float rankScale;

	private bool comboActive;

	private StatsManager sman;

	private GunControl gc;

	private float styleNameTime = 0.1f;

	private AudioSource aud;

	[Header("Multipliers")]
	public float bossStyleGainMultiplier = 1.5f;

	public float bossFreshnessDecayMultiplier = 1.5f;

	[Header("Freshness")]
	public bool dualWieldScale;

	public float freshnessDecayPerMove = 0.5f;

	public float freshnessDecayPerSec = 0.25f;

	[Space]
	public float freshnessRegenPerMove = 1f;

	public float freshnessRegenPerSec = 0.5f;

	[Space]
	public List<StyleFreshnessData> freshnessStateData = new List<StyleFreshnessData>();

	private Dictionary<StyleFreshnessState, StyleFreshnessData> freshnessStateDict;

	public Text freshnessSliderText;

	private float freshnessSliderValue;

	private Dictionary<GameObject, float> weaponFreshness = new Dictionary<GameObject, float>();

	private Dictionary<int, (float, float)> slotFreshnessLock = new Dictionary<int, (float, float)>();

	public Dictionary<string, float> freshnessDecayMultiplierDict = new Dictionary<string, float>
	{
		{ "ultrakill.shotgunhit", 0.15f },
		{ "ultrakill.nailhit", 0.1f },
		{ "ultrakill.explosionhit", 0.75f },
		{ "ultrakill.exploded", 1.25f },
		{ "ultrakill.kill", 0.3f },
		{ "ultrakill.firehit", 0f },
		{ "ultrakill.quickdraw", 0f },
		{ "ultrakill.projectileboost", 0f },
		{ "ultrakill.doublekill", 0.1f },
		{ "ultrakill.triplekill", 0.1f },
		{ "ultrakill.multikill", 0.1f },
		{ "ultrakill.arsenal", 0f }
	};

	private Dictionary<string, string> idNameDict = new Dictionary<string, string>
	{
		{ "ultrakill.kill", "KILL" },
		{ "ultrakill.doublekill", "<color=orange>DOUBLE KILL</color>" },
		{ "ultrakill.triplekill", "<color=orange>TRIPLE KILL</color>" },
		{ "ultrakill.bigkill", "BIG KILL" },
		{ "ultrakill.bigfistkill", "BIG FISTKILL" },
		{ "ultrakill.headshot", "HEADSHOT" },
		{ "ultrakill.bigheadshot", "BIG HEADSHOT" },
		{ "ultrakill.headshotcombo", "<color=cyan>HEADSHOT COMBO</color>" },
		{ "ultrakill.criticalpunch", "CRITICAL PUNCH" },
		{ "ultrakill.ricoshot", "<color=cyan>RICOSHOT</color>" },
		{ "ultrakill.limbhit", "LIMB HIT" },
		{ "ultrakill.secret", "<color=cyan>SECRET</color>" },
		{ "ultrakill.cannonballed", "CANNONBALLED" },
		{ "ultrakill.quickdraw", "<color=cyan>QUICKDRAW</color>" },
		{ "ultrakill.interruption", "<color=lime>INTERRUPTION</color>" },
		{ "ultrakill.fistfullofdollar", "<color=cyan>FISTFUL OF DOLLAR</color>" },
		{ "ultrakill.homerun", "HOMERUN" },
		{ "ultrakill.arsenal", "<color=cyan>ARSENAL</color>" },
		{ "ultrakill.catapulted", "<color=cyan>CATAPULTED</color>" },
		{ "ultrakill.splattered", "SPLATTERED" },
		{ "ultrakill.enraged", "<color=red>ENRAGED</color>" },
		{ "ultrakill.instakill", "<color=lime>INSTAKILL</color>" },
		{ "ultrakill.fireworks", "<color=cyan>FIREWORKS</color>" },
		{ "ultrakill.airslam", "<color=cyan>AIR SLAM</color>" },
		{ "ultrakill.airshot", "<color=cyan>AIRSHOT</color>" },
		{ "ultrakill.downtosize", "<color=cyan>DOWN TO SIZE</color>" },
		{ "ultrakill.projectileboost", "<color=lime>PROJECTILE BOOST</color>" },
		{ "ultrakill.parry", "<color=lime>PARRY</color>" },
		{ "ultrakill.chargeback", "CHARGEBACK" },
		{ "ultrakill.disrespect", "DISRESPECT" },
		{ "ultrakill.groundslam", "GROUND SLAM" },
		{ "ultrakill.overkill", "OVERKILL" },
		{ "ultrakill.friendlyfire", "FRIENDLY FIRE" },
		{ "ultrakill.exploded", "EXPLODED" },
		{ "ultrakill.fried", "FRIED" },
		{ "ultrakill.finishedoff", "<color=cyan>FINISHED OFF</color>" },
		{ "ultrakill.halfoff", "<color=cyan>HALF OFF</color>" },
		{ "ultrakill.mauriced", "MAURICED" },
		{ "ultrakill.bipolar", "BIPOLAR" },
		{ "ultrakill.attripator", "<color=cyan>ATTRAPTOR</color>" },
		{ "ultrakill.nailbombed", "NAILBOMBED" },
		{ "ultrakill.nailbombedalive", "<color=grey>NAILBOMBED</color>" },
		{ "ultrakill.multikill", "<color=orange>MULTIKILL</color>" },
		{ "ultrakill.shotgunhit", "" },
		{ "ultrakill.nailhit", "" },
		{ "ultrakill.explosionhit", "" },
		{ "ultrakill.firehit", "" },
		{ "ultrakill.compressed", "COMPRESSED" }
	};

	private Coroutine updateItemsRoutine;

	public StyleRank currentRank => ranks[rankIndex];

	public int rankIndex
	{
		get
		{
			return _rankIndex;
		}
		private set
		{
			_rankIndex = Mathf.Clamp(value, 0, ranks.Count - 1);
			rankImage.sprite = currentRank.sprite;
		}
	}

	private bool freshnessEnabled
	{
		get
		{
			if (!(MonoSingleton<AssistController>.Instance == null))
			{
				if (MonoSingleton<AssistController>.Instance.majorEnabled)
				{
					return !MonoSingleton<AssistController>.Instance.disableWeaponFreshness;
				}
				return true;
			}
			return true;
		}
	}

	public string GetLocalizedName(string id)
	{
		if (!idNameDict.ContainsKey(id))
		{
			return id;
		}
		return idNameDict[id];
	}

	private void Start()
	{
		styleHud = base.transform.GetChild(0).gameObject;
		styleSlider = GetComponentInChildren<Slider>();
		styleInfo = GetComponentInChildren<Text>();
		freshnessStateDict = freshnessStateData.ToDictionary((StyleFreshnessData x) => x.state, (StyleFreshnessData x) => x);
		sman = MonoSingleton<StatsManager>.Instance;
		gc = MonoSingleton<GunControl>.Instance;
		foreach (StyleFreshnessData freshnessStateDatum in freshnessStateData)
		{
			freshnessStateDatum.slider.minValue = freshnessStateDatum.min;
			freshnessStateDatum.slider.maxValue = freshnessStateDatum.max;
		}
		ComboOver();
		defaultPos = rankImage.transform.localPosition;
		aud = GetComponent<AudioSource>();
	}

	private new void OnEnable()
	{
		if (updateItemsRoutine != null)
		{
			StopCoroutine(updateItemsRoutine);
		}
		updateItemsRoutine = StartCoroutine(UpdateItems());
	}

	private void OnDisable()
	{
		if (updateItemsRoutine != null)
		{
			StopCoroutine(updateItemsRoutine);
		}
	}

	private void Update()
	{
		UpdateMeter();
		UpdateFreshness();
		UpdateHUD();
	}

	private IEnumerator UpdateItems()
	{
		while (true)
		{
			if (hudItemsQueue.Count > 0)
			{
				string text = hudItemsQueue.Dequeue();
				styleInfo.text = text + $"{'\n'}{styleInfo.text}";
				aud.Play();
				Invoke("RemoveText", 3f);
				yield return new WaitForSeconds(0.05f);
			}
			else
			{
				yield return null;
			}
		}
	}

	private void UpdateMeter()
	{
		if (currentMeter > 0f && !comboActive)
		{
			ComboStart();
		}
		if (currentMeter < 0f)
		{
			DescendRank();
		}
		else
		{
			currentMeter -= Time.deltaTime * (currentRank.drainSpeed * 15f);
		}
		styleHud.SetActive(comboActive || forceMeterOn);
	}

	private void UpdateFreshness()
	{
		if (!comboActive || !freshnessEnabled || !gc.activated)
		{
			return;
		}
		foreach (GameObject allWeapon in gc.allWeapons)
		{
			if (allWeapon == gc.currentWeapon)
			{
				AddFreshness(allWeapon, (0f - freshnessDecayPerSec) * Time.deltaTime);
				if (slotFreshnessLock.ContainsKey(gc.currentSlot))
				{
					(float, float) tuple = slotFreshnessLock[gc.currentSlot];
					weaponFreshness[allWeapon] = Mathf.Clamp(weaponFreshness[allWeapon], tuple.Item1, tuple.Item2);
				}
			}
			else
			{
				AddFreshness(allWeapon, freshnessRegenPerSec * Time.deltaTime);
			}
		}
	}

	private void UpdateHUD()
	{
		styleSlider.value = currentMeter / (float)currentRank.maxMeter;
		if (freshnessEnabled)
		{
			freshnessSliderText.gameObject.SetActive(value: true);
			foreach (KeyValuePair<StyleFreshnessState, StyleFreshnessData> item in freshnessStateDict)
			{
				Slider slider = item.Value.slider;
				slider.gameObject.SetActive(value: true);
				if (!gc.activated)
				{
					continue;
				}
				if (slider != null && gc != null && gc.allWeapons.Count > 0 && gc.currentWeapon != null)
				{
					if (weaponFreshness.ContainsKey(gc.currentWeapon))
					{
						freshnessSliderValue = Mathf.Lerp(freshnessSliderValue, weaponFreshness[gc.currentWeapon], 30f * Time.deltaTime);
					}
					else
					{
						Debug.LogWarning("Current weapon not in StyleHUD weaponFreshness dict!!!");
					}
				}
				slider.value = freshnessSliderValue;
			}
		}
		else if (freshnessSliderText.gameObject.activeSelf)
		{
			freshnessSliderText.gameObject.SetActive(value: false);
			foreach (StyleFreshnessData freshnessStateDatum in freshnessStateData)
			{
				freshnessStateDatum.slider.gameObject.SetActive(value: false);
			}
		}
		if (styleNameTime > 0f)
		{
			styleNameTime = Mathf.MoveTowards(styleNameTime, 0f, Time.deltaTime * 2f);
		}
		else
		{
			styleNameTime = 0.1f;
		}
		if (rankShaking > 0f)
		{
			rankImage.transform.localPosition = new Vector3(defaultPos.x + rankShaking * (float)Random.Range(-3, 3), defaultPos.y + rankShaking * (float)Random.Range(-3, 3), defaultPos.z);
			rankShaking -= Time.deltaTime * 5f;
		}
		if (rankScale > 0f)
		{
			rankImage.transform.localScale = new Vector3(2f, 1f, 1f) + Vector3.one * rankScale;
			rankScale -= Time.deltaTime;
		}
	}

	public void RegisterStyleItem(string id, string name)
	{
		idNameDict.Add(id, name);
	}

	public void ComboStart()
	{
		CancelInvoke("ResetFreshness");
		currentMeter = Mathf.Max(currentMeter, currentRank.maxMeter / 4);
		comboActive = true;
	}

	public void ComboOver()
	{
		currentMeter = 0f;
		rankIndex = 0;
		Invoke("ResetFreshness", 10f);
		comboActive = false;
	}

	private void AscendRank()
	{
		while (currentMeter >= (float)currentRank.maxMeter)
		{
			currentMeter -= currentRank.maxMeter;
			rankIndex++;
			if (rankIndex + 1 == ranks.Count - 1)
			{
				break;
			}
		}
		currentMeter = Mathf.Max(currentMeter, currentRank.maxMeter / 4);
		maxReachedRank = Mathf.Max(maxReachedRank, rankIndex);
		DiscordController.UpdateRank(rankIndex);
	}

	private void UpdateFreshnessSlider()
	{
		StyleFreshnessState freshnessState = GetFreshnessState(gc.currentWeapon);
		freshnessSliderText.text = freshnessStateDict[freshnessState].text;
	}

	public void ResetFreshness()
	{
		gc = gc ?? MonoSingleton<GunControl>.Instance;
		weaponFreshness.Clear();
		foreach (GameObject allWeapon in gc.allWeapons)
		{
			weaponFreshness.Add(allWeapon, 10f);
		}
	}

	public void SnapFreshnessSlider()
	{
		if (!(gc == null) && !(gc.currentWeapon == null) && weaponFreshness.ContainsKey(gc.currentWeapon))
		{
			freshnessSliderValue = weaponFreshness[gc.currentWeapon];
		}
	}

	public StyleFreshnessState GetFreshnessState(GameObject sourceWeapon)
	{
		StyleFreshnessState result = StyleFreshnessState.Dull;
		foreach (KeyValuePair<StyleFreshnessState, StyleFreshnessData> item in freshnessStateDict)
		{
			if (!weaponFreshness.ContainsKey(sourceWeapon))
			{
				Debug.LogWarning("Current weapon not in StyleHUD weaponFreshness dict!!!");
				result = StyleFreshnessState.Fresh;
			}
			else if (weaponFreshness[sourceWeapon] >= item.Value.min)
			{
				result = item.Key;
			}
		}
		return result;
	}

	public void LockFreshness(int slot, float? min = null, float? max = null)
	{
		if (slotFreshnessLock.ContainsKey(slot))
		{
			(float, float) tuple = slotFreshnessLock[slot];
			Dictionary<int, (float, float)> dictionary = slotFreshnessLock;
			float? num = min;
			float item;
			if (!num.HasValue)
			{
				(item, _) = tuple;
			}
			else
			{
				item = num.GetValueOrDefault();
			}
			dictionary[slot] = (item, max ?? tuple.Item2);
		}
		else
		{
			slotFreshnessLock.Add(slot, (min ?? 0f, max ?? 10f));
		}
	}

	public void LockFreshness(int slot, StyleFreshnessState? minState = null, StyleFreshnessState? maxState = null)
	{
		StyleFreshnessData styleFreshnessData = (maxState.HasValue ? freshnessStateDict[maxState.Value] : null);
		StyleFreshnessData styleFreshnessData2 = (minState.HasValue ? freshnessStateDict[minState.Value] : null);
		float value = 0f;
		float value2 = 10f;
		if (styleFreshnessData2 != null)
		{
			value = styleFreshnessData2.justAboveMin;
		}
		if (styleFreshnessData != null)
		{
			value2 = styleFreshnessData.max - 0.01f;
		}
		LockFreshness(slot, value, value2);
	}

	public void UnlockFreshness(int slot)
	{
		slotFreshnessLock.Remove(slot);
	}

	private void ClampFreshness(GameObject sourceWeapon)
	{
		float num = 0f;
		float max = 10f;
		int num2 = gc.allWeapons.Count;
		if (gc.slot5.Count > 0)
		{
			num2--;
		}
		if (num2 <= 1)
		{
			num = freshnessStateDict[StyleFreshnessState.Fresh].max;
		}
		else
		{
			switch (num2)
			{
			case 2:
				num = freshnessStateDict[StyleFreshnessState.Used].justAboveMin;
				break;
			case 3:
			case 4:
				num = freshnessStateDict[StyleFreshnessState.Stale].justAboveMin;
				break;
			default:
				num = freshnessStateDict[StyleFreshnessState.Dull].justAboveMin;
				break;
			}
		}
		if (sourceWeapon == gc.currentWeapon && slotFreshnessLock.ContainsKey(gc.currentSlot))
		{
			num = Mathf.Max(num, slotFreshnessLock[gc.currentSlot].Item1);
			max = slotFreshnessLock[gc.currentSlot].Item2;
		}
		weaponFreshness[sourceWeapon] = Mathf.Clamp(weaponFreshness[sourceWeapon], num, max);
	}

	public float GetFreshness(GameObject sourceWeapon)
	{
		if (weaponFreshness.TryGetValue(sourceWeapon, out var value))
		{
			return value;
		}
		Debug.LogWarning("Weapon " + sourceWeapon.name + " not in StyleHUD weaponFreshness dict");
		return -1f;
	}

	public void SetFreshness(GameObject sourceWeapon, float amt)
	{
		weaponFreshness[sourceWeapon] = amt;
		ClampFreshness(sourceWeapon);
		if (sourceWeapon == gc?.currentWeapon)
		{
			UpdateFreshnessSlider();
		}
	}

	public void AddFreshness(GameObject sourceWeapon, float amt)
	{
		float num = amt;
		DualWield[] componentsInChildren = MonoSingleton<GunControl>.Instance.GetComponentsInChildren<DualWield>();
		if (dualWieldScale && componentsInChildren.Length != 0)
		{
			num /= (float)(componentsInChildren.Length + 1);
		}
		SetFreshness(sourceWeapon, GetFreshness(sourceWeapon) + num);
	}

	public void DecayFreshness(GameObject sourceWeapon, string pointID, bool boss)
	{
		if (!weaponFreshness.ContainsKey(sourceWeapon))
		{
			Debug.LogWarning("Weapon " + sourceWeapon.name + " not in StyleHUD weaponFreshness dict");
			return;
		}
		float num = freshnessDecayPerMove;
		DualWield[] componentsInChildren = MonoSingleton<GunControl>.Instance.GetComponentsInChildren<DualWield>();
		if (dualWieldScale && componentsInChildren.Length != 0)
		{
			num /= (float)(componentsInChildren.Length + 1);
		}
		if (freshnessDecayMultiplierDict.ContainsKey(pointID))
		{
			num *= freshnessDecayMultiplierDict[pointID];
		}
		if (boss)
		{
			num *= bossFreshnessDecayMultiplier;
		}
		SetFreshness(sourceWeapon, GetFreshness(sourceWeapon) - num);
		foreach (GameObject allWeapon in gc.allWeapons)
		{
			if (allWeapon == sourceWeapon)
			{
				continue;
			}
			bool flag = true;
			foreach (List<GameObject> slot in gc.slots)
			{
				if (slot.Contains(allWeapon) && slot.Contains(sourceWeapon))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				if (freshnessDecayMultiplierDict.TryGetValue(pointID, out var value))
				{
					AddFreshness(allWeapon, freshnessRegenPerMove * value);
				}
				else
				{
					AddFreshness(allWeapon, freshnessRegenPerMove);
				}
			}
		}
		if (sourceWeapon == gc?.currentWeapon)
		{
			UpdateFreshnessSlider();
		}
	}

	public void DescendRank()
	{
		if (comboActive)
		{
			if (rankIndex > 0)
			{
				currentMeter = currentRank.maxMeter;
				rankIndex--;
				rankImage.sprite = ranks[rankIndex].sprite;
				currentMeter = currentRank.maxMeter - currentRank.maxMeter / 4;
			}
			else if (rankIndex == 0)
			{
				ComboOver();
			}
			DiscordController.UpdateRank(rankIndex);
		}
	}

	public void AddPoints(int points, string pointID, GameObject sourceWeapon = null, EnemyIdentifier eid = null, int count = -1, string prefix = "", string postfix = "")
	{
		GameObject gameObject = ((pointID == "ultrakill.arsenal") ? gc.currentWeapon : sourceWeapon);
		bool flag = eid?.GetComponentInChildren<BossIdentifier>() != null;
		if (points > 0)
		{
			float num = points;
			if (freshnessEnabled && gameObject != null)
			{
				StyleFreshnessState freshnessState = GetFreshnessState(gameObject);
				num *= freshnessStateDict[freshnessState].scoreMultiplier;
				DecayFreshness(gameObject, pointID, flag);
			}
			if (flag)
			{
				num *= bossStyleGainMultiplier;
			}
			sman.stylePoints += Mathf.RoundToInt(num);
			currentMeter += num;
			rankScale = 0.2f;
		}
		string localizedName = GetLocalizedName(pointID);
		if (localizedName != "")
		{
			if (count >= 0)
			{
				hudItemsQueue.Enqueue($"+ {prefix}{localizedName}{postfix} x{count}");
			}
			else
			{
				hudItemsQueue.Enqueue("+ " + prefix + localizedName + postfix);
			}
		}
		if (currentMeter >= (float)currentRank.maxMeter && rankIndex < 7)
		{
			AscendRank();
		}
		else if (currentMeter > (float)currentRank.maxMeter)
		{
			currentMeter = currentRank.maxMeter;
		}
	}

	public void RemovePoints(int points)
	{
		rankShaking = 5f;
		currentMeter -= points;
	}

	public void ResetFreshness(GameObject weapon)
	{
		if (weaponFreshness.ContainsKey(weapon))
		{
			weaponFreshness[weapon] = 10f;
		}
	}

	public void ResetAllFreshness()
	{
		foreach (GameObject allWeapon in gc.allWeapons)
		{
			ResetFreshness(allWeapon);
		}
	}

	private void RemoveText()
	{
		styleInfo.text = styleInfo.text.Substring(0, styleInfo.text.LastIndexOf("+"));
	}
}
