/*
 * Тази функция се използва за извикването на оркестраторни функции при постъпване на HTTP GET заявка
 * при функциите за планиране на седмично меню и извличане на персонализирани рецепти, 
 * като конкретната функция зависи от параметър
 */ 
 
#r "Microsoft.Azure.WebJobs.Extensions.DurableTask"
#r "Newtonsoft.Json"

using System.Net;

public static async Task<HttpResponseMessage> Run(
    HttpRequestMessage req,
    DurableOrchestrationClient starter,
    string input,
    ILogger log)
{
    // Function input comes from the request content.
    dynamic eventData = await req.Content.ReadAsAsync<object>();

    string id = input.Substring(4);
    string type = input.Substring(0,3);

    log.LogInformation($"Input '{type}' '{id}'.");

    string functionName="";

    if (type == "new") {
        functionName = "GetNewListOrchestrator";
    } else {
        if (type =="get") {
            functionName = "GetListOrchestrator";
        } else {
            if (type == "rec") {
                functionName = "GetQuickRecipes";
            } 
        }
    }

    // Pass the function name as part of the route 
    string instanceId = await starter.StartNewAsync(functionName, id); 

    log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

    return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, TimeSpan.FromSeconds(20));

    // curl -Uri "https://f-newrecipe.azurewebsites.net/api/orchestrators/GetListOrchestrator?code=FquSUbQQzph9sd6v8gRH2xavxjMqTGskK2ETLe4VNNS4NrG8TzJgsA==" -ContentType "application/json" -Method Post -Body '{ "id": "isdkljfvs" }'
}