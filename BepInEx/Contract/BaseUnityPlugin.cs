﻿using System;
using Zwojnicow.Bootstrap;
using Zwojnicow.Configuration;
using Zwojnicow.Logging;
using UnityEngine;

namespace Zwojnicow
{
	/// <summary>
	/// The base plugin type that is used by the Zwojnicow plugin loader.
	/// </summary>
	public abstract class BaseUnityPlugin : MonoBehaviour
	{
		/// <summary>
		/// Information about this plugin as it was loaded.
		/// </summary>
		public PluginInfo Info { get; }
		/// <summary>
		/// Logger instance tied to this plugin.
		/// </summary>
		protected ManualLogSource Logger { get; }

		/// <summary>
		/// Default config file tied to this plugin. The config file will not be created until 
		/// any settings are added and changed, or <see cref="ConfigFile.Save"/> is called.
		/// </summary>
		public ConfigFile Config { get; }

        /// <summary>
        /// Create a new instance of a plugin and all of its tied in objects.
        /// </summary>
        /// <exception cref="InvalidOperationException">DunHeroMod attribute is missing.</exception>
        protected BaseUnityPlugin()
		{
			var metadata = MetadataHelper.GetMetadata(this);
			if(metadata == null)
				throw new InvalidOperationException("Can't create an instance of " + GetType().FullName + " because it inherits from BaseUnityPlugin and the DunHeroMod attribute is missing.");

			if (!Chainloader.IsEditor && Chainloader.PluginInfos.TryGetValue(metadata.GUID, out var info))
				Info = info;
			else
			{
				Info = new PluginInfo
				{
					Metadata = metadata,
					Instance = this,
					Dependencies = MetadataHelper.GetDependencies(GetType()),
					Processes = MetadataHelper.GetAttributes<DunHeroProcess>(GetType()),
					Location = GetType().Assembly.Location
				};
			}

			Logger = Logging.Logger.CreateLogSource(metadata.Name);

			string configRoot = Chainloader.IsEditor ? "." : Paths.ConfigPath;
			Config = new ConfigFile(Utility.CombinePaths(configRoot, metadata.GUID + ".cfg"), false, metadata);
		}
	}
}