using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

//using UniRx;
//using UniRx.Triggers;
//using DG.Tweening;
//using RequestProcedure = Mobcast.Coffee.UI.UISettings.RequestProcedure;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// ユーザーインターフェースマネージャ.
	/// 画面のリクエスト、ナビゲーションを管理します.
	/// </summary>
	public class UIManager : MonoSingleton<UIManager>
	{
		
		/// <summary>カレントスクリーン.</summary>
		public static UIScreen currentScreen { get; protected set; }

		/// <summary>UI履歴.リクエストされたスクリーン・ダイアログの履歴を管理します.</summary>
		public static List<UIArgument> screenHistory { get { return instance.m_ScreenHistory; } }

		List<UIArgument> m_ScreenHistory = new List<UIArgument>();


		public static List<UIBase> cachedObjects { get { return instance.m_CachedObjects; } }

		List<UIBase> m_CachedObjects = new List<UIBase>();

		static readonly List<GameObject> s_GameObject = new List<GameObject>();

		public static List<UIBase> waitInitialize = new List<UIBase>();


		/// <summary>
		/// デフォルトで利用するCanvas.
		/// デフォルトでは、UILayerのルートTransformは、このキャンバスの下に生成されます.
		/// </summary>
		public static UIRootCanvas uiRootCanvas { get { return instance ? instance.m_UIRootCanvas : null; } }

		[SerializeField]UIRootCanvas m_UIRootCanvas;




		void Start()
		{
			// 
			UIRaycastBlocker.instance.Show();
		}





		//############################################################
		//############################################################
		//
		//シングルトン系
		//
		//############################################################
		//############################################################

		/// <summary>
		/// Moves the in.
		/// </summary>
		/// <returns>The in.</returns>
		/// <param name="ui">User interface.</param>
		public static Coroutine MoveIn(UIBase ui)
		{
			return instance.StartCoroutine(CoIn(ui));
		}

		/// <summary>
		/// Moves the out.
		/// </summary>
		/// <returns>The out.</returns>
		/// <param name="ui">User interface.</param>
		public static Coroutine MoveOut(UIBase ui)
		{
			return instance.StartCoroutine(CoOut(ui));
		}


		//############################################################
		//############################################################
		//
		//ダイアログ系
		//
		//############################################################
		//############################################################

		/// <summary>
		/// Closes the dialog.
		/// </summary>
		/// <returns>The dialog.</returns>
		public static Coroutine CloseDialog()
		{
			return instance.StartCoroutine(CoOut(currentScreen.dialogs.LastOrDefault()));
		}

		/// <summary>
		/// Closes the dialog.
		/// </summary>
		/// <returns>The dialog.</returns>
		/// <param name="dialog">Dialog.</param>
		public static Coroutine CloseDialog(UIDialog dialog)
		{
			return instance.StartCoroutine(CoOut(dialog, false, true));
		}

		/// <summary>
		/// Requests the dialog.
		/// </summary>
		/// <returns>The dialog.</returns>
		/// <param name="path">Path.</param>
		/// <param name="arg">Argument.</param>
		public static Coroutine RequestDialog(string path, UIArgument arg = null)
		{
			arg = arg ?? new UIArgument();
			if (!string.IsNullOrEmpty(path))
				arg.path = path;

			return instance.StartCoroutine(CoIn(arg, typeof(UIDialog)));
		}



		//############################################################
		//############################################################
		//
		//スクリーン系
		//
		//############################################################
		//############################################################

		/// <summary>
		/// Backs the screen.
		/// </summary>
		/// <returns>The screen.</returns>
		public static Coroutine BackScreen()
		{
			return BackScreen(screenHistory[screenHistory.Count - 2].path);
		}

		/// <summary>
		/// Backs the screen.
		/// </summary>
		/// <returns>The screen.</returns>
		/// <param name="path">Path.</param>
		public static Coroutine BackScreen(string path)
		{
			var index = screenHistory.FindIndex(x => x.path == path);
			if (index < 0)
				return null;

			//シュリンク
			Debug.LogFormat("{1:D6} &&&& [UIM] Find in History ! : {0}", path, Time.frameCount);
			var arg = screenHistory[index];
			for (int i = screenHistory.Count - 1; index < i; i--)
			{
				//シュリンク対象でサスペンド済みの場合、アウト
				instance.StartCoroutine(CoOut(GetCachedObject(screenHistory[i].path, true), false, true));
				screenHistory.RemoveAt(i);
			}

			arg.isBacked = true;
			return instance.StartCoroutine(CoIn(arg, typeof(UIScreen)));
		}

		/// <summary>
		/// Requests the screen.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="arg">Argument.</param>
		/// <param name="enableResume">If set to <c>true</c> enable resume.</param>
		public static Coroutine RequestScreen(string path, UIArgument arg = null, bool enableResume = true)
		{
			arg = arg ?? new UIArgument();
			if (!string.IsNullOrEmpty(path))
				arg.path = path;

			if (currentScreen && currentScreen.argument.path == arg.path)
			{
				Debug.LogError("同じスクリーンをリクエストしてる！");
				return null;
			}

			//レジューム可能なら、履歴チェック
			return (enableResume ? BackScreen(path) : null)
			?? instance.StartCoroutine(CoIn(arg, typeof(UIScreen)));
		}





		static IEnumerator CoIn(UIArgument arg, System.Type type)
		{
			string cond = "CoIn_" + arg.path;
			UIRaycastBlocker.instance.AddCondition(cond);

			Debug.LogWarningFormat("#### CoIn stat {0}", arg.path);
			//			screenHistory.Select(x => x.path).LogDump();
			//			cachedObject.Select(x => x.name).LogDump();

			// スクリーンの場合、アウト実行
			Coroutine coOut = null;
			if (typeof(UIScreen).IsAssignableFrom(type))
			{
				// スクリーンの場合、履歴に追加
				if (!arg.isBacked)
				{
					screenHistory.Add(arg);
				}

				// アウト実行
				// Backのばあい、サスペンドしない。
				coOut = instance.StartCoroutine(CoOut(currentScreen, !arg.isBacked, true));
				currentScreen = null;
			}

			//キャッシュから取得.
			UIBase uiIn = GetCachedObject(arg.path, false);//cachedObject.FirstOrDefault(ui => ui.argument.path == arg.path);
			if (uiIn)
			{
				Debug.LogFormat("{1:D6} >>>> [UIM] Find Instance in Cache : {0}", arg.path, Time.frameCount);
			}
			else
			{
				//シーンロード可能.
				int index = SceneUtility.GetBuildIndexByScenePath(arg.path);
				if (0 <= index)
				{
					//スクリーンシーンのロード開始.
					Debug.LogFormat("{1:D6} >>>> [UIM] Load Scene : {0}", arg.path, Time.frameCount);
					yield return SceneManager.LoadSceneAsync(arg.path, LoadSceneMode.Additive);
					uiIn = waitInitialize.Find(x => x && x.gameObject.scene.name == arg.path && type.IsInstanceOfType(x));

					//ダイアログの場合、シーンは破棄.
					if (typeof(UIDialog).IsAssignableFrom(type))
					{
						uiIn.transform.SetParent(null);
						SceneManager.MoveGameObjectToScene(uiIn.gameObject, currentScreen.gameObject.scene);
						SceneManager.UnloadSceneAsync(arg.path);
					}
				}
				//Resourcesからロード
				else
				{
					Debug.LogFormat("{1:D6} >>>> [UIM] Load Prefab : {0}", arg.path, Time.frameCount);
					var req = Resources.LoadAsync<GameObject>(arg.path);
					yield return req;
					var go = Object.Instantiate(req.asset, currentScreen.canvas.rootCanvas.transform) as GameObject;
					uiIn = go.GetComponent<UIBase>();
				}
			}

			uiIn.argument = arg;

			//スクリーンの場合、カレントを変更
			if (uiIn is UIScreen)
			{
				currentScreen = uiIn as UIScreen;
				uiIn.orderTracker.ignoreParentTracker = false;

				//TODO: List使うオーバーロードに切り替え
				uiIn.gameObject.scene.GetRootGameObjects(s_GameObject);
				foreach (var go in s_GameObject)
					go.SetActive(true);
				s_GameObject.Clear();
			}
			else if (uiIn is UIDialog)
			{
				//TODO: Transformのキャッシュ
				//TODO: UIRootCanvasのキャッシュ
				uiIn.FitByParent(currentScreen.canvas.rootCanvas.transform);
				uiIn.cachedTransform.SetAsLastSibling();

				currentScreen.dialogs.Add(uiIn as UIDialog);
				(uiIn as UIDialog).screen = currentScreen;
				uiIn.orderTracker.ignoreParentTracker = false;
			}

			yield return instance.StartCoroutine(CoIn(uiIn, coOut));
			UIRaycastBlocker.instance.RemoveCondition(cond);
		}




		static IEnumerator CoIn(UIBase uiIn, Coroutine coOut = null)
		{
			if (!uiIn)
				yield break;

			string cond = "CoIn_Instance_" + uiIn.name;

			if (UIRaycastBlocker.instance.ContainsCondition(cond))
			{
				yield break;
			}

			UIRaycastBlocker.instance.AddCondition(cond);
			cachedObjects.Remove(uiIn);

//			string cond = "CoIn_" + uiIn.name;
//			UIRaycastBlocker.instance.Add(cond);

			//スクリーンの場合、カレントを変更
			if (uiIn is UIScreen)
				currentScreen = uiIn as UIScreen;

			var uis = uiIn.GetEnumrator().ToList();

			// 初期化していないUIを全て初期化
			if (!uiIn.isInitialized)
			{
				Debug.LogFormat("{1:D6} >>>> [UIM] OnInitialize : {0}", uiIn.name, Time.frameCount);
				uis.ForEach(ui => ui.gameObject.SetActive(false));
				yield return WaitUITrigger(uis.Where(ui => !ui.isInitialized), ui => ui.OnInitialize());
				uis.ForEach(ui => ui.isInitialized = true);
			}


			// ロードしていないUIを全てロード
			if (!uiIn.isResourceLoaded)
			{
				Debug.LogFormat("{1:D6} >>>> [UIM] OnLoadResource : {0}", uiIn.name, Time.frameCount);
				yield return WaitUITrigger(uis.Where(ui => !ui.isResourceLoaded), ui => ui.OnLoadResource());
				uis.ForEach(ui => ui.isResourceLoaded = true);
			}

			// アウト待ち
			if (coOut != null)
				yield return coOut;


			//イン
			if (!uiIn.isMovedIn)
			{
				Debug.LogFormat("{1:D6} >>>> [UIM] OnMoveIn : {0}", uiIn.name, Time.frameCount);
				uis.ForEach(ui => ui.isTransiting = true);
				uis.ForEach(ui => ui.gameObject.SetActive(true));
				yield return WaitUITrigger(uis.Where(ui => !ui.isMovedIn), ui => ui.OnMoveIn());
				uis.ForEach(ui => ui.isTransiting = false);
				uis.ForEach(ui => ui.isMovedIn = true);
			}

			Debug.LogFormat("{1:D6} >>>> [UIM] Complete CoIn : {0}", uiIn.name, Time.frameCount);
			UIRaycastBlocker.instance.RemoveCondition(cond);

		}

		static IEnumerator CoOut(UIBase uiOut, bool suspend = true, bool cache = true)
		{
			if (!uiOut)
				yield break;

			string cond = "CoOut_" + uiOut.name;


			if (UIRaycastBlocker.instance.ContainsCondition(cond))
			{
				yield break;
			}
			
			UIRaycastBlocker.instance.AddCondition(cond);


			//アウト
			var uis = uiOut.GetEnumrator().ToList();
			if (uis.Any(ui => ui.isMovedIn))
			{
				Debug.LogFormat("{1:D6} >>>> [UIM] OnMoveOut : {0}", uiOut.name, Time.frameCount);
				uis.ForEach(ui => ui.isTransiting = true);
				yield return WaitUITrigger(uis.Where(ui => ui.isMovedIn), ui => ui.OnMoveOut());
				uis.ForEach(ui => ui.isTransiting = false);
				uis.ForEach(ui => ui.isMovedIn = false);
			}
			uis.ForEach(ui => ui.gameObject.SetActive(false));


			//			//サスペンド
			if (suspend)
			{
				uis.ForEach(SuspendObject);
				if (uiOut.isSuspendable)
				{
					Debug.LogFormat("{1:D6} [UIM] Complete CoOut by Suspend : {0}", uiOut.name, Time.frameCount);
					UIRaycastBlocker.instance.RemoveCondition(cond);
					yield break;
				}
			}

			// アンロード
			uis.ForEach(ui => ui.gameObject.SetActive(false));
			Debug.LogFormat("{1:D6} [UIM] OnUnloadResource : {0}", uiOut.name, Time.frameCount);
			yield return WaitUITrigger(uis.Where(ui => ui.isResourceLoaded), ui => ui.OnUnloadResource());
			uis.ForEach(ui => ui.isResourceLoaded = false);
			uis.ForEach(ResetObject);

			// キャッシュ
			if (cache)
			{
				uis.ForEach(CacheObject);
				uis.ForEach(ui => DestroyObject(ui, false));

				if (uiOut.isCacheable)
				{
					Debug.LogFormat("{1:D6} [UIM] Complete CoOut by Cache : {0}", uiOut.name, Time.frameCount);
					UIRaycastBlocker.instance.RemoveCondition(cond);
					yield break;
				}
			}

			// 削除
			Debug.LogFormat("{1:D6} [UIM] OnFinalize : {0}", uiOut.name, Time.frameCount);
			uis.ForEach(ui => waitInitialize.Remove(ui));
//			yield return WaitUITrigger(uis.Where(ui => ui.isInitialized), ui => ui.OnFinalize());
			uis.ForEach(ui => ui.isInitialized = false);
			uis.ForEach(ui => DestroyObject(ui, false));

			Debug.LogFormat("{1:D6} [UIM] Complete CoOut : {0}", uiOut.name, Time.frameCount);
			UIRaycastBlocker.instance.RemoveCondition(cond);
		}




		static void DestroyObject(UIBase ui, bool forceDestroy = false)
		{
			if (forceDestroy || !cachedObjects.Contains(ui))
			{
				cachedObjects.Remove(ui);

				//スクリーンはシーン破棄だが、ダイアログの場合はオブジェクト破棄のみでOK
				if (ui is UIScreen)
				{
					SceneManager.UnloadSceneAsync(ui.gameObject.scene);
				}
				else
				{
					Object.Destroy(ui.gameObject);
				}
			}
		}

		static void ResetObject(UIBase ui)
		{
			//ダイアログの場合、UIRootCanvas.mainにキャッシュ.
			if (ui is UIScreen)
			{
				(ui as UIScreen).dialogs.Clear();
			}
			else if (ui is UIDialog)
			{
				UIDialog dialog = ui as UIDialog;
				if (dialog && dialog.screen)
				{
					dialog.screen.dialogs.Remove(dialog);
					dialog.screen = null;
				}
			}
		}

		static void CacheObject(UIBase ui)
		{
			if (!ui.isCacheable)
				return;

			ui.argument = new UIArgument(){ path = ui.argument.path };
			if (!cachedObjects.Contains(ui))
				cachedObjects.Add(ui);
			
			//ダイアログの場合、UIRootCanvas.mainにキャッシュ.
			if (ui is UIScreen)
			{
				//スクリーンの場合、シーンのルートオブジェクトを無効化するだけ。
				ui.gameObject.scene.GetRootGameObjects(s_GameObject);
				foreach (var go in s_GameObject)
					go.SetActive(false);
				s_GameObject.Clear();
			}
			else
			{
				ui.cachedTransform.SetParent(uiRootCanvas.transform);
				ui.cachedTransform.SetAsLastSibling();
			}
		}

		static void SuspendObject(UIBase ui)
		{
			if (!ui.isSuspendable)
				return;

//			ui.argument = new UIArgument(){ path = ui.argument.path };
			if (!cachedObjects.Contains(ui))
				cachedObjects.Add(ui);

			//ダイアログの場合、UIRootCanvas.mainにキャッシュ.
			if (ui is UIScreen)
			{
				//スクリーンの場合、シーンのルートオブジェクトを無効化するだけ。
				ui.gameObject.scene.GetRootGameObjects(s_GameObject);
				foreach (var go in s_GameObject)
					go.SetActive(false);
				s_GameObject.Clear();
			}
		}




		/// <summary>
		/// Gets the cached object.
		/// </summary>
		/// <returns>The cached object.</returns>
		/// <param name="path">Path.</param>
		/// <param name="onlySuspended">If set to <c>true</c> suspended.</param>
		static UIBase GetCachedObject(string path, bool onlySuspended)
		{
			UIBase ret = null;
			var index = cachedObjects.FindIndex(x => x.argument.path == path && (!onlySuspended || x.isResourceLoaded));
			if (0 <= index)
			{
				ret = cachedObjects[index];
				cachedObjects.RemoveAt(index);
			}
			return ret;
		}


		public static void ClearCache()
		{
//			UIBase[] uis = cachedObject.Concat(screenHistory.Select(h => h.suspendedObject)).Where(ui => ui).ToArray();
			Debug.LogFormat("{1:D6} #### [UIM] ClearCache : {0}", cachedObjects.Count, Time.frameCount);
			foreach (var ui in cachedObjects.ToList())
			{
				//キャッシュなしでアウト.
				Debug.LogFormat("{1:D6} #### [UIM] ClearCache : {0}", ui.argument.path, Time.frameCount);
				instance.StartCoroutine(CoOut(ui, false, false));
			}
			cachedObjects.Clear();
			screenHistory.Clear();
		}


		static Coroutine WaitUITrigger(IEnumerable<UIBase> uis, System.Func<UIBase, IEnumerator> selector)
		{
			return instance.StartCoroutine(CoWaitUITrigger(uis, selector));
		}


		static IEnumerator CoWaitUITrigger(IEnumerable<UIBase> uis, System.Func<UIBase, IEnumerator> selector)
		{
			foreach (var co in uis.Select(x=>instance.StartCoroutine(selector(x))).ToArray())
				yield return co;
		}
	}
}