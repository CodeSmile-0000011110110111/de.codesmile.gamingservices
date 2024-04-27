// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using CodeSmile.GamingServices.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.GamingServices
{
	[RequireComponent(typeof(UIDocument))]
	public class NotificationPopup : GuiBehaviour
	{
		private const String PopupGroupName = "PopupGroup";
		private const String NotificationName = "Notification";
		private const String CopyButtonName = "CopyButton";
		private const String CloseButtonName = "CloseButton";

		private static TaskCompletionSource<Boolean> s_ShowModalPopup;

		private Dictionary<String, String> m_Notification;

		private void Awake() => Account.OnShowNotification += OnShowNotification;

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Hide();
			Account.OnShowNotification -= OnShowNotification;
		}

		private async Task OnShowNotification(Dictionary<String, String> notification) => await Show(notification);

		protected override void OnShowGUI()
		{
			base.OnShowGUI();
			UpdateContent();
			OnRegisterEvents();
		}

		private void UpdateContent()
		{
			FindFirst<GroupBox>(PopupGroupName).text = GetNotificationTitle();
			FindFirst<TextField>(NotificationName).value = GetNotificationAsText();
		}

		private String GetNotificationTitle() => m_Notification?["Type"];

		private String GetNotificationAsText() => $"{m_Notification?["Message"]}\n\n" +
		                                          $"Case ID: {m_Notification?["CaseId"]}\n" +
		                                          $"Player ID: {m_Notification?["PlayerId"]}\n" +
		                                          $"Project ID: {m_Notification?["ProjectId"]}";

		protected override void OnRegisterEvents()
		{
			FindFirst<Button>(CloseButtonName).clicked += OnClose;
			FindFirst<Button>(CopyButtonName).clicked += OnCopyToClipboard;
		}

		protected override void OnUnregisterEvents()
		{
			if (m_Root == null)
				return;

			FindFirst<Button>(CloseButtonName).clicked -= OnClose;
			FindFirst<Button>(CopyButtonName).clicked -= OnCopyToClipboard;
		}

		private void OnCopyToClipboard()
		{
			GUIUtility.systemCopyBuffer = GetNotificationAsText();

			var button = FindFirst<Button>(CopyButtonName);
			button.text = "Copied ......";
		}

		private void OnClose()
		{
			m_Notification = default;
			Hide();
		}

		public async Task<Boolean> Show(Dictionary<String, String> notification)
		{
			if (s_ShowModalPopup != null)
			{
				Debug.LogWarning("popup already showing");
				return false;
			}

			m_Notification = notification;
			gameObject.SetActive(true);

			s_ShowModalPopup = new TaskCompletionSource<Boolean>();
			return await s_ShowModalPopup.Task;
		}

		private void Hide()
		{
			gameObject.SetActive(false);
			s_ShowModalPopup?.SetResult(false);
			s_ShowModalPopup = null;
		}

#if UNITY_EDITOR
		[Serializable]
		public struct TestNotification
		{
			public String Id;
			public String CaseId;
			public String Message;
			public String PlayerId;
			public String ProjectId;
			public String Type;
			public String CreatedAt;
		}

		[Header("In-Editor Testing")]
		[SerializeField] private TestNotification m_TestNotification;
		[SerializeField] private Boolean m_ShowPopup;

		private void OnValidate() => TryShowPopup();

		private void TryShowPopup()
		{
			if (m_ShowPopup)
			{
				if (EditorApplication.isPlaying == false)
				{
					Debug.LogWarning("Enter playmode to test notifications ...");
					m_ShowPopup = false;
					return;
				}

				if (gameObject.activeSelf)
				{
					Debug.LogWarning("Popup already showing...");
					m_ShowPopup = false;
				}

				EditorApplication.delayCall += async () => m_ShowPopup = await Show(NotificationFromTestNotification());
			}
		}

		private Dictionary<String, String> NotificationFromTestNotification() => new()
		{
			{ "Type", m_TestNotification.Type },
			{ "CaseId", m_TestNotification.CaseId },
			{ "PlayerId", m_TestNotification.PlayerId },
			{ "ProjectId", m_TestNotification.ProjectId },
			{ "Message", m_TestNotification.Message },
		};
#endif
	}
}
