#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
using System.Net;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

// записва нова рецепта в дб recipes
// записва изображението в blob storage foodplannerweb
// извиква се с http post заявка


public class FormData
{
    public Recipe formData { get; set; }
}

public class Recipe
{
    public string id { get;set; }  
    public string author { get;set; }  
    public string authorId { get;set; }  
    public string title { get;set; }  
    public string complexity { get;set; }    
    public int cookingTime { get;set; }    
    public int portions { get;set; }    
    public string imag { get;set; } 
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

private static readonly Dictionary<string, string> _contentTypes = new Dictionary<string, string>()
{
    {"bmp", "image/bmp"},
    {"gif", "image/gif"},
    {"jpeg", "image/jpeg"},
    {"jpg", "image/jpeg" },
    {"png", "image/png"},
    {"svg", "image/svg+xml"},
    {"tiff", "image/tiff"},
    {"wbmp", "image/vnd.wap.wbmp"},
    {"webp", "image/webp"}
};

public static async Task<IActionResult> Run( 
    HttpRequest req,     
    ILogger log, 
    CloudBlockBlob outputBlob,
    IAsyncCollector<object> collection)
{ 
     log.LogInformation("Received a request. Will parse. ");
  
     string requestBody = String.Empty;
     using (StreamReader streamReader =  new  StreamReader(req.Body))
     {
        requestBody = streamReader.ReadToEnd();
     }
     
     FormData formData = JsonConvert.DeserializeObject<FormData>(requestBody);

     Recipe recipe = formData.formData;
  
     if (recipe.author == null) {
         recipe.author = "anonymous";  // for until we add authentication
     }     

     log.LogInformation("Received a recipe: " + recipe.title + " by author " + recipe.author);
//     log.LogInformation("RAW request: " + requestBody);
  
     string recipeId = Guid.NewGuid().ToString();
     var base64Data = Regex.Match(recipe.imag, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
     var imageExtension = Regex.Match(recipe.imag, @"data:image\/(?<extension>.+);(?<type>.+?);base64,(?<data>.+)").Groups["extension"].Value;

     string contentType = "application/octet-stream";
     _contentTypes.TryGetValue(imageExtension, out contentType);

     string imageFileName = System.IO.Path.GetFileName(outputBlob.Uri.ToString());
     log.LogInformation("image will be saved as " + imageFileName + " and is of type  " + contentType);
     outputBlob.Properties.ContentType = contentType;

     byte[] imageBytes = Convert.FromBase64String(base64Data);
     await outputBlob.UploadFromByteArrayAsync(imageBytes, 0, imageBytes.Length);
        
     object newRecipes = new {
            id = recipeId,
            author = recipe.author,
            authorId = recipe.authorId,
            title = recipe.title,
            imageName = imageFileName,
            imageType = contentType,
            instructions = recipe.instructions,                
            cookingTime = recipe.cookingTime,
            portions = recipe.portions,
            complexity = recipe.complexity,
            ingredients = recipe.ingredients,    
            tags = recipe.tags
     };
     await collection.AddAsync(newRecipes);

     return recipe.title != null
        ? (ActionResult)new OkObjectResult($"Recipe {recipe.title} was published.")
        : new BadRequestObjectResult("Not a valid recipe");
}
