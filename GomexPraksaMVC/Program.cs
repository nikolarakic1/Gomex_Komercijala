var builder = WebApplication.CreateBuilder(args);

// Force the app to bind to the same URLs defined in launchSettings so the MVC app always listens on 7067/5261
builder.WebHost.UseUrls("https://localhost:7067", "http://localhost:5261");

builder.Services.AddControllersWithViews();

// HttpClient to call GomexPraksa API
builder.Services.AddHttpClient("GomexApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7212/");
});

builder.Services.AddHttpClient("GomexApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7212/");
});
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // timer neaktivnosti
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Ensure root (/) redirects to Dashboard index so browsing to the site root works
app.MapGet("/", () => Results.Redirect("/Dashboard/Index"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();