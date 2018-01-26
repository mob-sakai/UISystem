using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEngine.Events;
using System;


namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// Prefab path attribure for UISingleton.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class UISingletonPrefabAttribute : Attribute
	{
		/// <summary>
		/// Prefab path.
		/// </summary>
		public readonly string path;

		public UISingletonPrefabAttribute(string path)
		{
			this.path = path;
		}
	}

	/// <summary>
	/// UIシングルトンクラス.
	/// シーン内に1つのみ存在できるUIです.
	/// UIシングルトンには、UIPrefabAttributeが必要です.
	/// </summary>
	public abstract class UISingleton<T> : UIBase where T : UIBase
	{
		/// <summary>
		/// インスタンスを取得します.
		/// シーン上にインスタンスが存在しない場合、自動的に生成されます.
		/// </summary>
		public static T instance
		{
			get
			{
				//インスタンスがない場合、新しく生成します.
				if (m_Instance == null)
				{
					//クラスに設定されているUIPrefabAttributeを取得します.
					var attr = typeof(T).GetCustomAttributes(typeof(UISingletonPrefabAttribute), true).FirstOrDefault() as UISingletonPrefabAttribute;
					if (attr != null && !string.IsNullOrEmpty(attr.path))
					{
						m_Instance = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(attr.path)).GetComponent<T>();
					}
					else
					{
						m_Instance = new GameObject(typeof(T).Name).AddComponent<T>();
					}
				}
				return m_Instance;
			}
		}

		protected static T m_Instance;


		/// <summary>
		/// コンポーネントの生成コールバック.
		/// インスタンスが生成された時に、コールされます.
		/// </summary>
		protected virtual void Awake()
		{
			//インスタンスとして登録します.
			if (m_Instance == null)
			{
				m_Instance = this as T;
			}
			//複数のインスタンスは許可しません.
			else if (m_Instance != this)
			{
				Debug.LogError(this + "," + "Multiple" + "," + typeof(T).Name + "," + "exist in scene. please fix it.");
				enabled = false;
				if (Application.isPlaying)
					Destroy(this);
				return;
			}

			FitByParent(UIManager.uiRootCanvas.transform);
			argument = new UIArgument(){ path = typeof(T).Name };
			orderTracker.ignoreParentTracker = true;
			orderTracker.addSortingOrder = sortingOrder;
			gameObject.SetActive(false);
		}


		/// <summary>
		/// コンポーネントの破棄コールバック.
		/// インスタンスが破棄された時にコールされます.
		/// </summary>
		protected virtual void OnDestroy()
		{
			//インスタンスの登録を解除します.
			if (object.ReferenceEquals(m_Instance, this))
				m_Instance = null;
		}

		/// <summary>
		/// Sorting order.
		/// </summary>
		protected abstract int sortingOrder { get; }

		/// <summary>
		/// Implement this property to indicate whether UI is cacheable.
		/// </summary>
		public sealed override bool isPoolable { get { return true; } }

		/// <summary>
		/// Implement this property to indicate whether UI is suspendable.
		/// </summary>
		public sealed override bool isSuspendable { get { return true; } }

		/// <summary>
		/// Show UI instance.
		/// </summary>
		public Coroutine Show()
		{
			return UIManager.Show(this);
		}

		/// <summary>
		/// Hide UI instance.
		/// </summary>
		public Coroutine Hide()
		{
			return UIManager.Hide(this);
		}
		
		/// <summary>
		/// This function is called when the UI will be focus/defocus.
		/// </summary>
		sealed public override void OnFocus(bool focus)
		{
		}
	}
}