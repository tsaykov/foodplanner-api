using Microsoft.AspNetCore.Mvc;

// извлича избрани данни за всички рецепти от дб recipes
// извиква се с http get заявка


public class Recipe
{
    public string id { get;set; }  
    public string author { get;set; }  
    public string title { get;set; }  
    public string imageName { get;set; }  
}

public static async Task<IActionResult> Run(HttpRequest req, ILogger log,
    IEnumerable<Recipe> dbRecipe)
{
    return new OkObjectResult(dbRecipe);
}
