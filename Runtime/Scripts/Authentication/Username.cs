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
	///     Upper and lowercase letters (english alphabet) and digits are always allowed. Valid symbols can be specified if
	///     they deviate from the default valid symbols for a UGS Username.
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
			/// <summary>
			///     Value is valid given current settings.
			/// </summary>
			Valid = 0,
			/// <summary>
			///     Value does not satisfy minimum length requirement.
			/// </summary>
			TooShort = 1 << 0,
			/// <summary>
			///     Value does not satisfy maximum length requirement.
			/// </summary>
			TooLong = 1 << 1,
			/// <summary>
			///     Value contains one or more invalid symbols.
			/// </summary>
			InvalidSymbol = 1 << 2,
		}

		private const Int32 DefaultMinLength = 3;
		private const Int32 DefaultMaxLength = 20;
		private const String DefaultValidSymbols = @"_\-.@";

		private readonly String m_InvalidPattern;
		private readonly String m_ReplacementChar;

		/// <summary>
		///     The username string. It may or may not be a valid username.
		/// </summary>
		/// <remarks>
		///     Any string can be assigned, including empty or null. To test if the current value is a valid username,
		///     check the IsValid and State properties.
		/// </remarks>
		public String Value { get; set; }

		/// <summary>
		///     The Username string requirements that have to be satisfied.
		/// </summary>
		public Requirements Requires { get; }

		public static implicit operator String(Username username) => username.Value;
		public static implicit operator Username(String username) => new(username);

		private static ValidationState Validate(String username, Int32 lengthMin, Int32 lengthMax, String invalidPattern)
		{
			if (username == null)
				return ValidationState.TooShort;

			var state = ValidationState.Valid;

			if (username.Length < lengthMin)
				state |= ValidationState.TooShort;

			if (username.Length > lengthMax)
				state |= ValidationState.TooLong;

			if (Regex.Match(username, invalidPattern).Success)
				state |= ValidationState.InvalidSymbol;

			return state;
		}

		private static String Sanitize(String username, String invalidPattern, String replacementChar, Int32 lengthMax)
		{
			if (username == null)
				return String.Empty;

			// username is case insensitive (lowercase only)
			username = username.ToLower();

			// invalid symbols are replaced with the first valid symbol
			if (String.IsNullOrEmpty(invalidPattern) == false)
				username = Regex.Replace(username, invalidPattern, replacementChar);

			return username.Length > lengthMax ? username.Substring(0, lengthMax) : username;
		}

		/// <summary>
		///     Creates a new Username with an empty string and default settings.
		/// </summary>
		public Username()
			: this(String.Empty) {}

		/// <summary>
		///     Creates a new Username with a string and validation requirements.
		/// </summary>
		/// <remarks>
		///     A Username always allows upper- and lowercase letters (english alphabet) and digits.
		///     You only specify valid symbols as a regex character class, properly escaped.
		/// </remarks>
		/// <param name="username">The username string.</param>
		/// <param name="minLength">Min length of username. Default: 3</param>
		/// <param name="maxLength">Max length of username. Default: 20</param>
		/// <param name="validSymbols">
		///     Regex compatible list of valid symbols, properly escaped where necessary. Imagine this
		///     string is emplaced within [ ] to form a regex character class. Thus do not enclose the string in [].
		///     Default is: @"_\-.@"
		/// </param>
		public Username(String username, Requirements requirements = null)
		{
			Value = username ?? String.Empty;
			Requires = requirements ?? new Requirements();
			m_InvalidPattern = $"[^a-zA-Z0-9{Requires.ValidSymbols}]"; // [^...] negates the character class
			m_ReplacementChar = Requires.ValidSymbols.Length > 0 ? Requires.ValidSymbols.Substring(0, 1) : "0";
		}

		/// <summary>
		///     The sanitized username string with invalid characters replaced and shortened to max length.
		/// </summary>
		/// <remarks>CAUTION: The returned string can still be an invalid username, eg it could be too short.</remarks>
		public String GetSanitized() => Sanitize(Value, m_InvalidPattern, m_ReplacementChar, Requires.MaxLength);

		/// <summary>
		///     Returns whether Value is a valid username. If not, the ValidationState encodes what's invalid about it.
		/// </summary>
		public ValidationState Validate() => Validate(Value, Requires.MinLength, Requires.MaxLength, m_InvalidPattern);

		/// <summary>
		///     Requirements specification for a Username string.
		/// </summary>
		public class Requirements
		{
			/// <summary>
			///     The minimum length for a username. Default: 3
			/// </summary>
			public Int32 MinLength { get; }
			/// <summary>
			///     The maximum length for a username. Default: 20
			/// </summary>
			public Int32 MaxLength { get; }
			/// <summary>
			///     Regex escaped characters of valid username symbols.
			/// </summary>
			public String ValidSymbols { get; }

			/// <summary>
			///     Creates a new instance.
			/// </summary>
			/// <param name="minLength"></param>
			/// <param name="maxLength"></param>
			/// <param name="validSymbols"></param>
			public Requirements(Int32 minLength = -1, Int32 maxLength = -1, String validSymbols = null)
			{
				MinLength = minLength >= 0 ? minLength : DefaultMinLength;
				MaxLength = maxLength >= 0 ? maxLength : DefaultMaxLength;
				ValidSymbols = validSymbols ?? DefaultValidSymbols;
			}
		}
	}
}
