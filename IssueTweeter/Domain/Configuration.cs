using System;
using System.Configuration;
using System.IO;
using System.Linq;
using IssueTweeter.Models;
using LitJson;

namespace IssueTweeter.Domain
{
    public class Configuration
    {
        // GitHub
        public GitHubAccount[] GitHubAccounts { get; }
        public string GitHubToken => ConfigurationManager.AppSettings["GitHubToken"];
        public string[] ExcludedUsers => ConfigurationManager.AppSettings["ExcludedUsers"]
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();

        // Twitter
        public string TwitterUser => ConfigurationManager.AppSettings["TwitterUser"];
        public string TwitterConsumerKey => ConfigurationManager.AppSettings["TwitterConsumerKey"];
        public string TwitterConsumerSecret => ConfigurationManager.AppSettings["TwitterConsumerSecret"];
        public string TwitterAccessToken => ConfigurationManager.AppSettings["TwitterAccessToken"];
        public string TwitterAccessTokenSecret => ConfigurationManager.AppSettings["TwitterAccessTokenSecret"];

        public Configuration()
        {
            GitHubAccounts = JsonMapper.ToObject<GitHubAccount[]>(File.ReadAllText(@"gitHubAccounts.json"));
        }
    }
}
