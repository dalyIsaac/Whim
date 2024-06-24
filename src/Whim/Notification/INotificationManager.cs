using Microsoft.Windows.AppNotifications;

namespace Whim;

/// <summary>
/// The notification manager allows the sending of toast notifications to the user.
/// </summary>
public interface INotificationManager : IDisposable
{
	/// <summary>
	/// The key used to store the notification id in the notification arguments.
	/// </summary>
	public const string NotificationIdKey = "WHIM_NOTIFICATION_ID";

	/// <summary>
	/// Initialize the notification manager.
	/// </summary>
	void Initialize();

	/// <summary>
	/// Register a notification handler for a given notification id.
	/// </summary>
	/// <param name="notificationId">
	/// The notification id to register the handler for. <br/>
	/// The notificationid should be prefixed by the plugin name to prevent collisions.
	/// </param>
	/// <param name="notificationReceived"></param>
	void Register(string notificationId, Action<AppNotificationActivatedEventArgs> notificationReceived);

	/// <summary>
	/// Unregister a notification handler for a given notification id.
	/// </summary>
	/// <param name="notificationId"></param>
	void Unregister(string notificationId);

	/// <summary>
	/// Send a toast notification to the user.
	/// </summary>
	/// <param name="appNotification">
	/// The notification to send. <br/>
	/// Make sure to call <c>SetArgument(INotificationManager.NOTIFICATION_ID_KEY, notificationId)</c>
	/// to the notification and each component which can be interacted with. <br/>
	/// Otherwise, the notification will not be handled.
	/// </param>
	/// <returns></returns>
	bool SendToastNotification(AppNotification appNotification);

	/// <summary>
	/// Handles a toast notification activation on app launch.
	/// </summary>
	/// <param name="args"></param>
	void ProcessLaunchActivationArgs(AppNotificationActivatedEventArgs args);
}
