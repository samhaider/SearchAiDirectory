using Microsoft.EntityFrameworkCore;
using SearchAiDirectory.Shared.Data;

namespace SearchAiDirectory.ConsoleApp;

public class Program
{
    public static async Task Main()
    {
        //await ScrapeWebsite.Scrape();
        await CleanTools.Clean();


        //var services = BgUtil.GetServices();
        //using var scope = services.CreateScope();
        //var toolService = scope.ServiceProvider.GetRequiredService<IToolService>();
        //var categories = await toolService.GetAllCategories();
        //categories = categories.Where(w => w.Tools.Count > 9).ToList();
        //foreach (var category in categories)
        //{
        //    if(string.IsNullOrEmpty(category.MetaDescription))
        //    {

        //        var prompt = $"For the Ai Tool Category with the following details:" +
        //            $"\nName: {category.Name}" +
        //            $"\nTools under this category: {string.Join(",",category.Tools.Take(5).Select(s => s.Name).ToArray())}";

        //        var categoryAgent = new AgentBase("Categorize Ai Tool", .6, SystemPrompts.Categorizing);
        //        string categorgyMetaDescription = await categoryAgent.DirectOpenAiResponse(prompt + "Create a meta description for this category without any additional context. the meta description should be less than 140 characters.");
        //        string categorgyMetaKeywords = await categoryAgent.DirectOpenAiResponse(prompt + "Create a comma delimited meta keywords for this category without any additional context. the meta keywords should be less than 140 characters.");

        //        var updatedCategory = categories.Where(w => w.ID == category.ID).SingleOrDefault();
        //        updatedCategory.MetaDescription = categorgyMetaDescription.Trim().Normalize();
        //        updatedCategory.MetaKeywords = categorgyMetaKeywords.Trim().Normalize();
        //        await toolService.UpdateCategory(updatedCategory);
        //    }
        //}
    }
}

public static class BgUtil
{
    public static ServiceProvider GetServices()
    {
        return new ServiceCollection()
            .AddDbContextFactory<ApplicationDataContext>(options => options.UseSqlServer("Server=tcp:searchaidirectory.database.windows.net,1433;Initial Catalog=sad_db;Persist Security Info=False;User ID=sadDBuser;Password=_2PSP2EE&R@?r2V1#hr;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            .AddTransient<IToolService, ToolService>()
            .AddTransient<IEmbeddingService, EmbeddingService>()
            .AddTransient<ICategoryService, CategoryService>()
            .BuildServiceProvider();
    }
}