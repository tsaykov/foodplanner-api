/*
 * Оркестраторна функция
 * Създава нов седмичен план на база на предварително подготвени списъци със съвместими рецепти
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
}

public class RecipeList
{
    public List<Recipe> mains { get;set; }
    public List<Recipe> breakfasts { get;set; }
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

    
    return JsonConvert.SerializeObject(recipeList);  
} 