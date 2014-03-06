#define VERBOSE
using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

namespace ExperimentIndicator
{
    public class Log
    {

        #region Assembly/Class Information
        /// <summary>
        /// Name of the Assembly that is running this MonoBehaviour
        /// </summary>
        internal static String _AssemblyName
        { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }

        /// <summary>
        /// Name of the Class - including Derivations
        /// </summary>
        internal String _ClassName
        { get { return this.GetType().Name; } }
        #endregion


        private static String FormatMessage(String msg)
        {
            return String.Format("{0}, {2}, {1}", DateTime.Now, msg, _AssemblyName);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Debug(String Message, params object[] strParams)
        {
            Write(Message, strParams);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Debug(String message)
        {
            Write(message);
        }


        internal static void Verbose(String Message, params object[] strParams)
        {
            Verbose(string.Format(Message, strParams));
        }

        internal static void Verbose(String message)
        {
#if VERBOSE
                Write("(info): " + message);
#endif
        }

        internal static void Write(String Message, params object[] strParams)
        {
            Write(String.Format(Message, strParams));
        }

        internal static void Write(String Message)
        {
            UnityEngine.Debug.Log(FormatMessage(Message));
        }

        internal static void Warning(String message, params object[] strParams)
        {
            Warning(String.Format(message, strParams));
        }

        internal static void Warning(String message)
        {
            UnityEngine.Debug.LogWarning(FormatMessage(message));
        }

        internal static void Error(String message, params object[] strParams)
        {
            Error(String.Format(message, strParams));
        }

        internal static void Error(String message)
        {
            UnityEngine.Debug.LogError(FormatMessage(message));
        }

    }
}
