using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoWorldUnity3D
{
    public class GoWorldLogger
    {
        public static void Debug(string subject, string msg, params object[] args)
        {
            try
            {
                UnityEngine.Debug.LogFormat("DEBUG - " + subject + " - " + msg, args);
            }
            catch (System.Security.SecurityException)
            {
                Console.WriteLine(String.Format("DEBUG - " + subject + " - " + msg, args));
            }
        }

        public static void Info(string subject, string msg, params object[] args)
        {
            try
            {
                UnityEngine.Debug.LogFormat("INFO - " + subject + " - " + msg, args);
            } catch (System.Security.SecurityException)
            {
                Console.WriteLine(String.Format("INFO - " + subject + " - " + msg, args));
            }
        }

        public static void Warn(string subject, string msg, params object[] args)
        {
            try
            {
                UnityEngine.Debug.LogWarningFormat("WARN - " + subject + " - " + msg, args);
            }
            catch (System.Security.SecurityException)
            {
                Console.WriteLine(String.Format("WARN - " + subject + " - " + msg, args));
            }
        }

        public static void Error(string subject, string msg, params object[] args)
        {
            try
            {
                UnityEngine.Debug.LogErrorFormat("ERROR - " + subject + " - " + msg, args);
            }
            catch (System.Security.SecurityException)
            {
                Console.WriteLine(String.Format("ERROR - " + subject + " - " + msg, args));
            }
        }

        public static void Fatal(string subject, string msg, params object[] args)
        {
            try
            {
                UnityEngine.Debug.LogErrorFormat("FATAL - " + subject + " - " + msg, args);
            }
            catch (System.Security.SecurityException)
            {
                Console.WriteLine(String.Format("FATAL - " + subject + " - " + msg, args));
            }
        }

        public static void Assert(bool condition)
        {
            try
            {
                UnityEngine.Debug.Assert(condition);
            }
            catch (System.Security.SecurityException)
            {
                System.Diagnostics.Debug.Assert(condition);
            }

        }

        public static void Assert(bool condition, string format, params object[] args)
        {
            try
            {
                UnityEngine.Debug.AssertFormat(condition, format, args);
            }
            catch (System.Security.SecurityException)
            {
                System.Diagnostics.Debug.Assert(condition, "Assert Error", format, args);
            }
            
        }
    }
}
