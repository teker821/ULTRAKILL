using UnityEngine;
using UnityEngine.EventSystems;

public class ConsoleWindowCorner : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private ConsoleWindow consoleWindow;

	[SerializeField]
	private GameObject feedbackIcon;

	[SerializeField]
	private Vector2Int corner = new Vector2Int(0, 0);

	private bool dragging;

	private bool hovering;

	public void OnPointerDown(PointerEventData eventData)
	{
		consoleWindow.StartResize(eventData, corner);
		dragging = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		consoleWindow.StopResize(eventData, corner);
		dragging = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovering = false;
	}

	private void Update()
	{
		feedbackIcon.SetActive(dragging || hovering);
	}
}
