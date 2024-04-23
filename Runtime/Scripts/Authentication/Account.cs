// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices.Authentication
{
	public static class Account
	{
		public static event Action OnBeforeDeleteAccount;
		public const Int32 MinPlayerNameLength = 1;
		public const Int32 MaxPlayerNameLength = 50;
		public const Int32 MinUserNameLength = 3;
		public const Int32 MaxUserNameLength = 20;
		public const Int32 MinPasswordLength = 8;
		public const Int32 MaxPasswordLength = 30;

		/// <summary>
		///     Regex character class with valid user name letters, digits and symbols.
		/// </summary>
		/// <remarks>
		///     "Letters" are lower ASCII (65-122) letters only. Extended ASCII or Unicode letters (ÖÔØ etc) are not allowed.
		/// </remarks>
		public static readonly String UserNameCharacterClass = @"a-zA-Z0-9\-_.@";

		/// <summary>
		///     Regex pattern that matches a valid user name.
		/// </summary>
		public static readonly String UserNamePattern = $"^[{UserNameCharacterClass}]+$";

		/// <summary>
		///     Regex pattern that matches any invalid character in a user name.
		/// </summary>
		public static readonly String NotUserNamePattern = $"[^{UserNameCharacterClass}]";

		/// <summary>
		///     Regex pattern that matches a valid password string.
		/// </summary>
		/// <remarks>
		///     Not all symbols have been tested to be allowed. Waiting for confirmation.
		/// </remarks>
		public static readonly String PasswordPattern =
			@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!""#$%&'´`()*+-.,/\\:;<=>?@^_{|}~\[\]])";

		private static IAuthenticationService AuthService => AuthenticationService.Instance;

		/// <summary>
		///     Returns true if the password is valid.
		/// </summary>
		/// <remarks>
		///     Password must be non-null, requires a minimum of 8 and a maximum of 30 characters and
		///     at least 1 lowercase letter, 1 uppercase letter, 1 number, and 1 symbol.
		/// </remarks>
		/// <param name="password"></param>
		/// <returns></returns>
		public static Boolean IsValidPassword(String password)
		{
			if (password == null)
				return false;

			if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
				return false;

			return Regex.Match(password, PasswordPattern).Success;
		}

		/// <summary>
		///     Returns true if the userName is valid.
		/// </summary>
		/// <remarks>
		///     User name is valid if non-null, does not contain whitespace, is between 3 and 20 characters,
		///     contains only english letters, numbers, or '-', '_', '.', '@' (hyphen, underscore, dot, at sign).
		///     Details: https://docs.unity.com/ugs/en-us/manual/authentication/manual/platform-signin-username-password
		/// </remarks>
		/// <param name="userName"></param>
		/// <returns></returns>
		public static Boolean IsValidUserName(String userName)
		{
			if (String.IsNullOrWhiteSpace(userName) || userName.Any(Char.IsWhiteSpace))
				return false;

			if (userName.Length < MinUserNameLength || userName.Length > MaxUserNameLength)
				return false;

			return userName.Equals(SanitizeUserName(userName));
		}

		/// <summary>
		///     Replaces all invalid username characters with underscores, then returns the first 20 characters.
		///     CAUTION: The returned string is not necessarily a valid username! For example it could be too short.
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>
		public static String SanitizeUserName(String userName)
		{
			if (String.IsNullOrWhiteSpace(userName))
				return String.Empty;

			var name = userName.RemoveWhitespace();
			name = name.Length > MaxUserNameLength ? name.Substring(0, MaxUserNameLength) : name;

			return Regex.Replace(name, NotUserNamePattern, "_");
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
			if (string.IsNullOrEmpty(name))
			{
				try
				{
					name = await AuthService.GetPlayerNameAsync();
				}
				catch (RequestFailedException ex)
				{
					ExceptionHandler.Handle(ex);
				}
			}

			return name;
		}

		public static async Task<PlayerInfo> GetPlayerInfoAsync()
		{
			var info = AuthService.PlayerInfo;
			if (info == null)
			{
				try
				{
					info = await AuthService.GetPlayerInfoAsync();
				}
				catch (RequestFailedException ex)
				{
					ExceptionHandler.Handle(ex);
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
		///     Subscribe to the OnBeforeDeleteAccount event to perform data deletion.
		/// </remarks>
		public static async void DeleteAsync()
		{
			OnBeforeDeleteAccount?.Invoke();

			await AuthService.DeleteAccountAsync();
		}
	}
}
