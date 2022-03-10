using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TennisBookings.Web.Configuration;
using TennisBookings.Web.Services;

namespace TennisBookings.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IGreetingService _greetingService;
        private readonly IWeatherForecaster _weatherForecaster;
        private readonly HomePageConfiguration _homePageConfig;

        public IndexModel(IGreetingService greetingService, 
            IOptions<HomePageConfiguration> options, 
            IWeatherForecaster weatherForecaster)
        {
            _greetingService = greetingService;
            _homePageConfig = options.Value;
            _weatherForecaster = weatherForecaster;

            GreetingColour = _greetingService.GreetingColour ?? "black";
        }

        public string Greeting { get; private set; }
        public bool ShowGreeting => !string.IsNullOrEmpty(Greeting);
        public string GreetingColour { get; private set; }
        public string ForecastSectionTitle { get; private set; }
        public string WeatherDescription { get; private set; }
        public bool ShowWeatherForecast { get; private set; }

        public async Task OnGet()
        {
            if (_homePageConfig.EnableGreeting)
            {
                Greeting = _greetingService.GetRandomGreeting();
            }

            ShowWeatherForecast = _homePageConfig.EnableWeatherForecast
                && _weatherForecaster.ForecastEnabled;

            if (!ShowWeatherForecast)
                return;

            var title = _homePageConfig.ForecastSectionTitle;
            ForecastSectionTitle = string.IsNullOrEmpty(title) ? "How is the weather?" : title;

            var currentWeather = await _weatherForecaster.GetCurrentWeatherAsync();

            if (currentWeather == null)
                return;

            switch (currentWeather.Description)
            {
                case "Sun":
                    WeatherDescription = "It's sunnaeaeeeyyyy!";
                    break;

                case "Rain":
                    WeatherDescription = "It's raining... No tennis today :(";
                    break;

                case "Cloud":
                    WeatherDescription = "Cloudy, no worries";
                    break;

                default:
                    WeatherDescription = "Weatherforecast is not determined yet";
                    break;
            }
        }
    }
}
