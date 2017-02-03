using IssueTweeter.Models;
using LitJson;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace IssueTweeter
{
    class Configuration
    {
        // GitHub
        internal static GitHubAccount[] GitHubAccounts = JsonMapper.ToObject<GitHubAccount[]>(File.ReadAllText(@"gitHubAccounts.json"));
        internal static string GitHubToken = ConfigurationManager.AppSettings["GitHubToken"];
        internal static string[] ExcludedUsers = ConfigurationManager.AppSettings["ExcludedUsers"]
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();

        // Twitter
        internal static string TwitterUser = ConfigurationManager.AppSettings["TwitterUser"];
        internal static string TwitterConsumerKey = ConfigurationManager.AppSettings["TwitterConsumerKey"];
        internal static string TwitterConsumerSecret = ConfigurationManager.AppSettings["TwitterConsumerSecret"];
        internal static string TwitterAccessToken = ConfigurationManager.AppSettings["TwitterAccessToken"];
        internal static string TwitterAccessTokenSecret = ConfigurationManager.AppSettings["TwitterAccessTokenSecret"];
    }
}
