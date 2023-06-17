using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Discord;

public class StorageManager
{
	internal struct FFIEvents
	{
	}

	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result ReadMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data, int dataLen, ref uint read);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ReadAsyncCallback(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ReadAsyncMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr callbackData, ReadAsyncCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ReadAsyncPartialCallback(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ReadAsyncPartialMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, ulong offset, ulong length, IntPtr callbackData, ReadAsyncPartialCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result WriteMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data, int dataLen);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void WriteAsyncCallback(IntPtr ptr, Result result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void WriteAsyncMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data, int dataLen, IntPtr callbackData, WriteAsyncCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result DeleteMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result ExistsMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, ref bool exists);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void CountMethod(IntPtr methodsPtr, ref int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result StatMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, ref FileStat stat);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result StatAtMethod(IntPtr methodsPtr, int index, ref FileStat stat);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetPathMethod(IntPtr methodsPtr, StringBuilder path);

		internal ReadMethod Read;

		internal ReadAsyncMethod ReadAsync;

		internal ReadAsyncPartialMethod ReadAsyncPartial;

		internal WriteMethod Write;

		internal WriteAsyncMethod WriteAsync;

		internal DeleteMethod Delete;

		internal ExistsMethod Exists;

		internal CountMethod Count;

		internal StatMethod Stat;

		internal StatAtMethod StatAt;

		internal GetPathMethod GetPath;
	}

	public delegate void ReadAsyncHandler(Result result, byte[] data);

	public delegate void ReadAsyncPartialHandler(Result result, byte[] data);

	public delegate void WriteAsyncHandler(Result result);

	private IntPtr MethodsPtr;

	private object MethodsStructure;

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

	internal StorageManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
	{
		if (eventsPtr == IntPtr.Zero)
		{
			throw new ResultException(Result.InternalError);
		}
		InitEvents(eventsPtr, ref events);
		MethodsPtr = ptr;
		if (MethodsPtr == IntPtr.Zero)
		{
			throw new ResultException(Result.InternalError);
		}
	}

	private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
	{
		Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
	}

	public uint Read(string name, byte[] data)
	{
		uint read = 0u;
		Result result = Methods.Read(MethodsPtr, name, data, data.Length, ref read);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return read;
	}

	[MonoPInvokeCallback]
	private static void ReadAsyncCallbackImpl(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		ReadAsyncHandler obj = (ReadAsyncHandler)gCHandle.Target;
		gCHandle.Free();
		byte[] array = new byte[dataLen];
		Marshal.Copy(dataPtr, array, 0, dataLen);
		obj(result, array);
	}

	public void ReadAsync(string name, ReadAsyncHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.ReadAsync(MethodsPtr, name, GCHandle.ToIntPtr(value), ReadAsyncCallbackImpl);
	}

	[MonoPInvokeCallback]
	private static void ReadAsyncPartialCallbackImpl(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		ReadAsyncPartialHandler obj = (ReadAsyncPartialHandler)gCHandle.Target;
		gCHandle.Free();
		byte[] array = new byte[dataLen];
		Marshal.Copy(dataPtr, array, 0, dataLen);
		obj(result, array);
	}

	public void ReadAsyncPartial(string name, ulong offset, ulong length, ReadAsyncPartialHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.ReadAsyncPartial(MethodsPtr, name, offset, length, GCHandle.ToIntPtr(value), ReadAsyncPartialCallbackImpl);
	}

	public void Write(string name, byte[] data)
	{
		Result result = Methods.Write(MethodsPtr, name, data, data.Length);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	[MonoPInvokeCallback]
	private static void WriteAsyncCallbackImpl(IntPtr ptr, Result result)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		WriteAsyncHandler obj = (WriteAsyncHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result);
	}

	public void WriteAsync(string name, byte[] data, WriteAsyncHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.WriteAsync(MethodsPtr, name, data, data.Length, GCHandle.ToIntPtr(value), WriteAsyncCallbackImpl);
	}

	public void Delete(string name)
	{
		Result result = Methods.Delete(MethodsPtr, name);
		if (result != 0)
		{
			throw new ResultException(result);
		}
	}

	public bool Exists(string name)
	{
		bool exists = false;
		Result result = Methods.Exists(MethodsPtr, name, ref exists);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return exists;
	}

	public int Count()
	{
		int count = 0;
		Methods.Count(MethodsPtr, ref count);
		return count;
	}

	public FileStat Stat(string name)
	{
		FileStat stat = default(FileStat);
		Result result = Methods.Stat(MethodsPtr, name, ref stat);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return stat;
	}

	public FileStat StatAt(int index)
	{
		FileStat stat = default(FileStat);
		Result result = Methods.StatAt(MethodsPtr, index, ref stat);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return stat;
	}

	public string GetPath()
	{
		StringBuilder stringBuilder = new StringBuilder(4096);
		Result result = Methods.GetPath(MethodsPtr, stringBuilder);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return stringBuilder.ToString();
	}

	public IEnumerable<FileStat> Files()
	{
		int num = Count();
		List<FileStat> list = new List<FileStat>();
		for (int i = 0; i < num; i++)
		{
			list.Add(StatAt(i));
		}
		return list;
	}
}
