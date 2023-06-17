using UnityEngine;
using UnityEngine.EventSystems;

public class ConsoleTitleBar : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	[SerializeField]
	private ConsoleWindow consoleWindow;

	public void OnPointerDown(PointerEventData eventData)
	{
		consoleWindow.StartDrag(eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		consoleWindow.EndDrag(eventData);
	}
}
