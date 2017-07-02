using UnityEngine;
using UnityEngine.UI;

namespace Mobcast.Coffee.UI
{
	#if UNITY_EDITOR
	using UnityEditor;

	[CustomEditor(typeof(GraphicRaycastReceiver), true)]
	sealed class GraphicRaycastReceiverEditor : Editor
	{
		/// <summary>
		/// Implement this function to make a custom inspector.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastTarget"));
			serializedObject.ApplyModifiedProperties();
		}
	}
	#endif

	/// <summary>
	/// A dummy Graphic.
	/// This component is not rendered nothing, but received GraphicRaycast.
	/// </summary>
	public class GraphicRaycastReceiver : MaskableGraphic
	{
		/// <summary>
		/// Callback function when a UI element needs to generate vertices.
		/// </summary>
		/// <param name="vh">VertexHelper utility.</param>
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
		}
	}
}