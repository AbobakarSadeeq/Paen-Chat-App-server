using DataAccess.DataContext_Class;
using Microsoft.EntityFrameworkCore;
using Presentation.AppSettings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddDbContextPool<DataContext>(options =>
                options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// connecting client
builder.Services.AddCors(opt => {
    opt.AddDefaultPolicy(builder => {
        builder.AllowAnyOrigin();
        builder.AllowAnyMethod();
        builder.AllowAnyHeader();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors(a => {
    a.WithOrigins(builder.Configuration.GetValue<string>("Client_URL"))
        .AllowAnyHeader()
        .AllowAnyMethod();
});

app.UseRouting();


app.UseAuthorization();

app.MapControllers();

app.Run();
