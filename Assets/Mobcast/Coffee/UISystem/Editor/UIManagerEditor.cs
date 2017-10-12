using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Mobcast.Coffee.UI;

namespace Mobcast.CoffeeEditor.UI
{

	/// <summary>
	/// UIマネージャのエディタ.
	/// </summary>
	[CustomEditor(typeof(UIManager))]
	public class UIManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();


//			UIManager manager = target as UIManager;

			GUILayout.Space(20);
			GUILayout.Label("インスタンスキャッシュ", EditorStyles.boldLabel);
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				foreach (var ui in UIManager.pooledObjects)
				{
					using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
					{
//						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.ObjectField(ui, typeof(UIBase), true);
							if (ui.isInitialized)
								GUILayout.Label(EditorGUIUtility.FindTexture("d_preaudioloopon"), GUILayout.Width(20));
							else
								GUILayout.Label(EditorGUIUtility.FindTexture("d_preaudioloopoff"), GUILayout.Width(20));
						}

//						if (GUILayout.Button(EditorGUIUtility.FindTexture("unityeditor.consolewindow"), EditorStyles.label))
//						{
//							Debug.Log(EditorJsonUtility.ToJson(ui.argument, false));
//						}

//						EditorGUILayout.Toggle("サスペンド?", ui.isResourceLoaded);
//						GUILayout.TextArea(EditorJsonUtility.ToJson(ui.argument, false));
					}
				}
			}


			GUILayout.Space(20);
			GUILayout.Label("アクティブ", EditorStyles.boldLabel);

			if (UIManager.currentScreen)
			{

				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
				{
					foreach (var ui in UIManager.currentScreen.GetEnumrator())
					{

						using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
						{
							GUILayout.Label(ui.name, EditorStyles.boldLabel);

							EditorGUILayout.ObjectField(ui, typeof(UIBase), true);
							if(ui.isTransiting)
								GUILayout.Label("遷移中", "sv_label_2");
							else if(ui.isShow)
								GUILayout.Label("表示済み", "sv_label_1");
							//else if(ui.isResourceLoaded)
								//GUILayout.Label("ロード済", "sv_label_5");
							else if(ui.isInitialized)
								GUILayout.Label("初期化済", "sv_label_6");
							else 
								GUILayout.Label("-", "sv_label_0");


//							EditorGUILayout.Toggle("初期化済?", ui.isInitialized);
//							EditorGUILayout.Toggle("ロード済?", ui.isResourceLoaded);
//							EditorGUILayout.Toggle("表示済?", ui.isMovedIn);
							GUILayout.TextArea(EditorJsonUtility.ToJson(ui.argument, false));
						}
					}
				}
			}


			GUILayout.Space(20);
			GUILayout.Label("スクリーン履歴", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				foreach (var arg in UIManager.screenHistory)
				{

					using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
					{
						GUILayout.Label(arg.path, EditorStyles.boldLabel);

						var ui = UIManager.pooledObjects.Find(x=>x.argument == arg);
						if (ui)
						{
							if (ui.isInitialized)
								GUILayout.Label(EditorGUIUtility.FindTexture("d_preaudioloopon"), GUILayout.Width(20));
							else
								GUILayout.Label(EditorGUIUtility.FindTexture("d_preaudioloopoff"), GUILayout.Width(20));
						}
						GUILayout.TextArea(EditorJsonUtility.ToJson(arg, false));
					}
				}
			}

			if (Application.isPlaying)
				Repaint();
		}
	}
}