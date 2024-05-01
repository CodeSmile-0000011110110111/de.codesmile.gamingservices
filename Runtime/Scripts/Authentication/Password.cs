// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices.Authentication
{
	public class Password
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
			TooFewLowercase = 1 << 3,
			TooFewUppercase = 1 << 4,
			TooFewDigits = 1 << 5,
			TooFewSymbols = 1 << 6,
		}

		private const Int32 DefaultLowercaseCount = 1;
		private const Int32 DefaultUppercaseCount = 1;
		private const Int32 DefaultDigitCount = 1;
		private const Int32 DefaultSymbolCount = 1;
		private const Int32 DefaultMinLength = 8;
		private const Int32 DefaultMaxLength = 30;

		private const String DefaultValidSymbols = @"?!.:;,_#§$%&@""´'`°^~*+-/\\|<=>(){}\[\]";

		private const String CountLowercasePattern = "(?=.*[a-z])";
		private const String CountUppercasePattern = "(?=.*[A-Z])";
		private const String CountDigitsPattern = "(?=.*[0-9])";

		private readonly String m_CountSymbolsPattern;
		private readonly String m_InvalidPattern;

		/// <summary>
		///     The password string. It may or may not be a valid password.
		/// </summary>
		public String Value { get; set; }
		/// <summary>
		///     The password requirements that have to be satisfied.
		/// </summary>
		public Requirements Requires { get; }

		public static implicit operator String(Password password) => password.Value;
		public static implicit operator Password(String password) => new(password);

		private static ValidationState Validate(String password, Requirements requires, String countSymbols, String invalidChar)
		{
			if (password == null)
				return ValidationState.TooShort;

			var state = ValidationState.Valid;

			if (password.Length < requires.MinLength)
				state |= ValidationState.TooShort;

			if (password.Length > requires.MaxLength)
				state |= ValidationState.TooLong;

			if (Regex.Match(password, CountLowercasePattern).Captures.Count < requires.LowercaseCount)
				state |= ValidationState.TooFewLowercase;

			if (Regex.Match(password, CountUppercasePattern).Captures.Count < requires.UppercaseCount)
				state |= ValidationState.TooFewUppercase;

			if (Regex.Match(password, CountDigitsPattern).Captures.Count < requires.DigitCount)
				state |= ValidationState.TooFewDigits;

			if (Regex.Match(password, countSymbols).Captures.Count < requires.SymbolCount)
				state |= ValidationState.TooFewSymbols;

			if (Regex.Match(password, invalidChar).Success)
				state |= ValidationState.InvalidSymbol;

			return state;
		}

		/// <summary>
		///     Creates a new password with an empty string and default settings.
		/// </summary>
		public Password()
			: this(String.Empty) {}

		/// <summary>
		///     Creates a new password with a string and validation requirements.
		/// </summary>
		/// <param name="password"></param>
		/// <param name="requirements"></param>
		public Password(String password, Requirements requirements = null)
		{
			Value = password ?? String.Empty;
			Requires = requirements ?? new Requirements();
			m_CountSymbolsPattern = $"(?=.*[{Requires.ValidSymbols}])";
			m_InvalidPattern = $"[^a-zA-Z0-9{Requires.ValidSymbols}]"; // [^...] negates the character class
		}

		/// <summary>
		///     Returns whether Value is a valid password. If not, the ValidationState encodes what's invalid about it.
		/// </summary>
		public ValidationState Validate() => Validate(Value, Requires, m_CountSymbolsPattern, m_InvalidPattern);

		public class Requirements
		{
			/// <summary>
			///     The minimum length for a password. Default: 8
			/// </summary>
			public Int32 MinLength { get; }
			/// <summary>
			///     The maximum length for a password. Default: 30
			/// </summary>
			public Int32 MaxLength { get; }
			/// <summary>
			///     Required number of lowercase letters.
			/// </summary>
			public Int32 LowercaseCount { get; }
			/// <summary>
			///     Required number of uppercase letters.
			/// </summary>
			public Int32 UppercaseCount { get; }
			/// <summary>
			///     Required number of digits.
			/// </summary>
			public Int32 DigitCount { get; }
			/// <summary>
			///     Required number of valid symbols.
			/// </summary>
			public Int32 SymbolCount { get; }
			/// <summary>
			///     Regex escaped characters of valid password symbols.
			/// </summary>
			public String ValidSymbols { get; }

			public Requirements(Int32 minLength = -1, Int32 maxLength = -1, Int32 lowercaseCount = -1,
				Int32 uppercaseCount = -1, Int32 digitCount = -1, Int32 symbolCount = -1, String validSymbols = null)
			{
				MinLength = minLength >= 0 ? minLength : DefaultMinLength;
				MaxLength = maxLength >= 0 ? maxLength : DefaultMaxLength;
				LowercaseCount = lowercaseCount >= 0 ? lowercaseCount : DefaultLowercaseCount;
				UppercaseCount = uppercaseCount >= 0 ? uppercaseCount : DefaultUppercaseCount;
				DigitCount = digitCount >= 0 ? digitCount : DefaultDigitCount;
				SymbolCount = symbolCount >= 0 ? symbolCount : DefaultSymbolCount;
				ValidSymbols = validSymbols ?? DefaultValidSymbols;
			}
		}
	}
}
