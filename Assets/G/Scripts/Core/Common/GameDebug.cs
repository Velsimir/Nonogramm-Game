using UnityEngine;

namespace G.Core.Common
{
    public static class GameDebug
    {
#if UNITY_EDITOR
        private const string LOG_PREF = "G.GameDebug.Log";
        private const string WARNING_PREF = "G.GameDebug.Warning";
        private const string ERROR_PREF = "G.GameDebug.Error";

        public static bool LogEnabled
        {
            get => UnityEditor.EditorPrefs.GetBool(LOG_PREF, true);
            set => UnityEditor.EditorPrefs.SetBool(LOG_PREF, value);
        }

        public static bool WarningEnabled
        {
            get => UnityEditor.EditorPrefs.GetBool(WARNING_PREF, true);
            set => UnityEditor.EditorPrefs.SetBool(WARNING_PREF, value);
        }

        public static bool ErrorEnabled
        {
            get => UnityEditor.EditorPrefs.GetBool(ERROR_PREF, true);
            set => UnityEditor.EditorPrefs.SetBool(ERROR_PREF, value);
        }
#elif DEVELOPMENT_BUILD
        public static bool LogEnabled = true;
        public static bool WarningEnabled = true;
        public static bool ErrorEnabled = true;
#else
        public static bool LogEnabled = false;
        public static bool WarningEnabled = false;
        public static bool ErrorEnabled = false;
#endif

        public static void Log(string message)
        {
            if (LogEnabled == false)
                return;

            Debug.Log(message);
        }

        public static void Log(string message, Object context)
        {
            if (LogEnabled == false)
                return;

            Debug.Log(message, context);
        }

        public static void LogWarning(string message)
        {
            if (WarningEnabled == false)
                return;

            Debug.LogWarning(message);
        }

        public static void LogWarning(string message, Object context)
        {
            if (WarningEnabled == false)
                return;

            Debug.LogWarning(message, context);
        }

        public static void LogError(string message)
        {
            if (ErrorEnabled == false)
                return;

            Debug.LogError(message);
        }

        public static void LogError(string message, Object context)
        {
            if (ErrorEnabled == false)
                return;

            Debug.LogError(message, context);
        }

        public static void LogConfigurationError(string message)
        {
            Debug.LogError(message);

#if UNITY_EDITOR
            throw new System.InvalidOperationException(message);
#endif
        }
    }
}
