// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.GUI.Base;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.GamingServices
{
	[RequireComponent(typeof(UIDocument))]
	public class DsaNotificationPopup : GuiBehaviour
	{
		private const String NotificationName = "Notification";
		private const String CopyButtonName = "CopyButton";
		private const String CloseButtonName = "CloseButton";
		private const String PopupGroupName = "PopupGroup";

		private static DsaNotificationPopup s_Instance;
		private static TaskCompletionSource<Boolean> s_ShowModalPopup;

		private Notification m_Notification;

		public static async Task<Boolean> ShowModal(Notification notification)
		{
			if (s_Instance == null)
				throw new InvalidOperationException($"No loaded instance of {nameof(DsaNotificationPopup)} available");

			return await s_Instance.Show(notification);
		}

		private void Awake()
		{
			if (s_Instance != null)
				throw new InvalidOperationException($"Only one instance of {nameof(DsaNotificationPopup)} allowed");

			s_Instance = this;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Hide();
		}

		protected override void OnShowGUI()
		{
			base.OnShowGUI();
			UpdateMessage();
			OnRegisterEvents();
		}

		private void UpdateMessage() => FindFirst<TextField>(NotificationName).value = GetNotificationAsText();

		private String GetNotificationAsText() => $"{m_Notification.Message}\n\n" +
		                                          $"Case ID: {m_Notification.CaseId}\n" +
		                                          $"Player ID: {m_Notification.PlayerId}\n" +
		                                          $"Project ID: {m_Notification.ProjectId}\n" +
		                                          $"Server ID: {m_Notification.Id}\n" +
		                                          $"Type: {m_Notification.Type}\n" +
		                                          $"Created at: {m_Notification.CreatedAt}";

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

		private async Task<Boolean> Show(Notification notification)
		{
			if (s_ShowModalPopup != null)
				throw new InvalidOperationException("popup already showing");

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

		private async void OnValidate() => await TryShowPopup();

		private async Task TryShowPopup()
		{
			if (m_ShowPopup)
			{
				if (EditorApplication.isPlaying == false)
				{
					Debug.LogWarning("Enter playmode to test notifications ...");
					m_ShowPopup = false;
					return;
				}

				m_ShowPopup = await ShowModal(NotificationFromTestNotification());
			}
		}

		private Notification NotificationFromTestNotification() => new()
		{
			Id = m_TestNotification.Id,
			CaseId = m_TestNotification.CaseId,
			Message = m_TestNotification.Message,
			PlayerId = m_TestNotification.PlayerId,
			ProjectId = m_TestNotification.ProjectId,
			Type = m_TestNotification.Type,
			CreatedAt = m_TestNotification.CreatedAt,
		};
#endif
	}
}
