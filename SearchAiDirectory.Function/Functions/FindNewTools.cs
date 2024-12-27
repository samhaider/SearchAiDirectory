namespace SearchAiDirectory.Function.Functions;

public class FindNewTools()
{
    [Function("FindNewTools")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
    {

    }
}
