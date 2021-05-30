/*
 * Работна функция, част от многостъпкова операция
 * Извлича детайли за рецепти по подаден списък от идентификатори
 */

#r "Microsoft.Azure.WebJobs.Extensions.DurableTask"

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


public static List<Recipe> Run(string[] IDs, IEnumerable<Recipe> dbRecipe, ILogger log)
{
    List<Recipe> recipes = new List<Recipe>();

    Recipe emptyRecipe = new Recipe();
    emptyRecipe.id = "empty";
    emptyRecipe.title = "без назначение";

    foreach (var id in IDs) 
    {
        log.LogInformation(id);
        if (id=="empty") {
            recipes.Add(emptyRecipe);
        } else {
            recipes.Add(dbRecipe.Where(a => a.id == id).First());
        }
        
    }

    string[] toSkip = { 
        "сол", "захар", "вода", "брашно", "черен пипер", "червен пипер",
        "мед", "сода", "прясно мляко", "сметана", "заквасена сметана", "кисело мляко"
    };
    int firstOnesToSkip = 1;

    foreach(var item in recipes)
    {
        if (item.id == "empty") { continue; }
        if (item.accents != null) { continue; }
        log.LogInformation(item.title); 
     
        item.accents = "с ";
        int icnt = 0;    

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


    //log.LogInformation(toReturn);

    return recipes;
}