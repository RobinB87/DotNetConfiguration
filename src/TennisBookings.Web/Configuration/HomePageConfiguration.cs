namespace TennisBookings.Web.Configuration
{
    public class HomePageConfiguration
    {
        public bool EnableGreeting { get; set; }
        public bool EnableWeatherForecast { get; set; }

        //[Required(ErrorMessage = "A title is required for the weather forecast section")]
        public string ForecastSectionTitle { get; set; }
    }
}
