using UnityEngine;
using UnityEngine.Events;

public class ParryReceiver : MonoBehaviour
{
	public bool parryHeal;

	public bool disappearOnParry;

	[Space]
	public UnityEvent onParry;

	public void Parry()
	{
		onParry.Invoke();
		if (disappearOnParry)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
	}
}
