using UnityEngine;

public class GabrielCombinedSwordsThrown : MonoBehaviour
{
	public Transform justice;

	public Transform splendor;

	public GameObject teleportSword;

	[HideInInspector]
	public GabrielSecond gabe;

	private Transform parent;

	private void Start()
	{
		if (!gabe)
		{
			gabe = Object.FindObjectOfType<GabrielSecond>();
		}
		parent = base.transform.parent;
	}

	private void OnDestroy()
	{
		if (!base.gameObject.scene.isLoaded)
		{
			return;
		}
		Debug.Log("Skiddd 1");
		if ((bool)parent && parent.gameObject.activeSelf)
		{
			Debug.Log("Skiddd 2");
			if (gabe.swordsCombined)
			{
				gabe.UnGattai(destroySwords: false);
			}
			CreateTrail(justice, gabe.leftHand);
			CreateTrail(splendor, gabe.rightHand);
			Debug.Log("Skiddd 4");
			if (Object.Instantiate(gabe.teleportSound, gabe.transform.position, Quaternion.identity).TryGetComponent<AudioSource>(out var component))
			{
				component.pitch = 1.5f;
			}
		}
	}

	private void CreateTrail(Transform start, Transform target)
	{
		int num = Mathf.RoundToInt(Vector3.Distance(start.position, target.position) / 2.5f);
		for (int i = 0; i < num; i++)
		{
			MindflayerDecoy[] componentsInChildren = Object.Instantiate(teleportSword, Vector3.Lerp(start.position, target.position, (float)i / (float)num), Quaternion.Lerp(start.rotation, target.rotation, (float)i / (float)num)).GetComponentsInChildren<MindflayerDecoy>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].fadeOverride = (float)i / (float)num + 0.1f;
			}
			Debug.Log("Skiddd 3 " + i);
		}
	}
}
