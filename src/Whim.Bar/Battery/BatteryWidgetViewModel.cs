using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Windows.System.Power;

namespace Whim.Bar;

/// <summary>
/// View model containing the active layout.
/// </summary>
internal class BatteryWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private bool _disposedValue;

	public string RemainingChargePercent { get; private set; }

	public string Icon { get; private set; } = BatteryWidgetIcons.BatteryUnknown;

	public event PropertyChangedEventHandler? PropertyChanged;

	public BatteryWidgetViewModel()
	{
		PowerManager.BatteryStatusChanged += Update;
		PowerManager.EnergySaverStatusChanged += Update;
		PowerManager.RemainingChargePercentChanged += Update;

		Update(null, this);
	}

	[MemberNotNull(nameof(RemainingChargePercent), nameof(Icon))]
	private void Update(object? sender, object e)
	{
		// Update the icon.
		RemainingChargePercent = $"{PowerManager.RemainingChargePercent}%";

		Icon = BatteryWidgetIcons.GetBatteryIcon(
			PowerManager.RemainingChargePercent,
			PowerManager.BatteryStatus == BatteryStatus.Charging,
			PowerManager.EnergySaverStatus == EnergySaverStatus.On
		);

		OnPropertyChanged(nameof(RemainingChargePercent));
		OnPropertyChanged(nameof(Icon));
	}

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				PowerManager.BatteryStatusChanged -= Update;
				PowerManager.EnergySaverStatusChanged -= Update;
				PowerManager.RemainingChargePercentChanged -= Update;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
