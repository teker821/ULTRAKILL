using UnityEngine;

public class ShopButton : MonoBehaviour
{
	public bool deactivated;

	public bool failure;

	public GameObject clickSound;

	public GameObject failSound;

	public GameObject[] toActivate;

	public GameObject[] toDeactivate;

	public VariationInfo variationInfo;

	private ControllerPointer pointer;

	private void Awake()
	{
		if (!TryGetComponent<ControllerPointer>(out pointer))
		{
			pointer = base.gameObject.AddComponent<ControllerPointer>();
		}
		pointer.OnPressed.AddListener(OnPointerClick);
	}

	private void OnPointerClick()
	{
		if (deactivated)
		{
			return;
		}
		if (!failure)
		{
			GameObject[] array = toActivate;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			array = toDeactivate;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			if (variationInfo != null)
			{
				variationInfo.WeaponBought();
			}
			if (clickSound != null)
			{
				Object.Instantiate(clickSound);
			}
		}
		else if (failure && failSound != null)
		{
			Object.Instantiate(failSound);
		}
	}
}
