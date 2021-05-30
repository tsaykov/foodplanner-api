#r "Newtonsoft.Json"
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

// записва нов коментар в дб commments
// извиква се с http post заявка


public class Comment
{
    public string recipeId { get;set; }
    public string author { get;set; }  
    public string text { get;set; }  
    public int rating { get;set; }  
   
}


public static IActionResult Run( 
    HttpRequest req,     
    ILogger log, 
    out object newComment,
    out string outQueue)

{ 
    log.LogInformation("Received a comment request. Will parse.");


    string requestBody = String.Empty;
    using (StreamReader streamReader =  new  StreamReader(req.Body))
    {
       requestBody = streamReader.ReadToEnd();
    }
     
    Comment comment= JsonConvert.DeserializeObject<Comment>(requestBody);

    // Comment comment = formData;
  
    if (comment.author == null) {
        comment.author = "anonymous";  // for until we add authentication
    }     

    log.LogInformation("Received a comment from author " + comment.author);
    log.LogInformation("RAW request: " + requestBody);
    string newId = Guid.NewGuid().ToString();
     
    newComment = new {
        id = newId,
        recipeId = comment.recipeId,
        author = comment.author,
        text = comment.text,
        rating = comment.rating
        
    };

    outQueue = comment.recipeId;

    return comment.recipeId != null
        ? (ActionResult)new OkObjectResult($"New comment for {comment.recipeId} was published.")
        : new BadRequestObjectResult("Not a valid comment"); 
}
