using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif


namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// SortingOrderTacker.
	/// OrderTacker tracks the parent's sorting order.
	/// When parent's sortingOrder has changed, modify own sorting order.
	/// If the renderer has '_Alpha' property, adjust alpha with CanvasGroup.
	/// If the renderer has '_ClipRect_x' properties, clip the renderer with RectMask2D.
	/// </summary>
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class OrderTracker : UIBehaviour, IClippable
	{
		static List<OrderTracker> s_TempRelatables = new List<OrderTracker>();

		/// <summary>
		/// Sorting order.
		/// It is equal to [parent sortingOrder] + [own addSortingOrder]
		/// </summary>
		public int sortingOrder { get { return m_Parent ? m_Parent.sortingOrder + addSortingOrder : addSortingOrder; } }
		public int sortingLayerID { get { return m_Parent ? m_Parent.m_SortingLayerID : m_SortingLayerID; } }
		public OrderTracker root { get { return m_Parent ? m_Parent.root : this; } }


		/// <summary>
		/// Should the OrderTracker ignore parent OrderTrackers?
		/// </summary>
		public bool ignoreParentTracker
		{
			get { return m_IgnoreParentTracker; }
			set
			{
				if (m_IgnoreParentTracker != value)
				{
					m_IgnoreParentTracker = value;
					OnTransformParentChanged();
				}
			}
		}

		[Tooltip("Should the OrderTracker ignore parent OrderTrackers?")]
		[SerializeField]
		bool m_IgnoreParentTracker = false;

		/// <summary>
		/// Value to add to sortingOrder.
		/// </summary>
		public int addSortingOrder
		{
			get { return m_AddSortingOrder; }
			set
			{
				if (m_AddSortingOrder != value)
				{
					m_AddSortingOrder = value;
					SetDirty();
				}
			}
		}


		[SerializeField] int m_SortingLayerID;


		[Tooltip("Value to add to sortingOrder.")]
		[SerializeField]
		int m_AddSortingOrder;

		/// <summary>
		/// Automatically set intervaled sortingOrder for direct children in hierarchy.
		/// </summary>
		/// <value>The child order interval.</value>
		public int childOrderInterval
		{
			get { return m_ChildOrderInterval; }
			set
			{
				if (m_ChildOrderInterval != value)
				{
					m_ChildOrderInterval = value;
					SetDirty();
				}
			}
		}


		[Tooltip("子を自動で整列させるかどうか＆sortingOrder間隔")]
		[SerializeField]
		int m_ChildOrderInterval;

		/// <summary>The Transform attached to this GameObject.</summary>
		public RectTransform rectTransform
		{
			get
			{
				if (!m_RectTransform)
					m_RectTransform = transform as RectTransform;
				return m_RectTransform;
			}
		}

		RectTransform m_RectTransform;


		public Rect rootCanvasRect
		{
			get
			{
				rectTransform.GetWorldCorners(s_Corners);
				if (canvas)
				{
					Canvas rootCanvas = canvas.rootCanvas;
					Transform rootTransform = rootCanvas.transform;
					for (int i = 0; i < 4; i++)
					{
						s_Corners[i] = rootTransform.InverseTransformPoint(s_Corners[i]);
					}
				}
				return new Rect(s_Corners[0].x, s_Corners[0].y, s_Corners[2].x - s_Corners[0].x, s_Corners[2].y - s_Corners[0].y);
			}
		}

		public Canvas canvas
		{
			get
			{
				s_TempCanvases.Clear();
				GetComponentsInParent<Canvas>(false, s_TempCanvases);
				m_Canvas = null;
				for (int i = 0; i < s_TempCanvases.Count; i++)
				{
					if (s_TempCanvases[i].isActiveAndEnabled)
					{
						m_Canvas = s_TempCanvases[i];
						break;
					}
				}
				s_TempCanvases.Clear();
				return m_Canvas;
			}
		}

		Canvas m_Canvas;


		/// <summary>Parent tracker.</summary>
		public OrderTracker parent { get { return m_Parent; } }

		OrderTracker m_Parent;

		/// <summary>Children trackers.</summary>
		public List<OrderTracker> children { get { return m_Children; } }

		List<OrderTracker> m_Children = new List<OrderTracker>();

		/// <summary>The Renderer attached to this GameObject.</summary>
		public Renderer cachedRenderer { get; private set; }

		/// <summary>Is the Renderer clippable.</summary>
		public bool isClippable { get { return cachedRenderer && cachedRenderer.sharedMaterial && cachedRenderer.sharedMaterial.HasProperty(s_ClipRectId[0]) && rectTransform; } }
		public bool isTransparent { get { return cachedRenderer && cachedRenderer.sharedMaterial && cachedRenderer.sharedMaterial.HasProperty(s_AlphaId); } }

		/// <summary>The Canvas attached to this GameObject.</summary>
		public Canvas cachedCanvas { get; private set; }

		RectMask2D m_ParentMask;

		/// <summary>Returns true if the OrderTracker is modifed.</summary>
		bool m_IsDirty = true;

		bool m_IsAlphaDirty = true;
		float m_FinalAlpha = 1;

		readonly List<RectMask2D> m_Masks = new List<RectMask2D>();

		static List<CanvasGroup> s_TempCanvasGroups = new List<CanvasGroup>();

		static List<Canvas> s_TempCanvases = new List<Canvas>();

		static int[] s_ClipRectId;

		static int s_AlphaId;

		static MaterialPropertyBlock s_PropertyBlock;
		static Vector3[] s_Corners = new Vector3[4];


		//vvvvvvvv  Unity Callbacks  vvvvvvvv
		/// <summary>
		/// Called when the script instance is being loaded.
		/// </summary>
		protected override void Awake()
		{
			cachedCanvas = GetComponent<Canvas>();
			cachedRenderer = GetComponent<Renderer>();
		}

		/// <summary>
		/// Called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			//m_Masks = null;
			m_Masks.Clear();
			SetParent(null);
		}


		/// <summary>
		/// Called when the object becomes enabled and active.
		/// </summary>
		protected override void OnEnable()
		{
			if (s_ClipRectId == null)
			{
				s_ClipRectId = new int[] {
					Shader.PropertyToID ("_ClipRect_0"),
					Shader.PropertyToID ("_ClipRect_1"),
					Shader.PropertyToID ("_ClipRect_2"),
					Shader.PropertyToID ("_ClipRect_3"),
				};

				s_AlphaId = Shader.PropertyToID("_Alpha");
				s_PropertyBlock = new MaterialPropertyBlock();
			}

			// Reset all children OrderTrackers.
			GetComponentsInChildren<OrderTracker>(true, s_TempRelatables);
			for (int i = 0; i < s_TempRelatables.Count; i++)
			{
				OrderTracker tracker = s_TempRelatables[i];
				if ((tracker == this || !tracker.ignoreParentTracker))
				{
					tracker.OnTransformParentChanged();
				}
			}
			s_TempRelatables.Clear();

			RecalculateClipping();
			SetDirty();
		}

		protected override void OnDisable()
		{
			RecalculateClipping();
			ApplyClipping();
		}

		/// <summary>
		/// Called every frame, if the Behaviour is enabled.
		/// </summary>
		void LateUpdate()
		{
			// If OrderTracker is dirty, recalculate sorting order.
			if (m_IsDirty)
			{
				m_IsDirty = false;
				OrderTracker rootTracker = root;
				rootTracker.ApplySortingOrder(rootTracker.sortingOrder, rootTracker.sortingLayerID);
			}

			if (m_IsAlphaDirty)
			{
				RecalculateAlpha();
				ApplyAlpha();
			}
		}

		/// <summary>
		/// Called when the state of the parent Canvas is changed.
		/// </summary>
		protected override void OnCanvasHierarchyChanged()
		{
			RecalculateClipping();
			SetDirty();
		}

		/// <summary>
		/// Raises the canvas group changed event.
		/// </summary>
		protected override void OnCanvasGroupChanged()
		{
			m_IsAlphaDirty = true;
		}

		/// <summary>
		/// Called when the parent property of the transform of the GameObject has changed.
		/// </summary>
		protected override void OnTransformParentChanged()
		{
			RecalculateClipping();
			SetDirty();

			// If ignoreParentTracker is true, the parent is not set.
			if (ignoreParentTracker)
			{
				SetParent(null);
			}
			else
			{
				//Find the nearest parent OrderTracker from transform hierarchy.
				var parentTransform = transform.parent;
				OrderTracker newParent = null;
				while (parentTransform && newParent == null)
				{
					newParent = parentTransform.GetComponent<OrderTracker>();
					parentTransform = parentTransform.parent;
				}
				SetParent(newParent);
			}

		}
		//^^^^^^^^  Unity Callbacks  ^^^^^^^^


		//vvvvvvvv  IClipable Implements  vvvvvvvv
		/// <summary>
		/// Called when the state of a parent IClippable changes.
		/// </summary>
		public void RecalculateClipping()
		{
			m_Masks.Clear();

			RectMask2D rectMask2D = isActiveAndEnabled && rectTransform ? MaskUtilities.GetRectMaskForClippable(this) : null;
			if (m_ParentMask != null && (rectMask2D != m_ParentMask || !rectMask2D.IsActive()))
			{
				m_ParentMask.RemoveClippable(this);
			}
			if (rectMask2D != null && rectMask2D.IsActive())
			{
				rectMask2D.AddClippable(this);
				MaskUtilities.GetRectMasksForClip(rectMask2D, m_Masks);
			}
			m_ParentMask = rectMask2D;
		}

		/// <summary>
		/// Clip and cull the IClippable given the clipRect.
		/// </summary>
		/// <param name="clipRect">Rectangle to clip against.</param>
		/// <param name="validRect">Is the Rect valid. If not then the rect has 0 size.</param>
		public void Cull(Rect clipRect, bool validRect)
		{
			ApplyClipping();
		}


		/// <summary>
		/// Set the clip rect for the IClippable.
		/// </summary>
		/// <param name="clipRect">Rectangle to clip against.</param>
		/// <param name="validRect">Is the Rect valid. If not then the rect has 0 size.</param>
		public void SetClipRect(Rect clipRect, bool validRect)
		{
		}

		//^^^^^^^^  IClipable Implements  ^^^^^^^^

		/// <summary>
		/// Set the parent of the OrderTracker.
		/// </summary>
		/// <param name="newParent">The parent OrderTracker to use..</param>
		protected void SetParent(OrderTracker newParent)
		{
			// The parent is not changed.
			if (m_Parent == newParent || this == newParent)
				return;

			// Detach from old parent.
			if (m_Parent && m_Parent.m_Children.Contains(this))
			{
				m_Parent.m_Children.Remove(this);
				m_Parent.m_Children.RemoveAll(x => x == null);
				m_Parent.SetDirty();
			}

			// Attach to new parent.
			m_Parent = newParent;
			if (m_Parent)
			{
				m_Parent.SetDirty();
				if (!m_Parent.m_Children.Contains(this))
					m_Parent.m_Children.Add(this);
			}
		}


		/// <summary>
		/// Set SortingOrder.
		/// </summary>
		/// <param name="order">Order.</param>
		void ApplySortingOrder(int order, int layerId)
		{
			m_IsDirty = false;

			// Set the sorting order of Canvas.
			if (cachedCanvas)
			{
				cachedCanvas.sortingOrder = order;
				cachedCanvas.overrideSorting = m_AddSortingOrder != 0;
				cachedCanvas.sortingLayerID = layerId;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(cachedCanvas);
#endif
			}

			// Set the sorting order of Renderer.
			if (cachedRenderer)
			{
				cachedRenderer.sortingOrder = order;
				cachedRenderer.sortingLayerID = layerId;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(cachedRenderer);
#endif
			}

			// Set sorting order of children OrderTrackers.
			foreach (var child in m_Children)
			{
				if (child.ignoreParentTracker)
					continue;

				// Automatically set sorting order for direct children.
				if (0 < m_ChildOrderInterval && child.rectTransform.parent == rectTransform)
					child.addSortingOrder = child.rectTransform.GetSiblingIndex() * m_ChildOrderInterval;

				child.ApplySortingOrder(order + child.addSortingOrder, layerId);
			}
		}

		/// <summary>
		/// Calculate the final alpha for own CanvasGroup.
		/// </summary>
		void RecalculateAlpha()
		{
			m_IsAlphaDirty = false;

			m_FinalAlpha = 1;
			GetComponentsInParent<CanvasGroup>(false, s_TempCanvasGroups);
			for (int i = 0; i<s_TempCanvasGroups.Count; i++)
			{
				m_FinalAlpha *= s_TempCanvasGroups[i].alpha;
				if (s_TempCanvasGroups[i].ignoreParentGroups)
					break;
			}
			s_TempCanvasGroups.Clear();
		}


		void ApplyAlpha()
		{
			//対応シェーダ以外なら処理しない.'_Alpha'プロパティを持つことが条件.
			if (!cachedRenderer || !cachedRenderer.sharedMaterial || !cachedRenderer.sharedMaterial.HasProperty(s_AlphaId))
				return;

			//アルファをオーバーライド.
			// Overide alpha property.
			cachedRenderer.GetPropertyBlock(s_PropertyBlock);
			s_PropertyBlock.SetFloat(s_AlphaId, m_FinalAlpha);
			cachedRenderer.SetPropertyBlock(s_PropertyBlock);
		}


		void ApplyClipping()
		{
			//対応シェーダ以外なら処理しない.'_ClipRect_0'プロパティを持つことが条件.
			// If the shader does not have '_ClipRect_0' property, do nothing.
			if (!cachedRenderer || !cachedRenderer.sharedMaterial || !cachedRenderer.sharedMaterial.HasProperty(s_ClipRectId[0]))
				return;


			//ワールド座標におけるクリップ領域を設定
			// Update clipping rect in world space. Maximum, four 'RectMask2D' are valid.
			cachedRenderer.GetPropertyBlock(s_PropertyBlock);
			for (int i = 0; i < 4; i++)
			{
				Vector4 rect = new Vector4(float.MinValue, float.MaxValue, float.MinValue, float.MaxValue);
				if (i < m_Masks.Count)
				{
					RectMask2D mask = m_Masks[i];
					if (mask && mask.isActiveAndEnabled)
					{
						(mask.transform as RectTransform).GetWorldCorners(s_Corners);
						rect.Set(s_Corners[0].x, s_Corners[2].x, s_Corners[0].y, s_Corners[2].y);
					}
				}
				s_PropertyBlock.SetVector(s_ClipRectId[i], rect);
			}
			cachedRenderer.SetPropertyBlock(s_PropertyBlock);
		}
		
		
		/// <summary>
		/// Mark the OrderTracker as dirty.
		/// </summary>
		void SetDirty()
		{
			m_IsDirty = true;
			m_IsAlphaDirty = true;
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
		
#if UNITY_EDITOR
		/// <summary>
		/// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
		/// </summary>
		protected override void OnValidate()
		{
			OnTransformParentChanged();
		}

		/// <summary>
		/// Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time.
		/// This function is only called in editor mode. Reset is most commonly used to give good default values in the inspector.
		/// </summary>
		protected override void Reset()
		{
			cachedCanvas = GetComponent<Canvas>();
			cachedRenderer = GetComponent<Renderer>();

			var graphic = GetComponent<Graphic>();
			if (graphic && !cachedCanvas)
			{
				cachedCanvas = gameObject.AddComponent<Canvas>();
			}
		}
#endif

	}
}