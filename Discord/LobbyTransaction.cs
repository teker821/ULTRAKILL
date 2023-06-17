using System;
using System.Runtime.InteropServices;

namespace Discord;

public struct LobbyTransaction
{
	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetTypeMethod(IntPtr methodsPtr, LobbyType type);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetOwnerMethod(IntPtr methodsPtr, long ownerId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetCapacityMethod(IntPtr methodsPtr, uint capacity);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result DeleteMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetLockedMethod(IntPtr methodsPtr, bool locked);

		internal SetTypeMethod SetType;

		internal SetOwnerMethod SetOwner;

		internal SetCapacityMethod SetCapacity;

		internal SetMetadataMethod SetMetadata;

		internal DeleteMetadataMethod DeleteMetadata;

		internal SetLockedMethod SetLocked;
	}

	internal IntPtr MethodsPtr;

	internal object MethodsStructure;

	private FFIMethods Methods
	{
		get
		{
			if (MethodsStructure == null)
			{
				MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
			}
			return (FFIMethods)MethodsStructure;
		}
	}

	public void SetType(LobbyType type)
	{
		if (MethodsPtr != IntPtr.Zero)
		{
			Result result = Methods.SetType(MethodsPtr, type);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}
	}

	public void SetOwner(long ownerId)
	{
		if (MethodsPtr != IntPtr.Zero)
		{
			Result result = Methods.SetOwner(MethodsPtr, ownerId);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}
	}

	public void SetCapacity(uint capacity)
	{
		if (MethodsPtr != IntPtr.Zero)
		{
			Result result = Methods.SetCapacity(MethodsPtr, capacity);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}
	}

	public void SetMetadata(string key, string value)
	{
		if (MethodsPtr != IntPtr.Zero)
		{
			Result result = Methods.SetMetadata(MethodsPtr, key, value);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}
	}

	public void DeleteMetadata(string key)
	{
		if (MethodsPtr != IntPtr.Zero)
		{
			Result result = Methods.DeleteMetadata(MethodsPtr, key);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}
	}

	public void SetLocked(bool locked)
	{
		if (MethodsPtr != IntPtr.Zero)
		{
			Result result = Methods.SetLocked(MethodsPtr, locked);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}
	}
}
