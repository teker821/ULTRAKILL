using UnityEngine;
using UnityEngine.Events;

public class CheckForOtherObject : MonoBehaviour
{
	public GameObject target;

	public bool disableSelf;

	public UnityEvent onOtherObjectFound;

	private void Start()
	{
		if (target != null && target.activeInHierarchy)
		{
			if (disableSelf)
			{
				base.gameObject.SetActive(value: false);
			}
			onOtherObjectFound?.Invoke();
		}
	}
}
