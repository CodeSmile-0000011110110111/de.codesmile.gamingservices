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
		private static IAuthenticationService Service => AuthenticationService.Instance;

		public static void LogState()
		{
			if (Service != null)
			{
				Debug.Log("Authentication state: " +
				          $"session token exists = {Service.SessionTokenExists}, " +
				          $"signed in = {Service.IsSignedIn}, " +
				          $"authorized = {Service.IsAuthorized}, " +
				          $"expired = {Service.IsExpired}");
			}
		}

		public static async Task SignInAnonymouslyAsync()
		{
			Debug.Log($"Sign in anonymously: {(Service.SessionTokenExists ? "cached player" : "new player")}");
			await SignInHandler(async () => await Service.SignInAnonymouslyAsync());
			SignUpWithUsernamePasswordAsync("test_user", "123ABcd!");
		}

		public static async Task SignUpWithUsernamePasswordAsync(String userName, String password)
		{
			//if (Account.IsValidUserName(userName))

			Debug.Log($"Sign in with username/password: {(Service.SessionTokenExists ? "cached player" : "new player")}");
			await SignInHandler(async () =>
			{
				if (Service.IsSignedIn)
					await Service.AddUsernamePasswordAsync(userName, password);
				else
					await Service.SignUpWithUsernamePasswordAsync(userName, password);
			});
		}

		internal static async void OnServicesInitialized()
		{
			RegisterAuthenticationEvents();
			await SignInAnonymouslyAsync();
		}

		private static async Task SignInHandler(Func<Task> signInFunc)
		{
			try
			{
				await Services.Initialize();
				await signInFunc?.Invoke();
				await Notifications.ShowDsa();
			}
			catch (AuthenticationException ex)
			{
				// TODO: Compare error code to AuthenticationErrorCodes
				// Notify the player with the proper error message
				Debug.LogError($"Authentication failed: [{ex.ErrorCode}] {ex}");
				if (ex.Notifications != null && ex.Notifications.Count > 0)
					Debug.LogError($"notifications: {ex.Notifications}");

				await Notifications.ShowAll(ex.Notifications);
				await ExceptionPopup.ShowModal(ex);
			}
			catch (RequestFailedException ex)
			{
				// TODO: Compare error code to CommonErrorCodes
				// Notify the player with the proper error message
				Debug.LogError($"Request failed: [{ex.ErrorCode}] {ex}");
				await ExceptionPopup.ShowModal(ex);
			}
		}

		private static void RegisterAuthenticationEvents()
		{
			var service = Service;

			// delegates are unassigned first in case of repeat calls
			service.SignedIn -= OnSignedIn;
			service.SignedIn += OnSignedIn;

			service.SignedOut += OnSignedOut;
			service.SignedOut -= OnSignedOut;

			service.SignInFailed += OnSignInFailed;
			service.SignInFailed -= OnSignInFailed;

			service.Expired += OnExpired;
			service.Expired -= OnExpired;
		}

		private static async void OnSignedIn()
		{
			var playerName = await Account.GetPlayerNameAsync();

			Debug.Log($"OnSignedIn: ID={Service.PlayerId}, Name={playerName}, Token={Service.AccessToken}");
		}

		private static void OnSignedOut() => Debug.LogWarning($"Player signed out: {Service.PlayerId}");

		private static void OnExpired() => Debug.LogWarning("Player session could not be refreshed and expired.");

		// TODO: ask to sign back in at the next best time (now?)
		private static void OnSignInFailed(RequestFailedException e) => Debug.LogError($"Sign-in failed: {e}");
	}
}
