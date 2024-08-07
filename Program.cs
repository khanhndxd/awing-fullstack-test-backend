global using awing_fullstack_test_backend.Models;
global using awing_fullstack_test_backend.Data;
global using Microsoft.EntityFrameworkCore;
using awing_fullstack_test_backend.Repositories.InputRepo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dùng in-memory database để test cho nhanh, không cần kết nối
builder.Services.AddDbContext<DataContext>(options =>
    options.UseInMemoryDatabase(builder.Configuration.GetConnectionString("InMemoryDb"))
);

builder.Services.AddScoped<IInputRepository, InputRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
