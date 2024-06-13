// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using UnityEditor;
using UnityEngine;

namespace CodeSmileEditor.Tests
{
	public static class Dummy
	{
		[InitializeOnLoadMethod]
		public static async void Test()
		{
			// Debug.Log($"cloud project id: {CloudProjectSettings.projectId}");
			// Debug.Log($"org key: {CloudProjectSettings.organizationKey}");
			// Debug.Log($"access token: {CloudProjectSettings.accessToken}");

			// var apiKey = "??";
			// var apiSecret = "??";

			// await EnvironmentsApi.Instance.RefreshAsync();
			// var envId = EnvironmentsApi.Instance.ActiveEnvironmentId.Value.ToString();
			// var projectId = CloudProjectSettings.projectId;
			//
			// Debug.Log($"project: {projectId} ({CloudProjectSettings.projectName})");
			// Debug.Log($"active environment: {envId} ({EnvironmentsApi.Instance.ActiveEnvironmentName})");
			//
			// var trustedClient = ApiService.CreateTrustedClient();
			// trustedClient.SetServiceAccount(apiKey, apiSecret);
			// var signInResponse = await trustedClient.SignInWithServiceAccount(projectId, envId);
			// Debug.Log($"trusted sign-in response: {signInResponse.ErrorText} {signInResponse.Content}");
			// var trustedResponse = await trustedClient.CloudSaveData.SetCustomItem(projectId, "trustedTestCustomItemId",
			// 	new SetItemBody("theKey", "someValue"));
			// Debug.Log($"trusted - set item response: {trustedResponse.ErrorText} {trustedResponse.Content}");
			//
			// var adminClient = ApiService.CreateAdminClient();
			// adminClient.SetServiceAccount(apiKey, apiSecret);
			// var adminResponse = await adminClient.CloudSaveData.SetCustomItem(projectId, envId, "adminTestCustomItemId",
			// 	new Unity.Services.Apis.Admin.CloudSave.SetItemBody("theKey", "someValue"));
			// Debug.Log($"admin - set item response: {adminResponse.ErrorText} {adminResponse.Content}");
			//
			// var configResponse = await adminClient.RemoteConfig.RemoteConfigV1ProjectsProjectIdConfigsGet(projectId);
			// Debug.Log($"admin - config: {configResponse.ErrorText} {configResponse.Content}");
		}
	}
}
