using Avalonia.Controls;
using KyoshinEewViewer.Core;
using KyoshinEewViewer.Localization;
using Splat;
using System;

namespace KyoshinEewViewer.Views;
public partial class SetupWizardWindow : Window
{
	public event Action? Continued;

	private LocalizationService LocalizationService { get; } = Locator.Current.RequireService<LocalizationService>();

	private int Index { get; set; }
	private UserControl[] Pages { get; } = [
		new KyoshinEewViewer.Views.SetupWizardPages.WelcomePage(),
		new KyoshinEewViewer.Views.SetupWizardPages.SelectThemePage(),
		new KyoshinEewViewer.Views.SetupWizardPages.SelectSeriesPage(),
		new KyoshinEewViewer.Views.SetupWizardPages.DmdataPromotion(),
		new KyoshinEewViewer.Views.SetupWizardPages.EpiloguePage(),
	];

	public SetupWizardWindow()
	{
		InitializeComponent();

		SkipButton.Tapped += (s, e) => Continued?.Invoke();
		BeforeButton.Tapped += (s, e) => { Index--; UpdatePage(); };
		NextButton.Tapped += (s, e) => { Index++; UpdatePage(); };
		UpdatePage();
	}

	private void UpdatePage()
	{
		if (Index == 0)
		{
			BeforeButton.IsEnabled = false;
			NextButton.IsEnabled = true;
			SkipButtonText.Text = LocalizationService.Get(LocalizationKey.SetupWizardSkipAndRun);
		}
		else if (Index >= Pages.Length - 1)
		{
			BeforeButton.IsEnabled = true;
			NextButton.IsEnabled = false;
			SkipButtonText.Text = LocalizationService.Get(LocalizationKey.SetupWizardRun);
		}
		else
		{
			BeforeButton.IsEnabled = true;
			NextButton.IsEnabled = true;
			SkipButtonText.Text = LocalizationService.Get(LocalizationKey.SetupWizardSkipAndRun);
		}
		SkipButtonText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
		ContentControl.Content = Pages[Index];
		PageGuide.Text = $"{Index + 1}/{Pages.Length}";
	}
}
