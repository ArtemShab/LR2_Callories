using LR2_Callories.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// якщо ключа немає працює заглушка, щоб проект запускався без налаштувань
var aiKey = builder.Configuration["Ai:ApiKey"];
if (string.IsNullOrWhiteSpace(aiKey))
{
    builder.Services.AddScoped<IFoodAnalysisService, MockFoodAnalysisService>();
}
else
{
    builder.Services.Configure<AiOptions>(builder.Configuration.GetSection("Ai"));
    builder.Services.AddHttpClient<IFoodAnalysisService, OpenAiFoodAnalysisService>((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value;

        // BaseAddress має закінчуватись слешем, інакше відносний шлях "chat/completions" загубиться
        var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/";
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 120);
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
