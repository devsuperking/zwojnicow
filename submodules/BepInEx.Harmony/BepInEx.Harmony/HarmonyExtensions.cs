﻿using System;

namespace Zwojnicow.Harmony
{
    /// <summary>
	/// An extension class for Harmony based operations.
	/// </summary>
    public static class HarmonyExtensions
    {
        /// <summary>
		/// Applies all patches specified in the type.
		/// </summary>
		/// <param name="harmonyInstance">The HarmonyInstance to use.</param>
		/// <param name="type">The type to scan.</param>
		[Obsolete("Use HarmonyLib.Harmony.PatchAll directly", true)]
        public static void PatchAll(this HarmonyLib.Harmony harmonyInstance, Type type)
        {
            HarmonyWrapper.PatchAll(type, harmonyInstance);
        }
    }
}
