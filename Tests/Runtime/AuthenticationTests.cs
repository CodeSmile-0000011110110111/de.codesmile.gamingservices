// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Tests
{
	public class AuthenticationTests
	{
		private static async Task InitializeServices()
		{
			await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName("testing"));
			AssertServicesInitialized();
		}

		private static void AssertServicesInitialized() =>
			Assert.True(UnityServices.State == ServicesInitializationState.Initialized);

		[SetUp] public async Task SetUp()
		{
			Authenticate.RethrowExceptions = true;
			await InitializeServices();
			AuthenticationService.Instance.ClearSessionToken();
		}

		[TearDown] public async Task TearDown()
		{
			await AuthenticationService.Instance.DeleteAccountAsync();
			AuthenticationService.Instance.SignOut(true);
		}

		[Test] public async Task SignInAnonymouslyTest()
		{
			await Authenticate.SignInAnonymouslyAsync();

			Assert.IsTrue(AuthenticationService.Instance.IsSignedIn);
		}

		[Test] public async Task SignUpWithUsernamePassword_SimpleTest()
		{
			await Authenticate.SignUpWithUsernamePasswordAsync("hellouser", "123_abc_ABC");

			Assert.IsTrue(AuthenticationService.Instance.IsSignedIn);
		}

		/*
		[TestCase("aA12bB34:")] [TestCase("aA12bB34;")] [TestCase("aA12bB34<")] [TestCase("aA12bB34.")] [TestCase("aA12bB34`")]
		[TestCase("aA12bB34=")] [TestCase("aA12bB34>")] [TestCase("aA12bB34?")] [TestCase("aA12bB34@")] [TestCase("aA12bB34(")]
		[TestCase("aA12bB34^")] [TestCase("aA12bB34_")] [TestCase("aA12bB34{")] [TestCase("aA12bB34|")] [TestCase("aA12bB34)")]
		[TestCase("aA12bB34}")] [TestCase("aA12bB34~")] [TestCase("aA12bB34[")] [TestCase("aA12bB34]")] [TestCase("aA12bB34*")]
		[TestCase("aA12bB34!")] [TestCase("aA12bB34#")] [TestCase("aA12bB34$")] [TestCase("aA12bB34,")] [TestCase("aA12bB34-")]
		[TestCase("aA12bB34%")] [TestCase("aA12bB34&")] [TestCase("aA12bB34'")] [TestCase("aA12bB34/")] [TestCase("aA12bB34+")]
		[TestCase(@"aA12bB34´")] [TestCase(@"aA12bB34\")] [TestCase(@"aA12bB34""")]
		public async Task SignUpWithUsernamePassword_PasswordWithAllowedSymbol(String password)
		{
			await Authenticate.SignUpWithUsernamePasswordAsync("hellouser", password);

			Assert.IsTrue(Account.IsValidPassword(password));
			Assert.IsTrue(AuthenticationService.Instance.IsSignedIn);
		}
	*/
	}
}
