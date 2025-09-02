# Linka ORM

A modern, type-safe Object-Relational Mapping (ORM) framework for .NET with PostgreSQL, designed for high performance and developer productivity.

## Features

- 🚀 **Type-Safe**: Strongly typed models with compile-time validation
- 🎯 **Field-Based Architecture**: Rich field types with built-in validation
- 🔄 **Change Tracking**: Automatic detection of field modifications
- 🧪 **Testing Framework**: Built-in test utilities with database mocking
- 🔍 **Query Builder**: Fluent API for complex database operations
- 📊 **Schema Management**: Automatic database schema generation and migration
- 🔒 **Validation**: Comprehensive field validation with custom validators
- 🎨 **Flexible**: Support for enums, custom types, and complex relationships

## Quick Start

### 1. Define Your Models

```csharp
public class UserModel : Model
{
    public IdDbField ID { get; set; } = new();
    public NameDbField Name { get; set; } = new();
    public EmailDbField Email { get; set; } = new();
    public PhoneDbField Phone { get; set; } = new();
    public EnumDbField<UserRank> Rank { get; set; } = new();
    public PasswordDbField Password { get; set; } = new();
    public DateTimeDbField Created { get; set; } = new();
    public BooleanDbField Verified { get; set; } = new();
    public BooleanDbField Login { get; set; } = new();
    public PriceDbField<Gbp> Credit { get; set; } = new();

    public enum UserRank { User, Admin, SuperAdmin }
}
```

### 2. Create Database Schema

```csharp
public class UserDbSchema : DbSchema
{
    public UserDbSchema()
    {
        DatabaseName = "user_database";
    }

    [ModelAttribute<UserModel>]
    public UserDbSchema(IServiceProvider serviceProvider) : base(serviceProvider) { }
}
```

### 3. Basic Operations

```csharp
// Create a new user
var user = new UserModel();
user.ID.GenerateValue();
user.Name.Value("John Doe");
user.Email.Value("john@example.com");
user.Phone.Value("555-1234");
user.Rank.Value(UserModel.UserRank.User);
user.Password.Value("securepassword");
user.Created.SetNow();
user.Verified.Value(true);
user.Login.Value(false);
user.Credit.Value(new Gbp(100.00m));

// Insert into database
using var dbService = new DbService<UserDbSchema>();
var result = dbService.Insert(user);

// Query users
var activeUser = dbService.First<UserModel>(u => u.Verified.Value() == true);
var adminUsers = dbService.Where<UserModel>(u => u.Rank.Value() == UserModel.UserRank.Admin);

// Update user
user.Name.Value("Jane Doe");
user.Email.Value("jane@example.com");
dbService.Update(user);

// Delete user
dbService.Delete(user);
```

## Field Types

Linka provides a rich set of field types with built-in validation:

### Text Fields
- `TextDbField` - General text with length validation
- `NameDbField` - Names (max 50 characters)
- `EmailDbField` - Email addresses with format validation
- `PasswordDbField` - Passwords with security requirements
- `HexColorDbField` - Hex color codes (6 characters)
- `PhoneDbField` - Phone numbers with format validation
- `UrlDbField` - URL slugs (lowercase, alphanumeric, hyphens)
- `HashDbField` - Hash values starting with 's'
- `PostcodeDbField` - UK postcodes

### Numeric Fields
- `IntDbField` - Integers with min/max validation
- `PriceDbField<T>` - Currency amounts (USD, EUR, GBP, JPY)

### Other Fields
- `BooleanDbField` - Boolean values
- `DateTimeDbField` - Date and time values
- `DateDbField` - Date-only values
- `EnumDbField<T>` - Strongly typed enums
- `IdDbField` - Unique identifiers with generation

### ID Fields
- `IdDbField` - 10-character alphanumeric IDs
- `ShortIdDbField` - 5-character IDs
- `TokenDbField` - 5-character alphanumeric tokens

## Field Operations

### Setting Values
```csharp
user.Name.Value("John Doe");
user.Created.SetNow();  // Set to current datetime
user.ID.GenerateValue(); // Generate unique ID
```

### Accessing Values
```csharp
string name = user.Name.StringValue();  // Get as string
object nameObj = user.Name.ObjectValue(); // Get as typed object
string userName = user.Name.Value();    // Get typed value
```

### Validation
```csharp
if (user.Email.IsValid(out string message))
{
    // Email is valid
}
else
{
    Console.WriteLine($"Invalid email: {message}");
}
```

### Change Tracking
```csharp
user.Name.Value("Original Name");
user.Name.ResetChange(); // Mark as unchanged

user.Name.Value("New Name");
if (user.Name.IsChanged())
{
    Console.WriteLine("Name was modified");
}
```

## Field Iterator

Use the `FieldIterator` for efficient field operations:

