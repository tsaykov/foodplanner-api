#r "Newtonsoft.Json"
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

// обновява рецепта в дб recipes
// извиква се с http post заявка


public class Recipe
{
    public string id { get;set; }  
    public string author { get;set; }  
    public string authorId { get;set; }  
    public string title { get;set; }  
    public string complexity { get;set; }    
    public int cookingTime { get;set; }    
    public int portions { get;set; }    
    public string imageName { get;set; } 
    public string imageType { get;set; } 
    public string instructions { get;set; } 
    public string[] tags { get;set; } 
    public List<Ingredient> ingredients { get;set; } 
}


public class ToPost
{
    public Recipe toPost { get; set; }
}


public class Ingredient
{
    public string title { get;set; }  
    public int quantity { get;set; }  
    public string type { get;set; }    
}

public static HttpResponseMessage Run( 
    HttpRequest req,     
    ILogger log,
    out object dbrecipe)
{ 
     log.LogInformation("Received a request. Will parse. ");
  
     string requestBody = String.Empty;
     
     using (StreamReader streamReader =  new  StreamReader(req.Body))
     {
        requestBody = streamReader.ReadToEnd();
     }

     log.LogInformation(requestBody);
        
    
     ToPost toPost = JsonConvert.DeserializeObject<ToPost>(requestBody);

     Recipe recipe = toPost.toPost;
     
     log.LogInformation("Received a recipe: " + recipe.title + " by author " + recipe.author);
        
     dbrecipe = new {
            id = recipe.id,
            author = recipe.author,
            authorId = recipe.authorId,
            title = recipe.title,
            imageName = recipe.imageName,
            imageType = recipe.imageType,
            instructions = recipe.instructions,                
            cookingTime = recipe.cookingTime,
            portions = recipe.portions,
            complexity = recipe.complexity,
            ingredients = recipe.ingredients,    
            tags = recipe.tags
     };

     return recipe.title != null
        ? new HttpResponseMessage(HttpStatusCode.Accepted)
        : new HttpResponseMessage(HttpStatusCode.BadRequest);

}
