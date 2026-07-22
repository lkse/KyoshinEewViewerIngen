using KyoshinEewViewer.Core;
using KyoshinEewViewer.JmaXmlParser;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Services.TelegramPublishers;
using KyoshinMonitorLib;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KyoshinEewViewer.Series.Earthquake.Models;

public class EarthquakeEvent : ReactiveObject
{
	public EarthquakeEvent(string eventId)
	{
		EventId = eventId;

		_isHypocenterAvailable = this.WhenAny(
			x => x.IsHypocenterOnly,
			x => x.IsDetailIntensityApplied,
			(only, applied) => only.Value || applied.Value
		).ToProperty(this, x => x.IsHypocenterAvailable);

		if (LocalizationService.Instance is { } loc)
			loc.LanguageChanged += (_, _) => RefreshAllLocalized();

		_isVeryShallow = this.WhenAny(
			x => x.Depth,
			depth => Depth <= 0
		).ToProperty(this, x => x.IsVeryShallow);

		_isNoDepthData = this.WhenAny(
			x => x.Depth,
			depth => depth.Value <= -1
		).ToProperty(this, x => x.IsNoDepthData);

		_isUnknownIntensity = this.WhenAny(
			x => x.Intensity,
			intensity => intensity.Value == JmaIntensity.Unknown
		).ToProperty(this, x => x.IsUnknownIntensity);

		RefreshTitle();
	}

	private bool _isSelecting;
	/// <summary>
	/// 該当項目が選択中か
	/// </summary>
	public bool IsSelecting
	{
		get => _isSelecting;
		set => this.RaiseAndSetIfChanged(ref _isSelecting, value);
	}

	private List<string> ProcessedTelegramIds { get; } = [];
	public ObservableCollection<EarthquakeInformationFragment> Fragments { get; } = [];

	// メモ イベントIDの振り分けは上位でやる
	public EarthquakeInformationFragment? ProcessTelegram(Telegram telegram, JmaXmlDocument document)
	{
		if (ProcessedTelegramIds.Contains(telegram.Key))
			return null;
		ProcessedTelegramIds.Add(telegram.Key);

		// 取り消し処理
		if (document.Head.InfoType == "取消")
		{
			foreach (var f in Fragments)
			{
				// 同種の電文をすべて取り消し扱いに
				if (f.Title == document.Control.Title)
					f.IsCancelled = true;
			}
			SyncProperties();
			return null;
		}
		// 訂正の場合、一番最後の情報を訂正済みにして、そのほかは普通に処理する
		if (document.Head.InfoType == "訂正" && Fragments.LastOrDefault(x => x.Title == document.Control.Title) is { } lastFragment)
			lastFragment.IsCorrected = true;

		// 電文をパース
		var fragment = EarthquakeInformationFragment.CreateFromJmxXmlDocument(telegram, document);
		Fragments.Add(fragment);

		SyncProperties();

		return fragment;
	}

	public void AddFragment(EarthquakeInformationFragment fragment)
	{
		Fragments.Add(fragment);
		SyncProperties();
	}

