using Tailor.Data.Persistence;
using Tailor.Data.UnitOfWork;
using Tailor.Interfaces;
using Tailor.Domain.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Stripe;
using Tailor.Domain.Utilities;
using Tailor.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//DbContext configuration
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
);

// Bind stripe settings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

// Bind EmailSender settings
builder.Services.Configure<EmailSenderSettings>(builder.Configuration.GetSection("EmailSenderSettings"));

// Bind Twilio settings
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));
 
// Authentication and authorization
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(/*options => options.SignIn.RequireConfirmedAccount = true*/)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();
builder.Services.AddAuthentication();
    //.AddFacebook(facebookOptions =>
    //{
    //    facebookOptions.AppId = builder.Configuration.GetSection("FaceTailorettings:AppId").Get<string>();
    //    facebookOptions.AppSecret = builder.Configuration.GetSection("FaceTailorettings:AppSecret").Get<string>();
    //})
    //.AddGoogle(googleOptions =>
    //{
    //    googleOptions.ClientId = builder.Configuration.GetSection("GoogleSettings:ClientId").Get<string>();
    //    googleOptions.ClientSecret = builder.Configuration.GetSection("GoogleSettings:ClientSecret").Get<string>();
    //});

//Services configuration
builder.Services.AddScoped<ApplicationUser>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
// Batabase initializer
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath= $"/Identity/Account/AccessDenied";

});

// Session cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Fixing the error "A possible object cycle was detected"
builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Getting rid of code that mapped one object to another.
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddApiVersioning();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tailor", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true;
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tailor API V1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

// Stripe global pipeline
StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings:SecretKey").Get<string>();

//Invoke function to seed database
SeedDatabase();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}"
    );
});

app.Run();

// Seed Database
void SeedDatabase()
{
    using var scope = app.Services.CreateAsyncScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    initializer.Initialize();
}