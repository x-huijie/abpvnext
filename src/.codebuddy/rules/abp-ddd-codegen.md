---
description: Xhj.Project 项目的代码生成规范，基于 ABP vNext 框架 + DDD 领域驱动设计。新增实体、应用服务、DTO、权限、API、持久化代码时必须遵守。
globs: ["**/*.cs", "**/*.json", "**/*.csproj"]
alwaysApply: true
---

# Xhj.Project 代码生成规范（ABP vNext + DDD）

## 0. 技术基线（不得擅自更改）

| 项 | 值 |
|---|---|
| 框架 | ABP vNext（ABP.IO）`10.6.1` |
| 运行时 | `net10.0` |
| ORM | EF Core `10.0.x` + SQL Server |
| 对象映射 | **Mapperly**（`Volo.Abp.Mapperly`），禁用 AutoMapper |
| 多租户 | 已启用（`MultiTenancyConsts.IsEnabled = true`） |
| 表前缀 | `ProjectConsts.DbTablePrefix = "App"`，`DbSchema = null` |
| 根命名空间 | `Xhj.Project` |
| 语言版本 | `LangVersion=latest`、`Nullable=enable`、隐式 `using`、file-scoped namespace |
| 认证 | OpenIddict（AuthServer） + JWT Bearer；动态 Claim（`IsDynamicClaimsEnabled = true`） |
| 缓存/锁 | Redis（`DistributedCacheOptions.KeyPrefix = "Project:"`） |

所有新代码必须遵循：`<Nullable>enable</Nullable>`、file-scoped `namespace Xxx;`、无 `using` 冗余导入、属性优先 `required` 或 `?` 显式可空。

## 1. 分层结构与依赖方向

```
Domain.Shared  ←  Domain  ←  Application.Contracts  ←  Application  ←  HttpApi  ←  HttpApi.Host
                     ↑                                      ↑
              EntityFrameworkCore  ←─────────────────────────┘（仅 Host/DbMigrator 直接依赖）
```

| 项目 | 只允许放 | 禁止 |
|---|---|---|
| `Xhj.Project.Domain.Shared` | 常量、枚举、静态 Consts、错误码（ `ProjectDomainErrorCodes` ）、本地化资源 `ProjectResource` + `Localization/Project/*.json` 、`MultiTenancyConsts` | 任何业务实体/服务 |
| `Xhj.Project.Domain` | 聚合根/实体/值对象、领域服务（`DomainService`）、仓储接口、领域事件、Settings 定义 | 引用 Application / EF Core / ASP.NET Core |
| `Xhj.Project.EntityFrameworkCore` | `ProjectDbContext`、`IEntityTypeConfiguration<T>`、仓储实现、`Migrations` | 业务逻辑 |
| `Xhj.Project.Application.Contracts` | `IXxxAppService` 接口、DTO、权限常量 `ProjectPermissions` + `ProjectPermissionDefinitionProvider` | 实现、EF 依赖 |
| `Xhj.Project.Application` | `XxxAppService` 实现、`XxxMapper`（Mapperly） | 控制器、`HttpContext`、EF `DbContext` 直接操作 |
| `Xhj.Project.HttpApi` | 仅在非约定式 API 时添加 Controller | 业务逻辑 |
| `Xhj.Project.HttpApi.Host` | 宿主配置、模块编排 | 业务代码 |

**严禁反向依赖**：Domain 不得 `using` Application 或 EF Core；Application 不得引用 `HttpApi.Host`。

## 2. 命名空间约定

统一 `Xhj.Project.<关注点>`：

- `Xhj.Project.Permissions`、`Xhj.Project.Localization`、`Xhj.Project.MultiTenancy`、`Xhj.Project.Settings`、`Xhj.Project.Data`
- `Xhj.Project.EntityFrameworkCore`（DbContext 例外，为 `Xhj.Project.EntityFrameworkCore` 目录下 `Xhj.Project.EntityFrameworkCore` 命名空间）
- 业务聚合：`Xhj.Project.<业务模块名>`（如 `Xhj.Project.Products`），其下按 `Entities / DomainServices / Events` 分子目录时保持命名空间跟随目录。

## 3. 领域层（DDD）

### 3.1 聚合根

```csharp
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Xhj.Project.Products;

public class Product : FullAuditedAggregateRoot<Guid>
{
    public virtual string Name { get; private set; }
    public virtual decimal Price { get; private set; }
    public virtual bool IsActive { get; private set; }

    protected Product() { } // 仅 ORM 使用

    public Product(Guid id, string name, decimal price) : base(id)
    {
        SetName(name);
        SetPrice(price);
        IsActive = true;
    }

    public virtual Product SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), ProductConsts.MaxNameLength);
        Name = name;
        return this;
    }

    public virtual void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new BusinessException(ProjectDomainErrorCodes.ProductPriceInvalid);
        }
        Price = price;
    }
}
```

规则：

