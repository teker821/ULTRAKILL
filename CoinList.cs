using System.Collections.Generic;

public class CoinList : MonoSingleton<CoinList>
{
	public List<Coin> revolverCoinsList = new List<Coin>();

	public void AddCoin(Coin coin)
	{
		if (!revolverCoinsList.Contains(coin))
		{
			revolverCoinsList.Add(coin);
		}
	}

	public void RemoveCoin(Coin coin)
	{
		if (revolverCoinsList.Contains(coin))
		{
			revolverCoinsList.Remove(coin);
		}
	}

	private void Start()
	{
		Invoke("SlowUpdate", 30f);
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 30f);
		for (int num = revolverCoinsList.Count - 1; num >= 0; num--)
		{
			if (revolverCoinsList[num] == null)
			{
				revolverCoinsList.RemoveAt(num);
			}
		}
	}
}
