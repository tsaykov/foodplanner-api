using Microsoft.AspNetCore.Mvc;

// извлича данните за дадено домакинство / потребителски профил от дб households
// извиква се с http get заявка

public class Plan
{
    public long _ts { get;set; }  // created timestamp
    public int days { get;set; }  
}

public static async Task<IActionResult> Run(HttpRequest req, ILogger log,
    IEnumerable<Plan> dbPlan)
{
    if (dbPlan.Count() == 0) {
        log.LogInformation("plan does not exist"); 
        return new OkObjectResult("no"); // няма активен план
    }


    DateTimeOffset now = DateTimeOffset.Now;

    foreach (var d in dbPlan) {
        DateTimeOffset startTime = DateTimeOffset.FromUnixTimeSeconds(d._ts);
        DateTimeOffset endTime = startTime.AddDays(d.days);

        if (DateTimeOffset.Compare(endTime, now) < 0) {
            log.LogInformation("plan has expired"); 
            return new OkObjectResult("no");
        }
        
    }
    
    log.LogInformation("plan is active"); 
    return new OkObjectResult("yes"); // има активен план
}
