using System.Linq.Expressions;
using System.Text;
using Heloilo.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Heloilo.Application.Helpers;

public static class PaginationHelper
{
    /// <summary>
    /// Aplica paginação cursor-based a uma query usando um campo específico
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <typeparam name="TKey">Tipo do campo usado como cursor (geralmente long para ID ou DateTime para timestamp)</typeparam>
    /// <param name="query">Query a ser paginada</param>
    /// <param name="cursor">Cursor para a página atual (opcional)</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="keySelector">Expressão para selecionar o campo usado como cursor</param>
    /// <param name="orderByAscending">Se true, ordena ascendente; se false, descendente</param>
    /// <returns>CursorPagedResult com os itens e cursors</returns>
    public static async Task<CursorPagedResult<T>> ToCursorPagedAsync<T, TKey>(
        IQueryable<T> query,
        string? cursor,
        int pageSize,
        Expression<Func<T, TKey>> keySelector,
        bool orderByAscending = true) where T : class where TKey : IComparable<TKey>
    {
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        // Decodificar cursor se fornecido
        TKey? cursorValue = default;
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            try
            {
                var decoded = DecodeCursor(cursor);
                cursorValue = ParseCursorValue<TKey>(decoded);
            }
            catch
            {
                // Cursor inválido, ignorar
            }
        }

        // Aplicar filtro de cursor se fornecido
        if (cursorValue != null)
        {
            // Criar expressão: x => keySelector(x) > cursorValue (ou < para descendente)
            var parameter = Expression.Parameter(typeof(T), "x");
            var keyExpression = Expression.Invoke(keySelector, parameter);
            var constant = Expression.Constant(cursorValue, typeof(TKey));
            var comparison = orderByAscending
                ? Expression.GreaterThan(keyExpression, constant)
                : Expression.LessThan(keyExpression, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
            query = query.Where(lambda);
        }

        // Ordenar
        query = orderByAscending
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);

        // Pegar um item a mais para verificar se há próxima página
        var itemsToTake = pageSize + 1;
        var items = await query.Take(itemsToTake).ToListAsync();

        var hasNextPage = items.Count > pageSize;
        if (hasNextPage)
        {
            items = items.Take(pageSize).ToList();
        }

        var result = new CursorPagedResult<T>
        {
            Items = items,
            PageSize = pageSize,
            HasNextPage = hasNextPage,
            HasPreviousPage = cursorValue != null
        };

        // Gerar cursors
        if (items.Any())
        {
            var lastItem = items.Last();
            var lastKey = keySelector.Compile()(lastItem);
            result.NextCursor = hasNextPage ? EncodeCursor(lastKey?.ToString() ?? string.Empty) : null;
            result.PreviousCursor = cursorValue != null ? EncodeCursor(cursorValue.ToString() ?? string.Empty) : null;
        }

        return result;
    }

    /// <summary>
    /// Aplica paginação cursor-based usando ID (para entidades que herdam BaseEntity)
    /// </summary>
    public static async Task<CursorPagedResult<T>> ToCursorPagedByIdAsync<T>(
        IQueryable<T> query,
        string? cursor,
        int pageSize,
        bool orderByAscending = true) where T : class
    {
        // Criar expressão para acessar a propriedade Id
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, "Id");
        var lambda = Expression.Lambda<Func<T, long>>(property, parameter);

        return await ToCursorPagedAsync(query, cursor, pageSize, lambda, orderByAscending);
    }

    /// <summary>
    /// Tenta fazer parse do valor do cursor para o tipo especificado
    /// </summary>
    private static TKey? ParseCursorValue<TKey>(string value) where TKey : IComparable<TKey>
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        if (typeof(TKey) == typeof(long))
        {
            if (long.TryParse(value, out var longValue))
            {
                return (TKey)(object)longValue;
            }
        }
        else if (typeof(TKey) == typeof(int))
        {
            if (int.TryParse(value, out var intValue))
            {
                return (TKey)(object)intValue;
            }
        }
        else if (typeof(TKey) == typeof(DateTime))
        {
            if (DateTime.TryParse(value, out var dateValue))
            {
                return (TKey)(object)dateValue;
            }
        }
        else if (typeof(TKey) == typeof(string))
        {
            return (TKey)(object)value;
        }

        return default;
    }

    /// <summary>
    /// Codifica um valor para usar como cursor
    /// </summary>
    public static string EncodeCursor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Decodifica um cursor para obter o valor original
    /// </summary>
    public static string DecodeCursor(string cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return string.Empty;
        }

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}

