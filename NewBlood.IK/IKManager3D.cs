using System.Collections.Generic;
using UnityEngine;

namespace NewBlood.IK;

public sealed class IKManager3D : MonoBehaviour
{
	[SerializeField]
	private List<Solver3D> m_Solvers = new List<Solver3D>();

	[Range(0f, 1f)]
	[SerializeField]
	private float m_Weight = 1f;

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

	public List<Solver3D> solvers => m_Solvers;

	public void AddSolver(Solver3D solver)
	{
		if (!m_Solvers.Contains(solver))
		{
			m_Solvers.Add(solver);
		}
	}

	public void RemoveSolver(Solver3D solver)
	{
		m_Solvers.Remove(solver);
	}

	public void UpdateManager()
	{
		foreach (Solver3D solver in m_Solvers)
		{
			if (!(solver == null) && solver.isActiveAndEnabled)
			{
				if (!solver.isValid)
				{
					solver.Initialize();
				}
				solver.UpdateIK(m_Weight);
			}
		}
	}

	private void LateUpdate()
	{
		UpdateManager();
	}

	private void FindChildSolvers()
	{
		m_Solvers.Clear();
		base.transform.GetComponentsInChildren(includeInactive: true, m_Solvers);
		for (int num = m_Solvers.Count - 1; num >= 0; num--)
		{
			if (m_Solvers[num].GetComponentInParent<IKManager3D>() != this)
			{
				m_Solvers.RemoveAt(num);
			}
		}
	}
}
