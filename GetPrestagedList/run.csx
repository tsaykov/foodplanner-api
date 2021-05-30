/*
 * Работна функция, част от многостъпкова операция
 * Извлича предварително подготвен списък от съвместими рецепти
 */

#r "Microsoft.Azure.WebJobs.Extensions.DurableTask"

public static string Run(string name, IEnumerable<object> dbList)
{
    return String.Join(", ", dbList.Select(o => o.ToString()));
    
} 