```csharp
var fieldIterator = user.GetFieldIterator();

// Iterate through all fields
foreach (var (fieldName, field) in fieldIterator.AllFields())
{
    Console.WriteLine($"{fieldName}: {field.StringValue()}");
}

// Only changed fields
foreach (var (fieldName, field) in fieldIterator.ChangedFields())
{
    Console.WriteLine($"Modified: {fieldName} = {field.StringValue()}");
}

// Only set fields
foreach (var (fieldName, field) in fieldIterator.SetFields())
{
    Console.WriteLine($"Set: {fieldName} = {field.StringValue()}");
}

// Fields ready for database insertion
foreach (var (fieldName, field) in fieldIterator.InsertableFields())
{
    Console.WriteLine($"Insertable: {fieldName} = {field.StringValue()}");
}
```

## Database Operations

### Querying
```csharp
// Single record
var user = dbService.First<UserModel>(u => u.Email.Value() == "john@example.com");
var userOrNull = dbService.FirstOrNull<UserModel>(u => u.ID.Value() == "USER123");

// Multiple records
var users = dbService.Where<UserModel>(u => u.Verified.Value() == true);
var adminUsers = dbService.Where<UserModel>(u => u.Rank.Value() == UserModel.UserRank.Admin);

// With includes (relationships)
var usersWithData = dbService.Include<UserModel>(u => u.Profile)
                            .Where(u => u.Active.Value() == true);
```

### CRUD Operations
```csharp
// Create
var result = dbService.Insert(user);

// Read
var user = dbService.First<UserModel>(u => u.ID.Value() == "USER123");

// Update
user.Name.Value("Updated Name");
dbService.Update(user);

// Delete
dbService.Delete(user);
```

## Testing

Linka includes a comprehensive testing framework with database mocking:

### Test Setup
```csharp
[RegisterSchema<UserDbSchema>]
[FixturesAttribute<UserFixture>]
[FixturesAttribute<AdminUserFixture>]
public class UserServiceTests
{
    [Test]
    public void CreateUser_ShouldSucceed()
    {
        // Test implementation
        // Database is automatically mocked with fixture data
    }
}
```

### Fixtures
```csharp
public class UserFixture : IFixture
{
    public DbSchema Schema() => DbSchema.Get<UserDbSchema>();
    
    public void Inject()
    {
        var user = new UserModel();
        user.ID.GenerateValue();
        user.Name.Value("Test User");
        user.Email.Value("test@example.com");
        user.Created.SetNow();
        user.Verified.Value(true);
        
        using var dbService = new DbService<UserDbSchema>();
        dbService.Insert(user);
    }
}
```

### Multiple Fixtures
You can apply multiple fixture attributes to set up complex test scenarios:

```csharp
[FixturesAttribute<UserFixture>]
[FixturesAttribute<AdminUserFixture>]
[FixturesAttribute<ProductFixture>]
public class IntegrationTests
{
    // All fixtures will be applied before tests run
}
```

## Custom Validators

Create custom field validation:

```csharp
public class CustomValidator : Validator
{
    public override bool IsValid(object? value, out string? message)
    {
        if (value is string str && str.StartsWith("CUSTOM_"))
        {
            message = null;
            return true;
        }
        
        message = "Value must start with 'CUSTOM_'";
        return false;
    }
}

// Use in field
public class CustomField : RefDbField<string>
{
    public CustomField() : base(new CustomValidator(), "VARCHAR(50)") { }
}
```

## Price and Currency Support

Built-in support for multiple currencies:

```csharp
// Different currency types
var usdPrice = new PriceDbField<Usd>();
var eurPrice = new PriceDbField<Eur>();
var gbpPrice = new PriceDbField<Gbp>();
var jpyPrice = new PriceDbField<Jpy>();

// Set values (in minor units - cents, pence, etc.)
usdPrice.Value(12345); // $123.45
gbpPrice.Value(5000);  // £50.00

// Display formatted
Console.WriteLine(usdPrice.Price.Display()); // "$123.45"
Console.WriteLine(gbpPrice.Price.Display()); // "£50.00"
```

## Configuration

### Connection Strings
Configure your database connection in your application settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=myapp;Username=user;Password=pass"
  }
}
```

### Schema Registration
Register your schemas with dependency injection:

```csharp
services.AddScoped<UserDbSchema>();
services.AddScoped<IDbService, DbService<UserDbSchema>>();
```

## Best Practices

1. **Always validate fields** before database operations
2. **Use appropriate field types** for your data
3. **Leverage change tracking** for efficient updates
4. **Write comprehensive tests** using the fixture system
5. **Use FieldIterator** for bulk field operations
6. **Generate IDs** using `GenerateValue()` for new records
7. **Set timestamps** using `SetNow()` for date fields
