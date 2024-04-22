// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.GUI.Base;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using Unity.Services.Authentication;
#endif

namespace CodeSmile.GamingServices
{
	[RequireComponent(typeof(UIDocument))]
	public class ExceptionPopup : GuiBehaviour
	{
		private const String NotificationName = "Notification";
		private const String CopyButtonName = "CopyButton";
		private const String CloseButtonName = "CloseButton";
		private const String PopupGroupName = "PopupGroup";

		private static ExceptionPopup s_Instance;
		private static TaskCompletionSource<Boolean> s_ShowModalPopup;

		private Exception m_Exception;

		public static async Task<Boolean> ShowModal(Exception exception)
		{
			if (s_Instance == null)
				throw new InvalidOperationException($"No loaded instance of {nameof(ExceptionPopup)} available");

			return await s_Instance.Show(exception);
		}

		private void Awake()
		{
			if (s_Instance != null)
				throw new InvalidOperationException($"Only one instance of {nameof(ExceptionPopup)} allowed");

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
			UpdateContent();
			OnRegisterEvents();
		}

		private void UpdateContent()
		{
			FindFirst<GroupBox>(PopupGroupName).text = GetExceptionTitle();
			FindFirst<TextField>(NotificationName).value = GetExceptionMessage();
		}

		private String GetExceptionTitle()
		{
			var exceptionType = m_Exception.GetType().Name;
			if (exceptionType.EndsWith(nameof(Exception)))
				exceptionType = exceptionType.Substring(0, exceptionType.Length - nameof(Exception).Length);

			var title = $"{exceptionType} Error";
			if (m_Exception is RequestFailedException requestFailed)
				title += $" {requestFailed.ErrorCode}";

			return title;
		}

		private String GetExceptionMessage() => m_Exception.Message;

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
			GUIUtility.systemCopyBuffer = $"{GetExceptionTitle()}: {GetExceptionMessage()}";

			var button = FindFirst<Button>(CopyButtonName);
			button.text = "Copied ......";
		}

		private void OnClose()
		{
			m_Exception = default;
			Hide();
		}

		private async Task<Boolean> Show(Exception exception)
		{
			if (s_ShowModalPopup != null)
				throw new InvalidOperationException("popup already showing");

			m_Exception = exception;
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
		[Header("In-Editor Testing")]
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
					return;
				}

				var ex = AuthenticationException.Create(10000, "test exception");
				EditorApplication.delayCall += async () => m_ShowPopup = await ShowModal(ex);
			}
		}
#endif
	}
}
