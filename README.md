![Nuget](https://img.shields.io/nuget/v/Johnjalani.Blazor.IndexedDB.WebAssembly?style=plastic)
![GitHub](https://img.shields.io/github/license/johnjalani/Blazor.IndexedDB.WebAssembly?style=plastic)

Blazor.IndexedDB.WebAssembly

An easy way to interact with IndexedDB and make it feel like EFCore but async.

# NuGet installation
```powershell
PM> Install-Package Johnjalani.Blazor.IndexedDB.WebAssembly -Version 1.1.0
```
## How to use
1. In your program.cs file add
```CSharp
builder.Services.AddScoped<IIndexedDbFactory, IndexedDbFactory>();
```
IIndexedDbFactory is used to create your database connection and will create the database instance for you.
IndexedDbFactory requires an instance IJSRuntime, should normally already be registered.

2. Add the following to your index.html, below the blazor js file declaration:
```Html
<script src="_content/TG.Blazor.IndexedDB/indexedDb.Blazor.js"></script>
```

3. Create any code first database model you'd like to create and inherit from IndexedDb. (Only properties with the type IndexedSet<> will be used, any other properties are beeing ignored)
```CSharp
public class ContextDb : IndexedDb
{
  public ContextDb(IJSRuntime jSRuntime, string name, int version) : base(jSRuntime, name, version) { }
  public IndexedSet<Student> Student { get; set; }
}
```
- Your model (eg. Student) should contain an Id property or a property marked with the key attribute.
```CSharp
public class Student
{
  [System.ComponentModel.DataAnnotations.Key]
  public long Id { get; set; }
  public string FirstName { get; set; }
  public string LastName { get; set; }
}
```

4. Now you can start using your database.
(Usage in razor via inject: @inject IIndexedDbFactory DbFactory)

### Adding records
```CSharp
using (var db = await this.DbFactory.Create<ContextDb>())
{
  db.Student.Add(new Student()
  {
    FirstName = "First",
    LastName = "Last"
  });
  await db.SaveChanges();
}
```
### Removing records
Note: To remove an element it is faster to use a already created reference. You should be able to also remove an object only by it's id but you have to use the .Remove(object) method (eg. .Remove(new object() { Id = 1 }))
```CSharp
using (var db = await this.DbFactory.Create<ContextDb>())
{
  var firstStudent = db.Student.First();
  db.Student.Remove(firstStudent);
  await db.SaveChanges();
}
```
### Modifying records
```CSharp
using (var db = await this.DbFactory.Create<ContextDb>())
{
  var studentWithId1 = db.Student.Single(x => x.Id == 1);
  studentWithId1.FirstName = "This is 100% a first name";
  await db.SaveChanges();
}
```

## License

Copyright (c) Johnjalani. All rights reserved.

Licensed under the [MIT](LICENSE) license.
