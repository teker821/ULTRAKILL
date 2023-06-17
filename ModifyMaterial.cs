using UnityEngine;

public class ModifyMaterial : MonoBehaviour
{
	private Renderer rend;

	private MaterialPropertyBlock block;

	private bool valuesSet;

	public void ChangeEmissionIntensity(float value)
	{
		SetValues();
		for (int i = 0; i < rend.materials.Length; i++)
		{
			rend.GetPropertyBlock(block, i);
			block.SetFloat(UKShaderProperties.EmissiveIntensity, value);
			rend.SetPropertyBlock(block, i);
		}
	}

	public void ChangeEmissionColor(Color clr)
	{
		SetValues();
		for (int i = 0; i < rend.materials.Length; i++)
		{
			rend.GetPropertyBlock(block, i);
			block.SetColor(UKShaderProperties.EmissiveColor, clr);
			rend.SetPropertyBlock(block, i);
		}
	}

	private void SetValues()
	{
		if (!valuesSet)
		{
			valuesSet = true;
			block = new MaterialPropertyBlock();
			rend = GetComponent<Renderer>();
		}
	}
}
