using Aashirwad.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Equivalent to Flask's app = Flask(__name__) + blueprint registration + app.secret_key
builder.Services.AddControllersWithViews();

// Persist data protection keys locally inside project
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "temp_keys")));

// Register MongoDB Service as Singleton — equivalent to Python's module-level db.py collections
builder.Services.AddSingleton<MongoDbService>();

// Session configuration — equivalent to Flask's session with app.secret_key = "anything_secret_is_good"
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "SmartHMS_Session";
});

builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Equivalent to Flask's @app.errorhandler(404)
    app.UseExceptionHandler("/error/500");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Session middleware — must be before routing endpoint resolution
app.UseSession();

// Default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
