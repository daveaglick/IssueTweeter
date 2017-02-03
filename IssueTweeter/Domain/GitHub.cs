using IssueTweeter.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueTweeter.Domain
{
    class GitHub
    {
        // Authorize to GitHub
        static GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("IssueTweeter"))
        {
            Credentials = new Credentials(Configuration.GitHubToken)
        };

        internal static async Task<List<KeyValuePair<string, string>>> GetIssuesForAccounts(GitHubAccount[] accounts, DateTimeOffset since, string[] excludedUsers)
        {
            // get repositories
            var repos = new List<string>();
            foreach (var account in accounts)
            {
                foreach (var repository in account.Repositories)
                {
                    repos.Add($"{account.Owner}\\{repository}");
                }
            }

            // get issues
            List<Task<List<KeyValuePair<string, string>>>> issuesTasks = repos
                .Select(x => GetIssuesForRepository(x, since, excludedUsers))
                .ToList();

            await Task.WhenAll(issuesTasks);

            return issuesTasks.SelectMany(x => x.Result).ToList();
        }

        // Kvp = owner/repo#issue, full text of tweet
        internal static async Task<List<KeyValuePair<string, string>>> GetIssuesForRepository(string repo, DateTimeOffset since, string[] excludedUsers)
        {
            List<KeyValuePair<string, string>> tweets = new List<KeyValuePair<string, string>>();
            string[] ownerName = repo.Split('\\');
            IReadOnlyList<Issue> issues = await gitHubClient.Issue
                .GetAllForRepository(ownerName[0], ownerName[1], new RepositoryIssueRequest { Since = since, State = ItemState.All });
            issues = issues.Where(x => x.CreatedAt > since && !excludedUsers.Contains(x.User.Login)).ToList();
            foreach (Issue issue in issues)
            {
                string key = $"{repo}#{issue.Number}";
                int remainingChars = 140 - (key.Length + 25);
                string value = $"{(issue.Title.Length <= remainingChars ? issue.Title : issue.Title.Substring(0, remainingChars))}\r\n{key} {issue.HtmlUrl}";
                tweets.Add(new KeyValuePair<string, string>(key, value));
            }
            return tweets;
        }
    }
}
