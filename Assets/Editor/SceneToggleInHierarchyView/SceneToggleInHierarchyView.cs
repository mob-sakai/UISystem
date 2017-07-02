#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;

namespace Mobcast.CoffeeEditor.Tool
{
	/// <summary>
	/// シーンロード切り替えトグル拡張(Editor拡張:ヒエラルキービュー).
	/// 非再生状態において、シーンオブジェクトの右端にトグルが追加されます.
	/// トグルにより、シーンのロード状態を切り替えられます.
	/// また、メニューに切り替えアクションを追加します.
	/// </summary>
	public class SceneToggleInHierarchyView
	{
		/// <summary>
		/// EditorSceneManager.GetSceneByHandleメソッド.
		/// インスタンスIDから、Sceneを取得します.
		/// </summary>
		static MethodInfo miGetSceneByHandle = typeof(EditorSceneManager).GetMethod("GetSceneByHandle", BindingFlags.NonPublic | BindingFlags.Static);

		/// <summary>
		/// ヒエラルキービューのGUIコールバックを追加します.
		/// 非再生状態において、シーンオブジェクトの右端にトグルが追加されます.
		/// トグルにより、シーンのロード状態を切り替えられます.
		/// </summary>
		[InitializeOnLoadMethod]
		static void OnHierarchyWindowItemOnGUI()
		{
			EditorApplication.hierarchyWindowItemOnGUI += (instanceID, rect) =>
			{
				//再生中、またはオブジェクトがシーン以外の場合はスキップ.
				if (Application.isPlaying || EditorUtility.InstanceIDToObject(instanceID))
					return;

				//インスタンスIDからシーンを取得.
				Scene scene = (Scene)miGetSceneByHandle.Invoke(null, new object[]{ instanceID });
				if (scene.isLoaded != GUI.Toggle(new Rect(rect.x + rect.width - 35, rect.y, 16, 16), scene.isLoaded, ""))
					ToggleSceneLoaded(scene);
			};
		}

		/// <summary>
		/// シーンロード状態を切り替えます.
		/// 非ロード状態になるとき、シーンを自動的に保存します.
		/// </summary>
		/// <param name="scene">シーン.</param>
		static void ToggleSceneLoaded(Scene scene)
		{
			//現在ロード済み.シーンを保存してunloadする.
			if (scene.isLoaded)
			{
				EditorSceneManager.SaveScene(scene);
				EditorSceneManager.CloseScene(scene, false);
			}
		//現在非ロード済み.シーンをloadする.
		else
				EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
		}

		/// <summary>
		/// 選択状態にあるシーンのロード状態を切り替えます(メニューコマンド).
		/// </summary>
		[MenuItem("Edit/Toggle Selected Scene #&s")]
		static void ToggleSceneLoadedFromMenu()
		{
			//再生中は何もしない.
			if (Application.isPlaying)
				return;
		//アクティブオブジェクトがシーンの場合、選択したシーンを切り替え.
		else if (!EditorUtility.InstanceIDToObject(Selection.activeInstanceID))
				ToggleSceneLoaded((Scene)miGetSceneByHandle.Invoke(null, new object[]{ Selection.activeInstanceID }));
		//アクティブゲームオブジェクトがある場合はオブジェクトがあるシーンを切り替え.
		else if (Selection.activeGameObject)
				ToggleSceneLoaded(Selection.activeGameObject.scene);
		}
	}
}
#endif