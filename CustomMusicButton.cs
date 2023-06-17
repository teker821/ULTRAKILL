using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomMusicButton : CustomContentButton, IDragHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private RectTransform canvasTransform;

	[SerializeField]
	private ControllerPointer pointer;

	[SerializeField]
	private UnityEvent<GameObject, Vector3> onDrag;

	[SerializeField]
	private UnityEvent<GameObject, Vector3> onDrop;

	private Vector3? dragPoint;

	public void OnPointerDown(PointerEventData eventData)
	{
		dragPoint = GetScreenPositionInCanvasSpace(eventData.position);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		dragPoint = null;
		onDrop?.Invoke(base.gameObject, GetScreenPositionInCanvasSpace(eventData.position));
	}

	private void Update()
	{
	}

	private Vector2 GetScreenPositionInCanvasSpace(Vector2 screenPos)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, screenPos, MonoSingleton<CameraController>.Instance.cam, out var localPoint);
		return localPoint;
	}

	public void OnDrag(PointerEventData eventData)
	{
	}
}
