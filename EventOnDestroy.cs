using UnityEngine;
using UnityEngine.Events;

public class EventOnDestroy : MonoBehaviour
{
	public UnityEvent stuff;

	private void OnDestroy()
	{
		if ((bool)base.transform.parent && base.transform.parent.gameObject.activeInHierarchy)
		{
			stuff?.Invoke();
		}
	}
}
