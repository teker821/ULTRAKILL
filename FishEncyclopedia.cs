using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishEncyclopedia : MonoBehaviour
{
	[SerializeField]
	private GameObject fishPicker;

	[SerializeField]
	private GameObject fishInfoContainer;

	[SerializeField]
	private Text fishName;

	[SerializeField]
	private Text fishDescription;

	[Space]
	[SerializeField]
	private Transform fishGrid;

	[SerializeField]
	private FishMenuButton fishButtonTemplate;

	[SerializeField]
	private GameObject fish3dRenderContainer;

	[Space]
	[SerializeField]
	private FishDB[] fishDbs;

	private Dictionary<FishObject, FishMenuButton> fishButtons = new Dictionary<FishObject, FishMenuButton>();

	private void Start()
	{
		fishButtonTemplate.gameObject.SetActive(value: false);
		foreach (KeyValuePair<FishObject, bool> recognizedFish in MonoSingleton<FishManager>.Instance.recognizedFishes)
		{
			FishObject fish = recognizedFish.Key;
			bool value = recognizedFish.Value;
			if (!fishButtons.ContainsKey(fish))
			{
				FishMenuButton fishMenuButton = UnityEngine.Object.Instantiate(fishButtonTemplate, fishGrid, worldPositionStays: false);
				fishButtons.Add(fish, fishMenuButton);
				fishMenuButton.gameObject.SetActive(value: true);
				fishMenuButton.Populate(fish, !value);
				fishMenuButton.GetComponent<ControllerPointer>().OnPressed.RemoveAllListeners();
				fishMenuButton.GetComponent<ControllerPointer>().OnPressed.AddListener(delegate
				{
					SelectFish(fish);
				});
			}
		}
		FishManager instance = MonoSingleton<FishManager>.Instance;
		instance.onFishUnlocked = (Action<FishObject>)Delegate.Combine(instance.onFishUnlocked, new Action<FishObject>(OnFishUnlocked));
	}

	private void OnFishUnlocked(FishObject obj)
	{
		if (fishButtons.ContainsKey(obj))
		{
			fishButtons[obj].Populate(obj, locked: false);
		}
	}

	private void DisplayFish(FishObject fish)
	{
		foreach (Transform item in fish3dRenderContainer.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (MonoSingleton<FishManager>.Instance.recognizedFishes[fish])
		{
			GameObject obj = fish.InstantiateDumb();
			obj.transform.SetParent(fish3dRenderContainer.transform);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			SandboxUtils.SetLayerDeep(obj.transform, LayerMask.NameToLayer("VirtualRender"));
		}
	}

	public void SelectFish(FishObject fish)
	{
		fishName.text = (MonoSingleton<FishManager>.Instance.recognizedFishes[fish] ? fish.fishName : "???");
		fishDescription.text = fish.description;
		fishPicker.SetActive(value: false);
		fishInfoContainer.SetActive(value: true);
		DisplayFish(fish);
	}

	public void HideFishInfo()
	{
		fishPicker.SetActive(value: true);
		fishInfoContainer.SetActive(value: false);
	}
}
