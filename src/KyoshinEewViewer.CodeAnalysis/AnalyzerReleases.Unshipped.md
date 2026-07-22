; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
KEVI001 | Serialization | Error | ワークフローの Trigger/Action の Control 返却プロパティに [JsonIgnore] を強制する (WorkflowDisplayControlAnalyzer)
KEVI002 | Localization | Error | 対応言語のカタログに不足しているローカライズキーを検出する
KEVI003 | Localization | Error | 未定義のローカライズキーとその参照を検出する
KEVI004 | Localization | Warning | カタログ内で重複しているローカライズキーを検出する
KEVI005 | Localization | Error | 言語間で一致しないプレースホルダーを検出する
