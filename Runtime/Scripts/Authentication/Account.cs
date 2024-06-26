﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices.Authentication
{
	public static partial class Account
	{
		public static event Func<Task<Boolean>> OnBeforeDelete;

		public const Int32 MinPlayerNameLength = 1;
		public const Int32 MaxPlayerNameLength = 50;

		private static IAuthenticationService AuthService => AuthenticationService.Instance;

		/// <summary>
		/// Returns true if the account is anonymous, meaning it has not been linked with an ID provider nor
		/// did the user sign up with username/password.
		/// </summary>
		/// <returns></returns>
		public static async Task<Boolean> IsAnonymous()
		{
			var playerInfo = await GetPlayerInfoAsync();
			return playerInfo.Identities?.Count == 0 && String.IsNullOrEmpty(playerInfo.Username);
		}

		/// <summary>
		///     Returns true if the playerName is valid.
		/// </summary>
		/// <remarks>
		///     Name is valid if non-null, does not contain whitespace, is between 3 and 50 characters.
		/// </remarks>
		/// <param name="playerName"></param>
		/// <returns></returns>
		public static Boolean IsValidPlayerName(String playerName)
		{
			if (String.IsNullOrWhiteSpace(playerName) || playerName.Any(Char.IsWhiteSpace))
				return false;

			if (playerName.Length < MinPlayerNameLength || playerName.Length > MaxPlayerNameLength)
				return false;

			return playerName.Equals(SanitizePlayerName(playerName));
		}

		/// <summary>
		///     Removes all whitespace characters and returns at most the first 50 characters of the remaining string.
		///     CAUTION: The returned string is not necessarily a valid username! For example it could be an empty string.
		/// </summary>
		/// <param name="playerName"></param>
		/// <returns></returns>
		public static String SanitizePlayerName(String playerName)
		{
			if (String.IsNullOrWhiteSpace(playerName))
				return String.Empty;

			var name = playerName.RemoveWhitespace();
			return name.Length > MaxPlayerNameLength ? name.Substring(0, MaxPlayerNameLength) : name;
		}

		/// <summary>
		///     Returns the player name.
		/// </summary>
		/// <remarks>If the player name is already cached the call returns instantly.</remarks>
		/// <returns></returns>
		public static async Task<String> GetPlayerNameAsync()
		{
			var name = AuthService.PlayerName;
			if (String.IsNullOrEmpty(name))
			{
				try
				{
					name = await AuthService.GetPlayerNameAsync();
				}
				catch (RequestFailedException ex)
				{
					Services.HandleServiceException(ex);
				}
			}

			return name;
		}

		public static async Task<PlayerInfo> GetPlayerInfoAsync()
		{
			var info = AuthService.PlayerInfo;
			if (info == null || info.CreatedAt == null)
			{
				try
				{
					info = await AuthService.GetPlayerInfoAsync();
				}
				catch (RequestFailedException ex)
				{
					Services.HandleServiceException(ex);
				}
			}

			return info;
		}

		/// <summary>
		///     Updates the player name in the cloud.
		/// </summary>
		/// <remarks>The string will be sanitized (remove whitespace, cut to max length) before making the API call.</remarks>
		/// <param name="playerName"></param>
		public static async void SetPlayerNameAsync(String playerName)
		{
			var sanitizedName = SanitizePlayerName(playerName);
			await AuthService.UpdatePlayerNameAsync(sanitizedName);
		}

		/// <summary>
		///     Deletes the user account.
		/// </summary>
		/// <remarks>
		///     CAUTION: does not delete player data in other services.
		///     Subscribe to the OnBeforeDelete event to perform data deletion beforehand.
		/// </remarks>
		/// <returns>True if deletion occured, false if it was denied.</returns>
		public static async void DeleteAsync()
		{
			await OnBeforeDelete?.Invoke();
			await AuthService.DeleteAccountAsync();
		}
	}
}
