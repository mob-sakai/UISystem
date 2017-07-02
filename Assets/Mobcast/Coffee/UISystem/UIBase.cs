using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// Argument for UI.
	/// </summary>
	[System.Serializable]
	public class UIArgument
	{
		/// <summary>
		/// Path.
		/// </summary>
		public string path = "";

		/// <summary>
		/// Indicating whether this UI is backed.
		/// </summary>
		public bool isBacked = false;
	}

	/// <summary>
	/// UIBase.
	/// </summary>
	[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(OrderTracker))]
	public abstract class UIBase : MonoBehaviour
	{
		/// <summary>
		/// Implement this property to indicate whether UI is cacheable.
		/// </summary>
		/// <value><c>true</c> if is cacheable; otherwise, <c>false</c>.</value>
		public abstract bool isCacheable { get; }

		/// <summary>
		/// Implement this property to indicate whether UI is suspendable.
		/// </summary>
		/// <value><c>true</c> if is suspendable; otherwise, <c>false</c>.</value>
		public abstract bool isSuspendable { get; }

		/// <summary>
		/// This function is called when the UI starts showing.
		/// Implement this function to show UI with animation.
		/// </summary>
		public abstract IEnumerator OnMoveIn();

		/// <summary>
		/// This function is called when the UI starts hiding.
		/// Implement this function to hide UI with animation.
		/// </summary>
		public abstract IEnumerator OnMoveOut();

		//オブジェクト生成など、１回すれば良い処理を行う
		//データバインディング
		//プレハブのインスタンス化
		//UIのエントリポイント
		//実行される前に、UIは非アクティブになっていることに注意
		/// <summary>
		/// This function is called when the UI instance is being loaded, and only once during the lifetime of the instance.
		/// Implement this function to initialize UI.
		/// </summary>
		public abstract IEnumerator OnInitialize();

		//オブジェクト破棄など、最後に１回すれば良い処理を行う
		//データバインディングの解除、Subscribeの解除など
		//このトリガーの後、オブジェクトは破棄される
		/// <summary>
		/// This function is called when the UI instance is being loaded, and only once during the lifetime of the instance.
		/// Implement this function to initialize UI.
		/// </summary>
//		public abstract IEnumerator OnFinalize();

		//リソースロードなど
		//APIも画面構成に必要な情報を持っているので、API待機もここで行う
		//実行時ロードが必要なアセット（バナー画像、アイテム画像）の読込み
		//UIキャッシュからロードされた場合は、こちらがエントリポイント

		/// <summary>
		/// This function is called before showing UI.
		/// Implement this function to load resources and instantiate other prefabs.
		/// </summary>
		public abstract IEnumerator OnLoadResource();

		//リソースアンロードなど
		//UIキャッシュに不要な画像（バナー画像、アイテム画像）があるばあい、こちらで破棄。
		/// <summary>
		/// This function is called after hiding UI.
		/// Implement this function to unload resources and decrease memory usage.
		/// </summary>
		public abstract IEnumerator OnUnloadResource();


		/// <summary>The RectTransform attached to this GameObject.</summary>
		public RectTransform cachedTransform
		{
			get
			{
				if (!m_Transform)
					m_Transform = transform as RectTransform;
				return m_Transform;
			}
		}

		RectTransform m_Transform;

		/// <summary>The OrderTracker attached to this GameObject.</summary>
		public OrderTracker orderTracker
		{
			get
			{
				if (!m_OrderTracker)
					m_OrderTracker = GetComponent<OrderTracker>() ?? gameObject.AddComponent<OrderTracker>();
				return m_OrderTracker;
			}
		}

		OrderTracker m_OrderTracker;

		/// <summary>The Canvas attached to this GameObject.</summary>
		public Canvas canvas
		{
			get
			{
				if (!m_Canvas)
					m_Canvas = GetComponent<Canvas>();
				return m_Canvas;
			}
		}

		Canvas m_Canvas;

		/// <summary>Argument for this UI.</summary>
		public UIArgument argument { get; set; }

		/// <summary>
		/// Indicating whether this UI is transiting.
		/// </summary>
		public virtual bool isTransiting { get; set; }

		/// <summary>
		/// Indicating whether this UI is initialized.
		/// </summary>
		public virtual bool isInitialized { get; set; }


		/// <summary>
		/// Indicating whether this UI is loaded.
		/// </summary>
		public virtual bool isResourceLoaded { get; set; }

		/// <summary>
		/// Indicating whether this UI is shown.
		/// </summary>
		public virtual bool isMovedIn { get; set; }

		protected UIBase()
		{
			UIManager.waitInitialize.Add(this);
		}

		/// <summary>
		/// Gets the enumrator of UI.
		/// </summary>
		public virtual IEnumerable<UIBase> GetEnumrator(System.Predicate<UIBase> predicate = null)
		{
			yield return this;
		}

		/// <summary>
		/// Fits by parent.
		/// </summary>
		public void FitByParent(Transform parent)
		{
			cachedTransform.SetParent(parent);

			cachedTransform.localPosition = Vector3.zero;
			cachedTransform.localScale = Vector3.one;
			cachedTransform.localRotation = Quaternion.identity;

			cachedTransform.anchorMax = Vector2.one;
			cachedTransform.anchorMin = Vector2.zero;
			cachedTransform.sizeDelta = Vector2.zero;
			cachedTransform.anchoredPosition = Vector2.zero;
			cachedTransform.pivot = Vector2.one / 2;
		}

	}
}