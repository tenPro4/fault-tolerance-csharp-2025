using FaultTolerancePractice.HttpClients;
using FaultTolerancePractice.Policies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddTransient<IMicroServicePolicies, MicroServicePolicies>();

builder.Services
    .AddHttpClient<WeatherServiceClient>(client =>
    {
        client.BaseAddress = new Uri($"http://localhost:5183");
    })
    .AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IMicroServicePolicies>().GetCombinedPolicy());



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
