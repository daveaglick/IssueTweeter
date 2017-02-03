using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;
using IssueTweeter.Models;
using IssueTweeter.Domain;

namespace IssueTweeter
{
    class Program
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
            GitHubAccount[] accounts = Configuration.GitHubAccounts;

            // Get excluded users
            string[] excludedUsers = Configuration.ExcludedUsers;

            // Get issues for each account repository
            DateTimeOffset since = DateTimeOffset.UtcNow.AddHours(-1);
            List<KeyValuePair<string, string>> issues =
                await GitHub.GetIssuesForAccounts(accounts, since, excludedUsers)
                .ConfigureAwait(false);

            // Get recent tweets
            List<Status> recentTweets = await Twitter.GetRecentTweets(200); 

            // Aggregate and eliminate issues already tweeted
            List<string> tweets = issues
                .Where(i => !recentTweets.Any(t => t.Text.Contains(i.Key)))
                .Select(i => i.Value)
                .ToList();

            // Send tweets
            await Twitter.SendTweets(tweets).ConfigureAwait(false);
        }
    }
}
