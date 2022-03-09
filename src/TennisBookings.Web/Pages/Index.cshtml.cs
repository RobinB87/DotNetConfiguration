using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using TennisBookings.Web.Services;

namespace TennisBookings.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IGreetingService _greetingService;
        private readonly IConfiguration _config;

        public IndexModel(IGreetingService greetingService, IConfiguration config)
        {
            _greetingService = greetingService;
            _config = config;
        }

        public string Greeting { get; private set; }
        public bool ShowGreeting => !string.IsNullOrEmpty(Greeting);
        public string ForecastSectionTitle { get; private set; }
        public string WeatherDescription { get; private set; }
        public bool ShowWeatherForecast { get; private set; }

        public async Task OnGet()
        {
            var homePageFeatures = _config.GetSection("Features:HomePage");

            if (homePageFeatures.GetValue<bool>("EnableGreeting"))
            {
                Greeting = _greetingService.GetRandomGreeting();
            }

            ShowWeatherForecast = homePageFeatures.GetValue<bool>("EnableWeatherForecast");
        }
    }
}
