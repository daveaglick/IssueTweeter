using LinqToTwitter;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueTweeter.Domain
{
    class Twitter
    {
        // Authorize to Twitter
        static readonly SingleUserAuthorizer TwitterAuth = new SingleUserAuthorizer
        {
            CredentialStore = new SingleUserInMemoryCredentialStore
            {
                ConsumerKey = Configuration.TwitterConsumerKey,
                ConsumerSecret = Configuration.TwitterConsumerSecret,
                AccessToken = Configuration.TwitterAccessToken,
                AccessTokenSecret = Configuration.TwitterAccessTokenSecret
            }
        };

        static readonly TwitterContext Context = new TwitterContext(TwitterAuth);

        internal static async Task<List<Status>> GetRecentTweets(int count)
        {
            return await Context.Status
                .Where(
                    x => x.Type == StatusType.User
                    && x.ScreenName == Configuration.TwitterUser
                    && x.Count == count)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        internal static async Task<List<Status>> SendTweets(IEnumerable<string> tweets)
        {
            List<Task<Status>> tweetTasks = tweets
                .Select(x => Context.TweetAsync(x))
                .ToList();
            await Task.WhenAll(tweetTasks).ConfigureAwait(false);
            return tweetTasks.Select(x => x.Result).ToList();
        }
    }
}
