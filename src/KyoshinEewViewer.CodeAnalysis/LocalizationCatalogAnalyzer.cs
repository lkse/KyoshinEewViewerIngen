using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace KyoshinEewViewer.CodeAnalysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LocalizationCatalogAnalyzer : DiagnosticAnalyzer
{
	private const string MissingKeyDiagnosticId = "KEVI002";
	private const string UnknownKeyDiagnosticId = "KEVI003";
	private const string DuplicateKeyDiagnosticId = "KEVI004";
	private const string PlaceholderMismatchDiagnosticId = "KEVI005";

	private static readonly DiagnosticDescriptor MissingKeyRule = new(
		MissingKeyDiagnosticId,
		"ローカライズキーが不足しています",
		"{0} カタログにローカライズキー '{1}' がありません",
		"Localization",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		customTags: new[] { WellKnownDiagnosticTags.CompilationEnd });

	private static readonly DiagnosticDescriptor UnknownKeyRule = new(
		UnknownKeyDiagnosticId,
		"未定義のローカライズキーです",
		"ローカライズキー '{0}' は LocalizationKey に定義されていません",
		"Localization",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		customTags: new[] { WellKnownDiagnosticTags.CompilationEnd });

	private static readonly DiagnosticDescriptor DuplicateKeyRule = new(
		DuplicateKeyDiagnosticId,
		"ローカライズキーが重複しています",
		"{0} カタログでローカライズキー '{1}' が重複しています",
		"Localization",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		customTags: new[] { WellKnownDiagnosticTags.CompilationEnd });

	private static readonly DiagnosticDescriptor PlaceholderMismatchRule = new(
		PlaceholderMismatchDiagnosticId,
		"ローカライズ文字列のプレースホルダーが一致しません",
		"ローカライズキー '{0}' のプレースホルダーが日本語と {1} で一致しません",
		"Localization",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		customTags: new[] { WellKnownDiagnosticTags.CompilationEnd });

	private static readonly Regex EntryRegex = new(
		"\"([A-Za-z0-9_]+)\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
		RegexOptions.Compiled | RegexOptions.CultureInvariant);

	private static readonly Regex PlaceholderRegex = new(
		"(?<!\\{)\\{(?:[A-Za-z_][A-Za-z0-9_]*|[0-9]+)(?:[^{}]*)\\}(?!\\})",
		RegexOptions.Compiled | RegexOptions.CultureInvariant);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		=> ImmutableArray.Create(MissingKeyRule, UnknownKeyRule, DuplicateKeyRule, PlaceholderMismatchRule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.RegisterCompilationAction(AnalyzeCompilation);
	}

	private static void AnalyzeCompilation(CompilationAnalysisContext context)
	{
		var keyType = context.Compilation.GetTypeByMetadataName("KyoshinEewViewer.Localization.LocalizationKey");
		if (keyType == null)
			return;

		var definedKeys = keyType.GetMembers()
			.OfType<IFieldSymbol>()
			.Where(f => f.HasConstantValue)
			.Select(f => f.Name)
			.ToImmutableHashSet(StringComparer.Ordinal);
		var catalogs = new Dictionary<string, Catalog>(StringComparer.Ordinal);

		foreach (var file in context.Options.AdditionalFiles)
		{
			var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(file);
			if (!options.TryGetValue("build_metadata.AdditionalFiles.LocalizationCulture", out var culture) || string.IsNullOrWhiteSpace(culture))
				continue;
			var source = file.GetText(context.CancellationToken);
			if (source == null)
				continue;

			var catalog = ParseCatalog(file.Path, source, culture, context);
			catalogs[culture] = catalog;
		}

		foreach (var culture in new[] { "ja", "en" })
		{
			if (!catalogs.TryGetValue(culture, out var catalog))
			{
				foreach (var key in definedKeys)
					context.ReportDiagnostic(Diagnostic.Create(MissingKeyRule, Location.None, culture, key));
				continue;
			}

			foreach (var key in definedKeys)
				if (!catalog.Values.ContainsKey(key))
					context.ReportDiagnostic(Diagnostic.Create(MissingKeyRule, Location.Create(catalog.Path, default, default), culture, key));

			foreach (var entry in catalog.Values)
				if (!definedKeys.Contains(entry.Key))
					context.ReportDiagnostic(Diagnostic.Create(UnknownKeyRule, catalog.Locations[entry.Key], entry.Key));
		}

		if (catalogs.TryGetValue("ja", out var japanese))
			foreach (var catalog in catalogs.Values.Where(c => c.Culture != "ja"))
				foreach (var key in definedKeys)
					if (japanese.Values.TryGetValue(key, out var japaneseValue) && catalog.Values.TryGetValue(key, out var localizedValue)
						&& !GetPlaceholders(japaneseValue).SetEquals(GetPlaceholders(localizedValue)))
						context.ReportDiagnostic(Diagnostic.Create(PlaceholderMismatchRule, catalog.Locations[key], key, catalog.Culture));

	}

	private static Catalog ParseCatalog(string path, SourceText source, string culture, CompilationAnalysisContext context)
	{
		var values = new Dictionary<string, string>(StringComparer.Ordinal);
		var locations = new Dictionary<string, Location>(StringComparer.Ordinal);
		var content = source.ToString();

		foreach (Match match in EntryRegex.Matches(content))
		{
			var key = match.Groups[1].Value;
			var location = CreateLocation(path, source, match.Groups[1]);

			if (values.ContainsKey(key))
			{
				context.ReportDiagnostic(Diagnostic.Create(DuplicateKeyRule, location, culture, key));
				continue;
			}
			values.Add(key, Regex.Unescape(match.Groups[2].Value));
			locations.Add(key, location);
		}

		return new Catalog(path, culture, values, locations);
	}

	private static ImmutableHashSet<string> GetPlaceholders(string value)
		=> PlaceholderRegex.Matches(value).Cast<Match>().Select(m => m.Value).ToImmutableHashSet(StringComparer.Ordinal);

	private static Location CreateLocation(string path, SourceText source, Group group)
	{
		var span = new TextSpan(group.Index, group.Length);
		return Location.Create(path, span, source.Lines.GetLinePositionSpan(span));
	}

	private sealed class Catalog
	{
		public string Path { get; }
		public string Culture { get; }
		public Dictionary<string, string> Values { get; }
		public Dictionary<string, Location> Locations { get; }

		public Catalog(string path, string culture, Dictionary<string, string> values, Dictionary<string, Location> locations)
		{
			Path = path;
			Culture = culture;
			Values = values;
			Locations = locations;
		}
	}
}
