using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Text.Json;

// -- START OF APPLICATION --
Console.WriteLine("MICROSOFT REWARDS APPLICATION");
Console.WriteLine("-------------------------------");
Console.WriteLine("Choose an option:");
Console.WriteLine("1. Activate Cards");
Console.WriteLine("2. Activate Mobile Search");
Console.WriteLine("3. Search Values");
Console.WriteLine("4. Random Word List");
Console.WriteLine("-------------------------------");
var option = Convert.ToInt32(Console.ReadLine());

switch (option)
{
    case 1:
        ActivateCards(CreateDriver(false));
        break;
    case 2:
        ActivateMobileSearch(CreateDriver(true));
        break;
    case 3:
        await SearchValues();
        break;
    case 4:
        RandomWordList();
        break;
    default:
        Console.WriteLine("Invalid option.");
        break;
}

return;

static EdgeDriver CreateDriver(bool isMobile)
{
    var options = new EdgeOptions();
    options.AddArgument("log-level=3");

    if (isMobile) options.AddArgument("--user-agent=Mozilla/5.0 (Linux; Android 14; SAMSUNG SM-F946U) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/24.0 Chrome/");

    return new EdgeDriver(options);
}

static void ActivateCards(EdgeDriver driver)
{
    driver.Navigate().GoToUrl("https://rewards.bing.com/");
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

    var delayLength = new Random().Next(1, 10) * 1000;

    try
    {
        wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("mee-card")));
        var rewardCards = driver.FindElements(By.CssSelector("[aria-label='plus']"));
        Console.WriteLine(rewardCards.Count + " reward cards found.");
        foreach (var card in rewardCards)
        {
            try
            {
                Thread.Sleep(delayLength);
                card.Click();
            }
            catch (ElementClickInterceptedException)
            {
                Console.WriteLine("element not found");
            }
        }
    }
    catch (NoSuchElementException)
    {
        Console.WriteLine("No reward cards found.");
    }
    driver.Quit();
}

static void ActivateMobileSearch(EdgeDriver driver)
{
    int count = 0;
    // string[] searchResults = await SearchValues();
    List<string> searchResults = RandomWordList();
    driver.Navigate().GoToUrl("https://www.bing.com/");
    var searchBar = driver.FindElement(By.Id("sb_form_q"));

    foreach (var search in searchResults)
    {
        foreach(char c in search)
        {
            searchBar.SendKeys(c.ToString());
            Thread.Sleep(10);
        }
        searchBar.SendKeys(Keys.Enter);
        Thread.Sleep(new Random().Next(1, 5) * 1000);
        // driver.Navigate().Back();
        searchBar = driver.FindElement(By.Id("sb_form_q"));
        searchBar.Click();
        searchBar.SendKeys(Keys.Control + "a");
        searchBar.SendKeys(Keys.Backspace);
        count += 1;
        Console.WriteLine($"Words Searched: {count}");
    }

    driver.Quit();
}

static async Task<string[]> SearchValues()
{
    var client = new HttpClient();
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    var configuration = builder.Build();
    var apikey = configuration["API_KEY"]; 
    var url = "https://www.searchapi.io/api/v1/search";
    string urlValues = "engine=google_trends_trending_daily&geo=AU";    
    var searchQueries = new List<string>();

    try
    {
        var response = await client.GetAsync($"{url}?api_key={apikey}&{urlValues}");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(responseBody);
        JsonElement root = doc.RootElement;
        JsonElement dailySearches = root.GetProperty("daily_searches");
        foreach (JsonElement search in dailySearches.EnumerateArray())
        {
            JsonElement searches = search.GetProperty("searches");
            foreach (JsonElement searchValue in searches.EnumerateArray())
            {
                JsonElement query = searchValue.GetProperty("query");
                searchQueries.Add(query.GetString() ?? string.Empty);
            }
        }
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine("\nException Caught!");
        Console.WriteLine("Message :{0} ", e.Message);
    }
    return [.. searchQueries];
}

static List<string> RandomWordList()
{
    string filePath = "C:\\Users\\aboud\\source\\repos\\MicrosoftRewards\\words.txt";
    string[] words = File.ReadAllLines(filePath);
    List<string> randWords = [];

    for (int i = 0; i < 40; i++)
    {
        int randomIndex = new Random().Next(words.Length);
        randWords.Add(words[randomIndex]);
    }

    return randWords;
}


