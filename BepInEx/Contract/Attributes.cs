using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zwojnicow.Bootstrap;
using Mono.Cecil;

namespace Zwojnicow
{
	#region BaseUnityPlugin

	/// <summary>
	/// This attribute denotes that a class is a plugin, and specifies the required metadata.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DunHeroMod : Attribute
	{
		/// <summary>
		/// The unique identifier of the plugin. Should not change between plugin versions.
		/// </summary>
		public string GUID { get; protected set; }


		/// <summary>
		/// The user friendly name of the plugin. Is able to be changed between versions.
		/// </summary>
		public string Name { get; protected set; }


		/// <summary>
		/// The specfic version of the plugin.
		/// </summary>
		public Version Version { get; protected set; }

		/// <param name="GUID">The unique identifier of the plugin. Should not change between plugin versions.</param>
		/// <param name="Name">The user friendly name of the plugin. Is able to be changed between versions.</param>
		/// <param name="Version">The specfic version of the plugin.</param>
		public DunHeroMod(string GUID, string Name, string Version)
		{
			this.GUID = GUID;
			this.Name = Name;

			try
			{
				this.Version = new Version(Version);
			}
			catch
			{
				this.Version = null;
			}
		}

		internal static DunHeroMod FromCecilType(TypeDefinition td)
		{
			var attr = MetadataHelper.GetCustomAttributes<DunHeroMod>(td, false).FirstOrDefault();

			if (attr == null)
				return null;

			return new DunHeroMod((string)attr.ConstructorArguments[0].Value, (string)attr.ConstructorArguments[1].Value, (string)attr.ConstructorArguments[2].Value);
		}
	}

	/// <summary>
	/// This attribute specifies any dependencies that this plugin has on other plugins.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DunHeroDependency : Attribute, ICacheable
	{
		/// <summary>
		/// Flags that are applied to a dependency
		/// </summary>
		[Flags]
		public enum DependencyFlags
		{
			/// <summary>
			/// The plugin has a hard dependency on the referenced plugin, and will not run without it.
			/// </summary>
			HardDependency = 1,

			/// <summary>
			/// This plugin has a soft dependency on the referenced plugin, and is able to run without it.
			/// </summary>
			SoftDependency = 2,
		}

		/// <summary>
		/// The GUID of the referenced plugin.
		/// </summary>
		public string DependencyGUID { get; protected set; }

		/// <summary>
		/// The flags associated with this dependency definition.
		/// </summary>
		public DependencyFlags Flags { get; protected set; }

		/// <summary>
		/// The minimum version of the referenced plugin.
		/// </summary>
		public Version MinimumVersion { get; protected set; }

		/// <summary>
		/// Marks this <see cref="BaseUnityPlugin"/> as depenant on another plugin. The other plugin will be loaded before this one.
		/// If the other plugin doesn't exist, what happens depends on the <see cref="Flags"/> parameter.
		/// </summary>
		/// <param name="DependencyGUID">The GUID of the referenced plugin.</param>
		/// <param name="Flags">The flags associated with this dependency definition.</param>
		public DunHeroDependency(string DependencyGUID, DependencyFlags Flags = DependencyFlags.HardDependency)
		{
			this.DependencyGUID = DependencyGUID;
			this.Flags = Flags;
			MinimumVersion = new Version();
		}

		/// <summary>
		/// Marks this <see cref="BaseUnityPlugin"/> as depenant on another plugin. The other plugin will be loaded before this one.
		/// If the other plugin doesn't exist or is of a version below <see cref="MinimumVersion"/>, this plugin will not load and an error will be logged instead.
		/// </summary>
		/// <param name="DependencyGUID">The GUID of the referenced plugin.</param>
		/// <param name="MinimumDependencyVersion">The minimum version of the referenced plugin.</param>
		/// <remarks>When version is supplied the dependency is always treated as HardDependency</remarks>
		public DunHeroDependency(string DependencyGUID, string MinimumDependencyVersion) : this(DependencyGUID)
		{
			MinimumVersion = new Version(MinimumDependencyVersion);
		}

		internal static IEnumerable<DunHeroDependency> FromCecilType(TypeDefinition td)
		{
			var attrs = MetadataHelper.GetCustomAttributes<DunHeroDependency>(td, true);
			return attrs.Select(customAttribute =>
			{
				var dependencyGuid = (string)customAttribute.ConstructorArguments[0].Value;
				var secondArg = customAttribute.ConstructorArguments[1].Value;
				if (secondArg is string minVersion) return new DunHeroDependency(dependencyGuid, minVersion);
				return new DunHeroDependency(dependencyGuid, (DependencyFlags)secondArg);
			}).ToList();
		}

		void ICacheable.Save(BinaryWriter bw)
		{
			bw.Write(DependencyGUID);
			bw.Write((int)Flags);
			bw.Write(MinimumVersion.ToString());
		}

		void ICacheable.Load(BinaryReader br)
		{
			DependencyGUID = br.ReadString();
			Flags = (DependencyFlags)br.ReadInt32();
			MinimumVersion = new Version(br.ReadString());
		}
	}

