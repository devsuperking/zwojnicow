using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Zwojnicow.Bootstrap;
using Zwojnicow.Logging;
using HarmonyLib;
using HarmonyXInterop;

namespace Zwojnicow.Preloader.RuntimeFixes
{
	internal static class HarmonyInteropFix
	{
		public static void Apply()
		{
			HarmonyInterop.Initialize(Paths.CachePath);
			Harmony.CreateAndPatchAll(typeof(HarmonyInteropFix), "org.zwojnicow.fixes.harmonyinterop");
		}

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Assembly), nameof(Assembly.LoadFile), typeof(string))]
		private static Assembly LoadFile(string path) => null;

		[HarmonyPatch(typeof(Assembly), nameof(Assembly.LoadFile), typeof(string))]
		[HarmonyPatch(typeof(Assembly), nameof(Assembly.LoadFrom), typeof(string))]
		[HarmonyPrefix]
		private static bool OnAssemblyLoad(ref Assembly __result, string __0)
		{
			HarmonyInterop.TryShim(__0, Paths.GameRootPath, Logger.LogWarning, TypeLoader.ReaderParameters);
			__result = LoadFile(__0);
			return true;
		}
	}
}