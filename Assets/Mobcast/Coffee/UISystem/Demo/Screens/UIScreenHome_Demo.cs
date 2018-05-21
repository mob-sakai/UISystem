using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mobcast.Coffee.UI;

public class UIScreenHome_Demo : UIScreen
{
	[SerializeField] Animator anim;

	public class Argument : UIArgument
	{
	}

	public override IEnumerator OnShow ()
	{
		yield return anim.PlayAndWait (UISystem_DemoUtil.AnimatorHash_Show);
	}

	public override IEnumerator OnHide ()
	{
		yield return anim.PlayAndWait (UISystem_DemoUtil.AnimatorHash_Hide);
	}

	public override IEnumerator OnInitialize ()
	{
		yield break;
	}

	public override IEnumerator OnFinalize ()
	{
		yield break;
	}



	public void ToTitle ()
	{
		
		UIManager.RequestDialog ("UIDialogMessage_Demo", new UIDialogMessage_Demo.Argument () {
			title = "Back to Title",
			message = "Back to title?",
			onYes = () => {
				UIManager.BackScreen ();
			},
			onNo = () => {
			}
		});
	}

	public void OpenDialog ()
	{
		UIManager.RequestDialog ("UIDialogMessage_Demo", new UIDialogMessage_Demo.Argument () {
			title = "dialog test",
			message = "message",
			onYes = () => {
			},
		});
	}
}
