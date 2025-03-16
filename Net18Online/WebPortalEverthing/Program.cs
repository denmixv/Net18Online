using Everything.Data;
using Everything.Data.Fake.Repositories;
using Everything.Data.Interface.Repositories;
using Everything.Data.Repositories;
using MazeCore.Builders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SimulatorOfPrinting.Models;
using W.Services.Services.Apis;
using WebPortalEverthing.CustomMiddlewares;
using WebPortalEverthing.Hubs;
using WebPortalEverthing.Providers;
using WebPortalEverthing.Services;
using WebPortalEverthing.Services.Apis;
using WebPortalEverthing.Services.BackgroundServices;
using WebPortalEverthing.Services.LoadTesting;
using TypeOfApplianceRepository = Everything.Data.Repositories.TypeOfApplianceRepository;
using TagField = TagGame.Classes.Base.Field;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(AuthService.AUTH_TYPE_KEY)
    .AddCookie(AuthService.AUTH_TYPE_KEY, config =>
    {
        config.LoginPath = "/SurveysAuth/Login";
        config.AccessDeniedPath = "/SurveysAuth/Forbidden";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<WebDbContext>(x =>
    x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

// Register in DI container services/repository for ServiceCenter
builder.Services.AddScoped<ITypeOfApplianceRepositoryReal, TypeOfApplianceRepository>();
builder.Services.AddSingleton<IDNDRepository, DNDRepository>();
builder.Services.AddSingleton<IChessPartiesRepository, ChessPartiesRepository>();
builder.Services.AddSingleton<IGameLifeRepository, GameLifeRepository>();

var registrationHelper = new RegistrationHelper();
registrationHelper.AutoRegisterRepositories(builder.Services);

//builder.Services
//    .AddAuthentication(LoadAuthService.AUTH_TYPE_KEY)
//    .AddCookie(LoadAuthService.AUTH_TYPE_KEY, config =>
//    {
//        config.LoginPath = "/LoadAuth/LoginLoadUserView";
//        config.AccessDeniedPath = "/Home/Forbidden";
//    });

builder.Services.AddScoped<HelperForValidatingCake>();
builder.Services.AddScoped<TextProvider>();
builder.Services.AddScoped<MazeBuilder>();
builder.Services.AddScoped<LoadAuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LoadUserService>();
builder.Services.AddScoped<LoadVolumeService>();
builder.Services.AddScoped<FileProvider>();
builder.Services.AddScoped<HelperForFile>();
builder.Services.AddScoped<TagGameHelper>();
builder.Services.AddScoped<TagField>();

builder.Services.AddHttpClient<HttpNumberApi>(httpClient =>
    httpClient.BaseAddress = new Uri("http://numbersapi.com/")
    );
builder.Services.AddHttpClient<HttpWoofApi>(httpClient =>
    httpClient.BaseAddress = new Uri("https://random.dog/")
    );
builder.Services.AddHttpClient<HttpJokeApi>(httpClient =>
    httpClient.BaseAddress = new Uri("https://official-joke-api.appspot.com/random_joke")
    );

builder.Services.AddHttpClient<WeatherApi>(httpClient =>
    httpClient.BaseAddress = new Uri("https://api.open-meteo.com/v1/")
    );

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services.AddHostedService<NotificationExparator>();

registrationHelper.AutoRegisterServiceByAttribute(builder.Services);
registrationHelper.AutoRegisterServiceByAttributeOnConstructor(builder.Services);

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(option =>
{
    option.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.SetIsOriginAllowed(origin => true);
        policy.AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();

// Load data into repository from JSON file
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var typeOfApplianceRepo = scope.ServiceProvider.GetRequiredService<ITypeOfApplianceRepositoryReal>();
    var jsonFilePath = Path.Combine(app.Environment.ContentRootPath, "Data", "ServiceCenter", "typeOfAppliance.json");
    typeOfApplianceRepo.LoadDataFromJson(jsonFilePath);
}

var seed = new Seed();
seed.Fill(app.Services);

var loadTestingseed = new LoadTestingSeed();
loadTestingseed.Fill(app.Services);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Who Am I?
app.UseAuthorization(); // May I?

app.UseMiddleware<CustomLocalizationMiddleware>();
app.UseMiddleware<CustomThemeMiddleware>();

app.MapHub<ChatHub>("/hub/chatMainPage");
app.MapHub<CoffeShopChatHub>("/hub/chatCoffePage");
app.MapHub<GameAlertHub>("/hub/alertGamePage");
app.MapHub<TakingSurveyHub>("/hub/takingSurvey");
app.MapHub<LoadChatHub>("/hub/loadChat");
app.MapHub<NotificationHub>("/hub/notification");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
