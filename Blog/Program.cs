using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Identity
builder.Services.AddIdentity<BlogUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<BlogIdentityDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<BlogUser>, AdditionalUserClaimsPrincipalFactory>();
builder.Services.AddDbContext<BlogIdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlDb")));
// End of Identity

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ISqlBlogData, SqlBlogData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "blog",
    pattern: "{controller=Blog}/{blogname}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();