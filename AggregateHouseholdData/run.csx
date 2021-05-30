#r "Microsoft.Azure.DocumentDB.Core"
#r "Newtonsoft.Json"
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using System.Linq;

// генерира списъци с предпочитани и нежелани продукти за цялото домакинство (на база на предпочитанията на всеки член от домакинството)
// извиква се при промяна на профил на домакинство в дб households
// записва резултата в дб aggregatedData


public class Ingredient
{
    public string title { get;set; }  
    public int quantity { get;set; }  
    public string type { get;set; }    
}

public class FamilyMember
{
    public string name { get;set; }
    public string[] likes { get;set; }  
    public string[] dislikes { get;set; }   
}

public class Preferences
{
    public string[] tags { get;set; } 
    public string complexity { get;set; }    
    public string cookingTime { get;set; }    
}

public class Household
{
    public FamilyMember[] persons { get;set; } 
    public Preferences preferences { get;set; }    
}


public static void Run(IReadOnlyList<Document> input, ILogger log,
    out object dbAggregated)
{

    log.LogInformation("Household was modified: " + input[0].Id);
    log.LogInformation("Will prepare some data for quicker operation later.");

    FamilyMember[] persons = input[0].GetPropertyValue<FamilyMember[]>("persons");

    log.LogInformation("Parsed data for " + persons.Count().ToString() + " persons.");

    /// извличам предпочитани и нежелани продукти за домакинството като цяло
    List<string> likes = new List<string>();
    List<string> dislikes = new List<string>();

    foreach(FamilyMember member in persons)
    {
        foreach(string product in member.likes)
        {
             likes.Add(product);
        }

        foreach(string product in member.dislikes)
        {
             dislikes.Add(product);
        }
    }


    /// подготвям обекта, който да бъде записан в базата

    Preferences preferences = input[0].GetPropertyValue<Preferences>("preferences");

    int cookingTime;
    try {
        cookingTime = Int32.Parse(preferences.cookingTime);
    }
    catch {
        cookingTime = 120;
    }
        

    dbAggregated = new {
        id = input[0].Id,
        likes = likes.Distinct(),
        dislikes = dislikes.Distinct(),
        complexity = preferences.complexity,
        cookingTime = cookingTime,
        tags = preferences.tags
    };

}
