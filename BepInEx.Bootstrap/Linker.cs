using System.Diagnostics;

namespace Zwojnicow.Bootstrap
{
	public static class Linker
	{
		public static void StartZwojnicow()
		{
			Chainloader.Initialize(Process.GetCurrentProcess().MainModule.FileName);
			Chainloader.Start();
		}
	}
}