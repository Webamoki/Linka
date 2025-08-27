using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.Testing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FixturesAttribute<T> : Attribute, ITestAction where T : IFixture, new()
{
    // ReSharper disable once StaticMemberInGenericType

    public void BeforeTest(ITest test)
    {
        // Create a Fixture Manager if it doesn't exist already
        var testClass = test.ClassName!;
        var fixtureManager = FixtureManager.Get(testClass);

        if (!fixtureManager.IsComplete())
        {
            fixtureManager.Add(new T());
        }
        
        var testClassAttributes = test.Fixture?.GetType().GetCustomAttributes(true) ?? [];
        var fixtureCount = testClassAttributes.Length;
        if (fixtureManager.Count != fixtureCount) return;

        if (!fixtureManager.IsLast<T>()) return;
        
        fixtureManager.Load();
    }

    public void AfterTest(ITest test)
    {
        var fixtureManager = FixtureManager.Get(test.ClassName!);
        fixtureManager.Complete();
        if (!fixtureManager.IsLast<T>()) return;
        fixtureManager.Dispose();
    }

    public ActionTargets Targets => ActionTargets.Test;
}


[AttributeUsage(AttributeTargets.Class)]
public class CompileSchemaAttribute<T> : Attribute, ITestAction where T : Schema, new()
{
    public void BeforeTest(ITest test)
    {
        Linka.TryCompile<T>();
    }

    public void AfterTest(ITest test)
    {

    }

    public ActionTargets Targets => ActionTargets.Suite;
}
