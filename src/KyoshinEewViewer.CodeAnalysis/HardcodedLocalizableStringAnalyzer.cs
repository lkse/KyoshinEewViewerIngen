using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace KyoshinEewViewer.CodeAnalysis;

/// <summary>
/// AXAML の表示系属性にローカライズされていない日本語リテラルが残っていないか検査する (KEVI006)。
/// 移行途中に大量の警告が出ることを避けるため既定では無効化しており、
/// <c>dotnet build -p:LocalizationAudit=true</c> で監査モードとして有効化する。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HardcodedLocalizableStringAnalyzer : DiagnosticAnalyzer
{
	private const string DiagnosticId = "KEVI006";

	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticId,
		"ローカライズされていない可能性のある文字列です",
		"{0} 属性の文字列 '{1}' がローカライズされていない可能性があります",
		"Localization",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: false,
		customTags: new[] { WellKnownDiagnosticTags.CompilationEnd });

	// 表示に用いられる代表的な属性のみを対象とし、誤検知を抑える
	private static readonly Regex AttributeRegex = new(
		"(?<=\\s)(Text|Content|Header|Description|Title|Watermark|PlaceholderText|OnContent|OffContent|Message|ToolTip\\.Tip)\\s*=\\s*\"([^\"]*)\"",
		RegexOptions.Compiled | RegexOptions.CultureInvariant);

	private static readonly Regex JapaneseRegex = new(
		"[\\p{IsHiragana}\\p{IsKatakana}\\p{IsCJKUnifiedIdeographs}]",
		RegexOptions.Compiled | RegexOptions.CultureInvariant);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.RegisterCompilationAction(AnalyzeCompilation);
	}

	private static void AnalyzeCompilation(CompilationAnalysisContext context)
	{
		foreach (var file in context.Options.AdditionalFiles)
		{
			if (!file.Path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
				continue;

			var source = file.GetText(context.CancellationToken);
			if (source == null)
				continue;

			foreach (Match match in AttributeRegex.Matches(source.ToString()))
			{
				var valueGroup = match.Groups[2];
				var value = valueGroup.Value.TrimStart();
				// マークアップ拡張･バインディング({...})と空文字は対象外
				if (value.Length == 0 || value[0] == '{')
					continue;
				if (!JapaneseRegex.IsMatch(value))
					continue;

				var span = new TextSpan(valueGroup.Index, valueGroup.Length);
				var location = Location.Create(file.Path, span, source.Lines.GetLinePositionSpan(span));
				context.ReportDiagnostic(Diagnostic.Create(Rule, location, match.Groups[1].Value, valueGroup.Value));
			}
		}
	}
}
