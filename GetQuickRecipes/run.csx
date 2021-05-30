/*
 * Оркестраторна функция
 * Извлича персонализирани предложения, на базата на предварително изчислени списъци от съвместими рецепти
 */

#r "Microsoft.Azure.WebJobs.Extensions.DurableTask"
#r "Newtonsoft.Json"
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class RecipeListIDs
{
    public string id { get;set; }  
    public string[] mains { get;set; }  
    public string[] breakfasts { get;set; }    
    public string[] desserts { get;set; }  
}

public class RecipeList
{
    public List<Recipe> mains { get;set; }
    public List<Recipe> breakfasts { get;set; }
    public List<Recipe> desserts { get;set; }
}

public class Recipe
{
    public string id { get;set; }  
    public string author { get;set; }  
    public string title { get;set; }  
    public string imageName { get;set; }  
    public string complexity { get;set; }  
    public int cookingTime { get;set; }  
    public int _ts { get;set; }  
    public string[] tags { get;set; }  
    public List<Ingredient> ingredients { get;set; } 
    public string accents { get;set; } 
}

public class Ingredient
{
    public string title { get;set; }  
    public int quantity { get;set; }  
    public string type { get;set; }    
}


public static async Task<string> Run(DurableOrchestrationContext context,
    ILogger log)
{
    var profileId = context.GetInput<string>();

    log.LogInformation($"Getting recipe list for household { profileId }");

    var dbRecipeList = await context.CallActivityAsync<string>("GetPrestagedList", profileId);   // извличане на списъка с подходящи рецепти
  
    log.LogInformation($"Received: { dbRecipeList } ");

    RecipeListIDs recipeListIDs = JsonConvert.DeserializeObject<RecipeListIDs>(dbRecipeList);

    var count = recipeListIDs.mains.Count();

    log.LogInformation($"Received: { count } ");

    RecipeList recipeList = new RecipeList();

    recipeList.mains      = await context.CallActivityAsync<List<Recipe>>("ListRecipesFromIds", recipeListIDs.mains);  // извличане на детайли за конкретните рецепти
    recipeList.breakfasts = await context.CallActivityAsync<List<Recipe>>("ListRecipesFromIds", recipeListIDs.breakfasts);
    recipeList.desserts   = await context.CallActivityAsync<List<Recipe>>("ListRecipesFromIds", recipeListIDs.desserts);
      
    List<Recipe> toReturn = new List<Recipe>();

    foreach (var r in recipeList.mains.Take(5)) { toReturn.Add(r); }
    foreach (var r in recipeList.breakfasts.Take(3)) { toReturn.Add(r); }
    foreach (var r in recipeList.desserts.Take(4)) { toReturn.Add(r); }

    Random rnd = new Random();
    toReturn = toReturn.OrderBy(x => rnd.Next()).Take(5).ToList();
    
    return JsonConvert.SerializeObject(toReturn);  
} 