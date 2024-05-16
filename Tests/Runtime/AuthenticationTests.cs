// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Tests
{
	public class AuthenticationTests : ServicesTestBase
	{
		[Test] public async Task SignInAnonymouslyTest()
		{
			await Authenticate.SignInAnonymouslyAsync();

			Assert.IsTrue(AuthenticationService.Instance.IsSignedIn);
		}

		[Test] public async Task SignUpWithUsernamePassword_IsSignedIn()
		{
			await Authenticate.SignUpWithUsernamePasswordAsync(TestUsername, TestPassword);

			Assert.IsTrue(AuthenticationService.Instance.IsSignedIn);
			var playerName = await Account.GetPlayerNameAsync();
			Assert.IsNotEmpty(playerName);
			Debug.Log(playerName);
		}

		[Test] public async Task SignInWithUsernamePassword_IsSignedIn()
		{
			await SignUpAndSignOut(TestUsername, TestPassword);

			await Authenticate.SignInWithUsernamePasswordAsync(TestUsername, TestPassword);
			Assert.IsTrue(AuthenticationService.Instance.IsSignedIn);
		}

		[TestCase(TestUsername, "Wr0ng-Passw0rd!")]
		[TestCase("wrong.username", TestPassword)]
		[TestCase("wrong.username", "Wr0ng-Passw0rd!")]
		public async Task SignInWithIncorrectCredentials_FailsWithInvalidUsernameOrPassword(String username, String password)
		{
			await SignUpAndSignOut(TestUsername, TestPassword);

			RequestFailedException expectedException = null;
			try
			{
				await Authenticate.SignInWithUsernamePasswordAsync(username, password);
			}
			catch (RequestFailedException ex) { expectedException = ex; }

			Assert.NotNull(expectedException);
			Assert.NotNull(expectedException.InnerException);
			// as of May 1st 2024 there is no better way ...
			Assert.IsTrue(expectedException.InnerException.Message.Contains(@"""title"":""WRONG_USERNAME_PASSWORD"""));
		}

		[Test] public async Task AddUserWithUsernamePassword_IsSignedIn()
		{
			await Authenticate.SignInAnonymouslyAsync();
			Assert.IsTrue(AuthenticationService.Instance.IsSignedIn);

			await Authenticate.AddUsernamePasswordAsync(TestUsername, TestPassword);
			Assert.IsTrue(AuthenticationService.Instance.IsSignedIn);
		}

		[Test] public async Task GetPlayerInfo()
		{
			await Authenticate.SignInAnonymouslyAsync();

			var info = AuthenticationService.Instance.PlayerInfo;
			LogPlayerInfo(info);

			info = await AuthenticationService.Instance.GetPlayerInfoAsync();
			LogPlayerInfo(info);

			await Authenticate.AddUsernamePasswordAsync(TestUsername, TestPassword);
			LogPlayerInfo(info);

			info = await AuthenticationService.Instance.GetPlayerInfoAsync();
			LogPlayerInfo(info);
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
