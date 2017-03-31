using LinqToTwitter;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueTweeter.Domain
{
    public class Twitter
    {
        private readonly TwitterContext _context;
        private readonly string _twitterUser;

        public Twitter(Configuration configuration)
        {
            _twitterUser = configuration.TwitterUser;
            SingleUserAuthorizer auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = configuration.TwitterConsumerKey,
                    ConsumerSecret = configuration.TwitterConsumerSecret,
                    AccessToken = configuration.TwitterAccessToken,
                    AccessTokenSecret = configuration.TwitterAccessTokenSecret
                }
            };
            _context = new TwitterContext(auth);
        }

        public async Task<List<Status>> GetRecentTweets(int count)
        {
            return await _context.Status
                .Where(
                    x => x.Type == StatusType.User
                    && x.ScreenName == _twitterUser
                    && x.Count == count)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<Status>> SendTweets(IEnumerable<string> tweets)
        {
            List<Task<Status>> tweetTasks = tweets
                .Select(x => _context.TweetAsync(x))
                .ToList();
            await Task.WhenAll(tweetTasks).ConfigureAwait(false);
            return tweetTasks.Select(x => x.Result).ToList();
        }
    }
}
