// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.GamingServices.Authentication
{
	public static partial class Account
	{
		public static event Func<Dictionary<String, String>, Task> OnShowNotification;

		private const String NotificationDateKey = "CodeSmile.GamingServices.Account.NotificationDate";
		private static Int64 LastNotificationDate
		{
			get => Int64.Parse(PlayerPrefs.GetString(NotificationDateKey, "0"), NumberFormatInfo.InvariantInfo);
			set => PlayerPrefs.SetString(NotificationDateKey, value.ToString(NumberFormatInfo.InvariantInfo));
		}

		public static async Task TryShowNewNotifications()
		{
			var authService = AuthenticationService.Instance;
			if (Int64.TryParse(authService.LastNotificationDate, out var lastDate) && lastDate > LastNotificationDate)
			{
				var notifications = await authService.GetNotificationsAsync();
				await ShowAllNotifications(notifications);
			}
		}

		public static async Task ShowAllNotifications(List<Notification> notifications)
		{
			if (notifications == null || notifications.Count == 0)
				return;

			foreach (var notification in notifications)
				await ShowNotification(notification);
		}

		private static async Task ShowNotification(Notification notification)
		{
			var contents = new Dictionary<String, String>
			{
				{ "Type", notification.Type },
				{ "ProjectId", notification.ProjectId },
				{ "PlayerId", notification.PlayerId },
				{ "CaseId", notification.CaseId },
				{ "Message", notification.Message },
			};

			await OnShowNotification?.Invoke(contents);

			if (Int64.TryParse(notification.CreatedAt, out var createdDate) && createdDate > LastNotificationDate)
				LastNotificationDate = createdDate;
		}
	}
}
