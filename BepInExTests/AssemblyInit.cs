﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zwojnicow.Tests
{
	[TestClass]
	public class AssemblyInit
	{
		private static string _testPath;

		[AssemblyInitialize]
		public static void InitAss(TestContext context)
		{
			_testPath = Path.Combine(Path.GetTempPath(), "BepinexTestDir");
			Directory.CreateDirectory(_testPath);
			
			string exePath = Path.Combine(_testPath, "Text.exe");
			File.WriteAllBytes(exePath, new byte[] { });

			Paths.SetExecutablePath(_testPath);
		}

		[AssemblyCleanup]
		public static void CleanupAss()
		{
			Directory.Delete(_testPath, true);
		}
	}
}