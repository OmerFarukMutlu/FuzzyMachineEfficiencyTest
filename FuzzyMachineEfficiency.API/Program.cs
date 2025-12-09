var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİSLERİ EKLEME KISMI ---

// Frontend'in Backend'e erişebilmesi için CORS izni ekliyoruz
builder.Services.AddCors(options =>
{
    options.AddPolicy("SunumIzni",
        policy =>
        {
            policy.AllowAnyOrigin()  // Her yerden gelen isteği kabul et
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Controller yapısını projeye tanıtıyoruz (MakineController.cs için şart)
builder.Services.AddControllers();

// OpenAPI (Swagger) yapılandırması
builder.Services.AddOpenApi();

var app = builder.Build();

// --- 2. UYGULAMA AYARLARI KISMI ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Eğer tarayıcıda Swagger UI görmek istersen "Scalar" veya "SwaggerUI" ekleyebilirsin
    // ama sunum için şart değil, API çalışır.
}

app.UseHttpsRedirection();

// CORS'u aktif ediyoruz (Sıralama önemli: UseAuthorization'dan önce olmalı)
app.UseCors("SunumIzni");

app.UseAuthorization();

// Controller dosyalarını (MakineController) bulup endpoint haline getirir
app.MapControllers();

app.Run();