	/// <summary>
	/// 震源・震度情報の同期
	/// </summary>
	private void SyncProperties()
	{
		// 取り消し状態を同期
		IsCancelled = Fragments.All(x => x.IsCancelled);

		// 訓練･試験チェック 1回でも読んだ記録があれば訓練扱いとする
		IsTraining = Fragments.Where(x => !x.IsCancelled && !x.IsCorrected).Any(x => x.IsTraining);
		IsTest = Fragments.Where(x => !x.IsCancelled && !x.IsCorrected).Any(x => x.IsTest);

		foreach (var fragment in Fragments)
		{
			// 有効でないものはスルー
			if (fragment.IsCancelled || fragment.IsCorrected)
				continue;

			UpdatedTime = fragment.ArrivedTime;

			// 震度速報
			if (fragment is IntensityInformationFragment i)
			{
				Intensity = i.MaxIntensity;
				// 震源情報･震源震度情報がない場合のみ震源情報を更新
				if (!IsDetailIntensityApplied)
				{
					IsSokuhou = true;
					if (!IsHypocenterOnly)
					{
						Time = i.DetectionTime;
						IsDetectionTime = true;
						Place = i.Place;
						// 震度速報の代表地域名は震央地名コードを持たないため英語化対象外
						PlaceCode = null;
						IsOnlypoint = i.IsOnlypoint;
						Depth = -1;
					}
				}
				Comment = i.Comment;
				FreeFormComment = i.FreeFormComment;
			}

			// 震源情報の更新
			if (fragment is HypocenterInformationFragment h)
			{
				Time = h.OccurrenceTime;
				IsDetectionTime = false;
				Place = h.Place;
				PlaceCode = h.PlaceCode;
				Location = h.Location;

				LocationError = h.LocationError;
				IsOnlypoint = true;
				Magnitude = h.Magnitude;
				MagnitudeAlternativeText = h.MagnitudeAlternativeText;
				Depth = h.Depth;
				DepthError = h.DepthError;
				// 震源震度情報を受信していた場合は震源のみのフラグを立てない
				IsHypocenterOnly = !IsDetailIntensityApplied;

				// コメント部分
				Comment = h.Comment ?? Comment;
				FreeFormComment = h.FreeFormComment;
			}

			// 震源震度情報
			if (fragment is HypocenterAndIntensityInformationFragment hi)
			{
				IsSokuhou = false;
				IsHypocenterOnly = false;

				IsForeign = hi.IsForeign;
				IsVolcano = hi.IsVolcano;
				VolcanoName = hi.VolcanoName;
				Intensity = hi.MaxIntensity;

				IsDetailIntensityApplied = true;
			}

			// 長周期地震動に関する観測情報
			if (fragment is LpgmIntensityInformationFragment lpgm)
			{
				LpgmIntensity = lpgm.MaxLpgmIntensity;
			}
		}
		RefreshTitle();
	}

	/// <summary>
	/// 地震の EventId
	/// </summary>
	public string EventId { get; }

	private string? _title;
	/// <summary>
	/// イベントのタイトル(現在の情報種別)
	/// </summary>
	public string? Title
	{
		get => _title;
		set => this.RaiseAndSetIfChanged(ref _title, value);
	}

	/// <summary>
	/// 言語に応じた日付文字列
	/// </summary>
	public string FormattedDate
	{
		get
		{
			if (LocalizationService.Instance?.SelectedLanguage.Code == "en")
				return "Approx " + _time.ToString("yyyy/MM/dd");
			return _time.ToString("yyyy年MM月dd日");
		}
	}

	/// <summary>
	/// 言語に応じた時刻文字列
	/// </summary>
	public string FormattedTime
	{
		get
		{
			if (LocalizationService.Instance?.SelectedLanguage.Code == "en")
				return _time.ToString("HH:mm");
			return _time.ToString("HH時mm分");
		}
	}

	/// <summary>
	/// 英語時は接尾辞の「約」を非表示にする
	/// </summary>
	public bool ShowApproxSuffix => LocalizationService.Instance?.SelectedLanguage.Code != "en";

	/// <summary>
	/// 言語に応じた更新時刻文字列
	/// </summary>
	public string FormattedUpdatedTime
	{
		get
		{
			if (LocalizationService.Instance?.SelectedLanguage.Code == "en")
				return $"Updated {GetOrdinalSuffix(_updatedTime.Day)} {_updatedTime:HH:mm}";
			return _updatedTime.ToString("d日HH:mm更新");
		}
	}

	/// <summary>
	/// 言語に応じたコメント(固定付加文)
	/// </summary>
	public string? LocalizedComment
	{
		get
		{
			if (_comment is null)
				return null;
			if (LocalizationService.Instance?.SelectedLanguage.Code == "en")
			{
				// 単一の付加文として完全一致すればそのまま採用する
				if (CommentTranslations.TryGetValue(_comment, out var en))
					return en;
				// 固定付加文は複数行が連結されて届くことがあるため、行単位でも英訳を試みる
				var lines = _comment.Split('\n');
				if (lines.Length > 1)
				{
					var translated = false;
					for (var i = 0; i < lines.Length; i++)
						if (CommentTranslations.TryGetValue(lines[i].Trim(), out var enLine))
						{
							lines[i] = enLine;
							translated = true;
						}
					if (translated)
						return string.Join('\n', lines);
				}
			}
			return _comment;
		}
	}

