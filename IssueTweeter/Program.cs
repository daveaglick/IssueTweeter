using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;
using IssueTweeter.Models;
using IssueTweeter.Domain;
using Configuration = IssueTweeter.Domain.Configuration;

namespace IssueTweeter
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AsyncMain().Wait();
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    HandleException(ex);

                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                }
            }
        }

        private static void HandleException(Exception ex)
        {
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner Exception Details:");
                HandleException(ex.InnerException);
            }
        }

        static async Task AsyncMain()
        {
            // Get accounts
            Configuration configuration = new Configuration();
            GitHubAccount[] accounts = configuration.GitHubAccounts;

            // Get excluded users
            string[] excludedUsers = configuration.ExcludedUsers;

            // Get issues for each account repository
            DateTimeOffset since = DateTimeOffset.UtcNow.AddMinutes(-30);
            GitHub gitHub = new GitHub(configuration);
            List<KeyValuePair<string, string>> issues =
                await gitHub.GetIssuesForAccounts(accounts, since, excludedUsers)
                .ConfigureAwait(false);

            // Get recent tweets
            Twitter twitter = new Twitter(configuration);
            List<Status> recentTweets = await twitter.GetRecentTweets(200); 

            // Aggregate and eliminate issues already tweeted
            List<string> tweets = issues
                .Where(i => !recentTweets.Any(t => t.Text.Contains(i.Key)))
                .Select(i => i.Value)
                .ToList();

            // Send tweets
            await twitter.SendTweets(tweets).ConfigureAwait(false);
        }
    }
}
