using UnityEngine;

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// Singleton for MonoBehavior.
	/// If the instance is duplicated, destroy itself.
	/// </summary>
	public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		/// <summary>
		/// Get instance object.
		/// If instance does not exist, Find instance in scene, or create new one.
		/// </summary>
		public static T instance
		{
			get
			{
				// Find instance in scene, or create new one.
				if (object.ReferenceEquals(s_Instance, null))
				{
					s_Instance = Object.FindObjectOfType<T>() ?? new GameObject(typeof(T).Name, typeof(T)).GetComponent<T>();
					s_Instance.gameObject.SetActive(true);
					s_Instance.enabled = true;
				}
				return s_Instance;
			}
		}

		static T s_Instance;

		//vvvvvvvv  Unity Callbacks  vvvvvvvv
		/// <summary>
		/// Awake is called when the script instance is being loaded.
		/// </summary>
		protected virtual void Awake()
		{
			// Hold the instance.
			if (s_Instance == null)
			{
				s_Instance = GetComponent<T>();
			}
			// If the instance is duplicated, destroy itself.
			else if (s_Instance != this)
			{
				UnityEngine.Debug.LogError("Multiple " + typeof(T).Name + " in scene. please fix it.", this.gameObject);
				enabled = false;
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(this);
				}
				return;
			}

			// Singleton has DontDestroy flag.
			DontDestroyOnLoad(gameObject);
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected virtual void OnDestroy()
		{
			// Clear instance on destroy.
			if (s_Instance == this)
				s_Instance = null;
		}
		//^^^^^^^^  Unity Callbacks  ^^^^^^^^
	}
}