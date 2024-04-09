﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices
{
	public static class Services
	{
		public static async Task Initialize()
		{
			if (UnityServices.State != ServicesInitializationState.Uninitialized)
				return;

			try
			{
				await UnityServices.InitializeAsync();
				Authenticate.OnServicesInitialized();
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to initialize Unity Services: {e}");
				throw;
			}
		}

		[RuntimeInitializeOnLoadMethod]
		private static async void OnRuntimeLoad()
		{
			try { await Initialize(); }
			catch (Exception) {}
		}
	}
}
