using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models.Contexts;
using Order.API.ViewModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL"));
});

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context,_configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
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

app.UseAuthorization();

app.MapControllers();

app.Run();
