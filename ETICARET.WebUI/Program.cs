using ETICARET.Business.Abstract;
using ETICARET.Business.Concrete;
using ETICARET.DataAccess.Abstract;
using ETICARET.DataAccess.Concrete.EfCore;
using ETICARET.WebUI.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();


// Seed Identitys
var userManager = builder.Services.BuildServiceProvider().GetService<UserManager<ApplicationUser>>();
var roleManager = builder.Services.BuildServiceProvider().GetService<RoleManager<IdentityRole>>();


builder.Services.Configure<IdentityOptions>(
    options =>
    {
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 0;


        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedEmail = true;
    }

);


/// Cookie configure

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.AccessDeniedPath = "/Account/accessdenied";
        options.LoginPath = "/Account/login";
        options.LogoutPath = "/Account/logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie = new CookieBuilder()
        {
            HttpOnly = true,
            Name = "ETICARET.Security.Cookie",
            SameSite = SameSiteMode.Strict
        };
    }

);

// Dataacces ve Business

builder.Services.AddScoped<IProductDal, EfCoreProductDal>();
builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<ICategoryDal, EfCoreCategoryDal>();
builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<ICommentDal, EfCoreCommentDal>();
builder.Services.AddScoped<ICommentService, CommentManager>();
builder.Services.AddScoped<ICartDal, EfCoreCartDal>();
builder.Services.AddScoped<ICartService, CartManager>();
builder.Services.AddScoped<IOrderDal, EfCoreOrderDal>();
builder.Services.AddScoped<IOrderService, OrderManager>();

// Projeyi MVC mimarisinde çalýþtýr

builder.Services.AddMvcCore().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Latest);
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




app.UseStaticFiles();

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthentication();


app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");

        endpoints.MapControllerRoute(
            name: "product",
            pattern: "products/{category?}",
            defaults: new { controller = "Shop", Action = "List" }
        );

        endpoints.MapControllerRoute(
           name: "adminProducts",
           pattern: "admin/products",
           defaults: new { controller = "Admin", Action = "ProductList" }
       );

        endpoints.MapControllerRoute(
           name: "adminProducts",
           pattern: "admin/products/{id?}",
           defaults: new { controller = "Admin", Action = "EditProduct" }
       );

        endpoints.MapControllerRoute(
          name: "adminProducts",
          pattern: "admin/categories",
          defaults: new { controller = "Admin", Action = "CategoryList" }
      );

        endpoints.MapControllerRoute(
         name: "adminProducts",
         pattern: "admin/categories/{id?}",
         defaults: new { controller = "Admin", Action = "EditCategory" }
     );

        endpoints.MapControllerRoute(
           name: "adminProducts",
           pattern: "admin/categories/{id?}",
           defaults: new { controller = "Admin", Action = "EditCategory" }
        );

        endpoints.MapControllerRoute(
          name: "cart",
          pattern: "cart",
          defaults: new { controller = "Cart", Action = "Index" }
       );

        endpoints.MapControllerRoute(
         name: "checkout",
         pattern: "checkout",
         defaults: new { controller = "Cart", Action = "Checkout" }
      );

        endpoints.MapControllerRoute(
        name: "orders",
        pattern: "orders",
        defaults: new { controller = "Cart", Action = "GetOrders" }
     );
    }

);



// Seed Identity
SeedIdentity.Seed(userManager, roleManager, app.Configuration).Wait();


// Seed Product-Category-ProductCategory 
SeedDatabase.Seed();

app.Run();
