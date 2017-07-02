using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using Mobcast.Coffee.UI;
using UnityEditorInternal;
using System.IO;
using System.Reflection;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class UITemplateWizard : EditorWindow
{
	string uiName = "";
	TextAsset templateAsset;

	static string outputDir
	{
		get
		{
			if (m_OutputDir == null)
			{
				m_OutputDir = EditorPrefs.GetString("UISystem_UITemplateWizard_OutputDir", "Assets");
			}
			return m_OutputDir;
		}
		set
		{
			if (m_OutputDir != value)
			{
				m_OutputDir = value;
				EditorPrefs.SetString("UISystem_UITemplateWizard_OutputDir", m_OutputDir);
			}
		}
	}

	static string m_OutputDir;

	string warning = "";

	static readonly Type[] s_UITypes = AppDomain.CurrentDomain.GetAssemblies()
		.SelectMany(x => x.GetTypes())
		.Where(x => typeof(UIBase).IsAssignableFrom(x))
		.ToArray();

	static readonly Dictionary<TextAsset, Type> s_TemplateInfoMap = new Dictionary<TextAsset, Type>();

	static readonly char[] s_GenericTypeChars = { '`', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

	/// <summary>
	/// Raises the focus event.
	/// </summary>
	void OnEnable()
	{
		titleContent.text = "UI Wizard";
	}

	/// <summary>
	/// Raises the focus event.
	/// </summary>
	void OnFocus()
	{
		s_TemplateInfoMap.Clear();

		var templateAssets = AssetDatabase.FindAssets("t:TextAsset")
			.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
			.Where(path => path.EndsWith(".txt"))
			.Select(path => AssetDatabase.LoadAssetAtPath<TextAsset>(path));

		Regex regUITemplate = new Regex(@"public[\s\n\r]+class[\s\n\r]+#SCRIPTNAME#[\s\n\r]+:[\s\n\r]+(\w+)");
		foreach (var asset in templateAssets)
		{
			Match match = regUITemplate.Match(asset.text);
			if (match != null)
			{
				var baseTypeName = match.Groups[1].Value;
				var baseType = s_UITypes.FirstOrDefault(x => x.Name.TrimEnd(s_GenericTypeChars) == baseTypeName);
				if (baseType != null && baseType.BaseType != typeof(UIBase))
					s_TemplateInfoMap.Add(asset, baseType);
			}
		}
		OnUINameChanged(false);
	}

	void OnGUI()
	{
		// There is no Template!
		if (s_TemplateInfoMap.Count == 0)
		{
			// 0. Create a template.
			GUILayout.Label("0. Create a template for Project", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			using (new EditorGUILayout.HorizontalScope())
			using (var cc = new EditorGUI.ChangeCheckScope())
			{
				uiName = EditorGUILayout.TextField("UI Sufix for the project", uiName);
				if (cc.changed)
					OnUINameChanged(false);
			}
			OutputDirectoryField();
			EditorGUI.indentLevel--;


			// [Warning] UI Name is incorrect.
			if (0 < warning.Length)
			{
				EditorGUILayout.HelpBox(warning, MessageType.Warning);
			}
			else
			{
				// 成果物表示
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
				{
					var sufix = uiName + ".cs";
					Texture textureCs = EditorGUIUtility.IconContent("cs script icon", null).image;
					GUILayout.Label(new GUIContent("UIScreen" + sufix, textureCs), GUILayout.Height(20));
					GUILayout.Label(new GUIContent("UIDialog" + sufix, textureCs), GUILayout.Height(20));
					GUILayout.Label(new GUIContent("UISingleton" + sufix, textureCs), GUILayout.Height(20));
				}

				if (GUILayout.Button("Create"))
				{
					CreateTemplates("UIScreen" + uiName, "UIScreenTemplate");
					CreateTemplates("UIDialog" + uiName, "UIDialogTemplate");
					CreateTemplates("UISingleton" + uiName, "UISingletonTemplate");
				}
			}
			return;
		}


		// 1. Select a template.
		GUILayout.Label("1. Select a template for UI", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		DrawTemplateField();
		EditorGUI.indentLevel--;

		// [Warning] Template is not selected.
		Type baseType;
		if (!templateAsset || !s_TemplateInfoMap.TryGetValue(templateAsset, out baseType))
		{
			EditorGUILayout.HelpBox("Select a template.", MessageType.Warning);
			return;
		}

		// 2. Input a unique name.
		GUILayout.Space(20);
		GUILayout.Label("2. Input a unique name for new " + baseType.Name, EditorStyles.boldLabel);

		EditorGUI.indentLevel++;
		using (new EditorGUILayout.HorizontalScope())
		using (var cc = new EditorGUI.ChangeCheckScope())
		{
			uiName = EditorGUILayout.TextField("UI Name", uiName);
			if (cc.changed)
				OnUINameChanged();
		}
		EditorGUI.indentLevel--;

		// [Warning] UI Name is incorrect.
		if (0 < warning.Length)
		{
			EditorGUILayout.HelpBox(warning, MessageType.Warning);
			return;
		}


		// 3. Create new UI.
		GUILayout.Space(20);
		GUILayout.Label("3. Create " + uiName + " as following.", EditorStyles.boldLabel);

		EditorGUI.indentLevel++;
		OutputDirectoryField();
		EditorGUI.indentLevel--;

		// 成果物表示
		using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
		{
			Texture textureCs = EditorGUIUtility.IconContent("cs script icon", null).image;
			Texture textureScene = EditorGUIUtility.IconContent("sceneasset icon", null).image;
			Texture textureGo = EditorGUIUtility.IconContent("gameobject icon", null).image;

			GUILayout.Label(new GUIContent(uiName + ".cs", textureCs), GUILayout.Height(20));
			GUILayout.Label(new GUIContent(uiName, textureGo), GUILayout.Height(20));
			if (typeof(UIScreen).IsAssignableFrom(baseType) || typeof(UIDialog).IsAssignableFrom(baseType))
				GUILayout.Label(new GUIContent(uiName + ".unity", textureScene), GUILayout.Height(20));
		}

		if (GUILayout.Button("Create"))
		{
			CreateNewUIWithTemplate(templateAsset, baseType, uiName);
			OnUINameChanged();
		}
	}

	static void OutputDirectoryField()
	{
		// Output directory
		bool open = false;
		using (new EditorGUILayout.HorizontalScope())
		{
			outputDir = EditorGUILayout.TextField("Output Directory", outputDir);
			if (GUILayout.Button(EditorGUIUtility.FindTexture("project"), EditorStyles.label, GUILayout.Width(20)))
			{
				open = true;
			}
		}

		if (open)
		{
			var path = EditorUtility.OpenFolderPanel("", outputDir, outputDir);
			if (!string.IsNullOrEmpty(path))
			{
				outputDir = path.Replace(Environment.CurrentDirectory + Path.DirectorySeparatorChar, "");
			}
		}
	}

	static void CreateTemplates(string typeName, string templateBaseName)
	{
		var templateBase = AssetDatabase.FindAssets(templateBaseName + " t:TextAsset")
				.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
				.First();

		string scriptPath = Path.Combine(outputDir, typeName + ".cs");
		Directory.CreateDirectory(Path.GetDirectoryName(scriptPath));
		miCreateScriptAssetFromTemplate.Invoke(null, new object[] { scriptPath, templateBase });

		//TODO: 基底スクリプト細工
		string script = File.ReadAllText(scriptPath);
		script = new Regex(@"public class " + typeName).Replace(script, "public abstract class " + typeName);
		script = new Regex(@"yield return .*").Replace(script, "yield break;");
		script = new Regex(@"(\w+) : (\w+)<\w+>").Replace(script, "$1<T> : $2<T> where T : $2<T>");
		File.WriteAllText(scriptPath, script);
		AssetDatabase.ImportAsset(scriptPath);


		//TODO: 継承テンプレート細工
		string tmpPath = "Assets/Editor/UITemplates/" + typeName + ".txt";
		string txt = File.ReadAllText(templateBase);
		txt = new Regex(@"(#SCRIPTNAME# : )\w+").Replace(txt, "$1" + typeName);
		txt = new Regex(@"(\s+)(.*)UIArgument").Replace(txt, "$1new $2" + typeName + ".Argument");

		// Save
		Directory.CreateDirectory(Path.GetDirectoryName(tmpPath));
		File.WriteAllText(tmpPath, txt);
		AssetDatabase.ImportAsset(tmpPath);
	}

	/// <summary>
	/// Raises the template changed event.
	/// </summary>
	/// <param name="newtemplate">Newtemplate.</param>
	void OnTemplateChanged(TextAsset newtemplate)
	{
		templateAsset = newtemplate;

		Type baseType;
		if (templateAsset && s_TemplateInfoMap.TryGetValue(templateAsset, out baseType))
		{
			uiName = baseType.Name.TrimEnd(s_GenericTypeChars);
			OnUINameChanged();
		}
	}

	/// <summary>
	/// Raises the user interface name changed event.
	/// </summary>
	void OnUINameChanged(bool checkAssets = true)
	{
		warning = "";
		if (uiName.Length == 0)
		{
			warning = "名前つけろや！";
		}
		else if (!new Regex(@"^[A-z0-9_]+$").IsMatch(uiName))
		{
			warning = "名前にへんなもんまじってる！英数字とアンスコのみしかダメ！！！！！";
		}
		else if (checkAssets && AssetDatabase.FindAssets(uiName)
			.Select(x => Path.GetFileName(AssetDatabase.GUIDToAssetPath(x)))
			.Any(x => x == uiName + ".unity" || x == uiName + ".cs"))
		{
			warning = "名前がダブってるシーンかスクリプトがある！";
		}
	}


	/// <summary>
	/// Draws the style asset field.
	/// </summary>
	/// <param name="property">Property.</param>
	/// <param name="onSelect">On select.</param>
	void DrawTemplateField()
	{
		Rect r = EditorGUILayout.GetControlRect();

		// Object field.
		Rect rField = new Rect(r.x, r.y, r.width - 16, r.height);
		EditorGUI.BeginChangeCheck();
		templateAsset = EditorGUI.ObjectField(rField, "Template Asset", templateAsset, typeof(TextAsset), false) as TextAsset;
		if (EditorGUI.EndChangeCheck())
			OnTemplateChanged(templateAsset);

		// Popup to select template in project.
		Rect rPopup = new Rect(r.x + rField.width, r.y + 4, 16, r.height - 4);
		if (GUI.Button(rPopup, EditorGUIUtility.FindTexture("icon dropdown"), EditorStyles.label))
		{
			EditorGUIUtility.keyboardControl = 0;
			GenericMenu gm = new GenericMenu();
			foreach (var pair in s_TemplateInfoMap)
				gm.AddItem(new GUIContent(pair.Key.name), pair.Key == templateAsset, o => OnTemplateChanged(o as TextAsset), pair.Key);
			gm.ShowAsContext();
		}

	}




	/// <summary>
	/// Creates the new user interface with template.
	/// </summary>
	/// <param name="template">Template.</param>
	/// <param name="type">Type.</param>
	/// <param name="uiName">User interface name.</param>
	public static void CreateNewUIWithTemplate(TextAsset template, Type type, string uiName)
	{
		Directory.CreateDirectory(outputDir);

		// If create UIScreen or UIDialog, need a scene.
		if (typeof(UIScreen).IsAssignableFrom(type) || typeof(UIDialog).IsAssignableFrom(type))
		{
			//新しいシーンを生成.
			// Create a new scene and add Camera.
			Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

			// Save the scene and append it to the build scenes.
			string scenePath = Path.Combine(outputDir, Path.ChangeExtension(uiName, "unity"));
			EditorSceneManager.SaveScene(scene, scenePath);
			EditorBuildSettings.scenes = EditorBuildSettings.scenes.Concat(new[] { new EditorBuildSettingsScene(scene.path, true) }).ToArray();
			EditorApplication.delayCall += () => EditorSceneManager.SaveScene(scene, scenePath);


			// Create a Camera.
			new GameObject("Camera", typeof(Camera), typeof(EditorOnly)).GetComponent<Camera>();

			//すでに存在するEventSystemを無効化.
			// Create a EventSystem object from menu.
			List<EventSystem> evs = UnityEngine.Object.FindObjectsOfType<EventSystem>()
				.Where(x=>x.gameObject.activeInHierarchy)
				.ToList();
			evs.ForEach(x => x.gameObject.SetActive(false));
			EditorApplication.ExecuteMenuItem("GameObject/UI/Event System");
			Selection.activeGameObject.AddComponent<EditorOnly>();
			evs.ForEach(x => x.gameObject.SetActive(true));

			//ルートキャンバスを追加.
			// Create a new GameObject for UIRootCanvas.
			EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
			Selection.activeGameObject.SetActive(false);
			var rc = Selection.activeGameObject.AddComponent<UIRootCanvas>();

			rc.defaultEventSystem = UnityEngine.Object.FindObjectsOfType<EventSystem>().FirstOrDefault(x => x.gameObject.scene == rc.gameObject.scene);
			rc.defaultCamera = Camera.allCameras.FirstOrDefault(x => x.gameObject.scene == rc.gameObject.scene);
			rc.gameObject.SetActive(true);
		}
		else
		{
			//UIManagerを検索. 無かったらルートキャンバス.
			UIManager uim = UnityEngine.Object.FindObjectOfType<UIManager>();
			if (uim && UIManager.uiRootCanvas)
			{
				Selection.activeGameObject = UIManager.uiRootCanvas.gameObject;
			}
			// RootCanvasがあればアクティブとして選択
			// Select rootCanvas as activeGameObject.
			else if (Selection.activeGameObject)
			{
				Canvas c = Selection.activeGameObject.GetComponentInParent<Canvas>();
				if (c)
					Selection.activeGameObject = c.rootCanvas.gameObject;
			}
		}


		//UIScreenのアタッチ先となるゲームオブジェクトを生成.
		// Create new GameObject for UI with required components.
		EditorApplication.ExecuteMenuItem("GameObject/UI/Panel");
		ComponentUtility.DestroyComponentsMatching(Selection.activeGameObject, c => !(c is Transform));
		GameObject go = Selection.activeGameObject;

		go.name = Path.GetFileNameWithoutExtension(uiName);
		type.GetCustomAttributes(typeof(RequireComponent), true)
			.OfType<RequireComponent>()
			.SelectMany(x => new[] { x.m_Type0, x.m_Type1, x.m_Type2 })
			.Distinct()
			.Where(x => x != null && !go.GetComponent(x))
			.ToList()
			.ForEach(x => go.AddComponent(x));


		//新しいスクリーンクラスをテンプレートから生成.
		// Create new script for UI from template.
		string scriptPath = Path.Combine(outputDir, uiName + ".cs");
		miCreateScriptAssetFromTemplate.Invoke(null, new object[] { scriptPath, AssetDatabase.GetAssetPath(template) });
		AssetDatabase.Refresh();

		//スクリプトを初期化.
		//コンパイル前のコンポーネントをアタッチ.
		// Initialize the script and add component to the GameObject before compile.
		MonoScript monoScript = AssetDatabase.LoadAssetAtPath(scriptPath, typeof(MonoScript)) as MonoScript;
		miSetScriptTypeWasJustCreatedFromComponentMenu.Invoke(monoScript, null);
		miAddScriptComponentUncheckedUndoable.Invoke(null, new object[] { go, monoScript });

		EditorGUIUtility.PingObject(go);
	}

	static readonly MethodInfo miCreateScriptAssetFromTemplate =
		typeof(ProjectWindowUtil)
			.GetMethod("CreateScriptAssetFromTemplate", BindingFlags.Static | BindingFlags.NonPublic);

	static readonly MethodInfo miSetScriptTypeWasJustCreatedFromComponentMenu =
		typeof(MonoScript)
			.GetMethod("SetScriptTypeWasJustCreatedFromComponentMenu", BindingFlags.Instance | BindingFlags.NonPublic);

	static readonly MethodInfo miAddScriptComponentUncheckedUndoable =
		typeof(InternalEditorUtility)
			.GetMethod("AddScriptComponentUncheckedUndoable", BindingFlags.Static | BindingFlags.NonPublic);
}
