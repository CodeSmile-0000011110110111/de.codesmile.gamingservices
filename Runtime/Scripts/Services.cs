// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices
{
	public static class Services
	{
		// TODO: auto-init only if requested (should be a setting)
		//[RuntimeInitializeOnLoadMethod] private static async void OnRuntimeLoad() => await Initialize();

		public static event Func<Exception, String, Int32, Task> OnServiceRequestFailed;

		public static async Task Initialize()
		{
			if (UnityServices.State != ServicesInitializationState.Uninitialized)
				return;

			try
			{
				Debug.Log("Initializing Unity Services ...");
				await UnityServices.InitializeAsync();

				Authenticate.OnServicesInitialized();
			}
			catch (Exception ex)
			{
				HandleServiceException(ex);
			}
		}

		internal static async void HandleServiceException(Exception ex)
		{
			// Notify the player with the proper error message
			var errorReason = ex.Message;
			var errorCode = -1;
			if (ex is RequestFailedException failed)
			{
				errorReason = GetErrorReason(failed.ErrorCode);
				errorCode = failed.ErrorCode;
			}

			await OnServiceRequestFailed?.Invoke(ex, errorReason, errorCode);
		}

		private static String GetErrorReason(Int32 errorCode)
		{
			// Common Error Codes
			if (errorCode == CommonErrorCodes.Unknown)
				return "Unknown";
			if (errorCode == CommonErrorCodes.TransportError)
				return "TransportError";
			if (errorCode == CommonErrorCodes.Timeout)
				return "Timeout";
			if (errorCode == CommonErrorCodes.ServiceUnavailable)
				return "ServiceUnavailable";
			if (errorCode == CommonErrorCodes.ApiMissing)
				return "ApiMissing";
			if (errorCode == CommonErrorCodes.RequestRejected)
				return "RequestRejected";
			if (errorCode == CommonErrorCodes.TooManyRequests)
				return "TooManyRequests";
			if (errorCode == CommonErrorCodes.InvalidToken)
				return "InvalidToken";
			if (errorCode == CommonErrorCodes.TokenExpired)
				return "TokenExpired";
			if (errorCode == CommonErrorCodes.Forbidden)
				return "Forbidden";
			if (errorCode == CommonErrorCodes.NotFound)
				return "NotFound";
			if (errorCode == CommonErrorCodes.InvalidRequest)
				return "InvalidRequest";
			if (errorCode == CommonErrorCodes.ProjectPolicyAccessDenied)
				return "ProjectPolicyAccessDenied";
			if (errorCode == CommonErrorCodes.PlayerPolicyAccessDenied)
				return "PlayerPolicyAccessDenied";
			if (errorCode == CommonErrorCodes.Conflict)
				return "Conflict";

			// Authentication Error Codes
			if (errorCode == AuthenticationErrorCodes.ClientInvalidUserState)
				return "ClientInvalidUserState";
			if (errorCode == AuthenticationErrorCodes.ClientNoActiveSession)
				return "ClientNoActiveSession";
			if (errorCode == AuthenticationErrorCodes.InvalidParameters)
				return "InvalidParameters";
			if (errorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
				return "AccountAlreadyLinked";
			if (errorCode == AuthenticationErrorCodes.AccountLinkLimitExceeded)
				return "AccountLinkLimitExceeded";
			if (errorCode == AuthenticationErrorCodes.ClientUnlinkExternalIdNotFound)
				return "ClientUnlinkExternalIdNotFound";
			if (errorCode == AuthenticationErrorCodes.ClientInvalidProfile)
				return "ClientInvalidProfile";
			if (errorCode == AuthenticationErrorCodes.InvalidSessionToken)
				return "InvalidSessionToken";
			if (errorCode == AuthenticationErrorCodes.InvalidProvider)
				return "InvalidProvider";
			if (errorCode == AuthenticationErrorCodes.BannedUser)
				return "BannedUser";
			if (errorCode == AuthenticationErrorCodes.EnvironmentMismatch)
				return "EnvironmentMismatch";

			Debug.LogWarning($"unhandled error code {errorCode} in error message mapping");
			return $"Error {errorCode}";
		}
	}
}
