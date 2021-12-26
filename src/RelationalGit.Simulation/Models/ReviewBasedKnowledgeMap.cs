using System;
using System.Collections.Generic;
using System.Linq;

using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    public class ReviewBasedKnowledgeMap
    {
        private readonly HashSet<string> _mapReviews = new HashSet<string>();

        private readonly Dictionary<string, List<PullRequest>> _mapDeveloperReview = new Dictionary<string, List<PullRequest>>();

        private readonly Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>> _map = new Dictionary<string, Dictionary<string, DeveloperFileReveiewDetail>>();

        private static List<PullRequest> _emptyList = new List<PullRequest>(0);

        public Dictionary<string, DeveloperFileReveiewDetail> this[string filePath]
        {
            get
            {
                return _map.GetValueOrDefault(filePath);
            }
        }

        public IEnumerable<DeveloperFileReveiewDetail> Details => _map.Values.SelectMany(q => q.Values);

        public void Add(string filePath, IEnumerable<Developer> reviewersNamesOfPullRequest, PullRequest pullRequest, Period period)
        {
            if (filePath == null)
            {
                return;
            }

            if (!_map.ContainsKey(filePath))
            {
                _map[filePath] = new Dictionary<string, DeveloperFileReveiewDetail>();
            }

            foreach (var reviewer in reviewersNamesOfPullRequest)
            {
                AssignKnowledgeToReviewer(pullRequest, reviewer, period, filePath);
            }
        }

        internal IEnumerable<DeveloperFileReveiewDetail> GetReviewersOfPeriod(long periodId)
        {
            return _map.Values.SelectMany(q => q.Values.Where(c => c.Periods.Any(p => p.Id == periodId)));
        }

        private void AssignKnowledgeToReviewer(PullRequest pullRequest, Developer reviewer, Period period, string filePath)
        {
            var reviewerName = reviewer.NormalizedName;

            if (!_map[filePath].ContainsKey(reviewerName))
            {
                _map[filePath][reviewerName] = new DeveloperFileReveiewDetail()
                {
                    FilePath = filePath,
                    Developer = reviewer
                };
            }

            if (!_map[filePath][reviewerName].Periods.Any(q => q.Id == period.Id))
            {
                _map[filePath][reviewerName].Periods.Add(period);
            }

            _map[filePath][reviewerName].PullRequests.Add(pullRequest);

            UpdateDeveloperReviews(pullRequest,reviewerName);
        }

        private void UpdateDeveloperReviews(PullRequest pullRequest, string reviewerName)
        {
            if (_mapReviews.Contains(reviewerName + pullRequest.Number))
                return;


            _mapReviews.Add(reviewerName + pullRequest.Number);

            if (!_mapDeveloperReview.ContainsKey(reviewerName))
            {
                _mapDeveloperReview[reviewerName] = new List<PullRequest>();
            }
            if (!_mapDeveloperReview[reviewerName].Contains(pullRequest))
                _mapDeveloperReview[reviewerName].Add(pullRequest);

        }


        public Dictionary<string, DeveloperFileReveiewDetail> GetReviewsOfFile(string filePath)
        {
            return _map.GetValueOrDefault(filePath);
        }

        public List<PullRequest> GetDeveloperReviews(string reviewerName)
        {
            if (_mapDeveloperReview.ContainsKey(reviewerName))
            {
                return _mapDeveloperReview[reviewerName];
            }

            return _emptyList;
        }

        public Dictionary<string, List<PullRequest>> GetReviewers()
        {
            return _mapDeveloperReview;
        }
        //Fahimeh
        public Dictionary<int, DateTime> GetReviowersOnFile(string normalizedName, string path)
        {

            var developersFileReviews = _map.GetValueOrDefault(path);

            if (developersFileReviews == null)
            {
                return new Dictionary<int, DateTime>
                {
                    [0] = DateTime.MinValue,
                };
            }
            var number2 = developersFileReviews.Where(q => q.Key == normalizedName);
            if (number2.Count() == 0)
            {
                return new Dictionary<int, DateTime>
                {
                    [0] = DateTime.MinValue,
                };
            }

            var recency = number2.Select(a => a.Value.PullRequests.Max(b => b.CreatedAtDateTime)).FirstOrDefault();

            return new Dictionary<int, DateTime>
            {
                [number2.Count()] = recency ?? DateTime.MinValue,
            };
        }
    }
}
