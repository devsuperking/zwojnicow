using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Zwojnicow.Bootstrap
{
	public static class Entrypoint
	{
		public static void Init()
		{
			AppDomain.CurrentDomain.AssemblyResolve += ResolveZwojnicow;

			Linker.StartZwojnicow();
		}

		private static readonly string LocalDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

		private static Assembly ResolveZwojnicow(object sender, ResolveEventArgs args)
		{
			string path = Path.Combine(LocalDirectory, $@"Zwojnicow\core\{new AssemblyName(args.Name).Name}.dll");

			if (!File.Exists(path))
				return null;

			try
			{
				return Assembly.LoadFile(path);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}