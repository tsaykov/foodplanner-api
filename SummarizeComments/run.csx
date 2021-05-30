#r "Microsoft.Azure.DocumentDB.Core"
using System.Linq;
using Microsoft.Azure.Documents;

// обобщава коментарите за дадена рецепта
// извиква се при въвеждане на нов коментар в дб comments
// записва осреднен рейтинг и брой на коментарите в дб aggregatedData


public class Comment
{
    public int rating { get;set; }  
}

public static void Run(
    ILogger log, 
    IEnumerable<Comment> allComments,
    string queueItem,
    out object summaryDb 
    )
{ 

    int cnt = allComments.Count();
    double avg = allComments.Where(item => item.rating > 0).Average(item => item.rating); 

    log.LogInformation("Summarized comments for recipe " + queueItem + ": " + cnt.ToString() + " comments, rating " + avg.ToString() ); 

    /// подготвям обекта, който да бъде записан в базата
    summaryDb = new {
        id = queueItem,
        cnt = cnt, 
        avg = avg

    };    
}
