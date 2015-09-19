using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToTwitter;
using Octokit;

namespace IssueTweeter
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncMain().Wait();
        }

        static async Task AsyncMain()
        {
            // Get repositories
            string[] repos = ConfigurationManager.AppSettings["Repositories"]
                .Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            // Authorize to GitHub
            string token = ConfigurationManager.AppSettings["GitHubToken"];
            GitHubClient github = new GitHubClient(new ProductHeaderValue("IssueTweeter"))
            {
                Credentials = new Credentials(token)
            };

            // Get issues for each repo
            DateTimeOffset since = DateTimeOffset.UtcNow.AddHours(-1);
            List<Task<List<KeyValuePair<string, string>>>> issuesTasks = repos.Select(x => GetIssues(github, x, since)).ToList();
            await Task.WhenAll(issuesTasks);

            // Authorize to Twitter
            SingleUserAuthorizer twitterAuth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = ConfigurationManager.AppSettings["TwitterConsumerKey"],
                    ConsumerSecret = ConfigurationManager.AppSettings["TwitterConsumerSecret"],
                    AccessToken = ConfigurationManager.AppSettings["TwitterAccessToken"],
                    AccessTokenSecret = ConfigurationManager.AppSettings["TwitterAccessTokenSecret"]
                }
            };
            TwitterContext twitterContext = new TwitterContext(twitterAuth);

            // Get recent tweets
            string twitterUser = ConfigurationManager.AppSettings["TwitterUser"];
            List<Status> timeline = await twitterContext.Status
                .Where(x => x.Type == StatusType.User && x.ScreenName == twitterUser && x.Count == 200)
                .ToListAsync();

            // Aggregate and eliminate issues already tweeted
            List<string> tweets = issuesTasks
                .SelectMany(x => x.Result.Where(i => !timeline.Any(t => t.Text.StartsWith(i.Key))).Select(i => i.Value))
                .ToList();

            // Send tweets
            List<Task<Status>> tweetTasks = tweets.Select(x => twitterContext.TweetAsync(x)).ToList();
            await Task.WhenAll(tweetTasks);
        }

        // Kvp = owner/repo#issue, full text of tweet
        static async Task<List<KeyValuePair<string, string>>> GetIssues(GitHubClient github, string repo, DateTimeOffset since)
        {
            List<KeyValuePair<string, string>> tweets = new List<KeyValuePair<string, string>>();
            string[] ownerName = repo.Split('\\');
            IReadOnlyList<Issue> issues = await github.Issue
                .GetAllForRepository(ownerName[0], ownerName[1], new RepositoryIssueRequest { Since = since, State = ItemState.All});
            issues = issues.Where(x => x.CreatedAt > since).ToList();
            foreach (Issue issue in issues)
            {
                string key = $"{repo}#{issue.Number}";
                int remainingChars = 140 - (key.Length + 25);
                string value = $"{key} {issue.HtmlUrl}\r\n{(issue.Title.Length <= remainingChars ? issue.Title : issue.Title.Substring(0, remainingChars))}";
                tweets.Add(new KeyValuePair<string, string>(key, value));
            }
            return tweets;
        }
    }
}