	/// <summary>
	/// This attribute specifies other plugins that are incompatible with this plugin.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DunHeroIncompatibility : Attribute, ICacheable
	{
		/// <summary>
		/// The GUID of the referenced plugin.
		/// </summary>
		public string IncompatibilityGUID { get; protected set; }
		
		/// <summary>
		/// Marks this <see cref="BaseUnityPlugin"/> as incompatible with another plugin. 
		/// If the other plugin exists, this plugin will not be loaded and a warning will be shown.
		/// </summary>
		/// <param name="IncompatibilityGUID">The GUID of the referenced plugin.</param>
		public DunHeroIncompatibility(string IncompatibilityGUID)
		{
			this.IncompatibilityGUID = IncompatibilityGUID;
		}

		internal static IEnumerable<DunHeroIncompatibility> FromCecilType(TypeDefinition td)
		{
			var attrs = MetadataHelper.GetCustomAttributes<DunHeroIncompatibility>(td, true);
			return attrs.Select(customAttribute =>
			{
				var dependencyGuid = (string)customAttribute.ConstructorArguments[0].Value;
				return new DunHeroIncompatibility(dependencyGuid);
			}).ToList();
		}

		void ICacheable.Save(BinaryWriter bw)
		{
			bw.Write(IncompatibilityGUID);
		}

		void ICacheable.Load(BinaryReader br)
		{
			IncompatibilityGUID = br.ReadString();
		}
	}

	/// <summary>
	/// This attribute specifies which processes this plugin should be run for. Not specifying this attribute will load the plugin under every process.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DunHeroProcess : Attribute
	{
		/// <summary>
		/// The name of the process that this plugin will run under.
		/// </summary>
		public string ProcessName { get; protected set; }

		/// <param name="ProcessName">The name of the process that this plugin will run under.</param>
		public DunHeroProcess(string ProcessName)
		{
			this.ProcessName = ProcessName;
		}

		internal static List<DunHeroProcess> FromCecilType(TypeDefinition td)
		{
			var attrs = MetadataHelper.GetCustomAttributes<DunHeroProcess>(td, true);
			return attrs.Select(customAttribute => new DunHeroProcess((string)customAttribute.ConstructorArguments[0].Value)).ToList();
		}
	}

	#endregion

	#region MetadataHelper

	/// <summary>
	/// Helper class to use for retrieving metadata about a plugin, defined as attributes.
	/// </summary>
	public static class MetadataHelper
	{
		internal static IEnumerable<CustomAttribute> GetCustomAttributes<T>(TypeDefinition td, bool inherit) where T : Attribute
		{
			var result = new List<CustomAttribute>();
			var type = typeof(T);
			var currentType = td;

			do
			{
				result.AddRange(currentType.CustomAttributes.Where(ca => ca.AttributeType.FullName == type.FullName));
				currentType = currentType.BaseType?.Resolve();
			} while (inherit && currentType?.FullName != "System.Object");


			return result;
		}

		/// <summary>
		/// Retrieves the DunHeroMod metadata from a plugin type.
		/// </summary>
		/// <param name="pluginType">The plugin type.</param>
		/// <returns>The DunHeroMod metadata of the plugin type.</returns>
		public static DunHeroMod GetMetadata(Type pluginType)
		{
			object[] attributes = pluginType.GetCustomAttributes(typeof(DunHeroMod), false);

			if (attributes.Length == 0)
				return null;

			return (DunHeroMod)attributes[0];
		}

		/// <summary>
		/// Retrieves the DunHeroMod metadata from a plugin instance.
		/// </summary>
		/// <param name="plugin">The plugin instance.</param>
		/// <returns>The DunHeroMod metadata of the plugin instance.</returns>
		public static DunHeroMod GetMetadata(object plugin)
			=> GetMetadata(plugin.GetType());

		/// <summary>
		/// Gets the specified attributes of a type, if they exist.
		/// </summary>
		/// <typeparam name="T">The attribute type to retrieve.</typeparam>
		/// <param name="pluginType">The plugin type.</param>
		/// <returns>The attributes of the type, if existing.</returns>
		public static T[] GetAttributes<T>(Type pluginType) where T : Attribute
		{
			return (T[])pluginType.GetCustomAttributes(typeof(T), true);
		}

		/// <summary>
		/// Gets the specified attributes of an instance, if they exist.
		/// </summary>
		/// <typeparam name="T">The attribute type to retrieve.</typeparam>
		/// <param name="plugin">The plugin instance.</param>
		/// <returns>The attributes of the instance, if existing.</returns>
		public static IEnumerable<T> GetAttributes<T>(object plugin) where T : Attribute
			=> GetAttributes<T>(plugin.GetType());

		/// <summary>
		/// Retrieves the dependencies of the specified plugin type.
		/// </summary>
		/// <param name="plugin">The plugin type.</param>
		/// <returns>A list of all plugin types that the specified plugin type depends upon.</returns>
		public static IEnumerable<DunHeroDependency> GetDependencies(Type plugin)
		{
			return plugin.GetCustomAttributes(typeof(DunHeroDependency), true).Cast<DunHeroDependency>();
		}
	}

	#endregion

	#region Build configuration

	/// <summary>
	/// This class is appended to AssemblyInfo.cs when Zwojnicow is built via a CI pipeline.
	/// It is mainly intended to signify that the current build is not a release build and is special, like for instance a bleeding edge build.
	/// </summary>
	internal class BuildInfoAttribute : Attribute
	{
		public string Info { get; }

		public BuildInfoAttribute(string info)
		{
			Info = info;
		}
	}

	#endregion
}