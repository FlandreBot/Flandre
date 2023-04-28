using System.Diagnostics.CodeAnalysis;

namespace Flandre.Framework.Common;

/// <summary>
/// 类型解析器委托
/// </summary>
/// <typeparam name="T">需要解析的类型</typeparam>
public delegate bool TypeResolverDelegate<T>(string raw, out T result);

internal delegate bool TypeResolverDelegate(string raw, [NotNullWhen(true)] out object? result);
