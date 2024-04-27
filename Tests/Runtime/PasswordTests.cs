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
		public void IsValidPassword_Valid_ReturnsTrue(String password) => Assert.IsTrue(Password.IsValidPassword(password));

		[TestCase("123Abc!")] // too short
		[TestCase("123Abc!123abc!123abc!123abc!_30")] // too long
		[TestCase("123 abc!")] // missing uppercase
		[TestCase("123 ABC!")] // missing lowercase
		[TestCase("abc ABC!")] // missing digit
		[TestCase("abcdABCD")] // missing symbol
		public void IsValidPassword_Invalid_ReturnsFalse(String password) => Assert.IsFalse(Password.IsValidPassword(password));
	}
}
