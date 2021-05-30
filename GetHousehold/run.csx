using Microsoft.AspNetCore.Mvc;

// извлича данните за дадено домакинство / потребителски профил от дб households
// извиква се с http get заявка


public static async Task<IActionResult> Run(HttpRequest req, ILogger log,
    IEnumerable<object> dbHousehold)
{
    return new OkObjectResult(dbHousehold);
}
