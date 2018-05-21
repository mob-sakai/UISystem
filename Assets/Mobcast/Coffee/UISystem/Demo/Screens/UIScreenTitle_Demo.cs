using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mobcast.Coffee.UI;

public class UIScreenTitle_Demo : UIScreen
{
	[SerializeField] Animator anim;

	public class Argument : UIArgument
	{
	}

	public override IEnumerator OnShow ()
	{
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



	public void StartGame()
	{
		UIManager.RequestScreen ("UIScreenHome_Demo", new UIScreenHome_Demo.Argument());
	}
}
