using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

#if EXAMPLE
//hogehoge
#endif

namespace Mobcast.Coffee.UI
{
#if UNITY_EDITOR
	[CustomEditor(typeof(UIRootCanvas))]
	public class UIRootCanvasEditor : Editor
	{
		/// <summary>
		/// Implement this function to make a custom inspector.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			SerializedProperty spSettings = serializedObject.FindProperty("m_Settings");

			EditorGUILayout.PropertyField(spSettings);

			using (new EditorGUI.DisabledGroupScope(!spSettings.objectReferenceValue))
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Space(EditorGUIUtility.labelWidth);

				if (GUILayout.Button("Save"))
				{
					(target as UIRootCanvas).Save();
					UIRootCanvas.list.ForEach(x => x.Load());
				}

				if (GUILayout.Button("Load"))
				{
					(target as UIRootCanvas).Load();
				}
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultCamera"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultEventSystem"));
			serializedObject.ApplyModifiedProperties();
		}
	}



#endif


	/// <summary>
	/// UIで利用するルートキャンバス.
	/// プロジェクト内におけるUI設定を、UISettingsで共有する.
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(OrderTracker))]
	public class UIRootCanvas : MonoBehaviour
	{
		/// <summary>The Canvas attached to this GameObject.</summary>
		public Canvas canvas { get { if (m_Canvas == null) m_Canvas = GetComponent<Canvas>(); return m_Canvas; } }
		Canvas m_Canvas;

		/// <summary>The CanvasScaler attached to this GameObject.</summary>
		public CanvasScaler canvasScaler { get { if (m_CanvasScaler == null) m_CanvasScaler = GetComponent<CanvasScaler>(); return m_CanvasScaler; } }
		CanvasScaler m_CanvasScaler;

		/// <summary>The OrderTracker attached to this GameObject.</summary>
		public OrderTracker orderTracker { get { if (m_OrderTracker == null) m_OrderTracker = GetComponent<OrderTracker>(); return m_OrderTracker; } }
		OrderTracker m_OrderTracker;

		/// <summary>Settings.</summary>
		public UIRootCanvasSettings settings { get { return m_Settings; } private set { m_Settings = value; } }
		[SerializeField] UIRootCanvasSettings m_Settings;

		/// <summary>Default Camera.</summary>
		public Camera defaultCamera { get { return m_DefaultCamera; } set { m_DefaultCamera = value; } }
		[SerializeField] Camera m_DefaultCamera;

		/// <summary>Default EventSystem.</summary>
		public EventSystem defaultEventSystem { get { return m_DefaultEventSystem; } set { m_DefaultEventSystem = value; } }
		[SerializeField] EventSystem m_DefaultEventSystem;

		public static UIRootCanvas main { get { return 0 < list.Count ? list[0] : null; } }

		public static readonly List<UIRootCanvas> list = new List<UIRootCanvas>();

		void OnEnable()
		{
			name = typeof(UIRootCanvas).Name;

#if UNITY_EDITOR
			if (!settings)
			{
				settings = AssetDatabase.FindAssets("t:" + typeof(UIRootCanvasSettings).Name)
					.Select(x => AssetDatabase.LoadAssetAtPath<UIRootCanvasSettings>(AssetDatabase.GUIDToAssetPath(x)))
					.FirstOrDefault();
			}
			if (!settings)
			{
				settings = ScriptableObject.CreateInstance<UIRootCanvasSettings>();
				Save();
				AssetDatabase.CreateAsset(settings, "Assets/UISettings.asset");
				AssetDatabase.SaveAssets();
			}
#endif

			if (!list.Contains(this))
				list.Add(this);

			OnMainRootCanvasChanged();
		}

		void OnDisable()
		{
			bool isMain = main == this;
			list.Remove(this);

			if (isMain)
			{
				list.ForEach(x => x.OnMainRootCanvasChanged());
			}
		}

		void OnMainRootCanvasChanged()
		{
			bool isMain = main == this;
			
			
			Debug.Log("#### OnMainRootCanvasChanged");
			Debug.Log(this.gameObject.scene.name,this);
			Debug.Log(isMain);
			Debug.Log(defaultCamera);
			Debug.Log(main.canvas.renderMode);
			Debug.Log(main.defaultCamera);
			
			canvas.renderMode = main.canvas.renderMode;
			canvas.worldCamera = main.defaultCamera;

#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(canvas);
#endif

			if (defaultCamera)
			{
				defaultCamera.enabled = isMain;
				
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(defaultCamera);
#endif
			}

			if (defaultEventSystem)
			{
				defaultEventSystem.enabled = isMain;
				
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(defaultEventSystem);
#endif
			}
			Load();

		}

		public void Load()
		{
			var s = settings;

			var cam = defaultCamera;
			if (cam)
			{
				cam.clearFlags = s.clearFlags;
				cam.backgroundColor = s.backgroundColor;
				cam.cullingMask = s.cullingMask;
				cam.orthographic = s.orthographic;
				cam.orthographicSize = s.orthographicSize;
				cam.farClipPlane = s.farClipPlane;
				cam.nearClipPlane = s.nearClipPlane;
				cam.fieldOfView = s.fieldOfView;
			}

			canvas.normalizedSortingGridSize = s.normalizedSortingGridSize;
			canvas.pixelPerfect = s.pixelPerfect;
			canvas.planeDistance = s.planeDistance;
			canvas.sortingLayerID = s.sortingLayerID;
			//			canvas.sortingLayerName = s.sortingLayerName;
			canvas.sortingOrder = s.sortingOrder;
			canvas.targetDisplay = s.targetDisplay;
			canvas.renderMode = s.renderMode;

			canvasScaler.referencePixelsPerUnit = s.referencePixelsPerUnit;
			canvasScaler.dynamicPixelsPerUnit = s.dynamicPixelsPerUnit;
			canvasScaler.defaultSpriteDPI = s.defaultSpriteDPI;
			canvasScaler.fallbackScreenDPI = s.fallbackScreenDPI;
			canvasScaler.matchWidthOrHeight = s.matchWidthOrHeight;
			canvasScaler.physicalUnit = s.physicalUnit;
			canvasScaler.referenceResolution = s.referenceResolution;
			canvasScaler.scaleFactor = s.scaleFactor;
			canvasScaler.uiScaleMode = s.uiScaleMode;
			canvasScaler.screenMatchMode = s.screenMatchMode;

			orderTracker.childOrderInterval = s.childOrderInterval;
			orderTracker.addSortingOrder = s.additiveOrder;
			orderTracker.ignoreParentTracker = true;

#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}


#if UNITY_EDITOR
		public void Save()
		{
			if (Application.isPlaying)
				return;

			var s = settings;

			var cam = defaultCamera;
			if (cam)
			{
				s.clearFlags = cam.clearFlags;
				s.backgroundColor = cam.backgroundColor;
				s.cullingMask = cam.cullingMask;
				s.orthographic = cam.orthographic;
				s.orthographicSize = cam.orthographicSize;
				s.farClipPlane = cam.farClipPlane;
				s.nearClipPlane = cam.nearClipPlane;
				s.fieldOfView = cam.fieldOfView;
			}

			s.normalizedSortingGridSize = canvas.normalizedSortingGridSize;
			s.pixelPerfect = canvas.pixelPerfect;
			s.planeDistance = canvas.planeDistance;
			s.sortingLayerID = canvas.sortingLayerID;
			//			s.sortingLayerName = canvas.sortingLayerName;
			s.sortingOrder = canvas.sortingOrder;
			s.targetDisplay = canvas.targetDisplay;
			s.renderMode = canvas.renderMode;

			s.referencePixelsPerUnit = canvasScaler.referencePixelsPerUnit;
			s.dynamicPixelsPerUnit = canvasScaler.dynamicPixelsPerUnit;
			s.defaultSpriteDPI = canvasScaler.defaultSpriteDPI;
			s.fallbackScreenDPI = canvasScaler.fallbackScreenDPI;
			s.matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
			s.physicalUnit = canvasScaler.physicalUnit;
			s.referenceResolution = canvasScaler.referenceResolution;
			s.scaleFactor = canvasScaler.scaleFactor;
			s.uiScaleMode = canvasScaler.uiScaleMode;
			s.screenMatchMode = canvasScaler.screenMatchMode;

			s.childOrderInterval = orderTracker.childOrderInterval;
			s.additiveOrder = orderTracker.addSortingOrder;


#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(s);
			AssetDatabase.SaveAssets();
#endif
		}
#endif
	}
}