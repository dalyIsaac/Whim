using System;
using Microsoft.Windows.AppNotifications;

namespace Whim;

/// <summary>
/// The notification manager allows the sending of toast notifications to the user.
/// </summary>
public interface INotificationManager : IDisposable
{
	/// <summary>
	/// Initialize the notification manager.
	/// </summary>
	void Initialize();

	/// <summary>
	/// Register a notification handler for a given notification id.
	/// </summary>
	/// <param name="notificationId"></param>
	/// <param name="notificationReceived"></param>
	void Register(int notificationId, Action<AppNotificationActivatedEventArgs> notificationReceived);

	/// <summary>
	/// Unregister a notification handler for a given notification id.
	/// </summary>
	/// <param name="notificationId"></param>
	void Unregister(int notificationId);

	/// <summary>
	/// Send a toast notification to the user.
	/// </summary>
	/// <param name="appNotification"></param>
	/// <returns></returns>
	bool SendToastNotification(AppNotification appNotification);
}
