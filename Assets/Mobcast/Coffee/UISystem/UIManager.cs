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


		public static List<UIBase> pooledObjects { get { return instance.m_CachedObjects; } }

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
		public static Coroutine Show(UIBase ui)
		{
			return instance.StartCoroutine(CoShow(ui));
		}

		/// <summary>
		/// Moves the out.
		/// </summary>
		/// <returns>The out.</returns>
		/// <param name="ui">User interface.</param>
		public static Coroutine Hide(UIBase ui)
		{
			return instance.StartCoroutine(CoHide(ui));
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
			return instance.StartCoroutine(CoHide(currentScreen.dialogs.LastOrDefault()));
		}

		/// <summary>
		/// Closes the dialog.
		/// </summary>
		/// <returns>The dialog.</returns>
		/// <param name="dialog">Dialog.</param>
		public static Coroutine CloseDialog(UIDialog dialog)
		{
			return instance.StartCoroutine(CoHide(dialog, false, true));
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

			return instance.StartCoroutine(CoShow(arg, typeof(UIDialog)));
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
				instance.StartCoroutine(CoHide(GetPooledObject(screenHistory[i].path, true), false, true));
				screenHistory.RemoveAt(i);
			}

			arg.isBacked = true;
			return instance.StartCoroutine(CoShow(arg, typeof(UIScreen)));
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
			?? instance.StartCoroutine(CoShow(arg, typeof(UIScreen)));
		}





		static IEnumerator CoShow(UIArgument arg, System.Type type)
		{
			string cond = "CoShow_" + arg.path;
			UIRaycastBlocker.instance.AddCondition(cond);

			Debug.LogWarningFormat("#### CoShow stat {0}", arg.path);

			// スクリーンの場合、アウト実行
			Coroutine coHide = null;
			if (typeof(UIScreen).IsAssignableFrom(type))
			{
				// スクリーンの場合、履歴に追加
				if (!arg.isBacked)
				{
					screenHistory.Add(arg);
				}

				// アウト実行
				// Backのばあい、サスペンドしない。
				coHide = instance.StartCoroutine(CoHide(currentScreen, !arg.isBacked, true));
				currentScreen = null;
			}

			//キャッシュから取得.
			UIBase uiShow = GetPooledObject(arg.path, false);
			if (uiShow)
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
					uiShow = waitInitialize.Find(x => x && x.gameObject.scene.name == arg.path && type.IsInstanceOfType(x));

					//ダイアログの場合、シーンは破棄.
					if (typeof(UIDialog).IsAssignableFrom(type))
					{
						uiShow.transform.SetParent(null);
						SceneManager.MoveGameObjectToScene(uiShow.gameObject, currentScreen.gameObject.scene);
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
					uiShow = go.GetComponent<UIBase>();
				}
			}

			uiShow.argument = arg;

			//スクリーンの場合、カレントを変更
			if (uiShow is UIScreen)
			{
				currentScreen = uiShow as UIScreen;
				uiShow.orderTracker.ignoreParentTracker = false;

				//TODO: List使うオーバーロードに切り替え
				uiShow.gameObject.scene.GetRootGameObjects(s_GameObject);
				foreach (var go in s_GameObject)
					go.SetActive(true);
				s_GameObject.Clear();
			}
			else if (uiShow is UIDialog)
			{
				//TODO: Transformのキャッシュ
				//TODO: UIRootCanvasのキャッシュ
				uiShow.FitByParent(currentScreen.canvas.rootCanvas.transform);
				uiShow.cachedTransform.SetAsLastSibling();

				currentScreen.dialogs.Add(uiShow as UIDialog);
				(uiShow as UIDialog).screen = currentScreen;
				uiShow.orderTracker.ignoreParentTracker = false;
			}

			yield return instance.StartCoroutine(CoShow(uiShow, coHide));
			UIRaycastBlocker.instance.RemoveCondition(cond);
		}




		static IEnumerator CoShow(UIBase uiShow, Coroutine coHide = null)
		{
			if (!uiShow)
				yield break;

			string cond = "CoShow_Instance_" + uiShow.name;

			if (UIRaycastBlocker.instance.ContainsCondition(cond))
			{
				yield break;
			}

			UIRaycastBlocker.instance.AddCondition(cond);
			pooledObjects.Remove(uiShow);

//			string cond = "CoShow_" + uiIn.name;
//			UIRaycastBlocker.instance.Add(cond);

			// スクリーンの場合、カレントを変更
			if (uiShow is UIScreen)
				currentScreen = uiShow as UIScreen;

			var uis = uiShow.GetEnumrator().ToList();

			// 初期化していないUIを全て初期化
			if (!uiShow.isInitialized)
			{
				Debug.LogFormat("{1:D6} >>>> [UIM] OnInitialize : {0}", uiShow.name, Time.frameCount);
				uis.ForEach(ui => ui.gameObject.SetActive(false));
				yield return WaitUITrigger(uis.Where(ui => !ui.isInitialized), ui => ui.OnInitialize());
				uis.ForEach(ui => ui.isInitialized = true);
			}

			// 非表示コルーチン待ち
			if (coHide != null)
				yield return coHide;


			// 表示コルーチン
			if (!uiShow.isShow)
			{
				Debug.LogFormat("{1:D6} >>>> [UIM] OnShow : {0}", uiShow.name, Time.frameCount);
				uis.ForEach(ui => ui.isTransiting = true);
				uis.ForEach(ui => ui.gameObject.SetActive(true));
				yield return WaitUITrigger(uis.Where(ui => !ui.isShow), ui => ui.OnShow());
				uis.ForEach(ui => ui.isTransiting = false);
				uis.ForEach(ui => ui.isShow = true);
			}

			Debug.LogFormat("{1:D6} >>>> [UIM] Complete CoShow : {0}", uiShow.name, Time.frameCount);
			UIRaycastBlocker.instance.RemoveCondition(cond);

		}

		static IEnumerator CoHide(UIBase uiHide, bool suspend = true, bool pool = true)
		{
			if (!uiHide)
				yield break;

			string cond = "CoHide_" + uiHide.name;


			if (UIRaycastBlocker.instance.ContainsCondition(cond))
			{
				yield break;
			}
			
			UIRaycastBlocker.instance.AddCondition(cond);


			//アウト
			var uis = uiHide.GetEnumrator().ToList();
			if (uis.Any(ui => ui.isShow))
			{
				Debug.LogFormat("{1:D6} >>>> [UIM] OnMoveOut : {0}", uiHide.name, Time.frameCount);
				uis.ForEach(ui => ui.isTransiting = true);
				yield return WaitUITrigger(uis.Where(ui => ui.isShow), ui => ui.OnHide());
				uis.ForEach(ui => ui.isTransiting = false);
				uis.ForEach(ui => ui.isShow = false);
			}
			uis.ForEach(ui => ui.gameObject.SetActive(false));


			//サスペンド
			if (suspend)
			{
				uis.ForEach(SuspendObject);
				if (uiHide.isSuspendable)
				{
					Debug.LogFormat("{1:D6} [UIM] Complete CoHide by Suspend : {0}", uiHide.name, Time.frameCount);
					UIRaycastBlocker.instance.RemoveCondition(cond);
					yield break;
				}
			}

			uis.ForEach(ResetObject);


			// 削除
			Debug.LogFormat("{1:D6} [UIM] OnFinalize : {0}", uiHide.name, Time.frameCount);
			uis.ForEach(ui => waitInitialize.Remove(ui));
			yield return WaitUITrigger(uis.Where(ui => ui.isInitialized), ui => ui.OnFinalize());
			uis.ForEach(ui => ui.isInitialized = false);
			
			
			// プール
			if (pool)
			{
				uis.ForEach(PoolObject);
			}

			Debug.LogFormat("{1:D6} [UIM] Complete CoHide : {0}, pooled : {1}", uiHide.name, Time.frameCount, pool);
			uis.ForEach(ui => DestroyObject(ui, false));
			UIRaycastBlocker.instance.RemoveCondition(cond);
		}




		static void DestroyObject(UIBase ui, bool forceDestroy = false)
		{
			if (forceDestroy || !pooledObjects.Contains(ui))
			{
				pooledObjects.Remove(ui);

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

		static void PoolObject(UIBase ui)
		{
			if (!ui.isPoolable)
				return;

			ui.argument = new UIArgument(){ path = ui.argument.path };
			if (!pooledObjects.Contains(ui))
				pooledObjects.Add(ui);
			
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
			if (!pooledObjects.Contains(ui))
			{
				pooledObjects.Add(ui);
			}

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
		/// Get pooled object.
		/// </summary>
		/// <returns>The cached object.</returns>
		/// <param name="path">Path.</param>
		/// <param name="onlySuspended">If set to <c>true</c> suspended.</param>
		static UIBase GetPooledObject(string path, bool onlySuspended)
		{
			UIBase ret = null;
			var index = pooledObjects.FindIndex(x => x.argument.path == path && (!onlySuspended || (x.isSuspendable && x.isInitialized)));
			if (0 <= index)
			{
				ret = pooledObjects[index];
				pooledObjects.RemoveAt(index);
			}

			if(ret)
			{
				Debug.LogFormat("{1:D6} #### [UIM] GetPooledObject successfuly: {0}, suspended: {2}", path, Time.frameCount,(ret.isSuspendable && ret.isInitialized));
			}
			else
			{
				Debug.LogFormat("{1:D6} #### [UIM] GetPooledObject failed, no objet pooled: {0}", path, Time.frameCount);
			}
			
			return ret;
		}


		public static void Clear()
		{
			Debug.LogFormat("{1:D6} #### [UIM] Clear : {0}", pooledObjects.Count, Time.frameCount);
			foreach (var ui in pooledObjects.ToList())
			{
				//キャッシュなしでアウト.
				Debug.LogFormat("{1:D6} #### [UIM] Clear : {0}", ui.argument.path, Time.frameCount);
				instance.StartCoroutine(CoHide(ui, false, false));
			}
			pooledObjects.Clear();
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