#if !DEBUG_LOG
using Conditional = System.Diagnostics.ConditionalAttribute;

public static class Debug
{
	const string Symbol = "DEBUG_LOG_______";

	[Conditional(Symbol)]
	public static void DrawLine ( UnityEngine.Vector3 start, UnityEngine.Vector3 end, UnityEngine.Color color, float duration, bool depthTest )
	{
	}

	[Conditional(Symbol)]
	public static void DrawLine ( UnityEngine.Vector3 start, UnityEngine.Vector3 end, UnityEngine.Color color, float duration )
	{
	}

	[Conditional(Symbol)]
	public static void DrawLine ( UnityEngine.Vector3 start, UnityEngine.Vector3 end, UnityEngine.Color color )
	{
	}

	[Conditional(Symbol)]
	public static void DrawLine ( UnityEngine.Vector3 start, UnityEngine.Vector3 end )
	{
	}

	[Conditional(Symbol)]
	public static void DrawRay ( UnityEngine.Vector3 start, UnityEngine.Vector3 dir, UnityEngine.Color color, float duration )
	{
	}

	[Conditional(Symbol)]
	public static void DrawRay ( UnityEngine.Vector3 start, UnityEngine.Vector3 dir, UnityEngine.Color color )
	{
	}

	[Conditional(Symbol)]
	public static void DrawRay ( UnityEngine.Vector3 start, UnityEngine.Vector3 dir )
	{
	}

	[Conditional(Symbol)]
	public static void DrawRay ( UnityEngine.Vector3 start, UnityEngine.Vector3 dir, UnityEngine.Color color, float duration, bool depthTest )
	{
	}

	[Conditional(Symbol)]
	public static void Break ()
	{
	}

	[Conditional(Symbol)]
	public static void DebugBreak ()
	{
	}

	[Conditional(Symbol)]
	public static void Log ( object message )
	{
	}

	[Conditional(Symbol)]
	public static void Log ( object message, UnityEngine.Object context )
	{
	}

	[Conditional(Symbol)]
	public static void LogFormat ( string format, params object[] args )
	{
	}

	[Conditional(Symbol)]
	public static void LogFormat ( UnityEngine.Object context, string format, params object[] args )
	{
	}

	public static void LogError ( object message )
	{
	}

	public static void LogError ( object message, UnityEngine.Object context )
	{
	}

	public static void LogErrorFormat ( string format, params object[] args )
	{
	}

	public static void LogErrorFormat ( UnityEngine.Object context, string format, params object[] args )
	{
	}

	public static void ClearDeveloperConsole ()
	{
	}

	public static void LogException ( System.Exception exception )
	{
	}

	public static void LogException ( System.Exception exception, UnityEngine.Object context )
	{
	}

	[Conditional(Symbol)]
	public static void LogWarning ( object message )
	{
	}

	[Conditional(Symbol)]
	public static void LogWarning ( object message, UnityEngine.Object context )
	{
	}

	[Conditional(Symbol)]
	public static void LogWarningFormat ( string format, params object[] args )
	{
	}

	[Conditional(Symbol)]
	public static void LogWarningFormat ( UnityEngine.Object context, string format, params object[] args )
	{
	}

	public static void Assert ( bool condition )
	{
	}

	public static void Assert ( bool condition, UnityEngine.Object context )
	{
	}

	public static void Assert ( bool condition, object message )
	{
	}

	public static void Assert ( bool condition, string message )
	{
	}

	public static void Assert ( bool condition, object message, UnityEngine.Object context )
	{
	}

	public static void Assert ( bool condition, string message, UnityEngine.Object context )
	{
	}

	public static void AssertFormat ( bool condition, string format, params object[] args )
	{
	}

	public static void AssertFormat ( bool condition, UnityEngine.Object context, string format, params object[] args )
	{
	}

	public static void LogAssertion ( object message )
	{
	}

	public static void LogAssertion ( object message, UnityEngine.Object context )
	{
	}

	public static void LogAssertionFormat ( string format, params object[] args )
	{
	}

	public static void LogAssertionFormat ( UnityEngine.Object context, string format, params object[] args )
	{
	}

	public static void Assert ( bool condition, string format, params object[] args )
	{
	}

	public static UnityEngine.ILogger logger
	{
		get { return UnityEngine.Debug.logger; }
	}

	public static bool developerConsoleVisible
	{
		get { return UnityEngine.Debug.developerConsoleVisible; }
		set { UnityEngine.Debug.developerConsoleVisible = value; }
	}

	public static bool isDebugBuild
	{
		get { return UnityEngine.Debug.isDebugBuild; }
	}

}
#endif