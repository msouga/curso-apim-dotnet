var builder = WebApplication.CreateBuilder(args);

// Agrega servicios al contenedor.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura el pipeline de peticiones HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ----------------------------------------------------

// --- Endpoints de la API para el Curso ---

app.MapGet("/api/products", () => 
{
    return Results.Ok(ProductRepository.Products);
})
.WithName("GetAllProducts")
.WithDescription("Obtiene la lista completa de productos.")
.WithOpenApi();

// Aquí añadiremos más endpoints durante el curso...

// ---------------------------

app.Run();

// --- Definición del Modelo y Datos para el Curso ---
public record Product(int Id, string Name, double Price, int Stock);

public static class ProductRepository
{
    public static List<Product> Products { get; } = new List<Product>
    {
        new(1, "Laptop Gamer Pro", 2100.50, 15),
        new(2, "Teclado Mecánico RGB", 120.00, 50),
        new(3, "Monitor Curvo 34\"", 899.99, 10),
        new(4, "Webcam 4K con Aro de Luz", 150.75, 30)
    };
}
