using Business_Core.IServices;
using Business_Core.IUnitOfWork;
using DataAccess.DataContext_Class;
using DataAccess.Services;
using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using paen_chat_app_server.SignalRChatHub;
using Presentation.AppSettings;
using Presentation.AutoMapper;
using StackExchange.Redis;
using System.Text;
using SpanJson;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddDbContextPool<DataContext>(options =>
                options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Token schema and it is used for to check the given token is valid or not. without it authorize attribute cannot work.
var key = Encoding.UTF8.GetBytes(builder.Configuration["ApplicationSettings:JWT_Secret"].ToString());
builder.Services.AddAuthentication(a =>
{
    a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    a.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => {
    x.RequireHttpsMetadata = false;
    x.SaveToken = false;
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(AutoMap));





var multiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisDbConnectionString"));
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);


// services registeration
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<IMessageRedisCacheService, MessageRedisCacheService>();
builder.Services.AddSingleton<IContactRedisCacheService, ContactRedisCacheService>();
builder.Services.AddSingleton<IUserRedisCacheService, UserRedisCacheService>();
builder.Services.AddTransient<IContactService, ContactService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IMessageService, MessageService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllHeaders",
          builder =>
          {
              builder.WithOrigins("AllowAll")
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
          });
});

builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

 
 app.UseCors("AllowAll");


app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chathub");
});
app.Run();
