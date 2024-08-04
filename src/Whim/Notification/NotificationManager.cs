using Microsoft.Windows.AppNotifications;

namespace Whim;

internal class NotificationManager : INotificationManager
{
	private bool _disposedValue;
	private bool _initialized;

	private readonly IContext _context;
	private readonly Dictionary<string, Action<AppNotificationActivatedEventArgs>> _notificationHandlers = [];

	public NotificationManager(IContext context)
	{
		_context = context;
	}

	public void Initialize()
	{
		// To ensure all Notification handling happens in this process instance, register for
		// NotificationInvoked before calling Register(). Without this a new process will
		// be launched to handle the notification.
		AppNotificationManager notificationManager = AppNotificationManager.Default;
		notificationManager.NotificationInvoked += AppNotificationManager_NotificationInvoked;
		notificationManager.Register();
		_initialized = true;
	}

	private void AppNotificationManager_NotificationInvoked(
		AppNotificationManager sender,
		AppNotificationActivatedEventArgs args
	)
	{
		try
		{
			string notificationId = args.Arguments[INotificationManager.NotificationIdKey];
			_notificationHandlers[notificationId]?.Invoke(args);
		}
		catch (Exception e)
		{
			_context.HandleUncaughtException(nameof(AppNotificationManager_NotificationInvoked), e);
		}
	}

	public void ProcessLaunchActivationArgs(AppNotificationActivatedEventArgs args)
	{
		AppNotificationManager_NotificationInvoked(AppNotificationManager.Default, args);
	}

	public void Register(string notificationId, Action<AppNotificationActivatedEventArgs> notificationReceived)
	{
		_notificationHandlers.Add(notificationId, notificationReceived);
	}

	public void Unregister(string notificationId)
	{
		_notificationHandlers.Remove(notificationId);
	}

	public bool SendToastNotification(AppNotification appNotification)
	{
		if (!_initialized)
		{
			throw new InvalidOperationException(
				"Notification manager must be initialized before sending notifications."
			);
		}

		AppNotificationManager.Default.Show(appNotification);

		return appNotification.Id != 0;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (_initialized)
			{
				AppNotificationManager.Default.Unregister();
			}

			_disposedValue = true;
		}
	}

	// Free unmanaged resources
	~NotificationManager()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
