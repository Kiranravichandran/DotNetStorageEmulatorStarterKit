using Microsoft.Extensions.Azure;
using Microsoft.OpenApi.Models;
using static Azure.Core.HttpHeader;


Console.WriteLine("Azure Table Storage - Getting Started Samples\n");
var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.
//builder.Services.AddSingleton(x => Common.CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString")));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TableStorage API", Version = "v1" });
});

var app = builder.Build();

// Configure middlewares
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TableStorage API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
app.MapGet("/customers", async () =>
{
    var table = await Common.CreateTableAsync("Customers");
    var query = new TableQuery<CustomerEntity>();
    var segment = await table.ExecuteQuerySegmentedAsync(query, null);
    return Results.Ok(segment.Results);
});
