/*
 * Работна функция, част от многостъпкова операция
 * Извлича съществуващ кулинарен план като списък от идентификатори
 */

#r "Microsoft.Azure.WebJobs.Extensions.DurableTask"

public static string Run(string name, IEnumerable<object> dbList)
{
    return String.Join(", ", dbList.Select(o => o.ToString()));
    
} 