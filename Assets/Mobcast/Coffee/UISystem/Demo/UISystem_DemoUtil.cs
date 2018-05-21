using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UISystem_DemoUtil
{
	public static readonly int AnimatorHash_Show = Animator.StringToHash("Show");
	public static readonly int AnimatorHash_Hide = Animator.StringToHash("Hide");

	public static IEnumerator PlayAndWait(this Animator self, int hash)
	{
		self.Play(hash);
		self.Update(0);

		AnimatorStateInfo info = self.GetCurrentAnimatorStateInfo(0);
		while (info.shortNameHash == hash && !self.IsInTransition(0) && info.normalizedTime < 1)
		{
			yield return null;
			info = self.GetCurrentAnimatorStateInfo(0);
		}

		yield break;
	}
}
