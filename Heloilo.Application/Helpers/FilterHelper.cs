namespace Heloilo.Application.Helpers;

public static class FilterHelper
{
    /// <summary>
    /// Valida um range de datas
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <returns>Tupla com (IsValid, ErrorMessage)</returns>
    public static (bool IsValid, string? ErrorMessage) ValidateDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            return (false, "A data inicial não pode ser maior que a data final");
        }

        if (startDate.HasValue && startDate.Value > DateOnly.FromDateTime(DateTime.Today))
        {
            return (false, "A data inicial não pode ser no futuro");
        }

        if (endDate.HasValue && endDate.Value > DateOnly.FromDateTime(DateTime.Today))
        {
            return (false, "A data final não pode ser no futuro");
        }

        return (true, null);
    }

    /// <summary>
    /// Valida um filtro de status
    /// </summary>
    /// <param name="status">Status a validar</param>
    /// <param name="allowedStatuses">Lista de status permitidos</param>
    /// <returns>Tupla com (IsValid, ErrorMessage)</returns>
    public static (bool IsValid, string? ErrorMessage) ValidateStatus<T>(T? status, IEnumerable<T> allowedStatuses) where T : struct, Enum
    {
        if (!status.HasValue)
        {
            return (true, null);
        }

        if (!allowedStatuses.Contains(status.Value))
        {
            var allowedValues = string.Join(", ", allowedStatuses.Select(s => s.ToString()));
            return (false, $"Status inválido. Valores permitidos: {allowedValues}");
        }

        return (true, null);
    }

    /// <summary>
    /// Valida múltiplos filtros combinados (AND/OR)
    /// </summary>
    /// <param name="filters">Dicionário de filtros (nome do filtro, valor)</param>
    /// <param name="logic">Lógica de combinação: 'AND' ou 'OR'</param>
    /// <returns>True se os filtros são válidos</returns>
    public static bool ValidateCombinedFilters(Dictionary<string, object?> filters, string logic = "AND")
    {
        if (filters == null || filters.Count == 0)
        {
            return true;
        }

        var validFilters = filters.Where(f => f.Value != null).ToList();

        if (validFilters.Count == 0)
        {
            return true;
        }

        // Com lógica AND, todos os filtros devem ter valores válidos
        if (logic.ToUpperInvariant() == "AND")
        {
            return validFilters.All(f => f.Value != null && !string.IsNullOrWhiteSpace(f.Value.ToString()));
        }

        // Com lógica OR, pelo menos um filtro deve ter valor válido
        if (logic.ToUpperInvariant() == "OR")
        {
            return validFilters.Any(f => f.Value != null && !string.IsNullOrWhiteSpace(f.Value.ToString()));
        }

        return false;
    }

    /// <summary>
    /// Normaliza uma string para busca (remove espaços, converte para minúsculas)
    /// </summary>
    public static string? NormalizeSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return null;
        }

        return searchTerm.Trim().ToLowerInvariant();
    }
}

