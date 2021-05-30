using Microsoft.AspNetCore.Mvc;

// извлича избрани данни за Х случайни рецепти от дб recipes
// извиква се с http get заявка


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


public static async Task<IActionResult> Run(HttpRequest req, ILogger log,
    IEnumerable<Recipe> dbRecipe)
{

    // броят рецепти се дефинира от параметър в заявката
    int cnt = Int32.Parse(req.Query["count"]);

    Random rnd = new Random();
    var randomrecipes = dbRecipe.OrderBy(x => rnd.Next()).Take(cnt);

    List<Recipe> selected = new List<Recipe>();

    foreach(var item in randomrecipes)
    {
       selected.Add(item); 
    }

    string[] toSkip = { 
        "сол", "захар", "вода", "брашно", "черен пипер", "червен пипер",
        "мед", "сода", "прясно мляко", "сметана", "заквасена сметана", "кисело мляко"
    };
    int firstOnesToSkip = 1;

    foreach(var item in selected)
    {
      // log.LogInformation(item.title); 
       item.accents = "с ";
       int icnt = 0;

       log.LogInformation(item.title); 

       foreach(var i in item.ingredients) {
           if (toSkip.Contains(i.title) == false)
           {
               if (icnt>2+firstOnesToSkip) { break; }
               if (icnt>0+firstOnesToSkip) { item.accents += ", "; }
               if (icnt>=firstOnesToSkip)  { item.accents += i.title; }
               icnt++;
           }

       }
       log.LogInformation(item.accents);
       item.ingredients = null; 

    }

    return new OkObjectResult(selected);
}
