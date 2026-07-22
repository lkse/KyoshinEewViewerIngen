using Avalonia.Controls;
using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Series;
using Splat;

namespace KyoshinEewViewer.Services.ExtarnalPublishers.Axis;

public class AxisSettingPage : ISettingPage
{
	public bool IsVisible => true;

	public string? Icon => null;

	public string Title => "AXIS(試験中)";
	public Localization.LocalizationKey? TitleKey => Localization.LocalizationKey.SettingAxisExperimental;

	public Control DisplayControl => new AxisPage() { DataContext = this };

	public ISettingPage[] SubPages => [];


	public KyoshinEewViewerConfiguration Config { get; }
	public AxisInformationProvider Client { get; }

	public AxisSettingPage(KyoshinEewViewerConfiguration config, AxisInformationProvider client)
	{
		SplatRegistrations.RegisterLazySingleton<AxisSettingPage>();

		Config = config;
		Client = client;
	}
}
