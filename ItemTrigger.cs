using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
	public ItemType targetType;

	public bool oneTime;

	private bool activated;

	public bool destroyActivator;

	public UltrakillEvent onEvent;

	private void OnTriggerEnter(Collider other)
	{
		if ((!oneTime || !activated) && other.gameObject.layer == 22 && (other.attachedRigidbody ? other.attachedRigidbody.TryGetComponent<ItemIdentifier>(out var component) : other.TryGetComponent<ItemIdentifier>(out component)) && component.itemType == targetType)
		{
			activated = true;
			onEvent?.Invoke();
			if (destroyActivator)
			{
				Object.Destroy(component.gameObject);
			}
		}
	}
}
