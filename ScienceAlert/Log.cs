using System;
using ReeperCommon.Logging;

namespace ScienceAlert
{
    public static class Log
    {
        private static ILog _instance;

        public static ILog Instance
        {
            get
            {
                _instance = _instance ?? new StandardLog("ScienceAlert");
                return _instance;
            }

            set { _instance = value; }
        }


        public static void Debug(string format, params string[] args)
        {
            Instance.Debug(format, args);
        }


        public static void Debug(Func<string> message)
        {
            Instance.Debug(message); // note to self: intended not to execute delegate yet
        }


        public static void Normal(string format, params string[] args)
        {
            Instance.Normal(format, args);
        }


        public static void Normal(Func<string> message)
        {
            Instance.Normal(message); // note to self: intended not to execute delegate yet
        }


        public static void Warning(string format, params string[] args)
        {
            Instance.Warning(format, args);
        }


        public static void Warning(Func<string> message)
        {
            Instance.Warning(message); // note to self: intended not to execute delegate yet
        }


        public static void Error(string format, params string[] args)
        {
            Instance.Error(format, args);
        }


        public static void Error(Func<string> message)
        {
            Instance.Error(message); // note to self: intended not to execute delegate yet
        }


        public static void Performance(string format, params string[] args)
        {
            Instance.Performance(format, args);
        }


        public static void Performance(Func<string> message)
        {
            Instance.Performance(message); // note to self: intended not to execute delegate yet
        }


        public static void Verbose(string format, params string[] args)
        {
            Instance.Verbose(format, args);
        }


        public static void Verbose(Func<string> message)
        {
            Instance.Verbose(message); // note to self: intended not to execute delegate yet
        }
    }
}
