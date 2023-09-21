using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using signalR.Hubs;
using signalR.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

//Add Services
builder.Services.AddSingleton<ChatService>();

builder.Services.AddSignalR();
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    //.AllowCredentials()
    .AllowAnyOrigin());
    //.WithOrigins("https://www.jmr24.com"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//Add Hubs
app.MapHub<ChatHubs>("/hubs/chat");

app.Run();
