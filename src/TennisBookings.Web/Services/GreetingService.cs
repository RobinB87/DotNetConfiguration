using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace TennisBookings.Web.Services
{
    public class GreetingService : IGreetingService
    {
        private static readonly ThreadLocal<Random> Random
            = new ThreadLocal<Random>(() => new Random());
        private GreetingConfiguration _greetingConfiguration;
        
        public GreetingService(IWebHostEnvironment webHostEnvironment, 
            ILogger<GreetingConfiguration> logger,
            IOptionsMonitor<GreetingConfiguration> options)
        {
            var webRootPath = webHostEnvironment.WebRootPath;

            var greetingsJson = File.ReadAllText(webRootPath + "/greetings.json");
            var greetingsData = JsonConvert.DeserializeObject<GreetingData>(greetingsJson);

            Greetings = greetingsData.Greetings;
            LoginGreetings = greetingsData.LoginGreetings;

            // Will only be called once
            _greetingConfiguration = options.CurrentValue;

            // But onchange is called more often
            // Hence can be used with Singleton
            // TODO: Breakpoint gets hit twice; not correct
            options.OnChange(config =>
            {
                _greetingConfiguration = config;
                logger.LogInformation("The greeting config has been updated.");
            });
        }
        
        public string[] Greetings { get; }

        public string[] LoginGreetings { get; }

        public string GreetingColour => _greetingConfiguration.GreetingColour;


        public string GetRandomGreeting()
        {
            return GetRandomValue(Greetings);
        }

        public string GetRandomLoginGreeting(string name)
        {
            var loginGreeting = GetRandomValue(LoginGreetings);

            return loginGreeting.Replace("{name}", name);
        }

        private string GetRandomValue(IReadOnlyList<string> greetings)
        {
            if (greetings.Count == 0) return string.Empty;

            var greetingToUse = Random.Value.Next(greetings.Count);

            return greetingToUse >= 0 ? greetings[greetingToUse] : string.Empty;
        }

        private class GreetingData
        {
            public string[] Greetings { get; set; }

            public string[] LoginGreetings { get; set; }
        }
    }
}
