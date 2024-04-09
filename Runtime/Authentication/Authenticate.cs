// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices.Authentication
{
	public static class Authenticate
	{
		private const Int32 MaxPlayerNameLength = 50;

		public static async Task SignInAnonymously()
		{
			var authService = AuthenticationService.Instance;
			try
			{
				await authService.SignInAnonymouslyAsync();
				await Notifications.ShowDsa();
			}
			catch (AuthenticationException ex)
			{
				await Notifications.ShowAll(ex.Notifications);

				// TODO: Compare error code to AuthenticationErrorCodes
				// Notify the player with the proper error message
				Debug.LogError($"Failed to authenticate: [{ex.ErrorCode}] {ex}");
			}
			catch (RequestFailedException ex)
			{
				// TODO: Compare error code to CommonErrorCodes
				// Notify the player with the proper error message
				Debug.LogError($"Failed request: [{ex.ErrorCode}] {ex}");
			}
		}

		public static String SanitizePlayerName(String playerName)
		{
			var name = playerName?.RemoveWhitespace();
			return name?.Length > MaxPlayerNameLength ? name.Substring(0, MaxPlayerNameLength) : name;
		}

		public static async Task<String> GetPlayerNameAsync()
		{
			var authService = AuthenticationService.Instance;
			return authService.PlayerName ?? await authService.GetPlayerNameAsync();
		}

		public static async void SetPlayerNameAsync(String name)
		{
			var authService = AuthenticationService.Instance;
			var sanitizedName = SanitizePlayerName(name);
			await authService.UpdatePlayerNameAsync(sanitizedName);
		}

		internal static async void OnServicesInitialized()
		{
			RegisterAuthenticationEvents();
			await SignInAnonymously();
		}

		private static void RegisterAuthenticationEvents()
		{
			var authService = AuthenticationService.Instance;

			// delegates are unassigned first in case of repeat calls
			authService.SignedIn -= OnSignedIn;
			authService.SignedIn += OnSignedIn;

			authService.SignedOut += OnSignedOut;
			authService.SignedOut -= OnSignedOut;

			authService.SignInFailed += OnSignInFailed;
			authService.SignInFailed -= OnSignInFailed;

			authService.Expired += OnExpired;
			authService.Expired -= OnExpired;
		}

		private static async void OnSignedIn()
		{
			var authService = AuthenticationService.Instance;

			var playerName = await GetPlayerNameAsync();
			Debug.LogWarning($"PlayerID: {authService.PlayerId}");
			Debug.LogWarning($"PlayerName: {playerName}");
			Debug.LogWarning($"Access Token: {authService.AccessToken}");
		}

		private static void OnSignedOut() => Debug.LogWarning($"Player signed out: {AuthenticationService.Instance.PlayerId}");

		private static void OnExpired() => Debug.LogWarning("Player session could not be refreshed and expired.");

		// TODO: ask to sign back in at the next best time (now?)
		private static void OnSignInFailed(RequestFailedException e) => Debug.LogError($"Sign-in failed: {e}");
	}
}
