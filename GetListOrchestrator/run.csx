/*
 * Оркестраторна функция
 * Извлича съществуващ седмичен план
 */ 

#r "Microsoft.Azure.WebJobs.Extensions.DurableTask"
#r "Newtonsoft.Json"
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class RecipeListIDs
{
    public string id { get;set; }  
    public int days { get;set; }  
    public int startDay { get;set; }  
    public string[] breakfasts { get;set; }    
    public string[] lunches { get;set; }  
    public string[] dinners { get;set; }  
}

public class RecipeList
{
    public int days { get;set; }  
    public int startDay { get;set; }  
    public List<Recipe> breakfasts { get;set; }
    public List<Recipe> lunches { get;set; }
    public List<Recipe> dinners { get;set; }
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

    var dbRecipeList = await context.CallActivityAsync<string>("GetExistingPlan", profileId);
  
    log.LogInformation($"Received: { dbRecipeList } ");

    RecipeListIDs recipeListIDs = JsonConvert.DeserializeObject<RecipeListIDs>(dbRecipeList);

    log.LogInformation($"Received: { recipeListIDs.days } ");

    RecipeList recipeList = new RecipeList();

    recipeList.breakfasts = await context.CallActivityAsync<List<Recipe>>("ListRecipesFromIds", recipeListIDs.breakfasts);
    recipeList.lunches    = await context.CallActivityAsync<List<Recipe>>("ListRecipesFromIds", recipeListIDs.lunches);
    recipeList.dinners    = await context.CallActivityAsync<List<Recipe>>("ListRecipesFromIds", recipeListIDs.dinners);

    recipeList.startDay   = recipeListIDs.startDay;
    recipeList.days       = recipeListIDs.days;

    //log.LogInformation(JsonConvert.SerializeObject(recipeList));

    return JsonConvert.SerializeObject(recipeList);
} 