// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices.Authentication
{
	/// <summary>
	///     Handles validation of a Username string.
	/// </summary>
	/// <remarks>
	///     Assumes letters and digits are always allowed. Valid symbols can be specified and default to UGS' default
	///     valid symbols for a Username.
	/// </remarks>
	/// <remarks>
	///     A UGS Username is valid if the string does not contain whitespace, is between 3 and 20 characters,
	///     contains only english letters, numbers, or symbols '-', '_', '.', '@' (hyphen, underscore, dot, at sign).
	///     Details: https://docs.unity.com/ugs/en-us/manual/authentication/manual/platform-signin-username-password
	/// </remarks>
	public class Username
	{
		/// <summary>
		///     ValidationState indicates whether the username is valid, or if not, what exactly is not valid.
		/// </summary>
		[Flags]
		public enum ValidationState
		{
			Valid = 0,
			TooShort = 1 << 0,
			TooLong = 1 << 1,
			InvalidSymbol = 1 << 2,
		}

		public const Int32 UgsUsernameLengthMin = 3;
		public const Int32 UgsUsernameLengthMax = 20;
		public const String UgsUsernameValidSymbols = @"_\-.@";

		/// <summary>
		///     The username string. It may or may not be a valid username.
		/// </summary>
		public String Name { get; set; } = String.Empty;

		/// <summary>
		///     The sanitized username string with invalid characters replaced and shortened to max length.
		/// </summary>
		/// <remarks>CAUTION: The returned string can still be an invalid username, eg it could be too short.</remarks>
		public String SanitizedName => Sanitize(Name, ValidSymbols, LengthMax);

		/// <summary>
		///     Returns true if the username is valid, false otherwise.
		/// </summary>
		public Boolean IsValid => State == ValidationState.Valid;

		/// <summary>
		///     Encodes whether and what part of the string failed to validate.
		/// </summary>
		public ValidationState State => Validate(Name, ValidSymbols, LengthMin, LengthMax);

		/// <summary>
		///     The minimum length for a username. Default: 3
		/// </summary>
		public Int32 LengthMin { get; }

		/// <summary>
		///     The maximum length for a username. Default: 20
		/// </summary>
		public Int32 LengthMax { get; }

		/// <summary>
		///     Regex character class matching valid username symbols.
		/// </summary>
		public String ValidSymbols { get; }

		public static implicit operator String(Username username) => username.Name;
		public static implicit operator Username(String username) => new(username);

		/// <summary>
		///     Returns whether the userName is valid and if not, what failed to validate.
		/// </summary>
		/// <remarks>
		///     User name is valid if non-null, does not contain whitespace, is between 3 and 20 characters,
		///     contains only english letters, numbers, or '-', '_', '.', '@' (hyphen, underscore, dot, at sign).
		///     Details: https://docs.unity.com/ugs/en-us/manual/authentication/manual/platform-signin-username-password
		/// </remarks>
		/// <param name="username">The username string to test for validity.</param>
		/// <param name="validSymbols">A Regex pattern matching only valid symbols.</param>
		/// <param name="lengthMin">Minimum username length.</param>
		/// <param name="lengthMax">Maximum username length.</param>
		/// <returns>ValidationState.Valid if the username is valid, otherwise flags encode what's wrong. </returns>
		public static ValidationState Validate(String username, String validSymbols = UgsUsernameValidSymbols,
			Int32 lengthMin = UgsUsernameLengthMin, Int32 lengthMax = UgsUsernameLengthMax)
		{
			if (username == null)
				return ValidationState.TooShort;

			var result = ValidationState.Valid;
			if (username.Length < lengthMin)
				result |= ValidationState.TooShort;

			if (username.Length > lengthMax)
				result |= ValidationState.TooLong;

			if (Regex.Match(username, GetInvalidPattern(validSymbols)).Success)
				result |= ValidationState.InvalidSymbol;

			return result;
		}

		/// <summary>
		///     Replaces all invalid username characters with the first symbol in the validSymbols list.
		/// </summary>
		/// <remarks>
		///     CAUTION: The returned string is not necessarily a valid username! For example, if the string is too short
		///     or null it will not be padded to match the min length.
		/// </remarks>
		/// <param name="username">The username string to sanitize.</param>
		/// <param name="validSymbols">A Regex pattern matching only valid symbols.</param>
		/// <param name="lengthMax">Maximum username length.</param>
		/// <returns>Returns the modified string capped to lengthMax characters.</returns>
		public static String Sanitize(String username, String validSymbols = UgsUsernameValidSymbols,
			Int32 lengthMax = UgsUsernameLengthMax)
		{
			if (username == null)
				return String.Empty;

			if (String.IsNullOrEmpty(validSymbols) == false)
				username = Regex.Replace(username, GetInvalidPattern(validSymbols), validSymbols.Substring(0, 1));

			return username.Length > lengthMax ? username.Substring(0, lengthMax) : username;
		}

		private static String GetInvalidPattern(String validSymbols = UgsUsernameValidSymbols) => $"[^a-zA-Z0-9{validSymbols}]";

		/// <summary>
		///     Creates a new Username with an empty string and default settings.
		/// </summary>
		public Username()
			: this(String.Empty) {}

		/// <summary>
		///     Creates a new Username with the given input string and validation requirements.
		/// </summary>
		/// <param name="username">The username string.</param>
		/// <param name="validSymbols">Regex compatible list of valid symbols.</param>
		/// <param name="lengthMin"></param>
		/// <param name="lengthMax"></param>
		public Username(String username, String validSymbols = UgsUsernameValidSymbols, Int32 lengthMin = UgsUsernameLengthMin,
			Int32 lengthMax = UgsUsernameLengthMax)
		{
			Name = username;
			ValidSymbols = validSymbols;
			LengthMin = lengthMin;
			LengthMax = lengthMax;
		}
	}
}
