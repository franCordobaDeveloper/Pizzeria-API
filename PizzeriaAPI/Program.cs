using PizzeriaAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.ConfigurarBaseDeDatos(builder.Configuration);
builder.Services.ConfigurarJWT(builder.Configuration);

builder.Services.ConfigurarServicios();

// Base de datos
builder.Services.ConfigurarBaseDeDatos(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("PizzeriaPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();