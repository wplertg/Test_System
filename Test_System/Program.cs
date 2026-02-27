using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models;
using Serilog;
using System.Text;
using Tools.Common;
using Tools.Extensions;
using Tools.JWT;
using Tools.middleware;
using Tools.SignalRHub;
using Tools.SignalRHub.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();   // ? 必须
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7   // 只保留最近 7 天
    )
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            )
        };
    });
builder.Services.AddVisitorQuartzJobs();
builder.Services.AddScoped<ExcelHelper>();
builder.Services.AddScoped<ImageHelper>();
builder.Services.AddScoped<JwtTokenHelper>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTcpClients();
builder.Services.AddSerialPorts();

builder.Services.AddSignalR();
builder.Services.AddSingleton<DeviceNotifyService>();

var app = builder.Build();

// 1. 配置 ForwardedHeadersOptions
// 这一步告诉 ASP.NET Core：信任代理发来的头，把它们当成真实 IP
var forwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};

// ⚠️ 注意：如果你在 Docker 或内网运行，默认的安全策略可能不信任你的代理
// 在开发环境或确认安全的内网中，可以清空限制列表以信任所有代理：
forwardedHeaderOptions.KnownNetworks.Clear();
forwardedHeaderOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeaderOptions);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ExceptionMiddleware>();
app.MapHub<DeviceHub>("/hubs/device");
app.Run();
