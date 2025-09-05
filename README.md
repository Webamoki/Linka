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
public class UserModel() : Model
{
    [Key]
    [Unique]
    [FullText]
    public IdDbField ID { get; } = new();

    [FullText]
    public NameDbField Name { get; } = new();

    [FullText]
    [Unique]
    public EmailDbField Email { get; } = new();

    [FullText]
    public PhoneDbField Phone { get; } = new();

    public enum RankEnum
    {
        User,
        Admin
    }

    public EnumDbField<RankEnum> Rank { get; } = new();

    [Unique]
    [NotRequired]
    public HashDbField Session { get; } = new();

    public TextDbField Password { get; } = new();

    [NotRequired] // Navigation Field Property
    public IdDbField CartToken { get; } = new();

    public DateDbField Created { get; } = new();

    public BooleanDbField Verified { get; } = new();
    public BooleanDbField Login { get; } = new();
    public PriceDbField<Gbp> Credit { get; } = new();

    [PkNavigationList(nameof(IpAddressModel.UserID))]
    public List<IpAddressModel> IpAddresses = null!;

    public UserModel(
        string name,
        string email,
        string phone,
        RankEnum rankEnum,
        string password,
        string? cartToken,
        bool verified,
        bool login,
        int credit
    ) : this()
    {
        Name.Value(name);
        Email.Value(email);
        Phone.Value(phone);
        Rank.Value(rankEnum);
        Password.Value(password);
        CartToken.Value(cartToken);
        Created.SetNow();
        Verified.Value(verified);
        Login.Value(login);
        Credit.Value(credit);
    }
}
```

### 2. Create Database Schema

```csharp
[method: Enum<UserModel.RankEnum>]
[method: Model<UserModel>]
[method: Model<IpAddressModel>]
public class UserSchema() : Schema("User");
```

### 3. Basic Operations

```csharp
// Create a new user using constructor
var model1 = new UserModel(
    "John",
    "johndoe@example.com",
    "1234567890",
    UserModel.RankEnum.User,
    "password",
    null,
    true,
    false,
    100
);
model1.ID.Value("AAAAAAAAAA");

// Insert into database
using var db = new DbService<UserSchema>();
db.Insert(model1);
db.SaveChanges();

// Query single user
var user = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
var userOrNull = db.GetOrNull<UserModel>(u => u.ID == "NONEXISTENT");

// Query multiple users
var adminUsers = db.GetMany<UserModel>(u => u.Rank == UserModel.RankEnum.Admin).Load();
var userCount = db.GetMany<UserModel>(u => u.Verified == true).Count();

// Update user fields directly
var foundUser = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
foundUser.Name.Value("John Updated");
foundUser.Rank.Value(UserModel.RankEnum.Admin);
// Changes are automatically tracked

// Delete user
db.Delete<UserModel>(u => u.ID == "AAAAAAAAAA");
```

## Database Operations

### Querying

```csharp
using var db = new DbService<UserSchema>();

// Single record queries
var user = db.Get<UserModel>(u => u.Email == "johndoe@example.com");
var userOrNull = db.GetOrNull<UserModel>(u => u.ID == "AAAAAAAAAA");

// Multiple record queries
var getManyExpression = db.GetMany<UserModel>(u => u.Verified == true);
var users = getManyExpression.Load(); // Execute and get results
var count = getManyExpression.Count(); // Get count without loading data

// Complex queries
var adminUsers = db.GetMany<UserModel>(u => u.Rank == UserModel.RankEnum.Admin).Load();
var verifiedUsers = db.GetMany<UserModel>(u => u.Verified == true).Load();
```

### CRUD Operations

```csharp
using var db = new DbService<UserSchema>();

// Create
var user = new UserModel(
    "Alice",
    "alice@example.com",
    "0987654321",
    UserModel.RankEnum.Admin,
    "securePass123",
    null,
    true,
    true,
    250
);
user.ID.Value("BBBBBBBBBB");
db.Insert(user);
db.SaveChanges();

// Read
var foundUser = db.Get<UserModel>(u => u.ID == "BBBBBBBBBB");

// Update (fields are automatically tracked)
foundUser.Name.Value("Alice Updated");
foundUser.Credit.Value(300);

// Delete
db.Delete<UserModel>(u => u.ID == "BBBBBBBBBB");
```

## Testing

Linka includes a comprehensive testing framework with database mocking:

### Test Setup
```csharp
[Fixtures<UserModelFixture>]
[Fixtures<IpAddressFixture>]
public class UserServiceTests
{
    [Test]
    public void Get_UserModel_ReturnsExpected()
    {
        using var db = new DbService<UserSchema>();
        var model = db.Get<UserModel>(u => u.ID == "AAAAAAAAAA");
        Ensure.Equal("John", model.Name.Value());
    }
}
```

### Fixtures
```csharp
public class UserModelFixture : Fixture<UserSchema>, IFixture
{
    public override void Inject()
    {
        var model1 = new UserModel(
            "John",
            "johndoe@example.com",
            "1234567890",
            UserModel.RankEnum.User,
            "password",
            null,
            true,
            false,
            100
        );
        model1.ID.Value("AAAAAAAAAA");

        using var db = new DbService<UserSchema>();
        db.Insert(model1);
        db.SaveChanges();
    }
}
```

### Multiple Fixtures
```csharp
[Fixtures<UserModelFixture>]
[Fixtures<IpAddressFixture>]
public class IntegrationTests
{
    // All fixtures will be applied before tests run
}
```


## Field Operations

### Setting Values
```csharp
user.Name.Value("John Doe");
user.Created.SetNow();  // Set to current date
user.ID.Value("AAAAAAAAAA"); // Set specific ID
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



## Best Practices

1. **Use constructors** for creating models with required fields
2. **Use GetOrNull** when records might not exist
3. **Use GetMany().Count()** for counting without loading data
4. **Use GetMany().Load()** when you need the actual records
6. **Use fixtures** for comprehensive test data setup
7. **Call SaveChanges()** after inserts or updates to commit transactions

## Requirements

- .NET 9.0 or later

## Installation

```bash
# Install via NuGet
dotnet add package Linka.ORM

# Or clone and build from source
git clone https://github.com/webamoki/linka-orm.git
cd linka-orm
dotnet build
```