	private string? _subtitle;
	/// <summary>
	/// 補足情報(存在する場合は外部から設定する)
	/// </summary>
	public string? Subtitle
	{
		get => _subtitle;
		set => this.RaiseAndSetIfChanged(ref _subtitle, value);
	}

	private DateTime _updatedTime;
	/// <summary>
	/// 最新の電文の発表時刻
	/// </summary>
	public DateTime UpdatedTime
	{
		get => _updatedTime;
		set
		{
			this.RaiseAndSetIfChanged(ref _updatedTime, value);
			this.RaisePropertyChanged(nameof(FormattedUpdatedTime));
		}
	}

	private bool _isSokuhou;
	/// <summary>
	/// 震度速報
	/// </summary>
	public bool IsSokuhou
	{
		get => _isSokuhou;
		set => this.RaiseAndSetIfChanged(ref _isSokuhou, value);
	}

	private bool _isForeign;
	/// <summary>
	/// 遠地地震
	/// </summary>
	public bool IsForeign
	{
		get => _isForeign;
		set => this.RaiseAndSetIfChanged(ref _isForeign, value);
	}

	private bool _isVolcano;
	/// <summary>
	/// 火山噴火
	/// </summary>
	public bool IsVolcano
	{
		get => _isVolcano;
		set => this.RaiseAndSetIfChanged(ref _isVolcano, value);
	}

	private string? _volcanoName;
	/// <summary>
	/// 火山名
	/// </summary>
	public string? VolcanoName
	{
		get => _volcanoName;
		set => this.RaiseAndSetIfChanged(ref _volcanoName, value);
	}

	private bool _isOnlypoint;
	/// <summary>
	/// 震度速報かつ最大震度の観測が1地域のみ
	/// </summary>
	public bool IsOnlypoint
	{
		get => _isOnlypoint;
		set => this.RaiseAndSetIfChanged(ref _isOnlypoint, value);
	}

	private bool _isTraining;
	/// <summary>
	/// 訓練
	/// </summary>
	public bool IsTraining
	{
		get => _isTraining;
		set => this.RaiseAndSetIfChanged(ref _isTraining, value);
	}

	private bool _isTest;
	/// <summary>
	/// 試験
	/// </summary>
	public bool IsTest
	{
		get => _isTest;
		set => this.RaiseAndSetIfChanged(ref _isTest, value);
	}

	private bool _isHypocenterOnly;
	/// <summary>
	/// 震源のみ
	/// </summary>
	public bool IsHypocenterOnly
	{
		get => _isHypocenterOnly;
		set => this.RaiseAndSetIfChanged(ref _isHypocenterOnly, value);
	}

	private bool _isDetailIntensityApplied;
	/// <summary>
	/// 震源震度情報を適用済み<br/>これ以降は震度速報は震度情報のみ更新する
	/// </summary>
	public bool IsDetailIntensityApplied
	{
		get => _isDetailIntensityApplied;
		set => this.RaiseAndSetIfChanged(ref _isDetailIntensityApplied, value);
	}

	private bool _isCancelled;
	/// <summary>
	/// 属しているすべての電文(=該当イベントID)がキャンセル扱いになっている
	/// </summary>
	public bool IsCancelled
	{
		get => _isCancelled;
		set => this.RaiseAndSetIfChanged(ref _isCancelled, value);
	}

	private DateTime _time;
	/// <summary>
	/// 発生もしくは検知時刻
	/// </summary>
	public DateTime Time
	{
		get => _time;
		set
		{
			this.RaiseAndSetIfChanged(ref _time, value);
			this.RaisePropertyChanged(nameof(FormattedDate));
			this.RaisePropertyChanged(nameof(FormattedTime));
		}
	}

	private bool _isDetectTime;
	/// <summary>
	/// 時刻は検知時刻を示しているか
	/// </summary>
	public bool IsDetectionTime
	{
		get => _isDetectTime;
		set => this.RaiseAndSetIfChanged(ref _isDetectTime, value);
	}

	private string? _place;
	/// <summary>
	/// 震央地名もしくは観測地名(震度速報)
	/// </summary>
	public string? Place
	{
		get => _place;
		set
		{
			this.RaiseAndSetIfChanged(ref _place, value);
			this.RaisePropertyChanged(nameof(LocalizedPlace));
		}
	}

