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
	public static partial class Authenticate
	{
		private static Boolean s_TryRepeatSignInAnonymously;

		public static async Task SignInAnonymouslyAsync(Boolean showNotifications = true)
		{
			if (s_TryRepeatSignInAnonymously == false)
				Debug.Log("Sign in anonymously ...");

			try
			{
				await Services.Initialize();
				await Service.SignInAnonymouslyAsync();

				if (showNotifications)
					await NotificationHandler.ShowDsa();
			}
			catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.InvalidSessionToken)
			{
				// simply try again if session token is invalid (token has already been cleared)
				// this can occur if the player was deleted via the cloud dashboard
				if (ShouldRetrySignIn())
					await SignInAnonymouslyAsync();
				else
					await DefaultExceptionHandling(ex);
			}
			catch (RequestFailedException ex) { await DefaultExceptionHandling(ex); }
			finally { s_TryRepeatSignInAnonymously = false; }
		}

		private static Boolean ShouldRetrySignIn() =>
			s_TryRepeatSignInAnonymously == false && (s_TryRepeatSignInAnonymously = true);

		private static async Task SignInCachedUser() => await SignInAnonymouslyAsync(false);
	}
}
