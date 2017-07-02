using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// User interface settings.
	/// </summary>
	[CreateAssetMenu]
	public class UIRootCanvasSettings : ScriptableObject
	{
		[Header("Camera")]
		public CameraClearFlags clearFlags = CameraClearFlags.Skybox;
		public Color backgroundColor;
		public LayerMask cullingMask = -1;
		public float depth;
		public float nearClipPlane = 0.3f;
		public float farClipPlane = 1000f;
		public float fieldOfView = 60f;
		public bool orthographic = true;
		public float orthographicSize = 5;


		[Header("Canvas")]
		public bool pixelPerfect;

		public float planeDistance = 100;

		public float normalizedSortingGridSize = 0.1f;

		public int sortingLayerID;

		public int sortingOrder;

		public int targetDisplay;
		
		public RenderMode renderMode;
		

		[Header("CanvasScaler")]
		[SerializeField, Tooltip("The amount of pixels per unit to use for dynamically created bitmaps in the UI, such as Text.")]
		public float dynamicPixelsPerUnit = 1;

		[SerializeField, Tooltip("The pixels per inch to use for sprites that have a 'Pixels Per Unit' setting that matches the 'Reference Pixels Per Unit' setting.")]
		public float defaultSpriteDPI = 96;

		[SerializeField, Tooltip("The DPI to assume if the screen DPI is not known.")]
		public float fallbackScreenDPI = 96;

		[Range(0, 1), SerializeField, Tooltip("Determines if the scaling is using the width or height as reference, or a mix in between.")]
		public float matchWidthOrHeight = 0;

		[SerializeField, Tooltip("The physical unit to specify positions and sizes in.")]
		public CanvasScaler.Unit physicalUnit = CanvasScaler.Unit.Points;

		[SerializeField, Tooltip("The resolution the UI layout is designed for. If the screen resolution is larger, the UI will be scaled up, and if it's smaller, the UI will be scaled down. This is done in accordance with the Screen Match Mode.")]
		public Vector2 referenceResolution = new Vector2(800, 600);

		[SerializeField, Tooltip("Scales all UI elements in the Canvas by this factor.")]
		public float scaleFactor = 1;

		[SerializeField, Tooltip("If a sprite has this 'Pixels Per Unit' setting, then one pixel in the sprite will cover one unit in the UI.")]
		public float referencePixelsPerUnit = 100;

		[SerializeField, Tooltip("Determines how UI elements in the Canvas are scaled.")]
		public CanvasScaler.ScaleMode uiScaleMode;

		[SerializeField, Tooltip("A mode used to scale the canvas area if the aspect ratio of the current resolution doesn't fit the reference resolution.")]
		public CanvasScaler.ScreenMatchMode screenMatchMode;


		[Header("OrderTracker")]
		public int childOrderInterval = 100;

		public int additiveOrder = 0;
	}
}