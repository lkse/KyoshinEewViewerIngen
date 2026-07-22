using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace KyoshinEewViewer.CodeAnalysis;

[Generator(LanguageNames.CSharp)]
public partial class CsvDictionaryGenerator : IIncrementalGenerator
{
	public static string GenerateClassFile(StringBuilder sb, string dictionaryName, string csvText, string keyType, string keyFormat, string valueType, string valueFormat)
	{
		using var reader = new StringReader(csvText);

		sb.AppendLine($"        public static System.Collections.Generic.IReadOnlyDictionary<{keyType}, {valueType}> {dictionaryName} {{ get; }} = new System.Collections.Generic.Dictionary<{keyType}, {valueType}>(){{");

		while (true)
		{
			var line = reader.ReadLine();
			if (line == null) break;
			var fields = SplitCsvLine(line);
			sb.AppendLine($"            {{ {string.Format(keyFormat, fields)}, {string.Format(valueFormat, fields)} }},");
		}

		sb.AppendLine("        };");

		return sb.ToString();
	}

	/// <summary>
	/// CSV 1行をフィールドに分割する。RFC4180 に準じ、"..." で囲まれたフィールドは
	/// 内部にカンマを含めることができ、"" は 1 つの " を表す。
	/// 引用符を含まないフィールドは従来の Split(',') と同じ挙動になる。
	/// </summary>
	private static string[] SplitCsvLine(string line)
	{
		var fields = new System.Collections.Generic.List<string>();
		var sb = new StringBuilder();
		var inQuotes = false;
		for (var i = 0; i < line.Length; i++)
		{
			var c = line[i];
			if (inQuotes)
			{
				if (c == '"')
				{
					if (i + 1 < line.Length && line[i + 1] == '"')
					{
						sb.Append('"');
						i++;
					}
					else
						inQuotes = false;
				}
				else
					sb.Append(c);
			}
			else if (c == '"')
				inQuotes = true;
			else if (c == ',')
			{
				fields.Add(sb.ToString());
				sb.Clear();
			}
			else
				sb.Append(c);
		}
		fields.Add(sb.ToString());
		return fields.ToArray();
	}

	private static StringBuilder SourceFilesFromEntries(ImmutableArray<(string ClassName, string CsvText, string KeyType, string KeyFormat, string ValueType, string ValueFormat)> entries)
	{
		var sb = new StringBuilder();
		sb.AppendLine(@"
#nullable enable
namespace KyoshinEewViewer {
    public static class CsvDictionary {");
		foreach (var entry in entries)
		{
			GenerateClassFile(sb, entry.ClassName, entry.CsvText, entry.KeyType, entry.KeyFormat, entry.ValueType, entry.ValueFormat);
		}
		sb.AppendLine("    }\r\n}");
		return sb;
	}

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var csvFiles = context.AdditionalTextsProvider
			.Where(file => Path.GetExtension(file.Path).Equals(".csv", StringComparison.OrdinalIgnoreCase))
			.Combine(context.AnalyzerConfigOptionsProvider)
			.Select((pair, cancellationToken) =>
			{
				var (file, optionsProvider) = pair;
				var options = optionsProvider.GetOptions(file);

				options.TryGetValue("build_metadata.Additionalfiles.KeyType", out var keyType);
				options.TryGetValue("build_metadata.Additionalfiles.KeyFormat", out var keyFormat);
				options.TryGetValue("build_metadata.Additionalfiles.ValueType", out var valueType);
				options.TryGetValue("build_metadata.Additionalfiles.ValueFormat", out var valueFormat);

				if (keyType == null)
					throw new Exception("KeyType is not defined.");
				if (keyFormat == null)
					throw new Exception("KeyFormat is not defined.");
				if (valueType == null)
					throw new Exception("ValueType is not defined.");
				if (valueFormat == null)
					throw new Exception("ValueFormat is not defined.");

				var className = Path.GetFileNameWithoutExtension(file.Path);
				var csvText = file.GetText(cancellationToken)!.ToString();

				return (ClassName: className, CsvText: csvText, KeyType: keyType, KeyFormat: keyFormat, ValueType: valueType, ValueFormat: valueFormat);
			})
			.Collect();

		context.RegisterSourceOutput(csvFiles, (sourceContext, entries) =>
		{
			if (entries.IsDefaultOrEmpty) return;
			var source = SourceFilesFromEntries(entries).ToString();
			sourceContext.AddSource("CsvDictionary.g.cs", SourceText.From(source, Encoding.UTF8));
		});
	}
}
