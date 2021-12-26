using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class WhoDoRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;
        private double _alpha;
        private double _beta;
        private int _riskOwenershipThreshold;
        private double _hoarderRatio;
        public WhoDoRecommendationStrategy(string knowledgeSaveReviewerReplacementType,
            ILogger logger, int? numberOfPeriodsForCalculatingProbabilityOfStay,
            string pullRequestReviewerSelectionStrategy,
            bool? addOnlyToUnsafePullrequests,
            string recommenderOption, bool changePast)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast)
        {
            _numberOfPeriodsForCalculatingProbabilityOfStay = numberOfPeriodsForCalculatingProbabilityOfStay;

            var parameters = GetParameters(recommenderOption);
            _alpha = parameters.Alpha;
            _beta = parameters.Beta;
            _riskOwenershipThreshold = parameters.RiskOwenershipThreshold;
            _hoarderRatio = parameters.HoarderRatio;
        }

        private double ComputeCommitScore(PullRequestContext pullRequestContext, string filePath, DeveloperKnowledge reviewer)
        {
            double score = 0.0;

            DateTime reviewer_recency = DateTime.MinValue;
            DateTime nowTime = pullRequestContext.PullRequest.CreatedAtDateTime ?? DateTime.Now;
            var reviewerCommits = pullRequestContext.KnowledgeMap.CommitBasedKnowledgeMap.GetDeveloperCommitsOnFile(reviewer.DeveloperName, filePath, nowTime, reviewer_recency);
            if (reviewerCommits == 0)
            {
                return score;
            }

            score = reviewerCommits;
            if (reviewer_recency != DateTime.MinValue)
            {
                var diff_review = (nowTime - reviewer_recency).TotalDays == 0 ? 1 : (nowTime - reviewer_recency).TotalDays;

                score = score / diff_review;
            }
            return score;
        }
        private double ComputeReviewScore(PullRequestContext pullRequestContext, string filePath, DeveloperKnowledge reviewer)
        {
            double score = 0.0;
            DateTime nowTime = pullRequestContext.PullRequest.CreatedAtDateTime ?? DateTime.Now;
            var data = pullRequestContext.KnowledgeMap.ReviewBasedKnowledgeMap.GetReviowersOnFile(reviewer.DeveloperName, filePath);

            var reviewNumber = data.Keys.FirstOrDefault();

            // var reviewerExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetReviewerExpertise(filePath, reviewer.DeveloperName);

            if (reviewNumber == 0)
            {
                return score;
            }
            score = reviewNumber;
            var recency = data.Values.FirstOrDefault();

            if (recency != DateTime.MinValue)
            {
                var diff_review = (nowTime - recency).TotalDays == 0 ? 1 : (nowTime - recency).TotalDays;
                score = score / diff_review;
            }
            return score;
        }




        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            double? commit_score = 0.0;
            double? review_score = 0.0;
            double? neighber_commit_score = 0.0;
            double? neighber_review_score = 0.0;

            foreach (var pullRequestFile in pullRequestContext.PullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);
                if (canonicalPath == null)
                {
                    continue;
                }

                commit_score += ComputeCommitScore(pullRequestContext, canonicalPath, reviewer);

                review_score += ComputeReviewScore(pullRequestContext, canonicalPath, reviewer);
                var blameSnapshot = pullRequestContext.KnowledgeMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(pullRequestContext.PullRequestPeriod.Id);
                var neighbors = blameSnapshot.Trie.GetFileNeighbors(1, canonicalPath);
                for (int j = 0; j < neighbors.Length; j++)
                {
                    var neighber_file = neighbors[j];
                    neighber_commit_score += ComputeCommitScore(pullRequestContext, neighber_file, reviewer);
                    neighber_review_score += ComputeReviewScore(pullRequestContext, neighber_file, reviewer);
                }
            }
            double loadscore = GetLoadScore(pullRequestContext, reviewer);
            var load = Math.Pow(Math.E, (0.5 * loadscore));

            double final_score = Convert.ToDouble(review_score + commit_score + neighber_commit_score + neighber_review_score) / load;
            double final_score_noload = Convert.ToDouble(review_score + commit_score + neighber_commit_score + neighber_review_score);

            return final_score;

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

        private (double Alpha, double Beta, int RiskOwenershipThreshold, double HoarderRatio) GetParameters(string recommenderOption)
        {
            if (string.IsNullOrEmpty(recommenderOption))
                return (0.5, 1, 3, 0.7);

            var options = recommenderOption.Split(',');
            var alphaOption = options.FirstOrDefault(q => q.StartsWith("alpha")).Substring("alpha".Length + 1);
            var betaOption = options.FirstOrDefault(q => q.StartsWith("beta")).Substring("beta".Length + 1);
            var riskOwenershipThreshold = options.FirstOrDefault(q => q.StartsWith("risk")).Substring("risk".Length + 1);
            var hoarderRatioOption = options.FirstOrDefault(q => q.StartsWith("hoarder_ratio")).Substring("hoarder_ratio".Length + 1);

            return (double.Parse(alphaOption), double.Parse(betaOption), int.Parse(riskOwenershipThreshold), double.Parse(hoarderRatioOption));
        }
    }
}
