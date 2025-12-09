var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("SunumIzni",
        policy =>
        {
            policy.AllowAnyOrigin()  
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});


builder.Services.AddControllers();


builder.Services.AddOpenApi();

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
}

app.UseHttpsRedirection();


app.UseCors("SunumIzni");

app.UseAuthorization();


app.MapControllers();

app.Run();