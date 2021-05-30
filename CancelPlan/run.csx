#r "Newtonsoft.Json"
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

// деактивира седмично кулинарно меню
// извиква се с http post заявка

public class Data
{
    public string id { get;set; }  // household id
   
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
     
     Data data = JsonConvert.DeserializeObject<Data>(requestBody);

     log.LogInformation("Received a plan update from " + data.id);
     log.LogInformation("RAW request: " + requestBody);
     
     dbPlan = new {
            id = data.id,
            days = -1
     };

     return data.id != null
        ? (ActionResult)new OkObjectResult($"Plan for {data.id} was cancelled.")
        : new BadRequestObjectResult("Not a valid request"); 
}