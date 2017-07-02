using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Mobcast.Coffee.UI;
using System.Reflection;

namespace Mobcast.CoffeeEditor.UI
{
	[CustomEditor(typeof(OrderTracker))]
	[CanEditMultipleObjects]
	public class OrderTrackerEditor : Editor
	{
		static GUIContent contentEnableMaskRect2D;
		static GUIContent contentDisableMaskRect2D;
		static GUIContent contentEnableCanvasGroup;
		static GUIContent contentDisableCanvasGroup;

		public override void OnInspectorGUI()
		{
			if (contentEnableMaskRect2D == null)
			{
				contentEnableMaskRect2D = new GUIContent(" Clipped (MaskRect2D)", EditorGUIUtility.FindTexture("testpassed"), "The Renderer will be clipped by MaskRect2D.");
				contentDisableMaskRect2D = new GUIContent(" Clipped (MaskRect2D)", EditorGUIUtility.FindTexture("console.warnicon.sml"), "Required '_ClipRect_n' properties in shader.\nRequired RectTransform component.");
				contentEnableCanvasGroup = new GUIContent(" Modified Alpha (CanvasGroup)", EditorGUIUtility.FindTexture("testpassed"), "The Renderer will be modified alpha by CanvasGroup.");
				contentDisableCanvasGroup = new GUIContent(" Modified Alpha (CanvasGroup)", EditorGUIUtility.FindTexture("console.warnicon.sml"), "Required '_Alpha' property in shader.");
			}


			OrderTracker current = target as OrderTracker;
			bool isAutoOrderedByParent = current.parent && current.parent.childOrderInterval != 0;

			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IgnoreParentTracker"));

			GUILayout.Space(EditorGUIUtility.singleLineHeight);
			EditorGUILayout.LabelField("Sorting Tracking", EditorStyles.boldLabel);
			using (new EditorGUI.DisabledGroupScope(isAutoOrderedByParent))
			{
				if (current.parent)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AddSortingOrder"), new GUIContent("Sorting Order"));
				}
				else
				{
					var spSortingLayer = serializedObject.FindProperty("m_SortingLayerID");
					SortingLayerField(new GUIContent(spSortingLayer.displayName), spSortingLayer, EditorStyles.popup, EditorStyles.label);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AddSortingOrder"));
				}
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ChildOrderInterval"));
			//		DrawDefaultInspector();
			serializedObject.ApplyModifiedProperties();

			//using (new EditorGUI.DisabledGroupScope(true))
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				int indentLevel = EditorGUI.indentLevel;
				if (current.parent)
				{
					OrderTrackerField(current.parent);
					EditorGUI.indentLevel++;
				}

				OrderTrackerField(current);
				EditorGUI.indentLevel++;

				foreach (var child in current.children.OrderBy(x => x.addSortingOrder))
					OrderTrackerField(child);

				EditorGUI.indentLevel = indentLevel;
			}


			// Draw rendering options.
			if (current.cachedRenderer)
			{
				GUILayout.Space(EditorGUIUtility.singleLineHeight);
				EditorGUILayout.LabelField("Rendering Options", EditorStyles.boldLabel);
				using (new EditorGUILayout.VerticalScope("helpbox"))
				{
					GUILayout.Label(current.isClippable ? contentEnableMaskRect2D : contentDisableMaskRect2D);
					GUILayout.Label(current.isTransparent ? contentEnableCanvasGroup : contentDisableCanvasGroup);
				}
			}
		}

		void OrderTrackerField(OrderTracker tracker)
		{
			if (!tracker)
				return;


			Rect r = EditorGUILayout.GetControlRect();

			if (tracker == target)
			{
				//GUI.enabled = true;
				Rect rBG = new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4);
				GUI.Label(rBG, "", "SelectionRect");
				//GUI.enabled = false;
			}
			//Debug.Log(EditorStyles.foldout..background);


			Rect rLable = new Rect(r.x, r.y, 100, r.height);
			string label = string.Format(" {0}{1} [{2:+#;-#;+0}]", tracker == target ? "(Self) " : "", tracker.sortingOrder, tracker.addSortingOrder);
			EditorGUI.LabelField(rLable, new GUIContent(tracker == target ? "<b>" + label + "</b>" : label), "IN Foldout");

			Rect rField = new Rect(r.x + rLable.width, r.y, r.width - rLable.width, r.height);
			EditorGUI.ObjectField(
				rField,
				//string.Format("{0} : {1} [{2:+#;-#;+0}]", label, tracker.sortingOrder, tracker.addSortingOrder),
				tracker,
				typeof(OrderTracker),
				true);
		}

		public static void SortingLayerField(GUIContent label, SerializedProperty layerID, GUIStyle style, GUIStyle labelStyle)
		{
			MethodInfo methodInfo = typeof(EditorGUILayout).GetMethod("SortingLayerField", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(GUIContent), typeof(SerializedProperty), typeof(GUIStyle), typeof(GUIStyle) }, null);

			if (methodInfo != null)
			{
				object[] parameters = { label, layerID, style, labelStyle };
				methodInfo.Invoke(null, parameters);
			}
		}
	}
}