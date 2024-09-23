using ApiBlog.Data;
using ApiBlog.Mappers;
using ApiBlog.Repositorio;
using ApiBlog.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//COnfiguramos la conexión a sq<<l server
builder.Services.AddDbContext <ApplicationDbContext> (options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql"));
});



// Add services to the container.
builder.Services.AddScoped<IPostRepositorio,PostRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>(); 


var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

//Agregar AutoMapper
builder.Services.AddAutoMapper(typeof(BlogMapper));

//Aquí se configura la Autenticación - Primera parte
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Aquí se configura la autenticación y autorización segunda parte
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
        "Autenticación JWT usando el esquema Bearer. \r\n\r\n " +
        "Ingresa la palabra 'Bearer' seguida de un [espacio] y despues su token en el campo de abajo \r\n\r\n" +
        "Ejemplo: \"Bearer tkdknkdllskd\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});


//Soporte para CORS
//Se pueden habilitar: 1-Un dominio, 2-multiples dominios, 3-Cualquier dominio (tener en cuenta seguridad)
//Usamos de ejemplo el dominio: http://localhost:3223, se debe cambiar por el correcto
//Se usa (*) para todos los dominios
builder.Services.AddCors(p => p.AddPolicy("PolicyCors", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Habilitar que se exponga el directorio de imagenes
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(Directory.GetCurrentDirectory(), @"ImagenesPosts")),
    RequestPath = "/ImagenesPosts"
});

//Soporte para CORS
app.UseCors("PolicyCors");

app.UseAuthentication ();

app.UseAuthorization();

app.MapControllers();

app.Run();
