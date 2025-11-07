using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Heloilo.WebAPI.Swagger;

/// <summary>
/// Filtro de schema para tratar tipos problemáticos e evitar erros na geração do Swagger
/// </summary>
public class SafeSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        try
        {
            var type = context.Type;
            
            // Tratar tipos anônimos
            if (type.Name.StartsWith("<>f__AnonymousType") || type.Name.Contains("AnonymousType"))
            {
                schema.Type = "object";
                schema.Description = "Anonymous object";
                schema.Properties ??= new Dictionary<string, OpenApiSchema>();
                schema.AdditionalPropertiesAllowed = true;
                return;
            }
            
            // Tratar tipos dinâmicos (dynamic, object)
            if (type == typeof(object) || type == typeof(Dictionary<string, object>))
            {
                schema.Type = "object";
                schema.Description = "Dynamic object";
                schema.AdditionalPropertiesAllowed = true;
                return;
            }
            
            // Garantir que propriedades sempre existam para tipos complexos
            if (schema.Properties == null && !type.IsPrimitive && type != typeof(string) && type != typeof(DateTime) && type != typeof(DateTimeOffset))
            {
                schema.Type = "object";
                schema.Properties = new Dictionary<string, OpenApiSchema>();
            }
        }
        catch
        {
            // Em caso de erro, definir como object genérico para não quebrar a geração
            schema.Type = "object";
            schema.Description = "Object";
            schema.AdditionalPropertiesAllowed = true;
        }
    }
}

