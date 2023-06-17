using System.Collections.Generic;

public class CheckPointsController : MonoSingleton<CheckPointsController>
{
	private int requests;

	public List<CheckPoint> cps = new List<CheckPoint>();

	private List<ShopZone> shops = new List<ShopZone>();

	public void DisableCheckpoints()
	{
		if (requests == 0)
		{
			foreach (CheckPoint cp in cps)
			{
				cp.forceOff = true;
			}
			foreach (ShopZone shop in shops)
			{
				shop.ForceOff();
			}
		}
		requests++;
	}

	public void EnableCheckpoints()
	{
		requests--;
		if (requests > 0)
		{
			return;
		}
		foreach (CheckPoint cp in cps)
		{
			cp.forceOff = false;
			cp.ReactivationEffect();
		}
		foreach (ShopZone shop in shops)
		{
			shop.StopForceOff();
		}
		requests = 0;
	}

	public void AddCheckpoint(CheckPoint cp)
	{
		if (!cps.Contains(cp))
		{
			cps.Add(cp);
			if (requests > 0)
			{
				cp.forceOff = true;
			}
			else if (cp.forceOff)
			{
				cp.forceOff = false;
				cp.ReactivationEffect();
			}
		}
	}

	public void RemoveCheckpoint(CheckPoint cp)
	{
		if (cps.Contains(cp))
		{
			cps.Remove(cp);
			if (cp.forceOff)
			{
				cp.forceOff = false;
				cp.ReactivationEffect();
			}
		}
	}

	public void AddShop(ShopZone shop)
	{
		if (!shops.Contains(shop))
		{
			shops.Add(shop);
			if (requests > 0)
			{
				shop.ForceOff();
			}
			else if (shop.forcedOff)
			{
				shop.StopForceOff();
			}
		}
	}

	public void RemoveShop(ShopZone shop)
	{
		if (shops.Contains(shop))
		{
			shops.Remove(shop);
			if (shop.forcedOff)
			{
				shop.StopForceOff();
			}
		}
	}
}
