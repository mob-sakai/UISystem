using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// Component for Editor.
	/// GameObjects with this component can be deactivated or destroyed on play in Editor.
	/// Also, 'EditorOnly' tag is automatically set to the GameObject, and the GameObject is not included in build.
	/// </summary>
	[ExecuteInEditMode]
	public class EditorOnly : MonoBehaviour
	{
		/// <summary>
		/// Action on play.
		/// </summary>
		public enum ActionOnPlay
		{
			/// <summary>No action on play.</summary>
			None,
			/// <summary>Deactive GameObject on play.</summary>
			Deactive,
			/// <summary>Destroy GameObject on play.</summary>
			Destroy,
		}

		[SerializeField]
		ActionOnPlay actionOnPlay = ActionOnPlay.Destroy;

		//vvvvvvvv  Unity Callbacks  vvvvvvvv
		void Awake()
		{
			#if UNITY_EDITOR
			if (enabled && Application.isPlaying)
			{
				switch (actionOnPlay)
				{
					case ActionOnPlay.Deactive:
						gameObject.SetActive(false);
						break;
					case ActionOnPlay.Destroy:
						Destroy(gameObject);
						break;
				}
			}
			#else
			Destroy(gameObject);
			#endif
		}

		#if UNITY_EDITOR
		static readonly Regex reg = new Regex("<.*>");
		string lastName = "";

		void OnValidate()
		{
			// When the property changes, tag as 'EditorOnly' and set name suffix to '<xxx On Play>'.
			gameObject.tag = "EditorOnly";
			gameObject.name = string.Format("{0}<{1} On Play>", reg.Replace(gameObject.name, ""), actionOnPlay);
			lastName = gameObject.name;
		}

		void Update()
		{
			// When the name of gameObject has changed, fix the name. 
			if (!Application.isPlaying && lastName != gameObject.name)
				OnValidate();
		}
		#endif
		//^^^^^^^^  Unity Callbacks  ^^^^^^^^
	}
}