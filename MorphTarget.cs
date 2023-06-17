using UnityEngine;

public class MorphTarget : MonoBehaviour
{
	public float speed = 1f;

	private SkinnedMeshRenderer rend;

	private Mesh skinmesh;

	private float blend;

	private int blendshapecount;

	private void Awake()
	{
		rend = GetComponent<SkinnedMeshRenderer>();
		skinmesh = rend.sharedMesh;
	}

	private void Update()
	{
		rend.SetBlendShapeWeight(0, blend);
		blend = (blend + Time.deltaTime * speed * 60f) % 100f;
	}
}