	private int? _placeCode;
	/// <summary>
	/// 震央地名コード(震央地名辞書のキー)。英語表示の解決に用いる。
	/// </summary>
	public int? PlaceCode
	{
		get => _placeCode;
		set
		{
			this.RaiseAndSetIfChanged(ref _placeCode, value);
			this.RaisePropertyChanged(nameof(LocalizedPlace));
		}
	}

	/// <summary>
	/// 言語に応じた震央地名。英語時は震央地名辞書(コード)から公式英訳を引く。
	/// </summary>
	public string? LocalizedPlace
	{
		get
		{
			if (LocalizationService.Instance?.SelectedLanguage.Code == "en"
				&& _placeCode is int code
				&& CsvDictionary.AreaEpicenter.TryGetValue(code, out var t)
				&& !string.IsNullOrEmpty(t.English))
				return t.English;
			return _place;
		}
	}

	private Location? _location;
	/// <summary>
	/// 震央座標
	/// </summary>
	public Location? Location
	{
		get => _location;
		set => this.RaiseAndSetIfChanged(ref _location, value);
	}

	private Location? _locationError;
	/// <summary>
	/// 震央座標の誤差 (±度)
	/// </summary>
	public Location? LocationError
	{
		get => _locationError;
		set => this.RaiseAndSetIfChanged(ref _locationError, value);
	}

	private JmaIntensity _intensity = JmaIntensity.Unknown;
	/// <summary>
	/// 最大震度
	/// </summary>
	public JmaIntensity Intensity
	{
		get => _intensity;
		set => this.RaiseAndSetIfChanged(ref _intensity, value);
	}

	private LpgmIntensity? _lpgmIntensity;
	/// <summary>
	/// 最大の長周期地震動階級
	/// </summary>
	public LpgmIntensity? LpgmIntensity
	{
		get => _lpgmIntensity;
		set => this.RaiseAndSetIfChanged(ref _lpgmIntensity, value);
	}

	private float _magnitude;
	/// <summary>
	/// 規模
	/// </summary>
	public float Magnitude
	{
		get => _magnitude;
		set => this.RaiseAndSetIfChanged(ref _magnitude, value);
	}

	private string? _magnitudeAlternativeText;
	/// <summary>
	/// 規模の代替テキスト
	/// </summary>
	public string? MagnitudeAlternativeText
	{
		get => _magnitudeAlternativeText;
		set => this.RaiseAndSetIfChanged(ref _magnitudeAlternativeText, value);
	}

	private int _depth = -1;
	/// <summary>
	/// 深さ(km)
	/// </summary>
	public int Depth
	{
		get => _depth;
		set => this.RaiseAndSetIfChanged(ref _depth, value);
	}

	private int? _depthError;
	/// <summary>
	/// 深さの誤差 (±km)
	/// </summary>
	public int? DepthError
	{
		get => _depthError;
		set => this.RaiseAndSetIfChanged(ref _depthError, value);
	}

	private string? _comment;
	/// <summary>
	/// コメント
	/// </summary>
	public string? Comment
	{
		get => _comment;
		set
		{
			this.RaiseAndSetIfChanged(ref _comment, value);
			this.RaisePropertyChanged(nameof(LocalizedComment));
		}
	}

	private string? _freeFormComment;
	/// <summary>
	/// 自由形式のコメント
	/// </summary>
	public string? FreeFormComment
	{
		get => _freeFormComment;
		set => this.RaiseAndSetIfChanged(ref _freeFormComment, value);
	}

	private readonly ObservableAsPropertyHelper<bool> _isHypocenterAvailable;
	public bool IsHypocenterAvailable => _isHypocenterAvailable.Value;

	private readonly ObservableAsPropertyHelper<bool> _isVeryShallow;
	public bool IsVeryShallow => _isVeryShallow.Value;

	private readonly ObservableAsPropertyHelper<bool> _isNoDepthData;
	public bool IsNoDepthData => _isNoDepthData.Value;

	private readonly ObservableAsPropertyHelper<bool> _isUnknownIntensity;
	public bool IsUnknownIntensity => _isUnknownIntensity.Value;

