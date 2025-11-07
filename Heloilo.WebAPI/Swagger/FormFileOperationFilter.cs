using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Heloilo.WebAPI.Swagger;

public class FormFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parameters = context.MethodInfo.GetParameters().ToList();
        
        // Verificar se há parâmetros com [FromForm] ou IFormFile
        var hasFormParameters = parameters.Any(p => 
            p.GetCustomAttributes(typeof(FromFormAttribute), false).Any() ||
            IsFormFileType(p.ParameterType));

        if (!hasFormParameters)
        {
            return;
        }

        // Remover TODOS os parâmetros que têm [FromForm] ou são IFormFile
        // Isso previne o erro antes que o Swagger tente processá-los
        var formParamNames = parameters
            .Where(p => 
            {
                var hasFromForm = p.GetCustomAttributes(typeof(FromFormAttribute), false).Any();
                var isFormFile = IsFormFileType(p.ParameterType);
                return hasFromForm || isFormFile;
            })
            .Select(p => p.Name)
            .Where(name => name != null)
            .ToHashSet();

        if (operation.Parameters != null && operation.Parameters.Count > 0)
        {
            // Remover parâmetros que devem estar no form
            var paramsToRemove = operation.Parameters
                .Where(p => formParamNames.Contains(p.Name))
                .ToList();
            
            foreach (var param in paramsToRemove)
            {
                operation.Parameters.Remove(param);
            }
        }

        // Criar ou atualizar request body para multipart/form-data
        var requestBody = operation.RequestBody ?? new OpenApiRequestBody();
        requestBody.Content ??= new Dictionary<string, OpenApiMediaType>();

        // Remover content types existentes se houver conflito
        if (requestBody.Content.ContainsKey("application/json"))
        {
            requestBody.Content.Remove("application/json");
        }

        var mediaType = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new HashSet<string>()
            }
        };

        // Processar todos os parâmetros form
        foreach (var param in parameters.Where(p => 
        {
            var hasFromForm = p.GetCustomAttributes(typeof(FromFormAttribute), false).Any();
            var isFormFile = IsFormFileType(p.ParameterType);
            return hasFromForm || isFormFile;
        }))
        {
            if (IsFormFileType(param.ParameterType))
            {
                // É um arquivo
                var isNullable = IsNullableFormFile(param.ParameterType);
                
                if (IsListOfFormFiles(param.ParameterType))
                {
                    // É uma lista de arquivos
                    mediaType.Schema.Properties[param.Name!] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        },
                        Description = "Lista de arquivos para upload",
                        Nullable = isNullable
                    };
                    if (!isNullable)
                    {
                        mediaType.Schema.Required.Add(param.Name!);
                    }
                }
                else
                {
                    // É um arquivo único
                    mediaType.Schema.Properties[param.Name!] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = "Arquivo para upload",
                        Nullable = isNullable
                    };
                    if (!isNullable)
                    {
                        mediaType.Schema.Required.Add(param.Name!);
                    }
                }
            }
            else if (param.ParameterType.IsClass && param.ParameterType != typeof(string))
            {
                // É um DTO - adicionar suas propriedades como form fields
                var dtoType = param.ParameterType;
                var properties = dtoType.GetProperties();

                foreach (var prop in properties)
                {
                    var propName = prop.Name;
                    var propType = prop.PropertyType;
                    var isNullable = IsNullableValueType(propType) || (propType.IsClass && propType != typeof(string));
                    
                    // Para tipos nullable, obter o tipo subjacente
                    Type underlyingType;
                    if (IsNullableValueType(propType))
                    {
                        underlyingType = Nullable.GetUnderlyingType(propType)!;
                        isNullable = true;
                    }
                    else
                    {
                        underlyingType = propType;
                        // Classes são nullable por padrão (exceto string quando não é nullable reference)
                        isNullable = propType.IsClass && propType != typeof(string);
                    }

                    var schema = CreateSchemaForType(underlyingType, isNullable);
                    if (schema != null)
                    {
                        mediaType.Schema.Properties[propName] = schema;
                        
                        // Adicionar à lista de required se não for nullable e tiver RequiredAttribute
                        var requiredAttr = prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
                        if (requiredAttr.Any() && !isNullable)
                        {
                            mediaType.Schema.Required.Add(propName);
                        }
                    }
                }
            }
            else if (param.ParameterType == typeof(string) || param.ParameterType.IsPrimitive || IsNullableValueType(param.ParameterType))
            {
                // Tipo simples (string, int, etc.) com [FromForm]
                var isNullable = IsNullableValueType(param.ParameterType) || param.ParameterType.IsClass;
                var underlyingType = IsNullableValueType(param.ParameterType) 
                    ? Nullable.GetUnderlyingType(param.ParameterType)! 
                    : param.ParameterType;
                
                var schema = CreateSchemaForType(underlyingType, isNullable);
                if (schema != null)
                {
                    mediaType.Schema.Properties[param.Name!] = schema;
                }
            }
        }

        requestBody.Content["multipart/form-data"] = mediaType;
        requestBody.Required = true;
        operation.RequestBody = requestBody;
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
        
        // Verificar nullable
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType == typeof(IFormFile);
        }

        return false;
    }

    private static bool IsListOfFormFiles(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var genericArg = type.GetGenericArguments()[0];
            return genericArg == typeof(IFormFile);
        }
        return false;
    }

    private static bool IsNullableFormFile(Type type)
    {
        // Verificar se é Nullable<IFormFile>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType == typeof(IFormFile);
        }
        return false;
    }

    private static bool IsNullableValueType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static OpenApiSchema? CreateSchemaForType(Type type, bool isNullable)
    {
        if (type == typeof(string))
        {
            return new OpenApiSchema { Type = "string", Nullable = isNullable };
        }
        else if (type == typeof(int))
        {
            return new OpenApiSchema { Type = "integer", Format = "int32", Nullable = isNullable };
        }
        else if (type == typeof(long))
        {
            return new OpenApiSchema { Type = "integer", Format = "int64", Nullable = isNullable };
        }
        else if (type == typeof(bool))
        {
            return new OpenApiSchema { Type = "boolean", Nullable = isNullable };
        }
        else if (type == typeof(float))
        {
            return new OpenApiSchema { Type = "number", Format = "float", Nullable = isNullable };
        }
        else if (type == typeof(double))
        {
            return new OpenApiSchema { Type = "number", Format = "double", Nullable = isNullable };
        }
        else if (type == typeof(decimal))
        {
            return new OpenApiSchema { Type = "number", Format = "double", Nullable = isNullable };
        }
        else if (type == typeof(DateOnly))
        {
            return new OpenApiSchema { Type = "string", Format = "date", Nullable = isNullable };
        }
        else if (type == typeof(DateTime))
        {
            return new OpenApiSchema { Type = "string", Format = "date-time", Nullable = isNullable };
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = type.GetGenericArguments()[0];
            var itemSchema = CreateSchemaForType(itemType, false);
            if (itemSchema != null)
            {
                return new OpenApiSchema 
                { 
                    Type = "array", 
                    Items = itemSchema,
                    Nullable = isNullable
                };
            }
        }
        else if (type.IsEnum)
        {
            var enumNames = System.Enum.GetNames(type)
                .Select(name => new OpenApiString(name) as IOpenApiAny)
                .ToList();
                
            return new OpenApiSchema
            {
                Type = "string",
                Enum = enumNames,
                Nullable = isNullable
            };
        }
        else
        {
            return new OpenApiSchema
            {
                Type = "string",
                Description = $"Propriedade do tipo {type.Name}",
                Nullable = isNullable
            };
        }

        return null;
    }
}
