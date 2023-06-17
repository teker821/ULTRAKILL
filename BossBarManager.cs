using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class BossBarManager : MonoSingleton<BossBarManager>
{
	[SerializeField]
	private float overflowShrinkFactor = 0.14f;

	[SerializeField]
	private float minimumSize = 0.3f;

	[SerializeField]
	private float baseOverflowedSize = 0.82f;

	[Space]
	[SerializeField]
	private RectTransform containerRect;

	[SerializeField]
	private BossHealthBarTemplate template;

	[SerializeField]
	private SliderLayer[] layers;

	private List<BossHealthBarTemplate> createdBossBars = new List<BossHealthBarTemplate>();

	private List<BossHealthBarTemplate> activeBossBars => createdBossBars.Where((BossHealthBarTemplate x) => x.gameObject.activeSelf).ToList();

	public void CreateBossBar(string bossName, HealthLayer[] healthLayers, ref BossHealthBarTemplate createdBossBar, ref Slider[] hpSliders, ref Slider[] hpAfterImages, ref GameObject bossBar)
	{
		Debug.Log("Creating boss bar for " + bossName);
		List<Slider> list = new List<Slider>();
		List<Slider> list2 = new List<Slider>();
		createdBossBar = UnityEngine.Object.Instantiate(template, template.transform.parent, worldPositionStays: true);
		bossBar = createdBossBar.gameObject;
		bossBar.SetActive(value: true);
		createdBossBar.bossNameText.text = bossName.ToUpper();
		float num = 0f;
		for (int i = 0; i < healthLayers.Length; i++)
		{
			BossHealthSliderTemplate bossHealthSliderTemplate = UnityEngine.Object.Instantiate(createdBossBar.sliderTemplate, createdBossBar.sliderTemplate.transform.parent);
			bossHealthSliderTemplate.name = "Health After Image " + bossName;
			list2.Add(bossHealthSliderTemplate.slider);
			bossHealthSliderTemplate.slider.minValue = num;
			bossHealthSliderTemplate.slider.maxValue = num + healthLayers[i].health;
			bossHealthSliderTemplate.gameObject.SetActive(value: true);
			bossHealthSliderTemplate.background.SetActive(i == 0);
			bossHealthSliderTemplate.fill.color = layers[i].afterImageColor;
			BossHealthSliderTemplate bossHealthSliderTemplate2 = UnityEngine.Object.Instantiate(createdBossBar.sliderTemplate, createdBossBar.sliderTemplate.transform.parent);
			bossHealthSliderTemplate2.name = "Health Slider " + bossName;
			list.Add(bossHealthSliderTemplate2.slider);
			bossHealthSliderTemplate2.slider.minValue = num;
			bossHealthSliderTemplate2.slider.maxValue = num + healthLayers[i].health;
			bossHealthSliderTemplate2.gameObject.SetActive(value: true);
			bossHealthSliderTemplate2.background.SetActive(value: false);
			bossHealthSliderTemplate2.fill.color = layers[i].color;
			num += healthLayers[i].health;
		}
		hpSliders = list.ToArray();
		hpAfterImages = list2.ToArray();
		template.sliderTemplate.gameObject.SetActive(value: false);
		template.gameObject.SetActive(value: false);
		bossBar.SetActive(value: false);
		createdBossBars.Add(createdBossBar);
		BossHealthBarTemplate obj = createdBossBar;
		obj.onDestroy = (Action)Delegate.Combine(obj.onDestroy, (Action)delegate
		{
			if (!(base.gameObject == null) && base.gameObject.activeInHierarchy)
			{
				MonoSingleton<BossBarManager>.Instance.StartCoroutine(DelayCall(RecalculateStretch));
			}
		});
		createdBossBar.Initialize();
		RecalculateStretch();
		StartCoroutine(DelayCall(delegate
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(template.transform.parent.GetComponent<RectTransform>());
		}));
	}

	private IEnumerator DelayCall(Action call)
	{
		yield return new WaitForFixedUpdate();
		call();
	}

	public void CreateThinBar(BossHealthSliderTemplate thinTarget, ref Slider slider, ref GameObject thinBar)
	{
		BossHealthSliderTemplate bossHealthSliderTemplate = UnityEngine.Object.Instantiate(thinTarget, thinTarget.transform.parent);
		thinBar = bossHealthSliderTemplate.gameObject;
		slider = bossHealthSliderTemplate.slider;
	}

	private void RecalculateStretch()
	{
		CleanUp();
		float b = 1f;
		if (activeBossBars.Count > 2)
		{
			b = baseOverflowedSize - (float)(activeBossBars.Count - 2) * overflowShrinkFactor;
		}
		b = Mathf.Max(minimumSize, b);
		containerRect.localScale = new Vector3(1f, b, 1f);
		foreach (BossHealthBarTemplate activeBossBar in activeBossBars)
		{
			activeBossBar.ScaleChanged(b);
		}
	}

	private void CleanUp()
	{
		List<int> toRemove = new List<int>();
		for (int i = 0; i < createdBossBars.Count; i++)
		{
			if (createdBossBars[i].Equals(null))
			{
				toRemove.Add(i);
			}
		}
		createdBossBars = createdBossBars.Where((BossHealthBarTemplate source, int index) => !toRemove.Contains(index)).ToList();
	}
}
