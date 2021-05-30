using Microsoft.AspNetCore.Mvc;

// проверява дали обработени данни за домакинството за налични или не
// те са необходими за функционалностите по планиране на меню и персонализирани бързи рецепта
// извиква се с http get заявка


public static async Task<IActionResult> Run(HttpRequest req, ILogger log,
    IEnumerable<object> dbHousehold)
{
    if (dbHousehold.Count()>0) {
        return new OkObjectResult("OK");
    }
    
    return new OkObjectResult("NOK");
}
