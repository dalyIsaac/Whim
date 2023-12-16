using System;
using System.Collections.Generic;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace Whim;

internal class NotificationManager : INotificationManager
{
	public const string NOTIFICATION_ID_KEY = "notificationId";

	private bool _disposedValue;
	private bool _initialized;

	private readonly IContext _context;
	private readonly Dictionary<int, Action<AppNotificationActivatedEventArgs>> _notificationHandlers = new();

	public NotificationManager(IContext context)
	{
		_context = context;
	}

	private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
	{
		try
		{
			int notificationId = int.Parse(args.UserInput[NOTIFICATION_ID_KEY]);
			_notificationHandlers[notificationId]?.Invoke(args);
		}
		catch (Exception e)
		{
			_context.HandleUncaughtException(nameof(OnNotificationInvoked), e);
		}
	}

	public void Initialize()
	{
		// To ensure all Notification handling happens in this process instance, register for
		// NotificationInvoked before calling Register(). Without this a new process will
		// be launched to handle the notification.
		AppNotificationManager notificationManager = AppNotificationManager.Default;
		notificationManager.NotificationInvoked += OnNotificationInvoked;
		notificationManager.Register();
		_initialized = true;

		RegisterNotification(_context);
	}

	int NOTIFICATION_ID = 0;

	void RegisterNotification(IContext context)
	{
		context
			.NotificationManager
			.Register(
				NOTIFICATION_ID,
				(a) =>
				{
					Logger.Debug("Notification clicked");
				}
			);
	}

	public void Register(int notificationId, Action<AppNotificationActivatedEventArgs> notificationReceived)
	{
		_notificationHandlers.Add(notificationId, notificationReceived);
	}

	public void Unregister(int notificationId)
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
