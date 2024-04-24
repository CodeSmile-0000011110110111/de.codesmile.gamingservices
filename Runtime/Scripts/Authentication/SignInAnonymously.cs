// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using CodeSmile.GamingServices.Authentication;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices
{
	public class SignInAnonymously : OneTimeTaskBehaviour
	{
		private async void Awake()
		{
			await Authenticate.SignInAnonymouslyAsync();
			TaskPerformed();
		}
	}
}
