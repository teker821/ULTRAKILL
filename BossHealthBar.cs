using System;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
	public HealthLayer[] healthLayers;

	private GameObject bossBar;

	private Slider[] hpSlider;

	private Slider[] hpAfterImage;

	private EnemyIdentifier eid;

	private Color[] hpColors;

	public string bossName;

	public bool secondaryBar;

	[SerializeField]
	private Color secondaryColor = Color.white;

	private Slider secondarySlider;

	private GameObject secondaryObject;

	private float introCharge;

	private GameObject filler;

	private float shakeTime;

	private Vector3 originalPosition;

	private bool done;

	private int currentHpSlider;

	private int currentAfterImageSlider;

	private MusicManager mman;

	private float waitForDamage;

	private float[] healFadeLerps;

	private void Awake()
	{
		eid = GetComponent<EnemyIdentifier>();
		eid.ForceGetHealth();
		if (healthLayers == null)
		{
			healthLayers = Array.Empty<HealthLayer>();
		}
		if (healthLayers.Length == 0)
		{
			healthLayers = new HealthLayer[1];
			healthLayers[0] = new HealthLayer();
			healthLayers[0].health = eid.health;
		}
		if (string.IsNullOrEmpty(bossName))
		{
			bossName = eid.fullName;
		}
		BossHealthBarTemplate createdBossBar = null;
		MonoSingleton<BossBarManager>.Instance.CreateBossBar(bossName, healthLayers, ref createdBossBar, ref hpSlider, ref hpAfterImage, ref bossBar);
		filler = createdBossBar.sliderTemplate.filler;
		originalPosition = filler.transform.localPosition;
		for (int num = hpSlider.Length - 1; num >= 0; num--)
		{
			if (eid.health > hpSlider[num].minValue)
			{
				currentHpSlider = num;
				currentAfterImageSlider = currentHpSlider;
				break;
			}
		}
		Slider[] array = hpSlider;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].value = 0f;
		}
		array = hpAfterImage;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].value = 0f;
		}
		hpColors = new Color[hpSlider.Length];
		healFadeLerps = new float[hpSlider.Length];
		for (int j = 0; j < hpColors.Length; j++)
		{
			hpColors[j] = hpSlider[j].targetGraphic.color;
		}
		if (secondaryBar)
		{
			Debug.Log(createdBossBar);
			MonoSingleton<BossBarManager>.Instance.CreateThinBar(createdBossBar.thinSliderTemplate, ref secondarySlider, ref secondaryObject);
			secondarySlider.targetGraphic.color = secondaryColor;
			secondaryObject.SetActive(value: true);
		}
		if (mman == null)
		{
			mman = MonoSingleton<MusicManager>.Instance;
		}
	}

	private void OnEnable()
	{
		if (!eid.dead)
		{
			if (mman == null)
			{
				mman = MonoSingleton<MusicManager>.Instance;
			}
			mman.PlayBossMusic();
			bossBar.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		DisappearBar();
	}

	public void UpdateSecondaryBar(float value)
	{
		if (secondaryBar && (bool)secondaryObject)
		{
			secondarySlider.value = value;
		}
	}

	public void SetSecondaryBarColor(Color clr)
	{
		if (secondaryBar && (bool)secondaryObject && !(secondaryColor == clr))
		{
			secondaryColor = clr;
			secondarySlider.targetGraphic.color = clr;
		}
	}

	private void Update()
	{
		if (hpSlider[currentHpSlider].value != eid.health)
		{
			if (introCharge < eid.health)
			{
				introCharge = Mathf.MoveTowards(introCharge, eid.health, (eid.health - introCharge) * Time.deltaTime * 3f);
				Slider[] array = hpSlider;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].value = introCharge;
				}
			}
			else
			{
				if (hpSlider[currentHpSlider].value < eid.health)
				{
					hpSlider[currentHpSlider].targetGraphic.color = Color.green;
					healFadeLerps[currentHpSlider] = 0f;
				}
				shakeTime = 5f * (hpSlider[currentHpSlider].value - eid.health);
				hpSlider[currentHpSlider].value = eid.health;
				waitForDamage = 0.15f;
				if (hpSlider[currentHpSlider].minValue > eid.health && currentHpSlider > 0)
				{
					currentHpSlider--;
					hpSlider[currentHpSlider].value = eid.health;
				}
				else if (hpSlider[currentHpSlider].maxValue < eid.health && currentHpSlider < hpSlider.Length - 1)
				{
					hpSlider[currentHpSlider].value = hpSlider[currentHpSlider].value;
					currentHpSlider++;
				}
			}
		}
		if (hpAfterImage[currentAfterImageSlider].value != hpSlider[currentHpSlider].value)
		{
			if (waitForDamage > 0f && hpSlider[0].value > 0f)
			{
				waitForDamage = Mathf.MoveTowards(waitForDamage, 0f, Time.deltaTime);
			}
			else if (hpAfterImage[currentAfterImageSlider].value > hpSlider[currentHpSlider].value)
			{
				hpAfterImage[currentAfterImageSlider].value = Mathf.MoveTowards(hpAfterImage[currentAfterImageSlider].value, hpSlider[currentHpSlider].value, Time.deltaTime * (Mathf.Abs((hpAfterImage[currentAfterImageSlider].value - hpSlider[currentHpSlider].value) * 5f) + 0.5f));
			}
			else
			{
				hpAfterImage[currentAfterImageSlider].value = hpSlider[currentHpSlider].value;
			}
			if (hpAfterImage[currentAfterImageSlider].value <= hpAfterImage[currentAfterImageSlider].minValue && currentAfterImageSlider > 0)
			{
				currentAfterImageSlider--;
			}
		}
		for (int j = 0; j < hpColors.Length; j++)
		{
			if (hpSlider[j].targetGraphic.color != hpColors[j])
			{
				healFadeLerps[j] = Mathf.MoveTowards(healFadeLerps[j], 1f, Time.deltaTime * 2f);
				hpSlider[j].targetGraphic.color = Color.Lerp(Color.green, hpColors[j], healFadeLerps[j]);
			}
		}
		if (shakeTime != 0f)
		{
			if (shakeTime > 10f)
			{
				shakeTime = 10f;
			}
			shakeTime = Mathf.MoveTowards(shakeTime, 0f, Time.deltaTime * 10f);
			if (shakeTime <= 0f)
			{
				shakeTime = 0f;
				filler.transform.localPosition = originalPosition;
			}
			else
			{
				filler.transform.localPosition = new Vector3(originalPosition.x + UnityEngine.Random.Range(0f - shakeTime, shakeTime), originalPosition.y + UnityEngine.Random.Range(0f - shakeTime, shakeTime), originalPosition.z);
			}
		}
		if (!done && hpSlider[0].value <= 0f && eid.dead)
		{
			done = true;
			Invoke("DestroyBar", 3f);
		}
	}

	public void DestroyBar()
	{
		if ((bool)bossBar)
		{
			UnityEngine.Object.Destroy(bossBar);
		}
		if (secondaryBar)
		{
			UnityEngine.Object.Destroy(secondaryObject);
		}
		base.enabled = false;
	}

	public void DisappearBar()
	{
		DestroyBar();
	}
}
