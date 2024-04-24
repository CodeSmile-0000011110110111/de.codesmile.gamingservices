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
