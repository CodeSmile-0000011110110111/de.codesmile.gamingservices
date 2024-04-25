// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Text.RegularExpressions;

namespace CodeSmile.GamingServices.Authentication
{
	public class Password
	{
		public const Int32 MinPasswordLength = 8;
		public const Int32 MaxPasswordLength = 30;

		/// <summary>
		///     The minimum length for a username. Default: 3
		/// </summary>
		public Int32 LengthMin { get; }

		/// <summary>
		///     The maximum length for a username. Default: 20
		/// </summary>
		public Int32 LengthMax { get; }

		/// <summary>
		///     Regex pattern that matches a valid password string.
		/// </summary>
		/// <remarks>
		///     Not all symbols have been tested to be allowed. Waiting for confirmation.
		/// </remarks>
		public static readonly String PasswordPattern =
			@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!""#$%&'´`()*+-.,/\\:;<=>?@^_{|}~\[\]])";

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
	}
}
