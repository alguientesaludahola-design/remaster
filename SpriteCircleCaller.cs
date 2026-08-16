using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngine.Scripting;
using UnityEngineInternal;

namespace UnityEngine
{
	// Token: 0x0200022D RID: 557
	[RequiredByNativeCode(GenerateProxy = true)]
	[NativeHeader("Runtime/Export/UnityEngineObject.bindings.h")]
	[NativeHeader("Runtime/GameCode/CloneObject.h")]
	[NativeHeader("Runtime/SceneManager/SceneManager.h")]
	[StructLayout(LayoutKind.Sequential)]
	public class Object
	{
		// Token: 0x060013C5 RID: 5061 RVA: 0x0001E7B8 File Offset: 0x0001C9B8
		[SecuritySafeCritical]
		public unsafe int GetInstanceID()
		{
			int result;
			if (this.m_CachedPtr == IntPtr.Zero)
			{
				result = 0;
			}
			else
			{
				if (Object.OffsetOfInstanceIDInCPlusPlusObject == -1)
				{
					Object.OffsetOfInstanceIDInCPlusPlusObject = Object.GetOffsetOfInstanceIDInCPlusPlusObject();
				}
				result = *(int*)((void*)new IntPtr(this.m_CachedPtr.ToInt64() + (long)Object.OffsetOfInstanceIDInCPlusPlusObject));
			}
			return result;
		}

		// Token: 0x060013C6 RID: 5062 RVA: 0x0001E81C File Offset: 0x0001CA1C
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		// Token: 0x060013C7 RID: 5063 RVA: 0x0001E838 File Offset: 0x0001CA38
		public override bool Equals(object other)
		{
			Object @object = other as Object;
			return (!(@object == null) || other == null || other is Object) && Object.CompareBaseObjects(this, @object);
		}

		// Token: 0x060013C8 RID: 5064 RVA: 0x0001E880 File Offset: 0x0001CA80
		public static implicit operator bool(Object exists)
		{
			return !Object.CompareBaseObjects(exists, null);
		}

		// Token: 0x060013C9 RID: 5065 RVA: 0x0001E8A0 File Offset: 0x0001CAA0
		private static bool CompareBaseObjects(Object lhs, Object rhs)
		{
			bool flag = lhs == null;
			bool flag2 = rhs == null;
			bool result;
			if (flag2 && flag)
			{
				result = true;
			}
			else if (flag2)
			{
				result = !Object.IsNativeObjectAlive(lhs);
			}
			else if (flag)
			{
				result = !Object.IsNativeObjectAlive(rhs);
			}
			else
			{
				result = object.ReferenceEquals(lhs, rhs);
			}
			return result;
		}

		// Token: 0x060013CA RID: 5066 RVA: 0x0001E903 File Offset: 0x0001CB03
		private void EnsureRunningOnMainThread()
		{
			if (!Object.CurrentThreadIsMainThread())
			{
				throw new InvalidOperationException("EnsureRunningOnMainThread can only be called from the main thread");
			}
		}

		// Token: 0x060013CB RID: 5067 RVA: 0x0001E91C File Offset: 0x0001CB1C
		private static bool IsNativeObjectAlive(Object o)
		{
			return o.GetCachedPtr() != IntPtr.Zero;
		}

		// Token: 0x060013CC RID: 5068 RVA: 0x0001E944 File Offset: 0x0001CB44
		private IntPtr GetCachedPtr()
		{
			return this.m_CachedPtr;
		}

		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x060013CD RID: 5069 RVA: 0x0001E960 File Offset: 0x0001CB60
		// (set) Token: 0x060013CE RID: 5070 RVA: 0x0001E97B File Offset: 0x0001CB7B
		public string name
		{
			get
			{
				return Object.GetName(this);
			}
			set
			{
				Object.SetName(this, value);
			}
		}

