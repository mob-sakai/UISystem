using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// Block raycast for UI when 'block condition' is exist.
	/// 
	/// UIバリアクラス.
	/// UIバリアは、UI遷移時におけるボタン押下を抑制する機能です.
	/// UI最前面に対してGraphicRaycastReceiverを設定することで、レイキャストをブロックします.
	/// </summary>
	[RequireComponent(typeof(GraphicRaycastReceiver))]
	public class UIRaycastBlocker : UISingleton<UIRaycastBlocker>
	{
		/// <summary>Sorting order.</summary>
		protected override int sortingOrder { get { return 32000; } }

		/// <summary>
		/// Block condition.
		/// </summary>
		[System.Serializable]
		public class Condition
		{
			/// <summary>
			/// Key for the block condition.
			/// </summary>
			public string key;

			/// <summary>
			/// When this method returns true, remove automatically the block condition..
			/// </summary>
			public Func<bool> predicate;
		}

		/// <summary>
		/// Block conditions.
		/// </summary>
		[SerializeField]
		List<Condition> m_Conditions = new List<Condition>();

		/// <summary>The GraphicRaycaster attached to this GameObject.</summary>
		GraphicRaycaster m_GraphicRaycaster;


		/// <summary>
		/// Add block condition.
		/// </summary>
		/// <param name="key">Key for the block condition.</param>
		public void AddCondition(string key)
		{
			AddCondition(key, null);
		}

		/// <summary>
		/// Adds block condition.
		/// </summary>
		/// <param name="key">Key for the block condition.</param>
		/// <param name="predicate">When this method returns true, remove automatically the block condition.</param>
		public void AddCondition(string key, Func<bool> predicate)
		{
			var cond = m_Conditions.Find(x => x.key == key);
			if (cond == null)
			{
				cond = new Condition(){ key = key };
				m_Conditions.Add(cond);
			}

			cond.predicate = predicate;
		}

		/// <summary>
		/// Remove block condition.
		/// </summary>
		/// <param name="key">Key for the block condition.</param>
		public void RemoveCondition(string key)
		{
			m_Conditions.RemoveAll(x => x.key == key);
		}

		/// <summary>
		/// Remove all block conditions.
		/// </summary>
		public void RemoveAll()
		{
			m_Conditions.Clear();
		}


		/// <summary>
		/// Contains block conditions.
		/// </summary>
		public bool ContainsCondition(string key)
		{
			return 0 <= m_Conditions.FindIndex(x => x.key == key);
		}

		/// <summary>
		/// LateUpdate is called every frame, if the Behaviour is enabled.
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (object.ReferenceEquals(m_GraphicRaycaster, null))
				m_GraphicRaycaster = GetComponent<GraphicRaycaster>();
			m_GraphicRaycaster.enabled = (m_Conditions.Count != 0);

			for (int i = 0; i < m_Conditions.Count;)
			{
				if (m_Conditions[i].predicate != null && m_Conditions[i].predicate())
					m_Conditions.RemoveAt(i);
				else
					i++;
			}
		}

		/// <summary>
		/// This function is called when the UI starts showing.
		/// Implement this function to show UI with animation.
		/// </summary>
		public override IEnumerator OnShow()
		{
			yield break;
		}

		/// <summary>
		/// This function is called when the UI starts hiding.
		/// Implement this function to hide UI with animation.
		/// </summary>
		public override IEnumerator OnHide()
		{
			yield break;
		}

		/// <summary>
		/// This function is called when the UI instance is being loaded, and only once during the lifetime of the instance.
		/// Implement this function to initialize UI.
		/// </summary>
		public override IEnumerator OnInitialize()
		{
			yield break;
		}
	
		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed or be pooled.
		/// Implement this function to finalize UI.
		/// </summary>
		public override IEnumerator OnFinalize()
		{
			yield break;
		}
	}
}