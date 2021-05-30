using Microsoft.AspNetCore.Mvc;

// извлича данните за дадена рецепта от дб recipes
// преобразува мерните единици на продуктите при единствено число
// извиква се с http get заявка


public class Recipe
{
    public string _ts { get;set; }  // created timestamp
    public string id { get;set; }  
    public string author { get;set; }  
    public string authorId { get;set; }  
    public string title { get;set; }  
    public string complexity { get;set; }    
    public int cookingTime { get;set; }    
    public int portions { get;set; }    
    public string imageName { get;set; } 
    public string instructions { get;set; } 
    public string[] tags { get;set; } 
    public List<Ingredient> ingredients { get;set; } 
}

public class Ingredient
{
    public string title { get;set; }  
    public int quantity { get;set; }  
    public string type { get;set; }    
}


public static async Task<IActionResult> Run(HttpRequest req, ILogger log,
    IEnumerable<Recipe> dbRecipe)
{
    foreach (var d in dbRecipe) {
        foreach (var i in d.ingredients) {
            if (i.quantity == 1) {
                i.type = i.type.Replace("броя", "брой").Replace("ени ", "ена ").Replace("лъжици", "лъжица").Replace("чаши", "чаша").Replace("грама", "грам").Replace("идки", "идка").Replace("литра", "литър");
            }
        }
       
    }

    return new OkObjectResult(dbRecipe);
}
