using UnityEngine;
using UnityEngine.AI;

public class CyberGrindNavHelper : MonoBehaviour
{
	private enum BridgeDirection
	{
		Left,
		Right,
		Top,
		Bottom
	}

	private struct BridgeBlock
	{
		public bool TopBridge;

		public bool BottomBridge;

		public bool LeftBridge;

		public bool RightBridge;
	}

	private BridgeBlock[][] bridges;

	public void ResetLinks()
	{
	}

	public void GenerateLinks(EndlessCube[][] cbs)
	{
	}

	private void CheckNeighbors(EndlessCube[][] cbs, int x, int y)
	{
		bool num = x - 1 >= 0;
		bool flag = x - 2 >= 0;
		bool flag2 = x + 1 < 16;
		bool flag3 = x + 2 < 16;
		bool flag4 = y - 1 >= 0;
		bool flag5 = y - 2 >= 0;
		bool flag6 = y + 1 < 16;
		bool flag7 = y + 2 < 16;
		if (num)
		{
			CheckNeighbors(cbs[x][y], cbs[x - 1][y]);
		}
		if (flag)
		{
			CheckIfBridge(cbs[x][y], cbs[x - 1][y], cbs[x - 2][y], BridgeDirection.Left);
		}
		if (flag2)
		{
			CheckNeighbors(cbs[x][y], cbs[x + 1][y]);
		}
		if (flag3)
		{
			CheckIfBridge(cbs[x][y], cbs[x + 1][y], cbs[x + 2][y], BridgeDirection.Right);
		}
		if (flag4)
		{
			CheckNeighbors(cbs[x][y], cbs[x][y - 1]);
		}
		if (flag5)
		{
			CheckIfBridge(cbs[x][y], cbs[x][y - 1], cbs[x][y - 2], BridgeDirection.Top);
		}
		if (flag6)
		{
			CheckNeighbors(cbs[x][y], cbs[x][y + 1]);
		}
		if (flag7)
		{
			CheckIfBridge(cbs[x][y], cbs[x][y + 1], cbs[x][y + 2], BridgeDirection.Bottom);
		}
	}

	private void CheckIfBridge(EndlessCube dom, EndlessCube mid, EndlessCube sub, BridgeDirection dir)
	{
		if (sub.transform.position.y == dom.transform.position.y && !(dom.transform.position.y - mid.transform.position.y <= 10f) && (dir != BridgeDirection.Bottom || !bridges[dom.positionOnGrid.x][dom.positionOnGrid.y].BottomBridge) && (dir != BridgeDirection.Top || !bridges[dom.positionOnGrid.x][dom.positionOnGrid.y].TopBridge) && (dir != 0 || !bridges[dom.positionOnGrid.x][dom.positionOnGrid.y].LeftBridge) && (dir != BridgeDirection.Right || !bridges[dom.positionOnGrid.x][dom.positionOnGrid.y].RightBridge))
		{
			GameObject obj = new GameObject("NavLink");
			obj.transform.parent = base.transform;
			obj.transform.position = dom.transform.position + Vector3.up * 25f;
			NavMeshLink navMeshLink = obj.AddComponent<NavMeshLink>();
			Vector3 position = sub.transform.position;
			Vector3 position2 = dom.transform.position;
			position.y = 0f;
			position2.y = 0f;
			Vector3 normalized = (position - position2).normalized;
			normalized *= 1.75f;
			navMeshLink.startPoint = normalized;
			navMeshLink.endPoint = sub.transform.position - dom.transform.position - normalized;
			navMeshLink.bidirectional = true;
			navMeshLink.width = 3f;
			navMeshLink.UpdateLink();
			switch (dir)
			{
			case BridgeDirection.Bottom:
				bridges[dom.positionOnGrid.x][dom.positionOnGrid.y].BottomBridge = true;
				bridges[sub.positionOnGrid.x][sub.positionOnGrid.y].TopBridge = true;
				break;
			case BridgeDirection.Top:
				bridges[dom.positionOnGrid.x][dom.positionOnGrid.y].TopBridge = true;
				bridges[sub.positionOnGrid.x][sub.positionOnGrid.y].BottomBridge = true;
				break;
			case BridgeDirection.Left:
				bridges[dom.positionOnGrid.x][dom.positionOnGrid.y].LeftBridge = true;
				bridges[sub.positionOnGrid.x][sub.positionOnGrid.y].RightBridge = true;
				break;
			case BridgeDirection.Right:
				bridges[dom.positionOnGrid.x][dom.positionOnGrid.y].RightBridge = true;
				bridges[sub.positionOnGrid.x][sub.positionOnGrid.y].LeftBridge = true;
				break;
			}
		}
	}

	private void CheckNeighbors(EndlessCube dom, EndlessCube sub)
	{
		if (!sub.blockedByPrefab && !dom.blockedByPrefab)
		{
			CheckNeighbors(dom.transform, sub.transform);
		}
	}

	private void CheckNeighbors(Transform dom, Transform sub)
	{
		if (!(sub.position.y >= dom.position.y) && !(dom.position.y - sub.position.y > 10f))
		{
			GameObject obj = new GameObject("NavLink");
			obj.transform.SetParent(base.transform);
			obj.transform.position = dom.position + Vector3.up * 25f;
			NavMeshLink navMeshLink = obj.AddComponent<NavMeshLink>();
			Vector3 position = sub.position;
			Vector3 position2 = dom.position;
			position.y = 0f;
			position2.y = 0f;
			Vector3 normalized = (position - position2).normalized;
			normalized *= 1.75f;
			navMeshLink.startPoint = normalized;
			navMeshLink.endPoint = sub.position - dom.position;
			navMeshLink.bidirectional = true;
			navMeshLink.width = 5f;
			navMeshLink.UpdateLink();
		}
	}
}
