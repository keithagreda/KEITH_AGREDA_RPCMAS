using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using RPCMAS.API.Auth;
using RPCMAS.API.Middleware;
using RPCMAS.Core.Interfaces;
using RPCMAS.Infrastructure;
using RPCMAS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, MockCurrentUserService>();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;
        o.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RPCMAS API", Version = "v1" });
    c.AddSecurityDefinition("MockUser", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = MockCurrentUserService.HeaderName,
        Type = SecuritySchemeType.ApiKey,
        Description = "Mock user id (e.g., 1, 2, 3, 4, 5)"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "MockUser" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("api-supported-versions")));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RPCMAS API v1"));
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.MapControllers();

app.Run();
