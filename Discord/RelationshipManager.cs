using System;
using System.Runtime.InteropServices;

namespace Discord;

public class RelationshipManager
{
	internal struct FFIEvents
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void RefreshHandler(IntPtr ptr);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void RelationshipUpdateHandler(IntPtr ptr, ref Relationship relationship);

		internal RefreshHandler OnRefresh;

		internal RelationshipUpdateHandler OnRelationshipUpdate;
	}

	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool FilterCallback(IntPtr ptr, ref Relationship relationship);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void FilterMethod(IntPtr methodsPtr, IntPtr callbackData, FilterCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result CountMethod(IntPtr methodsPtr, ref int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetMethod(IntPtr methodsPtr, long userId, ref Relationship relationship);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Result GetAtMethod(IntPtr methodsPtr, uint index, ref Relationship relationship);

		internal FilterMethod Filter;

		internal CountMethod Count;

		internal GetMethod Get;

		internal GetAtMethod GetAt;
	}

	public delegate bool FilterHandler(ref Relationship relationship);

	public delegate void RefreshHandler();

	public delegate void RelationshipUpdateHandler(ref Relationship relationship);

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

	public event RefreshHandler OnRefresh;

	public event RelationshipUpdateHandler OnRelationshipUpdate;

	internal RelationshipManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
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
		events.OnRefresh = OnRefreshImpl;
		events.OnRelationshipUpdate = OnRelationshipUpdateImpl;
		Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
	}

	[MonoPInvokeCallback]
	private static bool FilterCallbackImpl(IntPtr ptr, ref Relationship relationship)
	{
		return ((FilterHandler)GCHandle.FromIntPtr(ptr).Target)(ref relationship);
	}

	public void Filter(FilterHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.Filter(MethodsPtr, GCHandle.ToIntPtr(value), FilterCallbackImpl);
		value.Free();
	}

	public int Count()
	{
		int count = 0;
		Result result = Methods.Count(MethodsPtr, ref count);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return count;
	}

	public Relationship Get(long userId)
	{
		Relationship relationship = default(Relationship);
		Result result = Methods.Get(MethodsPtr, userId, ref relationship);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return relationship;
	}

	public Relationship GetAt(uint index)
	{
		Relationship relationship = default(Relationship);
		Result result = Methods.GetAt(MethodsPtr, index, ref relationship);
		if (result != 0)
		{
			throw new ResultException(result);
		}
		return relationship;
	}

	[MonoPInvokeCallback]
	private static void OnRefreshImpl(IntPtr ptr)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.RelationshipManagerInstance.OnRefresh != null)
		{
			discord.RelationshipManagerInstance.OnRefresh();
		}
	}

	[MonoPInvokeCallback]
	private static void OnRelationshipUpdateImpl(IntPtr ptr, ref Relationship relationship)
	{
		Discord discord = (Discord)GCHandle.FromIntPtr(ptr).Target;
		if (discord.RelationshipManagerInstance.OnRelationshipUpdate != null)
		{
			discord.RelationshipManagerInstance.OnRelationshipUpdate(ref relationship);
		}
	}
}
