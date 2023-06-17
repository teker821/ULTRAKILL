using UnityEngine;

public class StatueActivator : MonoBehaviour
{
	private void Start()
	{
		base.transform.parent.GetComponentInChildren<StatueFake>().Activate();
		Object.Destroy(base.gameObject);
	}
}
