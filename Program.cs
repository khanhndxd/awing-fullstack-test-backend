global using awing_fullstack_test_backend.Models;
global using awing_fullstack_test_backend.Data;
global using awing_fullstack_test_backend.DTO;
global using Microsoft.EntityFrameworkCore;
using awing_fullstack_test_backend.Repositories.InputRepo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Thêm CORS để gọi api từ client không bị chặn
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
});

// Dùng in-memory database để test cho nhanh, không cần kết nối
builder.Services.AddDbContext<DataContext>(options =>
    options.UseInMemoryDatabase(builder.Configuration.GetConnectionString("InMemoryDb"))
);

builder.Services.AddScoped<IInputRepository, InputRepository>();

var app = builder.Build();

// Dùng service cors vừa tạo ở trên
app.UseCors("AllowAllOrigins");

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
