using UnityEngine;

public class ChangeMaterials : MonoBehaviour
{
	public Material[] materials;

	private SkinnedMeshRenderer smr;

	private MeshRenderer mr;

	private Material[] oldMaterials;

	public bool enemySimplifierOverride;

	public bool enraged;

	public void Activate()
	{
		if (!smr)
		{
			smr = GetComponentInParent<SkinnedMeshRenderer>();
		}
		if ((bool)smr)
		{
			oldMaterials = smr.materials;
			smr.materials = materials;
			if (!enemySimplifierOverride || !TryGetComponent<EnemySimplifier>(out var component))
			{
				return;
			}
			EnemySimplifier.MaterialState stateToTarget = (enraged ? EnemySimplifier.MaterialState.enraged : EnemySimplifier.MaterialState.normal);
			for (int i = 0; i < smr.materials.Length; i++)
			{
				if (oldMaterials[i] != smr.materials[i])
				{
					component.ChangeMaterialNew(stateToTarget, oldMaterials[i]);
				}
			}
			return;
		}
		if (!mr)
		{
			mr = GetComponentInParent<MeshRenderer>();
		}
		if (!mr)
		{
			return;
		}
		oldMaterials = mr.materials;
		mr.materials = materials;
		if (!enemySimplifierOverride || !TryGetComponent<EnemySimplifier>(out var component2))
		{
			return;
		}
		EnemySimplifier.MaterialState stateToTarget2 = (enraged ? EnemySimplifier.MaterialState.enraged : EnemySimplifier.MaterialState.normal);
		for (int j = 0; j < mr.materials.Length; j++)
		{
			if (oldMaterials[j] != mr.materials[j])
			{
				component2.ChangeMaterialNew(stateToTarget2, oldMaterials[j]);
			}
		}
	}

	public void Reverse()
	{
		if (oldMaterials == null || oldMaterials.Length == 0)
		{
			return;
		}
		if ((bool)smr)
		{
			smr.materials = oldMaterials;
			if (!enemySimplifierOverride || !TryGetComponent<EnemySimplifier>(out var component))
			{
				return;
			}
			for (int i = 0; i < smr.materials.Length; i++)
			{
				if (materials[i] != smr.materials[i])
				{
					component.ChangeMaterial(materials[i], smr.materials[i]);
				}
			}
		}
		else
		{
			if (!mr)
			{
				return;
			}
			mr.materials = oldMaterials;
			if (!enemySimplifierOverride || !TryGetComponent<EnemySimplifier>(out var component2))
			{
				return;
			}
			for (int j = 0; j < mr.materials.Length; j++)
			{
				if (materials[j] != mr.materials[j])
				{
					component2.ChangeMaterial(materials[j], mr.materials[j]);
				}
			}
		}
	}
}
