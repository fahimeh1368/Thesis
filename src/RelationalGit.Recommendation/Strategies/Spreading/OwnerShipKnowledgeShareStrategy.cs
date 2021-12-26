using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System.Linq;

using RelationalGit.Data;
using System;
using System.Collections.Generic;

namespace RelationalGit.Recommendation
{
    public class OwnerShipKnowledgeShareStrategy : ScoreBasedRecommendationStrategy
    {
        private GitRepositoryDbContext _dbContext;
        private string knowledgeSaveReviewerReplacementType;
        private ILogger logger;
        private string pullRequestReviewerSelectionStrategy;
        private bool? addOnlyToUnsafePullrequests;
        private string recommenderOption;
        private bool changePast;

        public OwnerShipKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType, ILogger logger, string pullRequestReviewerSelectionStrategy, bool? addOnlyToUnsafePullrequests, string recommenderOption, bool changePast) : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast)
        {
            this.knowledgeSaveReviewerReplacementType = knowledgeSaveReviewerReplacementType;
            this.logger = logger;
            this.pullRequestReviewerSelectionStrategy = pullRequestReviewerSelectionStrategy;
            this.addOnlyToUnsafePullrequests = addOnlyToUnsafePullrequests;
            this.recommenderOption = recommenderOption;
            this.changePast = changePast;
            _dbContext = new GitRepositoryDbContext(false);
        }
        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var totalCommits = pullRequestContext.PullRequestKnowledgeables.Sum(q=>q.NumberOfCommits);
            var totalReviews = pullRequestContext.PullRequestKnowledgeables.Sum(q => q.NumberOfReviews);
            if (totalCommits==0 || totalReviews==0)
            {
                return 0;
            }
            var score = reviewer.NumberOfCommits / (double)totalCommits + reviewer.NumberOfReviews / (double)totalReviews;
            return score;
        }
        private long GetLoadScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var reviwes = new List<long>();
            var overitems = pullRequestContext.Overlap;
            if (overitems.Count == 0)
                return 0;

            foreach (var pullreq in pullRequestContext.KnowledgeMap.ReviewBasedKnowledgeMap.GetDeveloperReviews(reviewer.DeveloperName))
            {
                reviwes.Add(pullreq.Number);
            }
            var count = overitems.Intersect(reviwes);
            return count.Count();
        }
    }
}
