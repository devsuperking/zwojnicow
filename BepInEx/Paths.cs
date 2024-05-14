using System.IO;
using System.Linq;
using System.Reflection;
using MonoMod.Utils;

namespace Zwojnicow
{
	/// <summary>
	///     Paths used by Zwojnicow
	/// </summary>
	public static class Paths
	{
		internal static void SetExecutablePath(string executablePath, string bepinRootPath = null, string managedPath = null, string[] dllSearchPath = null)
		{
			ExecutablePath = executablePath;
			ProcessName = Path.GetFileNameWithoutExtension(executablePath);

			GameRootPath = PlatformHelper.Is(Platform.MacOS)
				? Utility.ParentDirectory(executablePath, 4)
				: Path.GetDirectoryName(executablePath);

			ManagedPath = managedPath ?? Utility.CombinePaths(GameRootPath, $"{ProcessName}_Data", "Managed");
			ZwojnicowRootPath = bepinRootPath ?? Path.Combine(GameRootPath, "Zwojnicow");
			ConfigPath = Path.Combine(ZwojnicowRootPath, "config");
			ZwojnicowConfigPath = Path.Combine(ConfigPath, "Zwojnicow.cfg");
			PluginPath = Path.Combine(ZwojnicowRootPath, "plugins");
			PatcherPluginPath = Path.Combine(ZwojnicowRootPath, "patchers");
			ZwojnicowAssemblyDirectory = Path.Combine(ZwojnicowRootPath, "core");
			ZwojnicowAssemblyPath = Path.Combine(ZwojnicowAssemblyDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.dll");
			CachePath = Path.Combine(ZwojnicowRootPath, "cache");
			DllSearchPaths = (dllSearchPath ?? new string[0]).Concat(new[] { ManagedPath }).Distinct().ToArray();
		}

		internal static void SetManagedPath(string managedPath)
		{
			if (managedPath == null)
				return;
			ManagedPath = managedPath;
		}

		internal static void SetPluginPath(string pluginPath)
		{
			PluginPath = Utility.CombinePaths(ZwojnicowRootPath, pluginPath);
		}

		/// <summary>
		///		List of directories from where Mono will search assemblies before assembly resolving is invoked.
		/// </summary>
		public static string[] DllSearchPaths { get; private set; }

		/// <summary>
		///     The directory that the core Zwojnicow DLLs reside in.
		/// </summary>
		public static string ZwojnicowAssemblyDirectory { get; private set; }

		/// <summary>
		///     The path to the core Zwojnicow DLL.
		/// </summary>
		public static string ZwojnicowAssemblyPath { get; private set; }

		/// <summary>
		///     The path to the main Zwojnicow folder.
		/// </summary>
		public static string ZwojnicowRootPath { get; private set; }

		/// <summary>
		///     The path of the currently executing program Zwojnicow is encapsulated in.
		/// </summary>
		public static string ExecutablePath { get; private set; }

		/// <summary>
		///     The directory that the currently executing process resides in.
		///		<para>On OSX however, this is the parent directory of the game.app folder.</para>
		/// </summary>
		public static string GameRootPath { get; private set; }

		/// <summary>
		///     The path to the Managed folder of the currently running Unity game.
		/// </summary>
		public static string ManagedPath { get; private set; }

		/// <summary>
		///		The path to the config directory.
		/// </summary>
		public static string ConfigPath { get; private set; }

		/// <summary>
		///		The path to the global Zwojnicow configuration file.
		/// </summary>
		public static string ZwojnicowConfigPath { get; private set; }

		/// <summary>
        ///		The path to temporary cache files.
        /// </summary>
		public static string CachePath { get; private set; }

		/// <summary>
		///     The path to the patcher plugin folder which resides in the Zwojnicow folder.
		/// </summary>
		public static string PatcherPluginPath { get; private set; }

		/// <summary>
		///     The path to the plugin folder which resides in the Zwojnicow folder.
		/// <para>
		///		This is ONLY guaranteed to be set correctly when Chainloader has been initialized.
		/// </para>
		/// </summary>
		public static string PluginPath { get; private set; }

		/// <summary>
		///     The name of the currently executing process.
		/// </summary>
		public static string ProcessName { get; private set; }
	}
}