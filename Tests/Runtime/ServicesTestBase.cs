// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using NUnit.Framework;
using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Tests
{
	public class ServicesTestBase
	{
		protected const String TestEnvironmentName = "testing";
		protected const String TestUsername = "UnitTestUser1";
		protected const String TestPassword = "ABC-abc_123!";

		private static InitializationOptions TestServiceOptions() =>
			new InitializationOptions().SetEnvironmentName(TestEnvironmentName);

		private static async Task InitializeServices()
		{
			await UnityServices.InitializeAsync(TestServiceOptions());
			Assert.True(UnityServices.State == ServicesInitializationState.Initialized);
		}

		protected static async Task SignUpAndSignOut(String username, String password)
		{
			await Authenticate.SignUpWithUsernamePasswordAsync(username, password);
			Assert.IsTrue(AuthenticationService.Instance.IsSignedIn);

			AuthenticationService.Instance.SignOut(true);
			Assert.IsFalse(AuthenticationService.Instance.IsSignedIn);
		}

		[SetUp] public async Task SetUp()
		{
			await InitializeServices();

			// previous TearDown may not have run due to test error
			await TearDown();

			Authenticate.RethrowExceptions = true;
		}

		[TearDown] public async Task TearDown()
		{
			// to delete account we need to be signed in, but we may not depending on test failure
			if (AuthenticationService.Instance.IsSignedIn == false)
			{
				Debug.Log("TearDown sign-in -- ignore any logs between here ...");
				try { await Authenticate.SignInWithUsernamePasswordAsync(TestUsername, TestPassword); }
				catch (Exception) {}
				finally
				{
					Debug.Log("... and here. (TearDown sign-in)");
				}
			}

			if (AuthenticationService.Instance.IsSignedIn)
			{
				await AuthenticationService.Instance.DeleteAccountAsync();
				AuthenticationService.Instance.SignOut(true);
			}
		}

		protected void LogPlayerInfo(PlayerInfo info)
		{
			var sb = new StringBuilder($"PlayerInfo {info.Id}: {info.Username} created: {info.CreatedAt}");
			foreach (var identity in info.Identities)
				sb.AppendLine($"identity {identity.UserId} is of type {identity.TypeId}");

			Debug.Log(sb.ToString());
		}
	}
}
