using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SaveSlotMenu : MonoBehaviour
{
	public class SlotData
	{
		public bool exists;

		public int highestLvlNumber;

		public int highestDifficulty;

		public override string ToString()
		{
			if (!exists)
			{
				return "EMPTY";
			}
			return GetMissionName.GetMission(highestLvlNumber) + " " + ((highestLvlNumber <= 0) ? string.Empty : ("(" + MonoSingleton<PresenceController>.Instance.diffNames[highestDifficulty] + ")"));
		}
	}

	public const int Slots = 5;

	private static readonly Color ActiveColor = new Color(1f, 0.66f, 0f);

	[SerializeField]
	private SlotRowPanel templateRow;

	[SerializeField]
	private Button closeButton;

	[FormerlySerializedAs("consentWrapper")]
	[SerializeField]
	private GameObject reloadConsentWrapper;

	[SerializeField]
	private Text wipeConsentContent;

	[SerializeField]
	private GameObject wipeConsentWrapper;

	private int queuedSlot;

	private SlotRowPanel[] slots;

	private void OnEnable()
	{
		if (slots != null)
		{
			return;
		}
		List<SlotRowPanel> list = new List<SlotRowPanel>();
		SlotData[] array = GameProgressSaver.GetSlots();
		for (int i = 0; i < 5; i++)
		{
			SlotRowPanel newRow = Object.Instantiate(templateRow, templateRow.transform.parent);
			newRow.slotIndex = i;
			newRow.gameObject.SetActive(value: true);
			UpdateSlotState(newRow, array[i]);
			newRow.selectButton.onClick.AddListener(delegate
			{
				SelectSlot(newRow.slotIndex);
			});
			newRow.deleteButton.onClick.AddListener(delegate
			{
				ClearSlot(newRow.slotIndex);
			});
			list.Add(newRow);
		}
		for (int j = 0; j < 5; j++)
		{
			list[j].selectButton.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = ((j > 0) ? list[j - 1].selectButton : null),
				selectOnDown = ((j + 1 < 5) ? list[j + 1].selectButton : closeButton),
				selectOnLeft = list[j].deleteButton,
				selectOnRight = list[j].deleteButton
			};
			list[j].deleteButton.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = ((j > 0) ? list[j - 1].deleteButton : null),
				selectOnDown = ((j + 1 < 5) ? list[j + 1].deleteButton : closeButton),
				selectOnLeft = list[j].selectButton,
				selectOnRight = list[j].selectButton
			};
		}
		closeButton.navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = list[4].selectButton,
			selectOnDown = list[0].selectButton
		};
		slots = list.ToArray();
		templateRow.gameObject.SetActive(value: false);
	}

	private void UpdateSlotState(SlotRowPanel targetPanel, SlotData data)
	{
		bool flag = GameProgressSaver.currentSlot == targetPanel.slotIndex;
		targetPanel.backgroundPanel.color = (flag ? ActiveColor : Color.black);
		targetPanel.slotNumberLabel.color = (flag ? Color.black : (data.exists ? Color.white : Color.red));
		targetPanel.stateLabel.color = (flag ? Color.black : (data.exists ? Color.white : Color.red));
		targetPanel.selectButton.interactable = !flag;
		targetPanel.selectButton.GetComponentInChildren<Text>().text = (flag ? "SELECTED" : "SELECT");
		targetPanel.deleteButton.interactable = data.exists;
		targetPanel.slotNumberLabel.text = $"Slot {targetPanel.slotIndex + 1}";
		targetPanel.stateLabel.text = data.ToString();
	}

	public void ConfirmReload()
	{
		GameProgressSaver.SetSlot(queuedSlot);
		SceneHelper.LoadScene("Main Menu");
	}

	public void ConfirmWipe()
	{
		GameProgressSaver.WipeSlot(queuedSlot);
		SceneHelper.LoadScene("Main Menu");
	}

	public void CancelConsent()
	{
		reloadConsentWrapper.SetActive(value: false);
		wipeConsentWrapper.SetActive(value: false);
	}

	private void SelectSlot(int slot)
	{
		queuedSlot = slot;
		reloadConsentWrapper.SetActive(value: true);
	}

	private void ClearSlot(int slot)
	{
		queuedSlot = slot;
		wipeConsentContent.text = $"Are you sure you want to <color=red>DELETE SLOT {slot + 1}</color>?";
		wipeConsentWrapper.SetActive(value: true);
	}
}
