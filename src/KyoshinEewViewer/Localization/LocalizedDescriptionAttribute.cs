using System;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// ローカライズキーで説明文を指定する属性。
/// テンプレートエディタの補完候補などで、言語に応じた説明を表示するために使用する。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class LocalizedDescriptionAttribute(LocalizationKey key) : Attribute
{
	public LocalizationKey Key { get; } = key;
}
