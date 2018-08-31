namespace ReeperCommon
{
	internal class Log
	{
		[System.Flags]
		internal enum LogMask
		{
			Normal = 1,
			Debug = 2,
			Verbose = 4,
			Performance = 8,
			Warning = 16,
			Error = 32,
			None = 0,
			All = -1
		}

		internal static LogMask Level = LogMask.Normal | LogMask.Warning | LogMask.Error;

		internal static string _AssemblyName => System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

	    internal string _ClassName => GetType().Name;

	    private static string FormatMessage(string msg)
		{
			return $"{_AssemblyName}, {msg}";
		}

		private static bool ShouldLog(LogMask messageType)
		{
			return (Level & messageType) != LogMask.None;
		}

		internal static void Write(string message, LogMask level)
		{
			if (ShouldLog(level))
			{
				string message2 = FormatMessage(message);
				if ((level & LogMask.Error) != LogMask.None)
				{
					UnityEngine.Debug.LogError(message2);
					return;
				}
				if ((level & LogMask.Warning) != LogMask.None)
				{
					UnityEngine.Debug.LogWarning(message2);
					return;
				}
				if ((level & LogMask.Normal) != LogMask.None)
				{
					UnityEngine.Debug.Log(message2);
					return;
				}
				if ((level & LogMask.Performance) != LogMask.None)
				{
					UnityEngine.Debug.Log(FormatMessage($"[PERF] {message}"));
					return;
				}
				UnityEngine.Debug.Log(message2);
			}
		}

		internal static void Write(string message, LogMask level, params object[] strParams)
		{
			if (ShouldLog(level))
			{
				Write(string.Format(message, strParams), level);
			}
		}

		internal static void SaveInto(ConfigNode parentNode)
		{
			ConfigNode configNode = parentNode.AddNode(new ConfigNode("LogSettings"));
			configNode.AddValue("LogMask", (int)Level);
			string[] names = System.Enum.GetNames(typeof(LogMask));
			System.Array values = System.Enum.GetValues(typeof(LogMask));
			configNode.AddValue("// Bit index", "message type");
			for (int i = 0; i < names.Length - 1; i++)
			{
				configNode.AddValue($"// Bit {i}", values.GetValue(i));
			}
			Debug("[ScienceAlert].SaveInto = {0}", configNode.ToString());
		}

		internal static void LoadFrom(ConfigNode parentNode)
		{
			if (parentNode == null || !parentNode.HasNode("LogSettings"))
			{
				Warning("[ScienceAlert] failed, did not find LogSettings in: {0}", parentNode != null ? parentNode.ToString() : "<null ConfigNode>");
				return;
			}
			ConfigNode node = parentNode.GetNode("LogSettings");
			try
			{
				if (!node.HasValue("LogMask"))
				{
					throw new System.Exception("[ScienceAlert]:No LogMask value in ConfigNode");
				}
				string value = node.GetValue("LogMask");
				int num = 0;
				if (int.TryParse(value, out num))
				{
					if (num == 0)
					{
						Warning("[ScienceAlert]: Log disabled");
					}
					Level = (LogMask)num;
					Debug("[ScienceAlert]:Loaded LogMask = {0} from ConfigNode", Level.ToString());
				}
				else
				{
					Debug("[ScienceAlert]:  LogMask value '{0}' cannot be converted to LogMask", value);
				}
			}
			catch (System.Exception ex)
			{
				Warning("[ScienceAlert] failed with exception: {0}", ex);
			}
		}

		internal static void Debug(string message, params object[] strParams)
		{
			Write(message, LogMask.Debug, strParams);
		}

		internal static void Normal(string message, params object[] strParams)
		{
			Write(message, LogMask.Normal, strParams);
		}

		internal static void Warning(string message, params object[] strParams)
		{
			Write(message, LogMask.Warning, strParams);
		}

		internal static void Error(string message, params object[] strParams)
		{
			Write(message, LogMask.Error, strParams);
		}
	}
}
