﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Zwojnicow.Preloader.RuntimeFixes
{
	/// <summary>
	/// This exists because the Mono implementation of <see cref="Trace"/> is/was broken, and would call Write directly instead of calling TraceEvent.
	/// </summary>
	internal static class TraceFix
	{
		private static Type TraceImplType;

		private static object ListenersSyncRoot;
		private static TraceListenerCollection Listeners;
		private static PropertyInfo prop_AutoFlush;

		private static bool AutoFlush => (bool)prop_AutoFlush.GetValue(null, null);


		public static void ApplyFix()
		{
			TraceImplType = AppDomain.CurrentDomain.GetAssemblies()
									 .First(x => x.GetName().Name == "System")
									 .GetTypes()
									 .FirstOrDefault(x => x.Name == "TraceImpl");
			// assembly that has already fixed this
			if (TraceImplType == null) return;

			ListenersSyncRoot = AccessTools.Property(TraceImplType, "ListenersSyncRoot").GetValue(null, null);

			Listeners = (TraceListenerCollection)AccessTools.Property(TraceImplType, "Listeners").GetValue(null, null);

			prop_AutoFlush = AccessTools.Property(TraceImplType, "AutoFlush");


			HarmonyLib.Harmony instance = new HarmonyLib.Harmony("com.bepis.bepinex.tracefix");

			instance.Patch(
				typeof(Trace).GetMethod("DoTrace", BindingFlags.Static | BindingFlags.NonPublic),
				prefix: new HarmonyMethod(typeof(TraceFix).GetMethod(nameof(DoTraceReplacement), BindingFlags.Static | BindingFlags.NonPublic)));
		}


		private static bool DoTraceReplacement(string kind, Assembly report, string message)
		{
			string arg = string.Empty;
			try
			{
				arg = report.GetName().Name;
			}
			catch (MethodAccessException) { }

			TraceEventType type = (TraceEventType)Enum.Parse(typeof(TraceEventType), kind);

			lock (ListenersSyncRoot)
			{
				foreach (object obj in Listeners)
				{
					TraceListener traceListener = (TraceListener)obj;
					traceListener.TraceEvent(new TraceEventCache(), arg, type, 0, message);

					if (AutoFlush)
					{
						traceListener.Flush();
					}
				}
			}

			return false;
		}
	}
}