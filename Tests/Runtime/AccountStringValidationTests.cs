// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Tests
{
	public class AccountStringValidationTests
	{
		[TestCase("Aa 00000")] // min length
		[TestCase("Aa 000000000000000000000000000")] // max length
		public void IsValidPassword_Valid_ReturnsTrue(String password) => Assert.IsTrue(Account.IsValidPassword(password));

		[TestCase("123Abc!")] // too short
		[TestCase("123Abc!123abc!123abc!123abc!_30")] // too long
		[TestCase("123 abc!")] // missing uppercase
		[TestCase("123 ABC!")] // missing lowercase
		[TestCase("abc ABC!")] // missing digit
		[TestCase("abcdABCD")] // missing symbol
		public void IsValidPassword_Invalid_ReturnsFalse(String password) => Assert.IsFalse(Account.IsValidPassword(password));

		[TestCase("123")] // min length
		[TestCase("abc")] // min length
		[TestCase("abcdef_-.@1234567890")] // max length
		[TestCase("abcd_efgh-ijkl.mnop@")] // max length
		public void SanitizeUserName_ValidName_MatchesInput(String userName)
		{
			var sanitizedName = Account.SanitizeUserName(userName);

			Assert.IsTrue(Account.IsValidUserName(userName));
			Assert.AreEqual(userName, sanitizedName);
		}

		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase("  ", "")]
		[TestCase(" 1 2 3 ", "123")]
		[TestCase("!()$%+#?", "________")]
		[TestCase("-My= Na`me.", "-My_Na_me.")]
		[TestCase("12", "12")] // too short
		[TestCase("                         ", "")] // too long
		[TestCase("00                      0", "000")] // too long
		[TestCase("01234567890123456789.123", "01234567890123456789")] // too long
		[TestCase(" 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 ", "01234567890123456789")] // too long
		public void SanitizeUserName_InvalidName_RemovesInvalidCharacters(String userName, String expected)
		{
			var sanitizedName = Account.SanitizeUserName(userName);

			Assert.IsFalse(Account.IsValidUserName(userName));
			Assert.AreEqual(expected, sanitizedName);
		}

		[TestCase("a")] // min length
		[TestCase("0")] // min length
		[TestCase("abcdefghijklmnopqrstuvwxyz0123456789_-@/#(){}[]*&?")] // max length
		public void SanitizePlayerName_ValidName_MatchesInput(String playerName)
		{
			var sanitizedName = Account.SanitizePlayerName(playerName);

			Assert.IsTrue(Account.IsValidPlayerName(playerName));
			Assert.AreEqual(playerName, sanitizedName);
		}

		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase(" ", "")]
		[TestCase("  ", "")]
		[TestCase("                                                   ", "")] // too long
		[TestCase("PlayerNames can have\tany\nchar\rexcept whitespace", "PlayerNamescanhaveanycharexceptwhitespace")]
		[TestCase("-My = Na`me.", "-My=Na`me.")]
		[TestCase("01234567890123456789012345678901234567890123456789.123",
			"01234567890123456789012345678901234567890123456789")] // too long
		public void SanitizePlayerName_InvalidName_RemovesInvalidCharacters(String playerName, String expected)
		{
			var sanitizedName = Account.SanitizePlayerName(playerName);

			Assert.IsFalse(Account.IsValidPlayerName(playerName));
			Assert.AreEqual(expected, sanitizedName);
		}
	}
}
