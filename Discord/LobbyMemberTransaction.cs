using System;
using System.Runtime.InteropServices;

namespace Discord;

public struct LobbyMemberTransaction
{
	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result SetMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result DeleteMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);

		internal SetMetadataMethod SetMetadata;

		internal DeleteMetadataMethod DeleteMetadata;
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
}
