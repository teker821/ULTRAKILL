using System;

namespace NewBlood.Rendering;

public struct Triangle<TIndex> where TIndex : unmanaged
{
	public TIndex Index0;

	public TIndex Index1;

	public TIndex Index2;

	public TIndex this[int index]
	{
		get
		{
			if ((uint)index > 2u)
			{
				ThrowIndexOutOfRangeException();
			}
			return index switch
			{
				1 => Index1, 
				2 => Index2, 
				_ => Index0, 
			};
		}
		set
		{
			if ((uint)index > 2u)
			{
				ThrowIndexOutOfRangeException();
			}
			switch (index)
			{
			default:
				Index0 = value;
				break;
			case 1:
				Index1 = value;
				break;
			case 2:
				Index2 = value;
				break;
			}
		}
	}

	public Triangle(TIndex index0, TIndex index1, TIndex index2)
	{
		Index0 = index0;
		Index1 = index1;
		Index2 = index2;
	}

	private static TIndex ThrowIndexOutOfRangeException()
	{
		throw new IndexOutOfRangeException();
	}
}
