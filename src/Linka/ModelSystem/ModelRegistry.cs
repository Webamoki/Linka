using System.Reflection;
using Sigil;
using Webamoki.Linka.Fields;
using Webamoki.Linka.SchemaSystem;

namespace Webamoki.Linka.ModelSystem;

internal static class ModelRegistry
{
    private static readonly Dictionary<Type, IModelInfo> ModelInfos = [];
    public static IModelInfo Get<T>() where T : Model => Get(typeof(T));

    public static IModelInfo Get(Type type)
    {
        if (ModelInfos.TryGetValue(type, out var modelInfo))
            return modelInfo;

        throw new KeyNotFoundException($"The model {type.Name} was not loaded.");
    }

    public static Model GetModel<T>() where T : Model => Get<T>().Model;

    
    private static BaseNavigationAttribute GetNavigationAttribute(FieldInfo field)
    {
        var attributes = field.GetCustomAttributes(false);
        BaseNavigationAttribute? navAttribute = null;
        foreach (var attribute in attributes)
        {
            if (attribute is not BaseNavigationAttribute nav) continue;
            if (navAttribute != null)
                throw new Exception($"Model {field.DeclaringType?.Name} has multiple navigation attributes on field {field.Name}.");
            navAttribute = nav;
        }
        if (navAttribute == null)
            throw new KeyNotFoundException($"Invalid field {field.Name} in model {field.DeclaringType?.Name}.");
        return navAttribute;
    }
    
    /// <summary>
    /// Configures the model's navigation properties and their constraints.
    /// Executed after all models are injected.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static bool ApplyNavigations<T>(Schema schema) where T : Model, new()
    {
        var info = Get<T>();
        if (info.Navigations.Count > 0 || info.NavigationLists.Count > 0)
            return false;
        var navigations = typeof(T).GetFields();
        foreach (var navReflectionInfo in navigations)
        {
            var navAttribute = GetNavigationAttribute(navReflectionInfo); 
            var navType = navReflectionInfo.FieldType;
            if (navType.IsGenericType && navType.GetGenericTypeDefinition() == typeof(List<>))
            {
                navType = navType.GetGenericArguments()[0];
            }

            if (!typeof(Model).IsAssignableFrom(navType)) throw new Exception($"The model {navType} is not a Model.");
            if (!schema.HasModel(navType))
                throw new Exception(
                    $"Cannot navigate unrelated model {navType.Name} for schema {schema.Name}.");
            var targetInfo = Get(navType);


            // Load both field and target field.
            var targetFieldName = navAttribute.TargetField;
            var fieldName = navAttribute.Field;

            if (navAttribute is PkNavigationAttribute)
            {
                if (targetInfo.PrimaryField == null)
                    throw new Exception(
                        $"Model {targetInfo.Model.GetType().Namespace} has more than one primary key.");
                targetFieldName = targetInfo.PrimaryField!.Name;
            }
            else if (navAttribute is PkNavigationListAttribute)
            {
                if (info.PrimaryField == null)
                    throw new Exception($"Model {typeof(T).Name} has more than one primary key.");
                fieldName = info.PrimaryField!.Name;
            }

            if (!info.Fields.TryGetValue(fieldName, out var field))
                throw new Exception($"Model {navType.Name} has no field {fieldName} defined.");

            if (!targetInfo.Fields.TryGetValue(targetFieldName, out var targetField))
                throw new Exception(
                    $"Model {navType.Name} has no field {targetFieldName} defined for navigation {navReflectionInfo.Name}.");
            if (field.Validator != targetField.Validator)
                throw new Exception(
                    $"Field {fieldName} in model {typeof(T).Name} and field {targetFieldName} in model {navType.Name} have different validators.");

            if (navAttribute is NavigationAttribute &&
                !targetInfo.UniqueFields.TryGetValue(targetFieldName, out var _))
                throw new Exception(
                    $"Field {targetFieldName} in model {navType.Name} is not unique for navigation {navReflectionInfo.Name}.");

            if (navAttribute is NavigationAttribute nAttribute)
            {
                if (!targetInfo.UniqueFields.TryGetValue(targetFieldName, out _))
                    throw new Exception(
                        $"Field {targetFieldName} in model {navType.Name} is not unique for navigation {navReflectionInfo.Name}.");

                var setter = CreateSetter<T>(navReflectionInfo, navType);
                info.Navigations.Add(navReflectionInfo.Name, new NavigationInfo(
                    fieldName,
                    targetFieldName,
                    setter,
                    targetInfo,
                    nAttribute.Constraint
                ));
            }
            else
            {
                var setter = CreateSetter<T>(navReflectionInfo, targetInfo.ListType);
                info.NavigationLists.Add(navReflectionInfo.Name, new NavigationListInfo(
                    fieldName,
                    targetFieldName,
                    setter,
                    targetInfo
                ));
            }
        }

        return true;
    }


