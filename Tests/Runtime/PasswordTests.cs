// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Tests
{
	public class PasswordTests
	{
		[TestCase("Aa_00000")] // min length
		[TestCase("Aa.000000000000000000000000000")] // max length
		public void Validate_Valid_IsValid(String password) =>
			Assert.AreEqual(Password.ValidationState.Valid, new Password(password).Validate());

		[TestCase("123Abc!")] // too short
		[TestCase("123Abc!123abc!123abc!123abc!_30")] // too long
		[TestCase("1234abc!")] // missing uppercase
		[TestCase("1234ABC!")] // missing lowercase
		[TestCase("abcdABC!")] // missing digit
		[TestCase("abcdABCD")] // missing symbol
		[TestCase("        ")] // invalid symbols
		public void Validate_InvalidPassword_IsNotValid(String password)
		{
			Debug.Log("state: " + new Password(password).Validate());
			Assert.AreNotEqual(Password.ValidationState.Valid, new Password(password).Validate());
		}

		[TestCase(null)] [TestCase("")] [TestCase("       ")]
		[TestCase("AAAAAAA")] [TestCase("aaaaaaa")] [TestCase("0000000")] [TestCase("_______")] [TestCase("_-@ABcd")]
		public void Validate_TooShort_FlaggedAsTooShort(String pw) =>
			Assert.IsTrue(new Password(pw).Validate().HasFlag(Password.ValidationState.TooShort));

		[TestCase("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAZ")]
		[TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaz")]
		[TestCase("0000000000000000000000000000009")]
		[TestCase("______________________________-")]
		[TestCase("AAAAAaaaaa00000_____BBBBBbbbbb-")]
		[TestCase("                               ")]
		public void Validate_TooLong_FlaggedAsTooLong(String pw) =>
			Assert.IsTrue(new Password(pw).Validate().HasFlag(Password.ValidationState.TooLong));

		[TestCase(null, Password.ValidationState.TooShort | Password.ValidationState.TooFewLowercase |
		                Password.ValidationState.TooFewUppercase | Password.ValidationState.TooFewDigits |
		                Password.ValidationState.TooFewSymbols)]
		[TestCase("", Password.ValidationState.TooShort | Password.ValidationState.TooFewLowercase |
		              Password.ValidationState.TooFewUppercase | Password.ValidationState.TooFewDigits |
		              Password.ValidationState.TooFewSymbols)]
		[TestCase(" ", Password.ValidationState.TooShort | Password.ValidationState.TooFewLowercase |
		               Password.ValidationState.TooFewUppercase | Password.ValidationState.TooFewDigits |
		               Password.ValidationState.TooFewSymbols | Password.ValidationState.InvalidSymbol)]
		[TestCase("A", Password.ValidationState.TooShort | Password.ValidationState.TooFewLowercase |
		               Password.ValidationState.TooFewDigits | Password.ValidationState.TooFewSymbols)]
		[TestCase("a", Password.ValidationState.TooShort | Password.ValidationState.TooFewUppercase |
		               Password.ValidationState.TooFewDigits | Password.ValidationState.TooFewSymbols)]
		[TestCase("1", Password.ValidationState.TooShort | Password.ValidationState.TooFewLowercase |
		               Password.ValidationState.TooFewUppercase | Password.ValidationState.TooFewSymbols)]
		[TestCase("_", Password.ValidationState.TooShort | Password.ValidationState.TooFewLowercase |
		               Password.ValidationState.TooFewUppercase | Password.ValidationState.TooFewDigits)]
		[TestCase("ABCD1234_", Password.ValidationState.TooFewLowercase)]
		[TestCase("abab1234_", Password.ValidationState.TooFewUppercase)]
		[TestCase("ABabCDcd_", Password.ValidationState.TooFewDigits)]
		[TestCase("ABab1234", Password.ValidationState.TooFewSymbols)]
		[TestCase("ABab 1234_", Password.ValidationState.InvalidSymbol)]
		[TestCase("ABab 1234", Password.ValidationState.InvalidSymbol | Password.ValidationState.TooFewSymbols)]
		public void Validate_VariousIssues_ContainsExpectedFlags(String pw, Password.ValidationState expectedState) =>
			Assert.AreEqual(expectedState, new Password(pw).Validate());
	}
}
