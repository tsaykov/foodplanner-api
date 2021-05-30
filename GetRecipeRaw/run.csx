using Microsoft.AspNetCore.Mvc;

// извлича данните за дадена рецепта от дб recipes
// извиква се с http get заявка


public static async Task<IActionResult> Run(HttpRequest req, ILogger log,
    IEnumerable<object> dbRecipe)
{

    return new OkObjectResult(dbRecipe);
}
