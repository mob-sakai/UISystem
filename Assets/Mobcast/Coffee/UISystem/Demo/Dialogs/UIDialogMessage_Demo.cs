using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mobcast.Coffee.UI;
using UnityEngine.UI;

public class UIDialogMessage_Demo : UIDialog
{
	[SerializeField] Animator anim;
	[SerializeField] Text title;
	[SerializeField] Text message;
	[SerializeField] Button buttonToYes;
	[SerializeField] Button buttonToNo;

	public class Argument : UIArgument
	{
		public string title;
		public string message;
		public System.Action onYes;
		public System.Action onNo;
	}

	public override IEnumerator OnShow ()
	{

		var arg = argument as Argument;
		buttonToNo.gameObject.SetActive (arg.onNo != null);

		yield return anim.PlayAndWait(UISystem_DemoUtil.AnimatorHash_Show);
	}

	public override IEnumerator OnHide ()
	{
		yield return anim.PlayAndWait(UISystem_DemoUtil.AnimatorHash_Hide);
	}

	public override IEnumerator OnInitialize ()
	{
		yield break;
	}

	public override IEnumerator OnFinalize ()
	{
		yield break;
	}



	public void Yes()
	{
		Close ();
		var arg = argument as Argument;
		if (arg.onYes != null) {
			arg.onYes ();
		}
	}

	public void No()
	{
		Close ();
		var arg = argument as Argument;
		if (arg.onNo != null) {
			arg.onNo ();
		}
	}
}
