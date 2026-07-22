using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Services;
using ReactiveUI;
using Splat;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace KyoshinEewViewer.Series.KyoshinMonitor.SettingPages;

public class KyoshinMonitorReplaySettingPage : ReactiveObject, ISettingPage
{
	public bool IsVisible => true;

	public string? Icon => null;

	public string Title => "リプレイ";
	public Localization.LocalizationKey? TitleKey => Localization.LocalizationKey.SettingReplay;

	public Control DisplayControl => new KyoshinMonitorReplayPage() { DataContext = this };

	public ISettingPage[] SubPages => [];

	public bool IsDebug { get; }
#if DEBUG
		= true;
#endif

	public KyoshinMonitorSeries Series { get; }
	public KyoshinEewViewerConfiguration Config { get; }
	private TimerService TimerService { get; }
	public ISubWindowsService? SubWindowService { get; }


	private int _timeshiftSeconds = 0;
	public int TimeshiftSeconds
	{
		get => _timeshiftSeconds;
		set {
			if (value > 10800)
				value = 10800;
			if (value < 0)
				value = 0;
			this.RaiseAndSetIfChanged(ref _timeshiftSeconds, value);
			UpdateTimeshiftString();
			TimeshiftedDateTime = TimerService.CurrentDisplayTime.AddSeconds(-TimeshiftSeconds);
		}
	}
	private static LocalizationService? Loc => Locator.Current.GetService<LocalizationService>();

	private string _timeshiftSecondsString = Loc?.Get(LocalizationKey.ReplayRealtime) ?? "リアルタイム";

	public KyoshinMonitorReplaySettingPage(
		KyoshinEewViewerConfiguration config,
		KyoshinMonitorSeries series,
		TimerService timerService,
		ISubWindowsService? subWindowService)
	{
		Series = series;
		Config = config;
		TimerService = timerService;
		SubWindowService = subWindowService;

		OffsetTimeshiftSeconds = ReactiveCommand.Create<string>(amountString =>
		{
			TimeshiftSeconds += int.Parse(amountString);
		});

		TimerService.DelayedTimerElapsed += t =>
		{
			TimeshiftedDateTime = t.AddSeconds(-TimeshiftSeconds);
		};
	}

	public string TimeshiftSecondsString
	{
		get => _timeshiftSecondsString;
		set => this.RaiseAndSetIfChanged(ref _timeshiftSecondsString, value);
	}
	private void UpdateTimeshiftString()
	{
		if (TimeshiftSeconds == 0)
		{
			TimeshiftSecondsString = Loc?.Get(LocalizationKey.ReplayRealtime) ?? "リアルタイム";
			return;
		}

		TimeshiftSecondsString = LocalizedTime.FormatAgo(TimeSpan.FromSeconds(TimeshiftSeconds));
	}

	private DateTime _timeshiftedDateTime;
	public DateTime TimeshiftedDateTime
	{
		get => _timeshiftedDateTime;
		set => this.RaiseAndSetIfChanged(ref _timeshiftedDateTime, value);
	}

	public ReactiveCommand<string, Unit> OffsetTimeshiftSeconds { get; }

	public async Task OpenReplayFile()
	{
		try
		{
			if (KyoshinEewViewerApp.TopLevelControl == null)
				return;
			var files = await KyoshinEewViewerApp.TopLevelControl.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
			{
				Title = Loc?.Get(LocalizationKey.ReplayOpenFileTitle) ?? "リプレイファイルを開く",
				FileTypeFilter = [FilePickerFileTypes.All],
				AllowMultiple = false,
			});
			if (files is not { Count: > 0 } || files[0].TryGetLocalPath() is not { } localPath)
				return;

			await Series.ReplayFileInformationHost.LoadAsync(localPath);
		}
		catch (Exception ex)
		{
			await ShowErrorDialog("リプレイファイルの読み込みに失敗しました", ex.Message);
		}
	}

	private async Task ShowErrorDialog(string title, string message)
	{
		var dialog = new FAContentDialog
		{

			Title = title,
			Content = message,
			CloseButtonText = "OK"
		};

		await dialog.ShowAsync(SubWindowService?.SettingWindow);
	}

	private async Task ShowInfoDialog(string title, string message)
	{
		var dialog = new FAContentDialog
		{
			Title = title,
			Content = message,
			CloseButtonText = "OK"
		};

		await dialog.ShowAsync();
	}
}
