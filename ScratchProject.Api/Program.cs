using ScratchProject.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder = builder.ConfigureServices();

var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseHsts();
app.UseHttpsRedirection();

app.UseSwagger(); // creates endpoints for swagger.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
}); // creates swagger UI for testing all web API endpoints / action methods

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
