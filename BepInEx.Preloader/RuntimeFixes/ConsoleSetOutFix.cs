﻿using System;
using System.IO;
using System.Text;
using Zwojnicow.Logging;
using HarmonyLib;

namespace Zwojnicow.Preloader.RuntimeFixes
{
	internal static class ConsoleSetOutFix
	{
		private static LoggedTextWriter loggedTextWriter;
		internal static ManualLogSource ConsoleLogSource = Logger.CreateLogSource("Console");

		public static void Apply()
		{
			loggedTextWriter = new LoggedTextWriter { Parent = Console.Out };
			Console.SetOut(loggedTextWriter);
			HarmonyLib.Harmony.CreateAndPatchAll(typeof(ConsoleSetOutFix));
		}

		[HarmonyPatch(typeof(Console), nameof(Console.SetOut))]
		[HarmonyPrefix]
		private static bool OnSetOut(TextWriter newOut)
		{
			loggedTextWriter.Parent = newOut;
			return false;
		}
	}

	internal class LoggedTextWriter : TextWriter
	{
		public override Encoding Encoding { get; } = Encoding.UTF8;

		public TextWriter Parent { get; set; }

		public override void Flush() => Parent.Flush();

		public override void Write(string value)
		{
			ConsoleSetOutFix.ConsoleLogSource.LogInfo(value);
			Parent.Write(value);
		}

		public override void WriteLine(string value)
		{
			ConsoleSetOutFix.ConsoleLogSource.LogInfo(value);
			Parent.WriteLine(value);
		}
	}
}