using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

//using UniRx;
using System.Collections.Generic;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// UIダイアログクラス.
	/// </summary>
	public abstract class UIDialog : UIBase
	{
		/// <summary>
		/// The relevant screen.
		/// </summary>
		public UIScreen screen { get; set; }

		/// <summary>
		/// Implement this property to indicate whether UI is cacheable.
		/// </summary>
		public override bool isPoolable { get { return false; } }

		/// <summary>
		/// Implement this property to indicate whether UI is suspendable.
		/// </summary>
		public sealed override bool isSuspendable { get { return false; } }


		/// <summary>
		/// Close this dialog.
		/// </summary>
		public virtual void Close()
		{
			UIManager.CloseDialog(this);
		}
		
		/// <summary>
		/// This function is called when the UI will be focus/defocus.
		/// </summary>
		public override void OnFocus(bool focus)
		{
			Debug.Log("<color=red>OnFocus</color> : " + this.name + ", " + focus, this);
		}
	}
}