		// Token: 0x060013CF RID: 5071 RVA: 0x0001E988 File Offset: 0x0001CB88
		[TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
		public static Object Instantiate(Object original, Vector3 position, Quaternion rotation)
		{
			Object.CheckNullArgument(original, "The Object you want to instantiate is null.");
			if (original is ScriptableObject)
			{
				throw new ArgumentException("Cannot instantiate a ScriptableObject with a position and rotation");
			}
			Object @object = Object.Internal_InstantiateSingle(original, position, rotation);
			if (@object == null)
			{
				throw new UnityException("Instantiate failed because the clone was destroyed during creation. This can happen if DestroyImmediate is called in MonoBehaviour.Awake.");
			}
			return @object;
		}

		// Token: 0x060013D0 RID: 5072 RVA: 0x0001E9E0 File Offset: 0x0001CBE0
		[TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
		public static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent)
		{
			Object result;
			if (parent == null)
			{
				result = Object.Instantiate(original, position, rotation);
			}
			else
			{
				Object.CheckNullArgument(original, "The Object you want to instantiate is null.");
				Object @object = Object.Internal_InstantiateSingleWithParent(original, parent, position, rotation);
				if (@object == null)
				{
					throw new UnityException("Instantiate failed because the clone was destroyed during creation. This can happen if DestroyImmediate is called in MonoBehaviour.Awake.");
				}
				result = @object;
			}
			return result;
		}

		// Token: 0x060013D1 RID: 5073 RVA: 0x0001EA3C File Offset: 0x0001CC3C
		[TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
		public static Object Instantiate(Object original)
		{
			Object.CheckNullArgument(original, "The Object you want to instantiate is null.");
			Object @object = Object.Internal_CloneSingle(original);
			if (@object == null)
			{
				throw new UnityException("Instantiate failed because the clone was destroyed during creation. This can happen if DestroyImmediate is called in MonoBehaviour.Awake.");
			}
			return @object;
		}

		// Token: 0x060013D2 RID: 5074 RVA: 0x0001EA7C File Offset: 0x0001CC7C
		[TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
		public static Object Instantiate(Object original, Transform parent)
		{
			return Object.Instantiate(original, parent, false);
		}

		// Token: 0x060013D3 RID: 5075 RVA: 0x0001EA9C File Offset: 0x0001CC9C
		[TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
		public static Object Instantiate(Object original, Transform parent, bool instantiateInWorldSpace)
		{
			Object result;
			if (parent == null)
			{
				result = Object.Instantiate(original);
			}
			else
			{
				Object.CheckNullArgument(original, "The Object you want to instantiate is null.");
				Object @object = Object.Internal_CloneSingleWithParent(original, parent, instantiateInWorldSpace);
				if (@object == null)
				{
					throw new UnityException("Instantiate failed because the clone was destroyed during creation. This can happen if DestroyImmediate is called in MonoBehaviour.Awake.");
				}
				result = @object;
			}
			return result;
		}

		// Token: 0x060013D4 RID: 5076 RVA: 0x0001EAF8 File Offset: 0x0001CCF8
		public static T Instantiate<T>(T original) where T : Object
		{
			Object.CheckNullArgument(original, "The Object you want to instantiate is null.");
			T t = (T)((object)Object.Internal_CloneSingle(original));
			if (t == null)
			{
				throw new UnityException("Instantiate failed because the clone was destroyed during creation. This can happen if DestroyImmediate is called in MonoBehaviour.Awake.");
			}
			return t;
		}

		// Token: 0x060013D5 RID: 5077 RVA: 0x0001EB4C File Offset: 0x0001CD4C
		public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object
		{
			return (T)((object)Object.Instantiate(original, position, rotation));
		}

		// Token: 0x060013D6 RID: 5078 RVA: 0x0001EB74 File Offset: 0x0001CD74
		public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
		{
			return (T)((object)Object.Instantiate(original, position, rotation, parent));
		}

		// Token: 0x060013D7 RID: 5079 RVA: 0x0001EB9C File Offset: 0x0001CD9C
		public static T Instantiate<T>(T original, Transform parent) where T : Object
		{
			return Object.Instantiate<T>(original, parent, false);
		}

		// Token: 0x060013D8 RID: 5080 RVA: 0x0001EBBC File Offset: 0x0001CDBC
		public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays) where T : Object
		{
			return (T)((object)Object.Instantiate(original, parent, worldPositionStays));
		}

		// Token: 0x060013D9 RID: 5081
		[NativeMethod(Name = "Scripting::DestroyObjectFromScripting", IsFreeFunction = true, ThrowsException = true)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void Destroy(Object obj, [DefaultValue("0.0F")] float t);

		// Token: 0x060013DA RID: 5082 RVA: 0x0001EBE4 File Offset: 0x0001CDE4
		[ExcludeFromDocs]
		public static void Destroy(Object obj)
		{
			float t = 0f;
			Object.Destroy(obj, t);
		}

		// Token: 0x060013DB RID: 5083
		[NativeMethod(Name = "Scripting::DestroyObjectFromScriptingImmediate", IsFreeFunction = true, ThrowsException = true)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void DestroyImmediate(Object obj, [DefaultValue("false")] bool allowDestroyingAssets);

		// Token: 0x060013DC RID: 5084 RVA: 0x0001EC00 File Offset: 0x0001CE00
		[ExcludeFromDocs]
		public static void DestroyImmediate(Object obj)
		{
			bool allowDestroyingAssets = false;
			Object.DestroyImmediate(obj, allowDestroyingAssets);
		}

		// Token: 0x060013DD RID: 5085
		[TypeInferenceRule(TypeInferenceRules.ArrayOfTypeReferencedByFirstArgument)]
		[FreeFunction("UnityEngineObjectBindings::FindObjectsOfType")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern Object[] FindObjectsOfType(Type type);

		// Token: 0x060013DE RID: 5086
		[FreeFunction("GetSceneManager().DontDestroyOnLoad")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void DontDestroyOnLoad(Object target);

		// Token: 0x17000400 RID: 1024
		// (get) Token: 0x060013DF RID: 5087
		// (set) Token: 0x060013E0 RID: 5088
		public extern HideFlags hideFlags { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

		// Token: 0x060013E1 RID: 5089 RVA: 0x0001EC17 File Offset: 0x0001CE17
		[Obsolete("use Object.Destroy instead.")]
		public static void DestroyObject(Object obj, [DefaultValue("0.0F")] float t)
		{
			Object.Destroy(obj, t);
		}

		// Token: 0x060013E2 RID: 5090 RVA: 0x0001EC24 File Offset: 0x0001CE24
		[ExcludeFromDocs]
		[Obsolete("use Object.Destroy instead.")]
		public static void DestroyObject(Object obj)
		{
			float t = 0f;
			Object.Destroy(obj, t);
		}

		// Token: 0x060013E3 RID: 5091
		[FreeFunction("UnityEngineObjectBindings::FindObjectsOfType")]
		[Obsolete("warning use Object.FindObjectsOfType instead.")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern Object[] FindSceneObjectsOfType(Type type);

		// Token: 0x060013E4 RID: 5092
		[FreeFunction("UnityEngineObjectBindings::FindObjectsOfTypeIncludingAssets")]
		[Obsolete("use Resources.FindObjectsOfTypeAll instead.")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern Object[] FindObjectsOfTypeIncludingAssets(Type type);

		// Token: 0x060013E5 RID: 5093 RVA: 0x0001EC40 File Offset: 0x0001CE40
		public static T[] FindObjectsOfType<T>() where T : Object
		{
			return Resources.ConvertObjects<T>(Object.FindObjectsOfType(typeof(T)));
		}

		// Token: 0x060013E6 RID: 5094 RVA: 0x0001EC6C File Offset: 0x0001CE6C
		public static T FindObjectOfType<T>() where T : Object
		{
			return (T)((object)Object.FindObjectOfType(typeof(T)));
		}

		// Token: 0x060013E7 RID: 5095 RVA: 0x0001EC98 File Offset: 0x0001CE98
		[Obsolete("Please use Resources.FindObjectsOfTypeAll instead")]
		public static Object[] FindObjectsOfTypeAll(Type type)
		{
			return Resources.FindObjectsOfTypeAll(type);
		}

		// Token: 0x060013E8 RID: 5096 RVA: 0x0001ECB3 File Offset: 0x0001CEB3
		private static void CheckNullArgument(object arg, string message)
		{
			if (arg == null)
			{
				throw new ArgumentException(message);
			}
		}

		// Token: 0x060013E9 RID: 5097 RVA: 0x0001ECC4 File Offset: 0x0001CEC4
		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		public static Object FindObjectOfType(Type type)
		{
			Object[] array = Object.FindObjectsOfType(type);
			Object result;
			if (array.Length > 0)
			{
				result = array[0];
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x060013EA RID: 5098 RVA: 0x0001ECF4 File Offset: 0x0001CEF4
		public override string ToString()
		{
			return Object.ToString(this);
		}

		// Token: 0x060013EB RID: 5099 RVA: 0x0001ED10 File Offset: 0x0001CF10
		public static bool operator ==(Object x, Object y)
		{
			return Object.CompareBaseObjects(x, y);
		}

		// Token: 0x060013EC RID: 5100 RVA: 0x0001ED2C File Offset: 0x0001CF2C
		public static bool operator !=(Object x, Object y)
		{
			return !Object.CompareBaseObjects(x, y);
		}

		// Token: 0x060013ED RID: 5101
		[NativeMethod(Name = "Object::GetOffsetOfInstanceIdMember", IsFreeFunction = true, IsThreadSafe = true)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int GetOffsetOfInstanceIDInCPlusPlusObject();

		// Token: 0x060013EE RID: 5102
		[NativeMethod(Name = "CurrentThreadIsMainThread", IsFreeFunction = true, IsThreadSafe = true)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool CurrentThreadIsMainThread();

		// Token: 0x060013EF RID: 5103
		[FreeFunction("CloneObject")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Object Internal_CloneSingle(Object data);

		// Token: 0x060013F0 RID: 5104
		[FreeFunction("CloneObject")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Object Internal_CloneSingleWithParent(Object data, Transform parent, bool worldPositionStays);

		// Token: 0x060013F1 RID: 5105 RVA: 0x0001ED4B File Offset: 0x0001CF4B
		[FreeFunction("InstantiateObject")]
		private static Object Internal_InstantiateSingle(Object data, Vector3 pos, Quaternion rot)
		{
			return Object.Internal_InstantiateSingle_Injected(data, ref pos, ref rot);
		}

		// Token: 0x060013F2 RID: 5106 RVA: 0x0001ED57 File Offset: 0x0001CF57
		[FreeFunction("InstantiateObject")]
		private static Object Internal_InstantiateSingleWithParent(Object data, Transform parent, Vector3 pos, Quaternion rot)
		{
			return Object.Internal_InstantiateSingleWithParent_Injected(data, parent, ref pos, ref rot);
		}

		// Token: 0x060013F3 RID: 5107
		[FreeFunction("UnityEngineObjectBindings::ToString")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string ToString(Object obj);

		// Token: 0x060013F4 RID: 5108
		[FreeFunction("UnityEngineObjectBindings::GetName")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string GetName(Object obj);

		// Token: 0x060013F5 RID: 5109
		[FreeFunction("UnityEngineObjectBindings::SetName")]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetName(Object obj, string name);

		// Token: 0x060013F6 RID: 5110
		[NativeMethod(Name = "UnityEngineObjectBindings::DoesObjectWithInstanceIDExist", IsFreeFunction = true, IsThreadSafe = true)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool DoesObjectWithInstanceIDExist(int instanceID);

		// Token: 0x060013F7 RID: 5111
		[FreeFunction("UnityEngineObjectBindings::FindObjectFromInstanceID")]
		[VisibleToOtherModules]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Object FindObjectFromInstanceID(int instanceID);

		// Token: 0x060013F9 RID: 5113
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Object Internal_InstantiateSingle_Injected(Object data, ref Vector3 pos, ref Quaternion rot);

		// Token: 0x060013FA RID: 5114
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Object Internal_InstantiateSingleWithParent_Injected(Object data, Transform parent, ref Vector3 pos, ref Quaternion rot);

		// Token: 0x040007B0 RID: 1968
		private IntPtr m_CachedPtr;

		// Token: 0x040007B1 RID: 1969
		internal static int OffsetOfInstanceIDInCPlusPlusObject = -1;

		// Token: 0x040007B2 RID: 1970
		private const string objectIsNullMessage = "The Object you want to instantiate is null.";

		// Token: 0x040007B3 RID: 1971
		private const string cloneDestroyedMessage = "Instantiate failed because the clone was destroyed during creation. This can happen if DestroyImmediate is called in MonoBehaviour.Awake.";
	}
}
