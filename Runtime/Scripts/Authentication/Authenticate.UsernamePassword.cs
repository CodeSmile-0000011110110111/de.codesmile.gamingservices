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
		public static async Task SignUpWithUsernamePasswordAsync(String username, String password) =>
			await AuthenticateWithUsernamePasswordAsync(UserPassOperation.SignUp, username, password);

		public static async Task SignInWithUsernamePasswordAsync(String username, String password) =>
			await AuthenticateWithUsernamePasswordAsync(UserPassOperation.SignIn, username, password);

		public static async Task AddUsernamePasswordAsync(String username, String password) =>
			await AuthenticateWithUsernamePasswordAsync(UserPassOperation.AddUser, username, password);

		public static async Task UpdatePasswordAsync(String currentPassword, String newPassword)
		{
			try
			{
				Debug.Log("Updating password ...");
				await Service.UpdatePasswordAsync(currentPassword, newPassword);
			}
			catch (RequestFailedException ex) { await DefaultExceptionHandling(ex); }
		}

		private static async Task AuthenticateWithUsernamePasswordAsync(UserPassOperation operation, String username,
			String password)
		{
			try
			{
				await Services.Initialize();

				Debug.Log($"{operation} with username '{username}' ...");

				switch (operation)
				{
					case UserPassOperation.SignIn:
						await Service.SignInWithUsernamePasswordAsync(username, password);
						break;
					case UserPassOperation.SignUp:
						await Service.SignUpWithUsernamePasswordAsync(username, password);
						break;
					case UserPassOperation.AddUser:
						await Service.AddUsernamePasswordAsync(username, password);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
				}

				await Account.TryShowNewNotifications();
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
