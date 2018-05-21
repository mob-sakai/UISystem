using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mobcast.Coffee.UI;

public class UISystem_DemoMain : MonoBehaviour
{
	// Entry point.
	void Start ()
	{
		UIManager.RequestScreen ("UIScreenTitle_Demo", new UIScreenTitle_Demo.Argument ());
	}
}
