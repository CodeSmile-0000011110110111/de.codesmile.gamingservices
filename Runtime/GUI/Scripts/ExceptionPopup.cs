// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.MultiPal.GUI.Base;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
#endif

namespace CodeSmile.GamingServices
{
	[RequireComponent(typeof(UIDocument))]
	public class ExceptionPopup : GuiBehaviour
	{
		private const String PopupGroupName = "PopupGroup";
		private const String NotificationName = "Notification";
		private const String CopyButtonName = "CopyButton";
		private const String CloseButtonName = "CloseButton";

		private static TaskCompletionSource<Boolean> s_ShowModalPopup;

		private Exception m_Exception;
		private String m_Reason;
		private Int32 m_ErrorCode;

		private void Awake() => Services.OnServiceRequestFailed += OnServiceRequestFailed;

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Hide();
			Services.OnServiceRequestFailed -= OnServiceRequestFailed;
		}

		private async Task OnServiceRequestFailed(Exception ex, String reason, Int32 errorCode) =>
			await Show(ex, reason, errorCode);

		public async Task<Boolean> Show(Exception exception, String reason, Int32 errorCode = -1)
		{
			if (s_ShowModalPopup != null)
			{
				Debug.LogWarning("popup already showing");
				return false;
			}

			m_Exception = exception;
			m_Reason = reason;
			m_ErrorCode = errorCode;

			gameObject.SetActive(true);

			s_ShowModalPopup = new TaskCompletionSource<Boolean>();
			return await s_ShowModalPopup.Task;
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

		private String GetExceptionTitle() => $"{m_Reason} ({(m_ErrorCode >= 0 ? m_ErrorCode : m_Exception.GetType().Name)})";

		private String GetExceptionMessage() => m_Exception?.Message;

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
			m_Reason = default;
			Hide();
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

				var msg = $"TEST {nameof(ExceptionPopup)}";
				var ex = new Exception(msg);
				EditorApplication.delayCall += async () => m_ShowPopup = await Show(ex, msg, 98765);
			}
		}
#endif
	}
}
