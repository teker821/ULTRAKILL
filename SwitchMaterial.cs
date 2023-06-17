using UnityEngine;

public class SwitchMaterial : MonoBehaviour
{
	public Material[] materials;

	private MeshRenderer mr;

	private void Start()
	{
		mr = GetComponent<MeshRenderer>();
	}

	public void Switch(int i)
	{
		mr.sharedMaterial = materials[i];
	}
}
