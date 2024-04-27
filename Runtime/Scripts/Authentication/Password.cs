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
			MissingUppercaseLetter = 1 << 4,
			MissingLowercaseLetter = 1 << 5,
			MissingDigit = 1 << 6,
			MissingSymbol = 1 << 3,
		}

		public const Int32 UgsPasswordLengthMin = 8;
		public const Int32 UgsPasswordLengthMax = 30;

		public const String HasDigitPattern = "(?=.*[0-9])";
		public const String HasLowercaseLetterPattern = "(?=.*[a-z])";
		public const String HasUppercaseLetterPattern = "(?=.*[A-Z])";
		public const String UgsHasValidSymbolPattern = @"(?=.*[!""#$%&'´`()*+-.,/\\:;<=>?@^_{|}~\[\]])";

		/// <summary>
		///     Regex pattern that matches a valid password string.
		/// </summary>
		/// <remarks>
		///     Whitespace is not allowed
		/// </remarks>
		public static readonly String UgsValidPasswordPattern =
			$"{HasDigitPattern}{HasLowercaseLetterPattern}{HasUppercaseLetterPattern}{UgsHasValidSymbolPattern}";

		/// <summary>
		///     The password string. It may or may not be a valid password.
		/// </summary>
		public String Value { get; set; } = String.Empty;

		/// <summary>
		///     The minimum length for a password. Default: 8
		/// </summary>
		public Int32 LengthMin { get; } = UgsPasswordLengthMin;

		/// <summary>
		///     The maximum length for a password. Default: 30
		/// </summary>
		public Int32 LengthMax { get; } = UgsPasswordLengthMax;

		public static implicit operator String(Password password) => password.Value;
		public static implicit operator Password(String password) => new(password);

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

			if (password.Length < UgsPasswordLengthMin || password.Length > UgsPasswordLengthMax)
				return false;

			return Regex.Match(password, UgsValidPasswordPattern).Success;
		}

		/// <summary>
		///     Creates a new password with an empty string and default settings.
		/// </summary>
		public Password()
			: this(null) {}

		public Password(String password/*, String validSymbols = UgsUsernameValidSymbols, Int32 lengthMin = UgsUsernameLengthMin,
			Int32 lengthMax = UgsUsernameLengthMax*/)
		{
			Value = password ?? String.Empty;
			/*
			ValidSymbols = validSymbols;
			LengthMin = lengthMin;
			LengthMax = lengthMax;
		*/
		}
	}
}
