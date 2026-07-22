using Splat;
using System;
using System.Text;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// 時間表現をローカライズする。
/// </summary>
public static class LocalizedTime
{
	/// <summary>
	/// 経過時間を「10秒前」「1時間5分前」のような相対表現に変換する。
	/// </summary>
	public static string FormatAgo(TimeSpan time)
	{
		var loc = Locator.Current.GetService<LocalizationService>();
		var sb = new StringBuilder();
		if (time.TotalHours >= 1)
			sb.Append(string.Format(loc?.Get(LocalizationKey.ReplayTimeAgoHour) ?? "{0}時間", (int)time.TotalHours));
		if (time.Minutes > 0)
			sb.Append(string.Format(loc?.Get(LocalizationKey.ReplayTimeAgoMinute) ?? "{0}分", time.Minutes));
		if (time.Seconds > 0)
			sb.Append(string.Format(loc?.Get(LocalizationKey.ReplayTimeAgoSecond) ?? "{0}秒", time.Seconds));
		sb.Append(loc?.Get(LocalizationKey.ReplayTimeAgoSuffix) ?? "前");
		return sb.ToString();
	}
}
