using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Heloilo.WebAPI.Swagger;

public class FormFileParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        var parameterInfo = context.ParameterInfo;
        
        if (parameterInfo == null)
            return;

        // Verificar se o parâmetro tem [FromForm] attribute
        var hasFromFormAttribute = parameterInfo.GetCustomAttributes(typeof(FromFormAttribute), false).Any();
        
        // Verificar se é do tipo IFormFile (incluindo nullable)
        var isFormFileType = IsFormFileType(parameterInfo.ParameterType);

        // Se for IFormFile com [FromForm], configurar o schema imediatamente para prevenir erros
        // O FormFileOperationFilter irá remover estes parâmetros e adicionar como parte do request body
        if (hasFromFormAttribute && isFormFileType)
        {
            // Esta é a combinação problemática: [FromForm] + IFormFile
            // O Swagger não suporta isso nativamente e tenta processar antes dos filtros
            // Configuramos um schema válido para evitar o erro de validação
            var isNullable = IsNullableFormFile(parameterInfo.ParameterType);
            
            // Definir schema válido que o Swagger possa processar
            // Isso evita o erro "Error reading parameter(s) for action"
            parameter.Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "binary",
                Description = "File upload (will be moved to request body by OperationFilter)",
                Nullable = isNullable
            };
            
            // Marcar como não requerido e não como query parameter
            parameter.Required = false;
            parameter.In = ParameterLocation.Query; // Temporário - será movido para body
            
            // IMPORTANTE: O OperationFilter precisa remover este parâmetro
            // e adicioná-lo ao request body como multipart/form-data
        }
        else if (hasFromFormAttribute && !isFormFileType)
        {
            // É um DTO com [FromForm] - o OperationFilter vai processar
            // Mas precisamos garantir que não cause erro durante a geração
            // Configurar um schema básico se não existir
            if (parameter.Schema == null)
            {
                parameter.Schema = new OpenApiSchema
                {
                    Type = "object",
                    Description = "Form data (will be moved to request body)"
                };
            }
        }
        else if (isFormFileType && !hasFromFormAttribute)
        {
            // IFormFile sem [FromForm] - configurar schema válido
            var isNullable = IsNullableFormFile(parameterInfo.ParameterType);
            parameter.Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "binary",
                Description = "File upload",
                Nullable = isNullable
            };
            parameter.Required = !isNullable;
        }
    }

    private static bool IsFormFileType(Type type)
    {
        if (type == null)
            return false;
            
        if (type == typeof(IFormFile))
            return true;
            
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var genericArg = type.GetGenericArguments()[0];
            return genericArg == typeof(IFormFile);
        }
        
        // Verificar nullable (IFormFile?)
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType == typeof(IFormFile);
        }

        return false;
    }
    
    private static bool IsNullableFormFile(Type type)
    {
        if (type == null)
            return false;
            
        // Verificar se é Nullable<IFormFile>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType == typeof(IFormFile);
        }
        return false;
    }
}