    /// <summary>
    ///     Stores a reference of the specified model type along with its associated
    ///     model information.
    /// </summary>
    /// <typeparam name="T">The type of the model to be injected, which must inherit from AbstractModel.</typeparam>
    public static void InitialCompile<T>() where T : Model, new()
    {
        var type = typeof(T);
        var model = new T();

        var info = new ModelInfo<T>(model);
        var fields = model.GetType().GetProperties();

        foreach (var propertyInfo in fields)
        {
            // Store information about fields/table columns.
            if (!typeof(DbField).IsAssignableFrom(propertyInfo.PropertyType)) continue;
            var field = (DbField)propertyInfo.GetValue(model)!;
            field.SetName(propertyInfo.Name);
            field.ApplyAttributes(propertyInfo.GetCustomAttributes(false));
            if (field.IsPrimary) info.PrimaryFields.Add(propertyInfo.Name, field);
            if (field.IsUnique) info.UniqueFields.Add(propertyInfo.Name, field);
            info.Fields.Add(propertyInfo.Name, field);
            var getter = CreateGetter<T>(propertyInfo);
            info.FieldGetters.Add(propertyInfo.Name, getter);
        }

        info.PrimaryField = info.PrimaryFields.Count switch
        {
            1 => info.PrimaryFields.Values.First(),
            0 => throw new Exception($"Model {model.GetType().Name} has no primary key defined."),
            _ => info.PrimaryField
        };
        if (info.PrimaryField != null)
            info.PrimaryField.IsUnique = true;
        ModelInfos[type] = info;
    }

    private static Func<Model, DbField> CreateGetter<T>(PropertyInfo propertyInfo) where T : Model =>
        Emit<Func<Model, DbField>>.NewDynamicMethod(propertyInfo.Name)
            .LoadArgument(0)
            .CastClass(typeof(T)).Call(propertyInfo.GetGetMethod()!).Return().CreateDelegate();


    private static Action<Model, object> CreateSetter<T>(FieldInfo field, Type modelType) where T : Model =>
        Emit<Action<Model, object>>.NewDynamicMethod($"Set_{typeof(T).Name}_{field.Name}")
            .LoadArgument(0)
            .CastClass(typeof(T))
            .LoadArgument(1)
            .CastClass(modelType)
            .StoreField(field)
            .Return()
            .CreateDelegate();
}

internal interface IModelInfo
{
    Model Model { get; }
    Type ModelType { get; }

    Dictionary<string, DbField> Fields { get; }
    Dictionary<string, Func<Model, DbField>> FieldGetters { get; }
    Dictionary<string, DbField> PrimaryFields { get; }
    Dictionary<string, DbField> UniqueFields { get; }
    Dictionary<string, NavigationInfo> Navigations { get; }
    Dictionary<string, NavigationListInfo> NavigationLists { get; }
    DbField? PrimaryField { get; }

    Model Create();
    Type ListType { get; }
    string TableName { get; }
    public void SetListNavigation(Action<Model, object> action, Model model, List<Model> models);
    FieldIterator FieldIterator(Model model);
}

internal class ModelInfo<T>(Model model) : IModelInfo where T : Model, new()
{
    public Model Model { get; } = model;
    public Type ModelType => typeof(T);
    public Dictionary<string, DbField> Fields { get; } = [];
    public Dictionary<string, Func<Model, DbField>> FieldGetters { get; } = [];
    public Dictionary<string, DbField> PrimaryFields { get; } = [];
    public Dictionary<string, DbField> UniqueFields { get; } = [];

    public Dictionary<string, NavigationInfo> Navigations { get; } = [];
    public Dictionary<string, NavigationListInfo> NavigationLists { get; } = [];
    public DbField? PrimaryField { get; set; }
    public Model Create() => new T();
    public Type ListType => typeof(List<T>);
    public string TableName => Model.TableName<T>();
    public void SetListNavigation(Action<Model, object> action, Model model, List<Model> models)
    {
        var list = models.Cast<T>().ToList();
        action.Invoke(model, list);
    }

    public FieldIterator FieldIterator(Model model)
    {
        return new FieldIterator(FieldGetters, model);
    }
}


internal struct NavigationInfo(
    string field,
    string targetField,
    Action<Model, object> setter,
    IModelInfo modelInfo,
    NavConstraint constraint)
{
    public readonly NavConstraint Constraint = constraint;
    public readonly string Field = field;
    public readonly string TargetField = targetField;
    public readonly Action<Model, object> Setter = setter;
    public readonly IModelInfo TargetModelInfo = modelInfo;
}

/// <summary>
/// Used to store information about what navigation lists are available in a model.
/// Setter is used to set the list field in the model.
/// </summary>
/// <param name="field"></param>
/// <param name="targetField"></param>
/// <param name="setter"></param> the object should be of type List&lt;T&gt; where T is the target model type.
/// <param name="modelInfo"></param>
internal struct NavigationListInfo(
    string field,
    string targetField,
    Action<Model, object> setter,
    IModelInfo modelInfo)
{
    public readonly string Field = field;
    public readonly string TargetField = targetField;
    public readonly Action<Model, object> Setter = setter;
    public readonly IModelInfo TargetModelInfo = modelInfo;
}