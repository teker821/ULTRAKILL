using UnityEngine;
using UnityEngine.EventSystems;

public class MenuChallengeHover : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private GameObject panelToActivate;

	public void OnPointerEnter(PointerEventData eventData)
	{
		panelToActivate.SetActive(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		panelToActivate.SetActive(value: false);
	}
}
