// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Tests
{
	public class UsernameTests
	{
		[TestCase("123")] // min length
		[TestCase("abc")] // min length
		[TestCase("abcdef_-.@1234567890")] // max length
		[TestCase("abcd_efgh-ijkl.mnop@")] // max length
		public void Validate_ValidUsername_Succeeds(String name)
		{
			Assert.AreEqual(Username.ValidationState.Valid, Username.Validate(name));
			Assert.IsTrue(new Username(name).IsValid);
		}

		[TestCase(null, Username.ValidationState.TooShort)]
		[TestCase("", Username.ValidationState.TooShort)]
		[TestCase(" ", Username.ValidationState.TooShort | Username.ValidationState.InvalidSymbol)]
		[TestCase("12", Username.ValidationState.TooShort)]
		[TestCase(" 1 2 3 ", Username.ValidationState.InvalidSymbol)]
		[TestCase("!()$%+#?", Username.ValidationState.InvalidSymbol)]
		[TestCase("-My= Na`me.", Username.ValidationState.InvalidSymbol)]
		[TestCase("                         ", Username.ValidationState.InvalidSymbol | Username.ValidationState.TooLong)]
		[TestCase("0000000000000000000000000", Username.ValidationState.TooLong)]
		[TestCase("01234567890123456789.123", Username.ValidationState.TooLong)]
		[TestCase(" 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 ", Username.ValidationState.InvalidSymbol | Username.ValidationState.TooLong)]
		public void Validate_InvalidUsername_FailsWithExpectedResult(String name, Username.ValidationState expected)
		{
			Assert.AreEqual(expected, Username.Validate(name));
			Assert.IsFalse(new Username(name).IsValid);
		}

		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase("  ", "__")]
		[TestCase(" 1 2 3 ", "_1_2_3_")]
		[TestCase("!()$%+#?", "________")]
		[TestCase("-My= Na`me.", "-My__Na_me.")]
		[TestCase("12", "12")] // too short
		[TestCase("                         ", "____________________")] // too long
		[TestCase("00                      0", "00__________________")] // too long
		[TestCase("01234567890123456789.123", "01234567890123456789")] // too long
		[TestCase(" 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 ", "_0_1_2_3_4_5_6_7_8_9")] // too long
		[TestCase("_Valid.User@Name-123", "_Valid.User@Name-123")]
		public void SanitizedName_ReplacesInvalidSymbols(String userName, String expected)
		{
			Assert.AreEqual(expected, new Username(userName).SanitizedName);
		}
	}
}
