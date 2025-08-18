namespace Webamoki.Linka.Fields;


/// <summary>
/// Class that is used to validate a specific value/format.
/// Used in form validation and in the database.
/// </summary>
public abstract class Validator
{
    private static readonly Dictionary<string, Validator?> Validators = new();
    public abstract bool IsValid(object? value, out string? message);
    
    /// <summary>
    /// This value determines whether the values that pass can be used to do injection attacks.
    /// True, the value has a strict format and can not be used to inject code.
    /// False, the value does not have a strict format and can be used to inject code.
    /// </summary>
    public bool IsInjectable { get; protected set; }

    protected static void Register<T>(string hash, T validator) where T : Validator
    {
        hash = typeof(T).Name + hash;
        if (!Validators.TryAdd(hash, validator))
            throw new Exception($"Validator {hash} already registered");
    }

    
    protected static bool Load<T>(string hash, out T? validator) where T : Validator
    {
        hash = typeof(T).Name + hash;
        if (!Validators.TryGetValue(hash, out var v))
        {
            validator = null;
            return false;
        }
        validator = (T)v!;
        return true;
    }
}