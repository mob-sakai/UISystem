using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// スクリーン基底クラス.
	/// スクリーンはシーンに1つだけ存在するUIです.
	/// UIManagerからスクリーンを呼び出した場合、ひとつ前のスクリーンは破棄され、履歴に登録されます.
	/// </summary>
	public abstract class UIScreen : UIBase
	{
		/// <summary>
		/// Implement this property to indicate whether UI is cacheable.
		/// </summary>
		public override bool isCacheable { get { return false; } }

		/// <summary>
		/// Implement this property to indicate whether UI is suspendable.
		/// </summary>
		public override bool isSuspendable { get { return false; } }

		/// <summary>
		/// The dialogs.
		/// </summary>
		public List<UIDialog> dialogs = new List<UIDialog>();

		/// <summary>
		/// Gets the enumrator of UI.
		/// </summary>
		public override IEnumerable<UIBase> GetEnumrator(System.Predicate<UIBase> predicate = null)
		{
			yield return this;
			foreach (var ui in dialogs)
			{
				if (predicate == null || predicate(ui))
					yield return ui;
			}
		}
	}


}