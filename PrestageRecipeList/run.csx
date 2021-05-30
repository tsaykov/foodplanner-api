#r "Microsoft.Azure.DocumentDB.Core"
#r "Newtonsoft.Json"
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using System.Linq;

// създава списък с рецепти за препоръчване за определен потребител
// на база на набор от критерии изчислява оценка за всяка рецепта и сортира

public class Recipe
{
    public string id { get;set; }  
    public string _ts { get;set; }  // created timestamp
    public string complexity { get;set; }    
    public int cookingTime { get;set; }    
    public int portions { get;set; }    
    public string[] tags { get;set; } 
    public List<Ingredient> ingredients { get;set; } 
    public int CommentsCount { get;set; }
    public double AverageStars { get;set; }
    public double score { get;set; }
}

public class AggregatedComment
{
    public string id { get;set; }  
    public int cnt { get;set; }    
    public double avg { get;set; }
}

public class Ingredient
{
    public string title { get;set; }  
    public int quantity { get;set; }  
    public string type { get;set; }    
}

public static void Run(
    ILogger log,
    IEnumerable<object> dbRecipes,
    IEnumerable<object> dbAggregatedComments,
    IReadOnlyList<Document> inputHousehold,
    out object outputList)
{
    log.LogInformation($"Will prepare recipe list for {inputHousehold[0].Id} using { dbRecipes.Count() } recipes and { dbAggregatedComments.Count() } summary data items.");


    // подготвам предпочитанията

    int prefCookTime = inputHousehold[0].GetPropertyValue<int>("cookingTime");
    string prefComplexityS = inputHousehold[0].GetPropertyValue<string>("complexity");
    string[] prefTags = inputHousehold[0].GetPropertyValue<string[]>("tags");

    string[] likes = inputHousehold[0].GetPropertyValue<string[]>("likes");
    string[] dislikes = inputHousehold[0].GetPropertyValue<string[]>("dislikes");

    int prefComplexity = 1;
    if (prefComplexityS == "средна") { prefComplexity = 2;  }
    else { if (prefComplexityS == "трудна") { prefComplexity = 3; } }

    /// подготвям някои рецепти, които пасват на профила на домакинството; това е с цел по-бързо извличане при заявка

    List<Recipe> allRecipes = new List<Recipe>();
    foreach(var item in dbRecipes)
    {
     //   log.LogInformation("- " + item.id);
        allRecipes.Add(JsonConvert.DeserializeObject<Recipe>(item.ToString()));
    }

    log.LogInformation("Found data for " + allRecipes.Count().ToString() + " recipes.");


    double s = 0;
    int diffInCookTime;
    int diffInComplexity;
    int recipeComplexity;
    int matchingTagsCount;
    double coefCommentsCount;
    double coefAvgStars;

    // свързвам данните за коментарите с рецептите
    AggregatedComment commentData;

    foreach(var item in dbAggregatedComments)
    {
        commentData = JsonConvert.DeserializeObject<AggregatedComment>(item.ToString());

        foreach (var r in allRecipes)
        {
            if (r.id == commentData.id)
            {                    
                log.LogInformation("- добавяне на данни за коментари за " + commentData.id);
                r.CommentsCount = commentData.cnt;
                r.AverageStars = commentData.avg;

            } 
        }
    }

    // изготвяне на списък с оценки за съвместимост на рецептите

    foreach (var r in allRecipes) 
    {
        s = 0;  // нулирам оценката


        // любими и нежелани съставки
        foreach (var j in r.ingredients) 
        {
            // log.LogInformation(j.title);
            
            if (dislikes.Contains(j.title)) {
                s -= 1.5;   // намалявам оценката поради нежелан продукт
                log.LogInformation("- " + r.id + " съдържа нежелан продукт " + j.title);
            }                            

            if (likes.Contains(j.title)) {
                s += 0.4;   // повишавам оценката поради любим продукт
                log.LogInformation("- " + r.id + " съдържа любим продукт " + j.title);
            }                            

        }        

        // време за готвене
        diffInCookTime = (prefCookTime - r.cookingTime) / 10;
        log.LogInformation("- " + r.id + " разлика във времето за готвене спрямо предпочитанията (/10) " + diffInCookTime);

        if (diffInCookTime > 0) 
        {
            s += diffInCookTime * 0.15;            
        } else {
            s += diffInCookTime * 0.40;
        }

        // сложност на рецептата
        recipeComplexity = 1;
        if (r.complexity == "средна") { recipeComplexity = 2;  }
        else { if (r.complexity == "трудна") { recipeComplexity = 3; } }

        diffInComplexity = (prefComplexity - recipeComplexity);
        log.LogInformation("- " + r.id + " разлика във сложността на приготвяне спрямо предпочитанията " + diffInComplexity);

        if (diffInComplexity > 0) 
        {
            s += diffInComplexity * 0.5;            
        } else {
            s += diffInComplexity * 1.5;
        }

        // категории
        matchingTagsCount = 0;
        foreach (var j in r.tags) 
        {
            if (prefTags.Contains(j)) 
            {
                matchingTagsCount++;
                log.LogInformation("- " + r.id + " е в желана категория " + j);
            }           
        }

        switch (matchingTagsCount)
        {
            case 0:
                break;
            case 1:
                s += 1;
                break;
            case 2:
            case 3:
                s += (0.5 + 0.5*matchingTagsCount);
                break;
            default:
                s += (1.25 + 0.25*matchingTagsCount);
                break;
        }

        // коментари и оценка от потребителите

        if (r.CommentsCount > 0)
        {
            coefCommentsCount = 0.2 * r.CommentsCount;
            if (coefCommentsCount > 1.5) { coefCommentsCount = 1.5; }
            coefAvgStars = r.AverageStars - 3;
            log.LogInformation("* " + r.id + " додатък от оценки на потребителите " + (coefCommentsCount * coefAvgStars));
            s += coefCommentsCount * coefAvgStars;

        }

        // резултат
        r.score = s;
        log.LogInformation("* " + r.id + " Крайна Оценка " + r.score);
    }

    // сортиране и изготвяне на окончателен списък (с макс 35 рецепти)
    log.LogInformation("");
    log.LogInformation("--- Окончателен списък:");

    var mainMealRecipes = allRecipes.Where(a => a.tags.Contains("закуска") == false).Where(a => a.tags.Contains("десерти") == false);
    var breakfastRecipes = allRecipes.Where(a => a.tags.Contains("закуска"));
    var dessertRecipes = allRecipes.Where(a => a.tags.Contains("десерти"));
    
    mainMealRecipes  = mainMealRecipes.OrderBy(o=>o.score).Reverse().Take(35).ToList();
    breakfastRecipes = breakfastRecipes.OrderBy(o=>o.score).Reverse().Take(35).ToList();
    dessertRecipes = dessertRecipes.OrderBy(o=>o.score).Reverse().Take(35).ToList();

    List<string> mains = new List<string>();
    List<string> breakfasts = new List<string>();
    List<string> desserts = new List<string>();

    foreach (var r in mainMealRecipes)
    {
        log.LogInformation("--- основно ястие " + r.id + " с оценка " + r.score);
        mains.Add(r.id); 
    }

    foreach (var r in breakfastRecipes)
    {
        log.LogInformation("--- закуска " + r.id + " с оценка " + r.score);
        breakfasts.Add(r.id); 
    }

    foreach (var r in dessertRecipes)
    {
        log.LogInformation("--- десерт " + r.id + " с оценка " + r.score);
        desserts.Add(r.id); 
    }

    outputList = new {
        id = inputHousehold[0].Id,
        mains = mains,
        breakfasts = breakfasts,
        desserts = desserts
    };
}
