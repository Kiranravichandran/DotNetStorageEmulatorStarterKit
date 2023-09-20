using Microsoft.Azure;
using Microsoft.OpenApi.Models;
using Microsoft.WindowsAzure.Storage.Table;

Console.WriteLine("Azure Table Storage - Getting Started Samples\n");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(x => Common.CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString")));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TableStorage API", Version = "v1" });
});

var app = builder.Build();

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
