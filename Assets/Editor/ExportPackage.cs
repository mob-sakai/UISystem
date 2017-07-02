using UnityEditor;

namespace Mobcast.Coffee.UI
{
	public static class ExportPackage
	{
		const string kPackageName = "UISystem.unitypackage";
		static readonly string[] kAssetPathes = {
			"Assets/Mobcast/Coffee/UISystem",
		};

		[MenuItem ("Export Package/" + kPackageName)]
		[InitializeOnLoadMethod]
		static void Export ()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;
			
			AssetDatabase.ExportPackage (kAssetPathes, kPackageName, ExportPackageOptions.Recurse | ExportPackageOptions.Default);
			UnityEngine.Debug.Log ("Export successfully : " + kPackageName);
		}
	}
}