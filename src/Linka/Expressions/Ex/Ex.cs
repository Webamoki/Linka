using Webamoki.Linka.ModelSystem;

namespace Webamoki.Linka.Expressions.Ex;

internal interface IEx<T> where T : Model;

internal interface IConditionEx<T>: IEx<T> where T : Model;

internal record EqualEx<T>(string name, bool isEqual) : IConditionEx<T> where T : Model;
internal record NullEx<T>(string name, bool isNull) : IConditionEx<T> where T : Model;
internal record EnumEx<T>(string name, bool isEqual, string value) : IConditionEx<T> where T : Model;
internal record IntEx<T>(string name, string op, int value) : IConditionEx<T> where T : Model;
internal record DateTimeEx<T>(string name, string op, string value) : IConditionEx<T> where T : Model;
internal record Ex<T>(IEx<T> left, string op, IEx<T> right) :  IEx<T> where T : Model;

internal interface IValueEx<T> where T : Model;

internal record ObjectValueEx<T>(object Value) : IValueEx<T> where T : Model;

internal record EmbeddedValue<T>(object Value) : IValueEx<T> where T : Model;

internal record CompareValu