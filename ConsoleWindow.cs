using UnityEngine;
using UnityEngine.EventSystems;

public class ConsoleWindow : MonoBehaviour
{
	private Vector2 minSize = new Vector2(520f, 480f);

	private Vector2 defaultSize;

	private RectTransform selfFrame;

	private bool isDragging;

	private Vector2 dragOffset;

	private bool isResizing;

	private Vector2 resizeCursorStart;

	private Vector2Int lastResolution;

	private Vector2 position
	{
		get
		{
			return selfFrame.anchoredPosition;
		}
		set
		{
			selfFrame.anchoredPosition = value;
		}
	}

	private Vector2 size
	{
		get
		{
			return selfFrame.sizeDelta;
		}
		set
		{
			selfFrame.sizeDelta = value;
		}
	}

	private void Awake()
	{
		selfFrame = GetComponent<RectTransform>();
		defaultSize = size;
		lastResolution = new Vector2Int(Screen.width, Screen.height);
		ResetWindow();
	}

	public void ResetWindow()
	{
		size = new Vector2(Mathf.Min(defaultSize.x, Screen.width), Mathf.Min(defaultSize.y, Screen.height));
		position = new Vector2((float)Screen.width / 2f - size.x / 2f, (float)(-Screen.height) / 2f + size.y / 2f);
	}

	private void Update()
	{
		if (isResizing)
		{
			Vector2 vector = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			Vector2 vector2 = vector - resizeCursorStart;
			vector2.y = 0f - vector2.y;
			size = new Vector2(Mathf.Max(minSize.x, size.x + vector2.x), Mathf.Max(minSize.y, size.y + vector2.y));
			if (position.x + size.x > (float)Screen.width)
			{
				size = new Vector2((float)Screen.width - position.x, size.y);
			}
			if (size.y - position.y > (float)Screen.height)
			{
				size = new Vector2(size.x, (float)Screen.height + position.y);
			}
			resizeCursorStart = vector;
		}
		if (isDragging)
		{
			Vector2 vector3 = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			Vector2 vector4 = vector3 - dragOffset;
			if (position.x + vector4.x < 0f)
			{
				vector4.x = 0f;
				position = new Vector2(0f, position.y);
			}
			else if (position.x + vector4.x > (float)Screen.width - size.x)
			{
				vector4.x = 0f;
				position = new Vector2((float)Screen.width - size.x, position.y);
			}
			if (position.y + vector4.y > 0f)
			{
				vector4.y = 0f;
				position = new Vector2(position.x, 0f);
			}
			else if (position.y + vector4.y < (float)(-Screen.height) + size.y)
			{
				vector4.y = 0f;
				position = new Vector2(position.x, (float)(-Screen.height) + size.y);
			}
			position += vector4;
			dragOffset = vector3;
		}
		if (lastResolution.x != Screen.width || lastResolution.y != Screen.height)
		{
			Debug.Log("Screen resolution changed, resetting console position");
			lastResolution = new Vector2Int(Screen.width, Screen.height);
			ResetWindow();
		}
	}

	public void StartDrag(PointerEventData eventData)
	{
		isDragging = true;
		dragOffset = eventData.position;
	}

	public void EndDrag(PointerEventData eventData)
	{
		isDragging = false;
	}

	public void StartResize(PointerEventData eventData, Vector2Int corner)
	{
		isResizing = true;
		resizeCursorStart = eventData.position;
	}

	public void StopResize(PointerEventData eventData, Vector2Int corner)
	{
		isResizing = false;
	}
}
