// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace CodeSmile.Tests
{
	public class WebRequestTests
	{
		private const String ContentTypeJson = "application/json";
		private const String ServicesBaseUrl = "https://services.api.unity.com";

		private static String CreateProjectIdParam(String projectId) => $"projectId={projectId}";
		private static String CreateEnvironmentIdParam(String environmentId) => $"environmentId={environmentId}";
		private static void SetAuthHeaderWithKeyAndSecret(UnityWebRequest request, string projectKeyId, string secret) =>
			request.SetRequestHeader("Authorization", $"Basic {projectKeyId}:{secret}");

		[UnityTest]
		public IEnumerator AdminWebRequestTest()
		{
			// curl -X POST -H "content-length: 0" -H "Authorization: Basic tokenhere" "https://services.api.unity.com/auth/v1/token-exchange?projectId=6a6199fd-806e-4c16-a75d-9aa3d4ee5c8b&environmentId=605bd057-b111-46d8-b37f-b7959138b9b9"

			var projectId = CreateProjectIdParam("6a6199fd-806e-4c16-a75d-9aa3d4ee5c8b");
			var envId = CreateEnvironmentIdParam("605bd057-b111-46d8-b37f-b7959138b9b9");
			var uri = new Uri($"{ServicesBaseUrl}/auth/v1/token-exchange?{projectId}&{envId}");

			var request = UnityWebRequest.Post(uri, "", ContentTypeJson);
			SetAuthHeaderWithKeyAndSecret(request,"cf711530-7244-49b2-9dff-72a0f23ab9ad","fOdG3auRV1uL6791EIf9LnA-40q_Ic4d");

			Debug.Log($"request: {uri}");
			Debug.Log($"auth header: {request.GetRequestHeader("Authorization")}");

			var operation = request.SendWebRequest();

			while (operation.isDone == false)
			{
				yield return null;
			}

			Debug.Log("result: " + request.result);
			Debug.Log("error: " + request.error);
			Debug.Log("dl text: " + request.downloadHandler?.text);
			Debug.Log("dl error: " + request.downloadHandler?.error);
			// Debug.Log(request.GetResponseHeader("RateLimit-Policy"));
			// Debug.Log(request.GetResponseHeader("RateLimit"));
			// Debug.Log(request.GetResponseHeader("Unity-RateLimit"));
			// Debug.Log(request.GetResponseHeader("Retry-After"));
		}


		/*public static Object DeserializeUntyped(String json, Boolean isArray = false)
		{
			if (isArray == false)
				isArray = json.Substring(0, 1) == "[";

			if (isArray)
			{
				var array = new List<Object>();
				foreach (var obj in Newtonsoft.Json.JsonConvert.DeserializeObject<List<Object>>(json))
					array.Add(obj is JObject or JArray ? DeserializeUntyped(obj.ToString(), obj is JArray) : obj);
				return array;
			}
			var dict = new Dictionary<String, Object>();
			foreach (var kvp in Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<String, Object>>(json))
			{
				dict.Add(kvp.Key, kvp.Value is JObject or JArray
					? DeserializeUntyped(kvp.Value.ToString(), kvp.Value is JArray)
					: kvp.Value);
			}
			return dict;
		}*/
	}
}
