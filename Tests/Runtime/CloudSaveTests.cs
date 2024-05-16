// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudSave;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using Object = System.Object;

namespace CodeSmile.Tests
{
	public class CloudSaveTests : ServicesTestBase
	{
		[TestCase("TestSaveKey", "Hello Data")]
		[TestCase("VarChars",
			@"°^!""§$\0%$&/\()=?`´ß09+ü*Ü'Ä#äöÖ_-:.;,<>|\}][{³²\n\t\r ■²ⁿ√·∙≈°¶‼  !▼▲↔∟←→↓↑↨▬§¶‼↕◄►☼♫♪♀♂◙○◘•♠♣♦♥☻☺")]
		public async Task CloudSavePlayerDataTest1(String key, String value)
		{
			await Authenticate.SignInAnonymouslyAsync();

			var data = new Dictionary<String, Object> { { key, value } };
			await CloudSaveService.Instance.Data.Player.SaveAsync(data);

			var cloudData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<String> { key });

			Debug.Log(cloudData[key]);
			Debug.Log(cloudData[key].Value);
			Debug.Log(cloudData[key].Value.GetAsString());

			Assert.AreEqual(value, cloudData[key].Value.GetAsString());
		}

		[Test]
		public async Task AnotherTest()
		{
			await Authenticate.SignInAnonymouslyAsync();

			var module = new CC_HelloWorldBindings(CloudCodeService.Instance);
			var result = await module.SayHello("World");
			Debug.Log($"result from cloud code: {result}");

			var key = "Key123";
			var data = await CloudSaveService.Instance.Data.Custom.LoadAsync("MyCustomDataID", new HashSet<String> { key });
			Debug.Log($"received: {data[key].Value.GetAsString()}");
		}

	}
}
