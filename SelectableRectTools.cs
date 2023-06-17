using UnityEngine;
using UnityEngine.UI;

public class SelectableRectTools : MonoBehaviour
{
	[SerializeField]
	private Selectable target;

	[SerializeField]
	private bool autoSwitchForDown;

	[SerializeField]
	private bool autoSwitchForUp;

	[SerializeField]
	private Selectable[] prioritySwitch;

	private void Awake()
	{
		if (target == null)
		{
			target = GetComponent<Selectable>();
		}
	}

	private void OnEnable()
	{
		Selectable[] array;
		if (autoSwitchForDown)
		{
			array = prioritySwitch;
			foreach (Selectable selectable in array)
			{
				if (selectable.gameObject.activeSelf && selectable.IsInteractable() && selectable.enabled)
				{
					ChangeSelectOnDown(selectable);
					break;
				}
			}
		}
		if (!autoSwitchForUp)
		{
			return;
		}
		array = prioritySwitch;
		foreach (Selectable selectable2 in array)
		{
			if (selectable2.gameObject.activeSelf && selectable2.IsInteractable() && selectable2.enabled)
			{
				ChangeSelectOnUp(selectable2);
				break;
			}
		}
	}

	public void ChangeSelectOnUp(Selectable newElement)
	{
		Navigation navigation = target.navigation;
		navigation.selectOnUp = newElement;
		target.navigation = navigation;
	}

	public void ChangeSelectOnDown(Selectable newElement)
	{
		Navigation navigation = target.navigation;
		navigation.selectOnDown = newElement;
		target.navigation = navigation;
	}
}
