# Building a CMS-managed model

HypnoTech generates an authenticated CRUD controller, routes, list page, add/edit
forms, validation, search and ordering from one fluent model mapping. A host does not
need a controller and Razor views for every admin data type.

The complete public example is in
[`examples/HypnoTech.Demo.Models`](../examples/HypnoTech.Demo.Models). The examples
below use its fictional `Article` model.

## 1. Define the model

Use an ordinary C# class. The mapping, rather than attributes on the class, controls
how each member appears in the CMS.

```csharp
public sealed class Article
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Body { get; set; }
    public DateTime PublishedAt { get; set; }
    public string HeroImagePath { get; set; }
    public bool Featured { get; set; }
    public string Slug { get; set; }
}
```

Keep persistence names stable after data exists. Renaming a BSON element or collection
is a data migration, not merely a C# refactor.

## 2. Describe the admin page

Register a `FormClassMapping<T>` before MVC discovers controllers. The mapping below
creates the `/Articles` admin area, a searchable and sorted table, add/edit forms,
validation, image handling, duplication and slug generation.

```csharp
FormClassMapping.RegisterClassMapping<Article>(map =>
{
    map.SetDisplayName("Articles");
    map.SetControllerName("Articles");
    map.RequiresRole("articles");
    map.AddDummyDataSource("articles");
    map.AddDataSource("demoArticles");

    map.WithTitleProperty(article => article.Title);
    map.WithDefaultSort(article => article.PublishedAt, ascending: false);
    map.EnableDuplicate(cloneFiles: true);

    map.MapHiddenInputMember(article => article.Id);
    map.MapTextInputMember(article => article.Title)
        .IncludeInTable(0).Searchable().Required();
    map.MapTextAreaInputMember(article => article.Summary).Required();
    map.MapRichTextInputMember(article => article.Body).Required();
    map.MapDateInputMember(article => article.PublishedAt)
        .IncludeInTable(1).WithDateStringFormatter().Required();
    map.MapImageUploadInputMember(article => article.HeroImagePath)
        .SetImageSize(1600, 1000)
        .SetTargetDirectory("DemoArticles")
        .WithDisplayText("Hero image")
        .Required();
    map.MapCheckboxInputMember(article => article.Featured).IncludeInTable(2);
    map.MapGeneratedMember(article => article.Slug,
        article => article.Title.GenerateSlug());

    map.SetIcon("fa fa-newspaper");
});
```

The class-level calls have distinct jobs:

- `SetDisplayName` supplies the human-readable plural name.
- `SetControllerName` becomes the generated controller and route name.
- `RequiresRole` restricts the generated controller to administrators with that role.
- `AddDataSource` selects the real collection; `AddDummyDataSource` supports the
  engine's form/list conventions.
- `WithTitleProperty` identifies the record name used by admin UI and selectors.
- `WithDefaultSort` supplies list ordering when the request has no explicit sort.
- `CanBeOrdered` is the alternative for manually ordered collections.
- `EnableDuplicate(cloneFiles: true)` enables duplication and copies mapped uploads.

Member mappings determine both the input control and related behavior. Common fluent
modifiers include `Required`, `Searchable`, `IncludeInTable`, `WithDisplayText`, and
the validators exposed by that mapping type.

The current mapping families include text, textarea, rich text, date, checkbox,
money, hidden, generated and read-only members; static, repository-backed and nested
selects; image and file uploads; photo galleries; and repeatable nested lists. See
[`DemoCmsMappings.cs`](../examples/HypnoTech.Demo.Admin/DemoCmsMappings.cs) for
working examples rather than treating this list as pseudocode.

## 3. Map MongoDB persistence

The current public provider is MongoDB. Register explicit element names and the model's
collection with `ModelRegistry`:

