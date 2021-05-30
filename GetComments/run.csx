using Microsoft.AspNetCore.Mvc;

// извлича коментарите за дадена рецепта от дб comments
// извиква се с http get заявка


public static async Task<IActionResult> Run(HttpRequest req, ILogger log,
    IEnumerable<object> dbComment)
{
    return new OkObjectResult(dbComment);
}
