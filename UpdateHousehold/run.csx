#r "Newtonsoft.Json"
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

// записва или обновява данни за дадено домакинство / потребителски профил в дб households
// извиква се с http post заявка


public class FamilyMember
{
    public string name { get;set; }
    public string[] likes { get;set; }  
    public string[] dislikes { get;set; }
   
}

public class Preferences
{
    public string cookingTime { get;set; }
    public string complexity { get;set; }  
    public string[] tags { get;set; }  
}


public class Household
{
    public FamilyMember[] persons { get;set; } 
    public Preferences preferences { get;set; } 
    public string username { get;set; }
    public string id { get;set; }

}

public class FormData
{
    public Household formData { get; set; }
}


public static IActionResult Run( 
    HttpRequest req,     
    ILogger log, 
    out object dbHousehold)

{ 
     log.LogInformation("Received a household update request. Will parse.");

     string requestBody = String.Empty;
     using (StreamReader streamReader =  new  StreamReader(req.Body))
     {
        requestBody = streamReader.ReadToEnd();
     }
     
     FormData formData = JsonConvert.DeserializeObject<FormData>(requestBody);

     Household household = formData.formData;
  
     //string username = "anonymous";  // for until we add authentication    

     log.LogInformation("Received a household update from " + household.id);
     log.LogInformation("RAW request: " + requestBody);

     foreach (var person in household.persons)
     {
        if (person.likes == null)
        {
            person.likes = new List<String>().ToArray();
        }
        if (person.likes == null)
        {
            person.likes = new List<String>().ToArray();
        }
     }

     
     dbHousehold = new {
            id = household.id,
            username = household.username,  // this is unnecessary and can be removed once username is not a unique key
            preferences = household.preferences,
            persons = household.persons
        
     };

     return household.persons != null
        ? (ActionResult)new OkObjectResult($"Household for {household.id} was updated.")
        : new BadRequestObjectResult("Not a valid request"); 
}
