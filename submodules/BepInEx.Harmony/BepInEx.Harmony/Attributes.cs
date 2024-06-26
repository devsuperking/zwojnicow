﻿using System;

namespace Zwojnicow.Harmony
{
	/// <summary>
	/// Specifies the indices of parameters that are ByRef.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	[Obsolete("Use HarmonyLib.ParameterByRefAttribute directly", true)]
	public class ParameterByRefAttribute : HarmonyLib.ParameterByRefAttribute
	{
		/// <summary>
		/// The indices of parameters that are ByRef.
		/// </summary>
		public new int[] ParameterIndices => base.ParameterIndices;

		/// <param name="parameterIndices">The indices of parameters that are ByRef.</param>
		public ParameterByRefAttribute(params int[] parameterIndices) : base(parameterIndices)
		{
		}
	}
}