- 聚合根继承 `AggregateRoot<Guid>` / `FullAuditedAggregateRoot<Guid>` / `AuditedAggregateRoot<Guid>`；子实体继承 `Entity<Guid>`，**不得**继承 `AggregateRoot`。
- 属性 `set` 一律 `private`/`protected`，通过行为方法（`ChangeXxx`、`SetXxx`、`Activate()`）修改状态。
- 构造函数最小必备：`protected Xxx() { }` 给 ORM，另一个 public 只接收**必需**参数并进行校验。
- 不变式（业务规则）写在实体内部；跨聚合协调才引入领域服务。
- 聚合之间**只通过 Id 引用**（`Guid CategoryId`），不用导航属性跨聚合。
- 需要软删除用 `ISoftDelete`，需要多租户用 `IMultiTenant`（ABP 自动按 `MultiTenancyConsts.IsEnabled` 过滤）。

### 3.2 值对象

不可变、无 Id、按值相等；优先 `record` 或只读属性类。

```csharp
public record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);
}
```

### 3.3 领域服务

仅在以下情况创建（放 `Domain/Services` 或业务目录下）：

- 逻辑跨多个聚合；
- 需要仓储/inject 其他基础设施，但不应放进 AppService。

```csharp
public class ProductManager : DomainService
{
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductManager(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Product> CreateAsync(string name, decimal price, Category category)
    {
        await CheckDuplicateNameAsync(name);
        return await _productRepository.InsertAsync(new Product(GuidGenerator.Create(), name, price));
    }
}
```

命名后缀：`Manager` / `DomainService`。

### 3.4 领域事件

- 进程内：继承 `Entity` 并在实体中 `AddLocalEvent(new ProductCreatedEto { ... })`；Handler 实现 `ILocalEventHandler<ProductCreatedEto>`。
- 跨服务：事件对象以 `Eto` 结尾，实现 `IDistributedEventHandler<TEto>`。
- 散播时机：实体内只添加事件，由 AppService/UoW 提交时统一触发。

### 3.5 错误码与本地化

```csharp
// Domain.Shared/ProjectDomainErrorCodes.cs
public static class ProjectDomainErrorCodes
{
    public const string ProductPriceInvalid = "Project:001";
    public const string ProductNameAlreadyExists = "Project:002";
}
```

- 异常码必须以 `Project:` 前缀开头（已通过 `MapCodeNamespace("Project", typeof(ProjectResource))` 映射）。
- 抛出：`throw new BusinessException(ProjectDomainErrorCodes.ProductPriceInvalid)`。
- **禁止硬编码中文/英文提示字符串**，全部本地化：在 `Domain.Shared/Localization/Project/<culture>.json` 中补充键。

## 4. 应用层

### 4.1 接口与实现

`Application.Contracts/Products/IProductAppService.cs`：

```csharp
public interface IProductAppService :
    ICrudAppService<ProductDto, Guid, GetProductListInput, CreateUpdateProductDto>
{
}
```

`Application/Products/ProductAppService.cs`：

```csharp
using Volo.Abp.Application.Services;
using Xhj.Project.Permissions;

namespace Xhj.Project.Products;

[Authorize(ProjectPermissions.Products.Default)]
public class ProductAppService :
    CrudAppService<Product, ProductDto, Guid, GetProductListInput, CreateUpdateProductDto>,
    IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
    }

    [Authorize(ProjectPermissions.Products.Create)]
    public override Task<ProductDto> CreateAsync(CreateUpdateProductDto input)
    {
        return base.CreateAsync(input);
    }
}
```

要求：

- 所有 AppService 继承 `ProjectAppService`（内含本地化资源），并实现 `IXxxAppService`。
- 命名：`XxxAppService`；方法异步一律 `Async` 后缀。
- 已开启 `AddDefaultRepositories(includeAllEntities: true)`，可直接注入 `IRepository<TEntity, TKey>`。
- 复杂查询用 `Repository.GetQueryableAsync()` + 扩展法，禁止在 AppService 内直接 `await SaveChangesAsync()`（由 UoW 统一提交）；需要显式控制时标注 `[UnitOfWork]`。
- 写操作必须有权限校验；类级 `[Authorize]` + 方法级覆盖。
- 业务逻辑（不变式）下沉到 Domain；AppService 只做编排、权限、映射、调用。

### 4.2 DTO

放 `Application.Contracts/Products/`：

- 输出：`ProductDto`（带审计字段则继承 `FullAuditedEntityDto<Guid>`）。
- 输入：`CreateUpdateProductDto`、`GetProductListInput : PagedAndSortedResultRequestDto`。
- 校验用 DataAnnotations（`[Required]`、`[StringLength(ProductConsts.MaxNameLength)]`），复杂交叉校验实现 `IValidatableObject`。
- 简单长度/约束常量放 `Domain.Shared` 的 `XxxConsts`，DTO 与实体共享。

### 4.3 对象映射（Mapperly）

不使用 AutoMapper。`Application/ProjectApplicationMappers.cs` 中扩展：

```csharp
[Mapper]
public partial class ProjectApplicationMappers
{
    [MapProperty(nameof(Product.Name), nameof(ProductDto.Title))] // 仅字段名不一致时写
    public partial ProductDto MapToDto(Product source);
    public partial void MapToEntity(CreateUpdateProductDto source, Product destination);
}
```

