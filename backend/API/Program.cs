using BDIP.Application.Auth;
using BDIP.Infrastructure.Auth;
using BDIP.Application.Users.Import;
using BDIP.Application.Users;
using BDIP.Infrastructure.Users;
using BDIP.Application.Units;
using BDIP.Infrastructure.Units;

using BDIP.Application.Common;
using BDIP.Infrastructure.LDAP;

using BDIP.Application.Dashboard;
using BDIP.Infrastructure.Dashboard;

using BDIP.Application.Groups;
using BDIP.Infrastructure.LDAP.Repository;

using BDIP.Application.ImportGroups;
using BDIP.Application.ImportUsers;
using BDIP.Infrastructure.ImportGroups;
using BDIP.Infrastructure.ImportUsers;
using BDIP.Persistence.PostgreSQL;
using BDIP.Persistence.Sessions;
using BDIP.Application.Sessions;
using BDIP.Application.Roles;
using BDIP.Infrastructure.Roles;
using BDIP.Application.Locations;
using BDIP.Infrastructure.Locations;
using BDIP.Application.NAP;
using BDIP.Infrastructure.NAP;
using Microsoft.AspNetCore.Routing;
using BDIP.Application.Provisioning;
using BDIP.Infrastructure.Provisioning;
using BDIP.Infrastructure.RouterOS;
using BDIP.API.Services.Sessions;

var builder = WebApplication.CreateBuilder(args);

// CORS: izinkan browser frontend BDIP mengakses Backend API.
const string BdipFrontendCorsPolicy = "BdipFrontendCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(BdipFrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://192.168.100.120:3000",
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<
    IAuthService,
    LdapAuthService>();

builder.Services.AddScoped<IUserService, PostgreSqlUserService>();
builder.Services.AddScoped<
    ILdapProvisioningService,
    UserService>();
var bdipSessionSecret =
    builder.Configuration["BdipSession:Secret"]
    ?? throw new InvalidOperationException(
        "BdipSession:Secret is not configured.");

builder.Services.AddSingleton<IBdipSessionService>(
    new BdipSessionService(bdipSessionSecret));

builder.Services.AddScoped<IUnitService, PostgreSqlUnitService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ILdapDashboardRepository, LdapDashboardRepository>();

builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IUserDnResolver, LdapUserDnResolver>();
builder.Services.AddScoped<ISynologyUserImportService, SynologyUserImportService>();
builder.Services.AddScoped<ISynologyUserUploadService, SynologyUserUploadService>();
builder.Services.AddScoped<IGroupRepository, LdapGroupRepository>();

builder.Services.AddScoped<ICsvGroupParser, CsvGroupParser>();
builder.Services.AddScoped<IGroupImportService, GroupImportService>();
builder.Services.AddScoped<ISynologyUserCsvParser, SynologyUserCsvParser>();

builder.Services.AddScoped<ILdapNumberGenerator, LdapNumberGenerator>();
builder.Services.AddScoped<ILdapConnectionFactory, LdapConnectionFactory>();
builder.Services.AddScoped<LdapUserImporter>();

builder.Services.AddScoped<IGroupMemberReader, LdapGroupMemberReader>();

builder.Services.Configure<PostgreSqlOptions>(
    builder.Configuration.GetSection("SessionsDb"));

builder.Services.Configure<ApplicationDbOptions>(
    builder.Configuration.GetSection("ApplicationDb"));    

builder.Services.Configure<RadiusDbOptions>(
    builder.Configuration.GetSection("RadiusDb"));

builder.Services.Configure<RouterOsOptions>(
    builder.Configuration.GetSection("RouterOs"));

builder.Services.AddScoped<ISessionService, PostgreSqlSessionService>();
builder.Services.AddScoped<IRoleService, LdapRoleService>();
builder.Services.AddScoped<ILocationService, PostgreSqlLocationService>();
builder.Services.AddScoped<IPolicyService, PostgreSqlPolicyService>();
builder.Services.AddScoped<IUserNapService, PostgreSqlUserNapService>();
builder.Services.AddScoped<ISessionService, PostgreSqlSessionService>();
builder.Services.AddScoped<IRoleService, LdapRoleService>();
builder.Services.AddScoped<ILocationService, PostgreSqlLocationService>();
builder.Services.AddScoped<IPolicyService, PostgreSqlPolicyService>();
builder.Services.AddScoped<INapSynchronizationService, NapSynchronizationService>();

builder.Services.AddScoped<
    IRadiusProvisioningService,
    PostgreSqlRadiusProvisioningService>();

builder.Services.AddScoped<
    IRouterOsService,
    RouterOsService>();

builder.Services.AddScoped<
    UnifiedSessionService>();

builder.Services.Configure<LdapOptions>(
    builder.Configuration.GetSection("Ldap"));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

Console.WriteLine("STEP 1");

var app = builder.Build();

Console.WriteLine("STEP 2");

if (args.Length > 0 &&
    args[0].Equals("ldap-import",
        StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("STEP 3");

    using var scope = app.Services.CreateScope();

    var importer =
        scope.ServiceProvider
             .GetRequiredService<LdapUserImporter>();

    var imported = await importer.ImportAsync();

    Console.WriteLine($"Imported : {imported}");

    Console.WriteLine("STEP 4");

    return;
}

Console.WriteLine("STEP 5");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// CORS harus berada sebelum authorization dan endpoint controller.
// CORS harus berada sebelum authorization dan endpoint controller.
app.UseCors(BdipFrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();

foreach (var endpoint in endpointDataSource.Endpoints)
{
    Console.WriteLine(endpoint.DisplayName);
}

app.Run();
