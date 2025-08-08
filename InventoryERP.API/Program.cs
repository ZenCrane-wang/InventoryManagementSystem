using System.Text;
using System.Reflection;  // 添加这行
using InventoryERP.Infrastructure;
using InventoryERP.Infrastructure.Repositories;
using InventoryERP.Infrastructure.Services;
using InventoryERP.Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(conn, ServerVersion.AutoDetect(conn))
);

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 注册用户、角色和权限服务
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// 注册JWT配置
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // 添加这行以支持Swagger
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "InventoryERP API",
        Version = "v1",
        Description = "InventoryERP系统API接口"
    });
    // 启用XML注释（如果文件存在）
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Ensure database is created with all tables
    db.Database.EnsureDeleted(); // First delete existing database to start fresh
    db.Database.EnsureCreated();
    
    // Add some sample data if the Products table is empty
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new InventoryERP.Infrastructure.Entities.Product
            {
                Code = "P001",
                Name = "Sample Product 1",
                Unit = "Piece",
                Price = 10.99m,
                Stock = 100
            },
            new InventoryERP.Infrastructure.Entities.Product
            {
                Code = "P002",
                Name = "Sample Product 2",
                Unit = "Box",
                Price = 25.50m,
                Stock = 50
            }
        );
        db.SaveChanges();
    }
    
    // 添加默认角色和权限
    if (!db.Roles.Any())
    {
        // 添加默认角色
        var adminRole = new InventoryERP.Infrastructure.Entities.Role
        {
            Name = "Admin",
            Description = "系统管理员，拥有所有权限"
        };
        
        var userRole = new InventoryERP.Infrastructure.Entities.Role
        {
            Name = "User",
            Description = "普通用户，拥有基本操作权限"
        };
        
        db.Roles.AddRange(adminRole, userRole);
        db.SaveChanges();
        
        // 添加默认权限
        var permissions = new List<InventoryERP.Infrastructure.Entities.Permission>
        {
            // 用户模块权限
            new InventoryERP.Infrastructure.Entities.Permission { Name = "查看用户", Description = "查看用户列表和详情", Module = "User", Action = "View" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "创建用户", Description = "创建新用户", Module = "User", Action = "Create" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "编辑用户", Description = "编辑用户信息", Module = "User", Action = "Edit" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "删除用户", Description = "删除用户", Module = "User", Action = "Delete" },
            
            // 产品模块权限
            new InventoryERP.Infrastructure.Entities.Permission { Name = "查看产品", Description = "查看产品列表和详情", Module = "Product", Action = "View" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "创建产品", Description = "创建新产品", Module = "Product", Action = "Create" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "编辑产品", Description = "编辑产品信息", Module = "Product", Action = "Edit" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "删除产品", Description = "删除产品", Module = "Product", Action = "Delete" },
            
            // 供应商模块权限
            new InventoryERP.Infrastructure.Entities.Permission { Name = "查看供应商", Description = "查看供应商列表和详情", Module = "Supplier", Action = "View" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "创建供应商", Description = "创建新供应商", Module = "Supplier", Action = "Create" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "编辑供应商", Description = "编辑供应商信息", Module = "Supplier", Action = "Edit" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "删除供应商", Description = "删除供应商", Module = "Supplier", Action = "Delete" },
            
            // 采购订单模块权限
            new InventoryERP.Infrastructure.Entities.Permission { Name = "查看采购订单", Description = "查看采购订单列表和详情", Module = "PurchaseOrder", Action = "View" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "创建采购订单", Description = "创建新采购订单", Module = "PurchaseOrder", Action = "Create" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "编辑采购订单", Description = "编辑采购订单信息", Module = "PurchaseOrder", Action = "Edit" },
            new InventoryERP.Infrastructure.Entities.Permission { Name = "删除采购订单", Description = "删除采购订单", Module = "PurchaseOrder", Action = "Delete" }
        };
        
        db.Permissions.AddRange(permissions);
        db.SaveChanges();
        
        // 为Admin角色分配所有权限
        var allPermissions = db.Permissions.ToList();
        foreach (var permission in allPermissions)
        {
            db.RolePermissions.Add(new InventoryERP.Infrastructure.Entities.RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            });
        }
        
        // 为User角色分配查看权限
        var viewPermissions = allPermissions.Where(p => p.Action == "View").ToList();
        foreach (var permission in viewPermissions)
        {
            db.RolePermissions.Add(new InventoryERP.Infrastructure.Entities.RolePermission
            {
                RoleId = userRole.Id,
                PermissionId = permission.Id
            });
        }
        
        db.SaveChanges();
        
        // 创建默认管理员用户
        var salt = PasswordHasher.GenerateSalt();
        var passwordHash = PasswordHasher.HashPassword("Admin123!", salt);
        
        var adminUser = new InventoryERP.Infrastructure.Entities.User
        {
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = passwordHash,
            PasswordSalt = salt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        db.Users.Add(adminUser);
        db.SaveChanges();
        
        // 为管理员用户分配Admin角色
        db.UserRoles.Add(new InventoryERP.Infrastructure.Entities.UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        });
        
        db.SaveChanges();
    }
}

// 无论是开发环境还是生产环境，都启用Swagger
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "InventoryERP API V1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "InventoryERP API",
        Version = "v1",
        Description = "InventoryERP系统API接口"
    });
    // 启用XML注释（如果文件存在）
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});
