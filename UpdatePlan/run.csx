#r "Newtonsoft.Json"
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

// записва или обновява седмично кулинарно меню
// извиква се с http post заявка


public class Plan
{
    public string id { get;set; }  // household id
    public int startDay { get;set; }  // първия ден от менюто
    public int days { get;set; }  // за колко дни е менюто
    public string[] breakfasts { get;set; }  
    public string[] lunches { get;set; }  
    public string[] dinners { get;set; }  
   
}

public static IActionResult Run( 
    HttpRequest req,     
    ILogger log, 
    out object dbPlan)
{
    log.LogInformation("Received a plan update request. Will parse.");

     string requestBody = String.Empty;
     using (StreamReader streamReader =  new  StreamReader(req.Body))
     {
        requestBody = streamReader.ReadToEnd();
     }
     
     Plan plan = JsonConvert.DeserializeObject<Plan>(requestBody);

     log.LogInformation("Received a plan update from " + plan.id);
     log.LogInformation("RAW request: " + requestBody);
     
     dbPlan = new {
            id = plan.id,
            days = plan.days,
            startDay = plan.startDay,
            breakfasts = plan.breakfasts, 
            lunches = plan.lunches, 
            dinners = plan.dinners        
     };

     return plan.lunches != null
        ? (ActionResult)new OkObjectResult($"Plan for {plan.id} was updated.")
        : new BadRequestObjectResult("Not a valid request"); 
}