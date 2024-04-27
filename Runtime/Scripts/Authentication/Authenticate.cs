// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices.Authentication
{
	public static partial class Authenticate
	{
		private static IAuthenticationService Service => AuthenticationService.Instance;
		public static Boolean RethrowExceptions { get; set; } = false;
		public static Boolean PopupExceptions { get; set; } = true;

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

		internal static void OnServicesInitialized() => RegisterAuthenticationEvents();

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

			PlayerAccountService.Instance.SignedIn += OnSignInWithUnity;
		}

		private static void OnSignInWithUnity()
		{
			Debug.Log("OnSignInWithUnity");
			OnSignedIn();
		}

		private static async void OnSignedIn()
		{
			try
			{
				var playerName = await Account.GetPlayerNameAsync();
				Debug.Log($"OnSignedIn: ID={Service.PlayerId}, Name={playerName}, Token={Service.AccessToken}");
			}
			catch (RequestFailedException ex) { await DefaultExceptionHandling(ex); }
		}

		private static void OnSignedOut() => Debug.LogWarning($"Player signed out: {Service.PlayerId}");

		private static void OnExpired()
		{
			Debug.LogWarning("Player session could not be refreshed and expired.");

#pragma warning disable 4014
			// try to sign back in (not awaited on purpose)
			SignInCachedUser();
#pragma warning restore 4014
		}

		// TODO: ask to sign back in at the next best time (now?)
		private static void OnSignInFailed(RequestFailedException e) => Debug.LogError($"Sign-in failed: {e}");

		private static async Task DefaultExceptionHandling(RequestFailedException ex)
		{
			if (ex is AuthenticationException authEx)
				await Account.ShowAllNotifications(authEx.Notifications);

			if (PopupExceptions) Services.HandleServiceException(ex);
			if (RethrowExceptions) throw ex;
		}
	}
}
