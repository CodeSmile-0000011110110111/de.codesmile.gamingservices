// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices.Authentication
{
	public static partial class Authenticate
	{
		public static async Task SignInWithUsernamePasswordAsync(String username, String password) =>
			await WithUsernamePasswordAsync(UserPassOperation.SignIn, username, password);

		public static async Task SignUpWithUsernamePasswordAsync(String username, String password)
		{
			var procedure = Service.IsSignedIn ? UserPassOperation.AddUser : UserPassOperation.SignUp;
			await WithUsernamePasswordAsync(procedure, username, password);
		}

		public static async Task UpdatePasswordAsync(String currentPassword, String newPassword)
		{
			try
			{
				Debug.Log("Updating password ...");
				await Service.UpdatePasswordAsync(currentPassword, newPassword);
			}
			catch (RequestFailedException ex) { await DefaultExceptionHandling(ex); }
		}

		private static async Task WithUsernamePasswordAsync(UserPassOperation operation, String username, String password)
		{
			try
			{
				await Services.Initialize();
				switch (operation)
				{
					case UserPassOperation.SignIn:
						Debug.Log("Sign in with username/password ...");
						await Service.SignInWithUsernamePasswordAsync(username, password);
						break;
					case UserPassOperation.SignUp:
						Debug.Log("Sign up with username/password ...");
						await Service.SignUpWithUsernamePasswordAsync(username, password);
						break;
					case UserPassOperation.AddUser:
						Debug.Log("Update anonymous player to username/password ...");
						await Service.AddUsernamePasswordAsync(username, password);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
				}

				await NotificationHandler.ShowDsa();
			}
			catch (RequestFailedException ex) { await DefaultExceptionHandling(ex); }
		}

		private enum UserPassOperation
		{
			SignIn,
			SignUp,
			AddUser,
		}
	}
}
