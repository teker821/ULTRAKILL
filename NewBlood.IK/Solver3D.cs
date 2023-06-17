using System.Collections.Generic;
using UnityEngine;

namespace NewBlood.IK;

public abstract class Solver3D : MonoBehaviour
{
	[SerializeField]
	private bool m_ConstrainRotation = true;

	[SerializeField]
	private bool m_SolveFromDefaultPose = true;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_Weight = 1f;

	private List<Vector3> m_TargetPositions = new List<Vector3>();

	public int chainCount => GetChainCount();

	public bool constrainRotation
	{
		get
		{
			return m_ConstrainRotation;
		}
		set
		{
			m_ConstrainRotation = value;
		}
	}

	public bool solveFromDefaultPose
	{
		get
		{
			return m_SolveFromDefaultPose;
		}
		set
		{
			m_SolveFromDefaultPose = value;
		}
	}

	public bool isValid
	{
		get
		{
			for (int i = 0; i < chainCount; i++)
			{
				if (!GetChain(i).isValid)
				{
					return false;
				}
			}
			return DoValidate();
		}
	}

	public bool allChainsHaveTargets
	{
		get
		{
			for (int i = 0; i < chainCount; i++)
			{
				if (GetChain(i).target == null)
				{
					return false;
				}
			}
			return true;
		}
	}

	public float weight
	{
		get
		{
			return m_Weight;
		}
		set
		{
			m_Weight = Mathf.Clamp01(value);
		}
	}

	public void UpdateIK(float globalWeight)
	{
		if (allChainsHaveTargets)
		{
			PrepareEffectorPositions();
			UpdateIK(m_TargetPositions, globalWeight);
		}
	}

	public void UpdateIK(List<Vector3> positions, float globalWeight)
	{
		if (positions.Count != chainCount)
		{
			return;
		}
		float num = globalWeight * m_Weight;
		if (Mathf.Approximately(num, 0f) || !isValid)
		{
			return;
		}
		Prepare();
		if (num < 1f)
		{
			StoreLocalRotations();
		}
		DoUpdateIK(positions);
		if (constrainRotation)
		{
			for (int i = 0; i < chainCount; i++)
			{
				IKChain3D chain = GetChain(i);
				if (!(chain.target == null))
				{
					chain.effector.rotation = chain.target.rotation;
				}
			}
		}
		if (num < 1f)
		{
			BlendFKToIK(num);
		}
	}

	public void Initialize()
	{
		DoInitialize();
		for (int i = 0; i < chainCount; i++)
		{
			GetChain(i).Initialize();
		}
	}

	public abstract IKChain3D GetChain(int index);

	protected abstract int GetChainCount();

	protected abstract void DoUpdateIK(List<Vector3> effectorPositions);

	protected virtual bool DoValidate()
	{
		return true;
	}

	protected virtual void DoInitialize()
	{
	}

	protected virtual void DoPrepare()
	{
	}

	protected virtual Transform GetRootTransform()
	{
		if (chainCount > 0)
		{
			return GetChain(0).rootTransform;
		}
		return null;
	}

	protected virtual void OnValidate()
	{
		m_Weight = Mathf.Clamp01(m_Weight);
	}

	private void Prepare()
	{
		if (solveFromDefaultPose)
		{
			for (int i = 0; i < chainCount; i++)
			{
				IKChain3D chain = GetChain(i);
				bool targetRotationIsConstrained = constrainRotation && chain.target != null;
				chain.RestoreDefaultPose(targetRotationIsConstrained);
			}
		}
		DoPrepare();
	}

	private void PrepareEffectorPositions()
	{
		m_TargetPositions.Clear();
		for (int i = 0; i < chainCount; i++)
		{
			IKChain3D chain = GetChain(i);
			if (!(chain.target == null))
			{
				m_TargetPositions.Add(chain.target.position);
			}
		}
	}

	private void StoreLocalRotations()
	{
		for (int i = 0; i < chainCount; i++)
		{
			GetChain(i).StoreLocalRotations();
		}
	}

	private void BlendFKToIK(float finalWeight)
	{
		for (int i = 0; i < chainCount; i++)
		{
			IKChain3D chain = GetChain(i);
			bool targetRotationIsConstrained = constrainRotation && chain.target != null;
			chain.BlendFKToIK(finalWeight, targetRotationIsConstrained);
		}
	}
}
