using FaultTolerancePractice.HttpClients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<WeatherServiceClient>(client =>
{
    client.BaseAddress = new Uri($"http://localhost:5183");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Cors
app.UseCors();

//Endpoints
app.MapControllers();

app.Run();