```csharp
BsonClassMapRegistration.RegisterIfNeeded<Article>(map =>
{
    map.MapMember(model => model.Id).SetElementName("_id")
        .SetSerializer(new StringSerializer(BsonType.ObjectId));
    map.MapMember(model => model.Title).SetElementName("title");
    map.MapMember(model => model.Summary).SetElementName("summary");
    map.MapMember(model => model.Body).SetElementName("body");
    map.MapMember(model => model.PublishedAt).SetElementName("publishedAt")
        .SetSerializer(new DateTimeSerializer(BsonType.DateTime));
    map.MapMember(model => model.HeroImagePath).SetElementName("heroImagePath");
    map.MapMember(model => model.Featured).SetElementName("featured");
    map.MapMember(model => model.Slug).SetElementName("slug");
});

ModelRegistry.Register(typeof(Article), "demoArticles", "Articles");
```

`RegisterIfNeeded` makes repeated host setup safe. Use explicit BSON names so normal C#
renames do not silently change stored documents. A future persistence provider will
replace this provider-specific step while retaining the CMS form mapping.

## 4. Register the host

Register mappings before `AddCms`, then add the provider:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    DemoCmsMappings.Register();

    services.AddCms(Configuration, _ => { });
    services.UseMongoDbForCms(Configuration);

    DemoModelMappings.Map();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
{
    app.ConfigureCms(environment, Configuration);
}
```

Applications that use Common, Media or Ecommerce data register those modules and
their provider extensions as well. The complete composition is in
[`examples/HypnoTech.Demo.Admin/Startup.cs`](../examples/HypnoTech.Demo.Admin/Startup.cs).

Keep this fluent edit mapping in the Admin project. A shared model project can contain
the model classes and BSON mapping used by both applications, but the public website
should not reference `HypnoTech.Cms` or `HypnoTech.Cms.MongoDb`. Give the website small
read repositories over `HypnoTech.MongoDb` for the data it renders; this prevents admin
controllers, form metadata, and static assets from entering the public host.

Mappings are stored in process-wide registries. Register each mapping once, or use the
idempotent guard shown by `DemoCmsMappings.Register`, especially in test hosts.

## Relationships, ordering and nested data

Repository-backed selects use another registered model for their options:

```csharp
map.MapSelectListInputMember(item => item.ArtistId)
    .SetListOptions<Artist, string, string>(artist => artist.Id, artist => artist.Name)
    .WithDisplayText("Lead artist")
    .Required();
```

For manually ordered top-level records:

```csharp
map.CanBeOrdered(artist => artist.Order);
```

For an embedded list, map the collection and then its child members:

```csharp
map.MapListInputMember<ScheduleItem, List<ScheduleItem>>(item => item.Schedule)
    .MapMembers(schedule =>
    {
        schedule.MapId(entry => entry.Id);
        schedule.MapTextInputMember(entry => entry.Time, isNameField: true).Required();
        schedule.MapTextInputMember(entry => entry.Title).Required();
        schedule.CanBeOrdered(entry => entry.Order);
    })
    .SetSingularObjectName("Schedule item")
    .Required();
```

Register BSON mappings for embedded types too. The Event mapping in Lantern House also
demonstrates static selects, money, file upload, generated and read-only fields.

## Uploads

`SetTargetDirectory` is relative to the configured upload root. Admin and public hosts
must point at the same physical upload store if one application writes files and the
other serves them. The demo launcher passes one absolute `.demo/uploads` path to both
hosts for exactly this reason.

Use `SetImageSize` to request the CMS image dimensions. Use
`EnableDuplicate(cloneFiles: true)` only when duplicating a record should also copy its
mapped upload files.

## Verification checklist

Before treating a new model as complete:

1. Start the admin host and confirm list, add, edit, validation and deletion.
2. Confirm the required role can access the route and an unrelated role cannot.
3. Exercise search, explicit sorting and default sorting.
4. Exercise ordering boundaries when `CanBeOrdered` is enabled.
5. Upload, replace, duplicate and delete mapped files using a disposable upload root.
6. Inspect a stored document to confirm its collection and BSON element names.
7. Add focused characterisation tests when the mapping uses relationships, nested
   lists, galleries, generated values or other non-trivial behavior.