按业务模块可拆多个 `[Mapper]` 分部类，但**必须保持 `[Mapper]` 与 `partial`**。

## 5. 基础设施层（EF Core）

DbContext：

```csharp
// ProjectDbContext.cs
public DbSet<Product> Products { get; set; }
```

实体配置优先用独立配置类：

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable(ProjectConsts.DbTablePrefix + "Products", ProjectConsts.DbSchema);
        b.ConfigureByConvention();
        b.Property(x => x.Name).IsRequired().HasMaxLength(ProductConsts.MaxNameLength);
        b.HasIndex(x => x.Name);
    }
}
```

规则：

- 表名必须 `ProjectConsts.DbTablePrefix + <复数名>`，且必须调用 `b.ConfigureByConvention()`。
- 值对象一律 `b.OwnsOne(...)` 或 `HasConversion`（不要让值对象成为独立实体）。
- 新增/修改实体后必须：
  1. `dotnet ef migrations add <Name>`（在 `Xhj.Project.EntityFrameworkCore` 目录）；
  2. 运行 `Xhj.Project.DbMigrator` 应用迁移。
- 种子数据放 `IDataSeedContributor`（参照 `OpenIddictDataSeedContributor`）。

## 6. 表现层（HttpApi）

- **优先使用 ABP 约定式 API**：AppService 会被自动发布为 REST 控制器（`options.ConventionalControllers.Create(typeof(ProjectApplicationModule).Assembly)` 已配置），一般不需要手写 Controller。
- 确需手写控制器时继承 `ProjectController`，路由 `[Route("api/app/xxx")]`，且控制器内不写业务。
- 所有新权限必须在 Swagger 可见（`options.DocInclusionPredicate((docName, description) => true)` 已开启）。

## 7. 权限

```csharp
// Application.Contracts/Permissions/ProjectPermissions.cs
public static class ProjectPermissions
{
    public const string GroupName = "Project";

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
```

同时在 `ProjectPermissionDefinitionProvider.Define` 中注册，并在 `Localization/Project/*.json`（至少 `en.json` 与 `zh-Hans.json`）添加 `"Permission:Products"`、`"Permission:Products.Create"` 等显示名。

## 8. 本地化清单

新增任何用户可见文本（权限名、菜单、异常消息、枚举显示名）时同步修改：

```
Xhj.Project.Domain.Shared/Localization/Project/en.json
Xhj.Project.Domain.Shared/Localization/Project/zh-Hans.json
```

（该项目已启用多语言：ar/cs/en/en-GB/hu/hr/fi/fr/hi/it/pt-BR/ru/sk/tr/zh-Hans/zh-Hant/de-DE/es，至少补齐中英两项。）

## 9. 通用禁止项

1. 禁止在 Domain 层出现 `DbContext`、`HttpContext`、`IHttpContextAccessor`、`ICurrentUser` 之外的应用层类型。
2. 禁止在 AppService 中写业务不变式判断，必须下沉至实体/领域服务。
3. 禁止使用 AutoMapper（`Profile`/`CreateMap`），一律 Mapperly。
4. 禁止硬编码表名/Schema，统一走 `ProjectConsts`。
5. 禁止在实体中注入仓储；禁止直接 new AppService/DomainService，用构造函数注入。
6. 禁止吞异常（`catch { }`）；异常一律用 `BusinessException` + 错误码。
7. 禁止 `.Result` / `.Wait()` 阻塞异步；返回 `Task` 而非 `void`。
8. 禁止硬编码租户 Id；多租户数据隔离依赖 ABP 自动过滤。
9. 不要生成完整 giant file 覆盖已有文件；优先局部改动并保留现有注释与模块注册。

## 10. 新功能落地的标准步骤（Checklist）

1. `Domain.Shared`：新增 `XxxConsts`、枚举、`ProjectDomainErrorCodes` 错误码、本地化键。
2. `Domain`：实体/聚合根 + 值对象 + 必要领域服务；注册到 `Domain` 命名空间。
3. `EntityFrameworkCore`：`DbSet` + `IEntityTypeConfiguration<T>`；`dotnet ef migrations add`；跑 `DbMigrator`。
4. `Application.Contracts`：`IXxxAppService`、Dto/输入、`ProjectPermissions` + Provider 注册、本地化。
5. `Application`：`XxxAppService`（继承 `ProjectAppService`）+ Mapperly 映射 + `[Authorize]`。
6. `HttpApi`：默认无需改动（约定式 API 自动暴露）。
7. 编译验证：`dotnet build .\Xhj.Project.HttpApi.Host\Xhj.Project.HttpApi.Host.csproj`。

## 11. 注释与风格

- 中文 XML doc 摘要仅对 public API 必要处添加（`CS1591` 已忽略，不需要到处写 `///`）。
- 关键业务规则、非直觉写法必须写简短中文行内注释。
- 使用 `Guard` 风格校验：`Check.NotNullOrWhiteSpace(...)`、`ObjectHelper.GetValueOrDefault(...)`。
- 属性/方法上一律 `virtual`（便于 EF 代理与测试替换），内部调用亦然。
