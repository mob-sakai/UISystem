using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;
using Mobcast.Coffee.UI;
using System.IO;
using System.Reflection;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mobcast.CoffeeEditor.UI
{
	public class UISystemMenuOption : MonoBehaviour
	{
		/// <summary>
		/// UISystemのセットアップ
		/// </summary>
		[UnityEditor.MenuItem("Coffee/UI/Setup UISystem", false, 0)]
		static void Setup()
		{
			// シーン内にEventSystemが存在しない場合は作成
			if (null == Object.FindObjectOfType<EventSystem>())
			{
				EditorApplication.ExecuteMenuItem("GameObject/UI/Event System");
				Selection.activeGameObject.GetComponent<EventSystem>();
			}


			// シーン内にUIManagerが存在しない場合は作成
			if (null == Object.FindObjectOfType<UIManager>())
			{
				new GameObject("UIManager").AddComponent<UIManager>();
			}


			// シーン内にUIRootCanvasが存在しない場合は作成
			var rc = Object.FindObjectOfType<UIRootCanvas>();
			if (null == rc)
			{
				EditorApplication.ExecuteMenuItem("GameObject/UI/Panel");
				var go = Selection.activeGameObject.transform.parent.gameObject;
				DestroyImmediate(Selection.activeGameObject);
				Selection.activeGameObject = go;
				go.SetActive(false);
				rc = go.AddComponent<UIRootCanvas>();
			}
			rc.defaultEventSystem = UnityEngine.Object.FindObjectsOfType<EventSystem>().FirstOrDefault(x => x.gameObject.scene == rc.gameObject.scene);
			rc.defaultCamera = Camera.allCameras.FirstOrDefault(x => x.gameObject.scene == rc.gameObject.scene);
			rc.gameObject.SetActive(true);
		}

		[MenuItem("Coffee/UI/UISetting", false, 1)]
		static void SelectUISetting()
		{
			Selection.activeObject = AssetDatabase.FindAssets("t:" + typeof(UIRootCanvasSettings).Name)
				.Select(x => AssetDatabase.LoadAssetAtPath<UIRootCanvasSettings>(AssetDatabase.GUIDToAssetPath(x)))
				.FirstOrDefault();
			
			if(!Selection.activeObject)
			{
				var settings = ScriptableObject.CreateInstance<UIRootCanvasSettings>();
				AssetDatabase.CreateAsset(settings, "Assets/UISettings.asset");
				AssetDatabase.SaveAssets();
				Selection.activeObject = settings;
			}
		}

		[MenuItem("Coffee/UI/Create UI from Template ...", false, 20)]
		static void CreateUIFromTemplate()
		{
			EditorWindow.GetWindow<UITemplateWizard>();
		}
	}
}