using WEB.Filters;
using WEB.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC con filtro global de login
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<RequireLoginAttribute>();
});

// HttpClient para llamar a la API
builder.Services.AddHttpClient("SIGESA_API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]
        ?? "https://localhost:7001");
});

builder.Services.AddScoped<IUtilitario, Utilitario>();
builder.Services.AddScoped<IRestProvider, RestProvider>();

// Sesion
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".SIGESA.Session";
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();