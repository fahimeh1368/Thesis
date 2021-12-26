using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using RelationalGit.Data;
using RelationalGit.Gathering.GitHub;
using System.Linq;

namespace RelationalGit.Commands
{
    public class GetPullRequestsCommand
    {
        private readonly ILogger _logger;

        public GetPullRequestsCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(string token, string agenName, string owner, string repo, string branch)
        {
            using (var dbContext = new GitRepositoryDbContext(true))
            {
                dbContext.Database.ExecuteSqlCommand($"TRUNCATE TABLE PullRequests");
                var githubExtractor = new GithubDataFetcher(token, agenName, _logger);
                var pullRequests = await githubExtractor.FetchAllPullRequests(owner, repo).ConfigureAwait(false);
                _logger.LogInformation("{datetime}: trying to save {count} pull requests.", DateTime.Now, pullRequests.Length);
                foreach (PullRequest pullrequest in pullRequests)
                    {
                     var startdate = pullrequest.CreatedAtDateTime;
                     var enddate = pullrequest.ClosedAtDateTime;
                     var overlap = pullRequests.Where(a => a.ClosedAtDateTime > startdate && a.CreatedAtDateTime < enddate && a.Number != pullrequest.Number).ToList();
                        foreach (PullRequest item in overlap)
                        {
                            if (item.Number < pullrequest.Number)
                                pullrequest.OverlapPullRequest = string.Concat(pullrequest.OverlapPullRequest, item.Number.ToString() + ",");
                        }

                    }
                dbContext.AddRange(pullRequests);
                dbContext.SaveChanges();
                _logger.LogInformation("{datetime}: pull requests has been saved successfully.", DateTime.Now);
            }
        }
    }
}