	private void RefreshTitle()
	{
		if (IsSokuhou && IsHypocenterOnly)
			Title = LocalizationService.Instance?.Get(LocalizationKey.EarthquakeTitleSokuhouAndHypocenter);
		else if (IsSokuhou)
			Title = LocalizationService.Instance?.Get(LocalizationKey.EarthquakeTitleSokuhou);
		else if (IsHypocenterOnly)
			Title = LocalizationService.Instance?.Get(LocalizationKey.EarthquakeTitleHypocenter);
		else if (IsVolcano)
			Title = LocalizationService.Instance?.Get(LocalizationKey.EarthquakeTitleVolcano);
		else if (IsForeign)
			Title = LocalizationService.Instance?.Get(LocalizationKey.EarthquakeTitleForeign);
		else
			Title = LocalizationService.Instance?.Get(LocalizationKey.EarthquakeHypocenterAndIntensity);
	}

	private void RefreshAllLocalized()
	{
		RefreshTitle();
		this.RaisePropertyChanged(nameof(FormattedDate));
		this.RaisePropertyChanged(nameof(FormattedTime));
		this.RaisePropertyChanged(nameof(FormattedUpdatedTime));
		this.RaisePropertyChanged(nameof(LocalizedComment));
		this.RaisePropertyChanged(nameof(LocalizedPlace));
		this.RaisePropertyChanged(nameof(ShowApproxSuffix));
	}

	private static string GetOrdinalSuffix(int day)
	{
		if (day >= 11 && day <= 13)
			return $"{day}th";
		return (day % 10) switch
		{
			1 => $"{day}st",
			2 => $"{day}nd",
			3 => $"{day}rd",
			_ => $"{day}th",
		};
	}

