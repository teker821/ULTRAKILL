using System.Collections.Generic;
using UnityEngine;

public class MindflayerDecoy : MonoBehaviour
{
	private Renderer[] rends;

	private List<Material> mats = new List<Material>();

	private Color clr;

	public bool enraged;

	public Material enrageMaterial;

	public float fadeOverride;

	public float fadeSpeed = 1f;

	private void Start()
	{
		rends = GetComponentsInChildren<Renderer>();
		Renderer[] array = rends;
		foreach (Renderer renderer in array)
		{
			if (enraged)
			{
				renderer.material = enrageMaterial;
			}
			mats.AddRange(renderer.materials);
		}
		clr = mats[0].color;
		if (fadeOverride == 0f)
		{
			return;
		}
		clr.a = fadeOverride;
		foreach (Material mat in mats)
		{
			mat.color = clr;
		}
	}

	private void Update()
	{
		if (!(clr.a > 0f))
		{
			return;
		}
		clr.a = Mathf.MoveTowards(clr.a, 0f, Time.deltaTime * fadeSpeed);
		if (clr.a <= 0f)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		foreach (Material mat in mats)
		{
			mat.color = clr;
		}
	}
}
