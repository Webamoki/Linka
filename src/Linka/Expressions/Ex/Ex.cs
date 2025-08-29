namespace Webamoki.Linka.Expressions.Ex;

internal interface IEx;

internal record AssignEx(string name, string op, IValueEx value) : IEx;

internal record Ex(IEx left, string op, IEx right) : IEx;

internal interface IValueEx;
internal record NullValueEx : IValueEx;

internal record ObjectValueEx(object Value) : IValueEx;