	private static readonly Dictionary<string, string> CommentTranslations = new()
	{
		// 津波の有無・影響
		{"この地震による津波の心配はありません。", "This earthquake poses no tsunami risk."},
		{"この地震による日本への津波の影響はありません。", "This earthquake poses no tsunami risk to Japan."},
		{"この地震により、日本の沿岸では若干の海面変動があるかもしれませんが、被害の心配はありません。", "Although there may be slight sea-level changes in coastal regions/ this earthquake has caused no damage to Japan."},
		{"日本への津波の有無については現在調査中です。", "The possibility of tsunami generation toward Japan in currently under evaluation."},

		// 津波警報・注意報の発表状況
		{"津波警報等（大津波警報・津波警報あるいは津波注意報）を発表中です。", "Tsunami warnings or advisories are currently in effect."},
		{"大津波警報が発表されています。", "A major tsunami warning is in effect."},
		{"津波警報が発表されています。", "A tsunami warning is in effect."},
		{"津波注意報が発表されています。", "A tsunami advisory is in effect."},

		// 海面変動
		{"今後もしばらく海面変動が続くと思われます。", "Sea-level changes may be observed."},
		{"今後もしばらく海面変動が続くと思われますので、海水浴や磯釣り等を行う際は注意してください。", "Pay attention when fishing, swimming or engaging in other marine activities, as there may still be slight sea-level changes."},
		{"今後もしばらく海面変動が続くと思われますので、磯釣り等を行う際は注意してください。", "Pay attention when fishing or engaging in other marine activities, as there may still be slight sea-level changes."},

		// 沖合観測に伴う切り替え
		{"沖合で高い津波を観測したため大津波警報・津波警報に切り替えました。", "Upgrade to Major Tsunami Warnings/Tsunami Warnings implemented in response to high tsunami waves observed offshore"},
		{"沖合で高い津波を観測したため大津波警報・津波警報を切り替えました。", "Major Tsunami Warnings/Tsunami Warnings updated, in response to high tsunami waves offshore"},
		{"沖合で高い津波を観測したため大津波警報に切り替えました。", "Upgrade to Major Tsunami Warnings implemented in response to high tsunami waves observed offshore"},
		{"沖合で高い津波を観測したため大津波警報を切り替えました。", "Major Tsunami Warnings updated, in response to high tsunami waves offshore"},
		{"沖合で高い津波を観測したため津波警報に切り替えました。", "Upgrade to Tsunami Warnings implemented high tsunami waves observed offshore"},
		{"沖合で高い津波を観測したため津波警報を切り替えました。", "Tsunami Warnings updated, in response to high tsunami waves offshore"},
		{"沖合で高い津波を観測したため予想される津波の高さを切り替えました。", "Estimated tsunami heights updated in response to high tsunami waves offshore"},

		// 遠地・広域津波の可能性
		{"太平洋の広域に津波発生の可能性があります。", "There is a possiblity of a destructive ocean-wide tsunami in the Pacific Ocean."},
		{"太平洋で津波発生の可能性があります。", "There is a possiblity of a destructive regional tsunami in the Pacific Ocean."},
		{"北西太平洋で津波発生の可能性があります。", "There is a possiblity of a destructive regional tsunami  in the Northwest Pacific Ocean."},
		{"インド洋の広域に津波発生の可能性があります。", "There is a possiblity of a destructive ocean-wide tsunami in the Indian Ocean."},
		{"インド洋で津波発生の可能性があります。", "There is a possiblity of a destructive regional tsunami in the Indian Ocean."},
		{"震源の近傍で津波発生の可能性があります。", "There is a possibility of a destructive local tsunami near the epicenter."},
		{"震源の近傍で小さな津波発生の可能性がありますが、被害をもたらす津波の心配はありません。", "Minor local tsunami may occur near the epicenter, but no tsunami damage is expected."},
		{"一般的に、この規模の地震が海域の浅い領域で発生すると、津波が発生することがあります。", "A shallow earthquake with the same magnitude in a sea area may generate a tsunami."},

		// 緊急地震速報
		{"この地震について、緊急地震速報を発表しています。", "Earthquake Early Warning is in effect for this earthquake."},
		{"この地震について、緊急地震速報を発表しています。この地震の最大震度は２でした。", "Earthquake Early Warning is in effect for this earthquake. Its maximum seismic intensity was 2."},
		{"この地震について、緊急地震速報を発表しています。この地震の最大震度は１でした。", "Earthquake Early Warning is in effect for this earthquake. Its maximum seismic intensity was 1."},
		{"この地震について、緊急地震速報を発表しています。この地震で震度１以上は観測されていません。", "Earthquake Early Warning is in effect for this earthquake. There was no observation of seismic intensity 1 or above."},
		{"この地震で緊急地震速報を発表しましたが、強い揺れは観測されませんでした。", "Earthquake Early Warning was issued for this earthquake, however no strong tremors were observed."},
		{"地震です　落ち着いて　身を守ってください", "An earthquake has just occurred. Stay calm and secure your personal safety."},

		// 震度・震源
		{"震度３以上が観測された地域はありません。", "There are no areas that recorded a seismic intensity of 3 or stronger."},
		{"震度３以上が観測された市町村はありません。", "There are no municipalities that record a seismic intensity of 3 or stronger."},
		{"震源要素を訂正します。", "Information related to the hypocenter has been corrected."},
		{"＊印は気象庁以外の震度観測点についての情報です。", "* mark: Local Governments' or NIED's station"},

		// その他
		{"強い揺れに警戒してください。", "Watch out for strong tremors."},
		{"今後の情報に注意してください。", "Check the information which will be issued from now on."},
		{"ただちに避難してください。", "Evacuate immediately"},
		{"南海トラフ地震臨時情報を発表しています。", "Nankai Trough Earthquake Extra Information is in effect."},
	};

	[Obsolete("GetNotificationMessage()は非推奨です。代わりにScribanテンプレートを使用してください。")]
	public string GetNotificationMessage()
	{
		var parts = new List<string>();
		if (IsCancelled)
			parts.Add("[取消]");
		if (IsTraining)
			parts.Add("[訓練]");
		if (IsTest)
			parts.Add("[試験]");
		if (Intensity != JmaIntensity.Unknown)
			parts.Add($"最大{Intensity.ToLongString()}");

		if (IsHypocenterOnly || IsDetailIntensityApplied)
		{
			parts.Insert(0, $"{Time:HH:mm}");
			parts.Add(Place ?? "不明");
			if (!IsNoDepthData)
			{
				if (IsVeryShallow)
					parts.Add("ごく浅い");
				else
					parts.Add(Depth + "km");
			}
			parts.Add(MagnitudeAlternativeText ?? $"M{Magnitude:0.0}");
		}
		return string.Join('/', parts);
	}
}
