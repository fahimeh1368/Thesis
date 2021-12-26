using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    public class PullRequestContext
    {
        private static Dictionary<long, (int TotalReviews, int TotalCommits)> _totalContributionsInPeriodDic = new Dictionary<long, (int TotalReviews, int TotalCommits)>();

        public string PRSubmitterNormalizedName { get; set; }

        public string SelectedReviewersType { get; set; }

        public string Features { get; set; }

        public Developer[] AvailableDevelopers;

        private bool? _isSafe;

        private static Dictionary<string, (int TotalReviews, int TotalCommits)> _contributionsDic = new Dictionary<string, (int TotalReviews, int TotalCommits)>();

        private static CommitComparer _commitComparer = new CommitComparer();

        private static PullRequestComparer _pullRequestComparer = new PullRequestComparer();
        private string[] _riskyFiles;

        public DeveloperKnowledge[] ActualReviewers { get;   set; }

        public PullRequestFile[] PullRequestFiles { get;   set; }

        public PullRequest PullRequest { get;   set; }

        public KnowledgeDistributionMap KnowledgeMap { get;   set; }

        public Dictionary<string, string> CanononicalPathMapper { get;   set; }

        public Period PullRequestPeriod { get;   set; }

        public ReadOnlyDictionary<string, Developer> Developers { get;   set; }

        public BlameSnapshot Blames { get;   set; }

        public DeveloperKnowledge[] PullRequestKnowledgeables { get;   set; }

        public Dictionary<long, Period> Periods { get;   set; }

        public HashSet<string> Hoarders { get;   set; } = null;

        public Dictionary<string,List<string>> _fileOwners { get; set; } = null;

        public List<long> Overlap { get; set; }

       
        public bool PullRequestFilesAreSafe
        {
            get
            {

                if (_isSafe == null)
                {
                    FindHoarders();
                }

                return _isSafe.Value;
            }
        }

        public bool IsHoarder(string normalizedDeveloperName)
        {
            if (Hoarders == null)
            {
                FindHoarders();
            }

            return Hoarders.Contains(normalizedDeveloperName);
        }

        private void FindHoarders()
        {
            Hoarders = new HashSet<string>();
            _fileOwners = new Dictionary<string,List<string>>();

            var availableDevelopersOfPeriod = AvailableDevelopers.Select(q => q.NormalizedName).ToHashSet();
            var blameSnapshot = KnowledgeMap.CommitBasedKnowledgeMap;

            foreach (var pullRequestFile in PullRequestFiles)
            {
                var canonicalPath = CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);

                if (canonicalPath == null)
                {
                    continue;
                }

                var committers = KnowledgeMap.CommitBasedKnowledgeMap[canonicalPath]?.Where(q => q.Value.Developer.LastParticipationDateTime > PullRequest.CreatedAtDateTime)
                    ?.Select(q => q.Value.Developer.NormalizedName) ?? Array.Empty<string>();

                var reviewers = KnowledgeMap.ReviewBasedKnowledgeMap[canonicalPath]?.Where(q => q.Value.Developer.LastParticipationDateTime > PullRequest.CreatedAtDateTime)
                    ?.Select(q => q.Value.Developer.NormalizedName) ?? Array.Empty<string>();

                var availableContributors = committers.Union(reviewers).Where(q => availableDevelopersOfPeriod.Contains(q)).ToArray();

                if (availableContributors.Length < 2)
                {
                    _isSafe = false;

                    if (availableContributors.Length == 1)
                    {
                        Hoarders.Add(availableContributors[0]);
                    }
                }

                foreach (var availableContributor in availableContributors)
                {
                    if (!_fileOwners.ContainsKey(canonicalPath))
                    {
                        _fileOwners[canonicalPath] = new List<string>();
                    }

                    _fileOwners[canonicalPath].Add(availableContributor);
                }
            }

            if (!_isSafe.HasValue)
            {
                _isSafe = true;
            }
        }
      
        public DateTime? GetLastContribution(Developer developer)
        {
            if (developer.LastCommitDateTime == null)
            {
                return developer.LastReviewDateTime;
            }
            if (developer.LastReviewDateTime == null)
            {
                return developer.LastCommitDateTime;
            }
            if (developer.LastCommitDateTime < developer.LastReviewDateTime)
            {
                return developer.LastReviewDateTime;
            }
            else
            {
                return developer.LastCommitDateTime;
            }
        }
        public string[] GetRiskyFiles(int riskOwnershipTreshold)
        {
            if (Hoarders == null)
            {
                FindHoarders();
            }

            if(_riskyFiles == null)
            {
                _riskyFiles = _fileOwners.Where(q => q.Value.Count() < riskOwnershipTreshold).Select(q => q.Key).ToArray();
            }

            return _riskyFiles;
        }

        public bool? IsRisky()
        {
            if (_riskyFiles == null)
                return null;
            
            return _riskyFiles.Length > 0;
        }

        public double GetEffort(string developer, int numberOfPeriodsForCalculatingProbabilityOfStay)
        {
            var lastYear = PullRequest.CreatedAtDateTime.Value.Subtract(TimeSpan.FromDays(365));
            //var lastMonth= PullRequest.CreatedAtDateTime.Value.Subtract(TimeSpan.FromDays(30));
            var totalContribution = GetTotalContributionsBestweenPeriods(lastYear, PullRequest.CreatedAtDateTime.Value);
            var developerTotalContribution = GetDeveloperTotalContributionsBestweenPeriods(lastYear, PullRequest.CreatedAtDateTime.Value, developer);
          
                    return ((developerTotalContribution.TotalReviews) + developerTotalContribution.TotalCommits)
                / (double)((totalContribution.TotalReviews) + totalContribution.TotalCommits);
        }
       

        private (int TotalReviews, int TotalCommits) GetDeveloperTotalContributionsBestweenPeriods(DateTime from, DateTime to, string developer)
        {
            var totalCommits = 0;
            var commits = KnowledgeMap.CommitBasedKnowledgeMap.GetDeveloperCommits(developer);
            var baselineCommit = new Commit() { AuthorDateTime = from };
            var index = commits.BinarySearch(baselineCommit,_commitComparer);

            if (index < 0)
                index = ~index;
            totalCommits += commits.Count - index;

            int totalReviews = 0;
            var reviews = KnowledgeMap.ReviewBasedKnowledgeMap.GetDeveloperReviews(developer);
          
            var baselinePullRequest = new PullRequest() { CreatedAtDateTime = from };

            index = reviews.BinarySearch(baselinePullRequest,_pullRequestComparer);

            if (index < 0)
                index = ~index;
            totalReviews += reviews.Count - index;


            return (totalReviews, totalCommits);
        }

        private (double? TotalReviews, double TotalCommits) GetDeveloperTotalContributionsBestweenPeriods_(DateTime from, DateTime to, string developer)
        {
            var totalCommits = 0.0;
            var commits = KnowledgeMap.CommitBasedKnowledgeMap.GetDeveloperCommits(developer);
            var related_commits = commits.Where(a => a.AuthorDateTime >= from);
            if (related_commits.Count() != 0)
            {
                var test = related_commits.GroupBy(x => new { x.AuthorDateTime.Year, x.AuthorDateTime.Month }).Select(q => new { key = q.Key, count = q.Count() }).ToArray();
                var grouped_commit = related_commits.GroupBy(x => new { x.AuthorDateTime.Year, x.AuthorDateTime.Month }).Select(q => new { key = q.Key, count = q.Count() });
                var max_date_month = related_commits.Max(a => a.AuthorDateTime).Month;
                var max_year = related_commits.Max(a => a.AuthorDateTime).Year;
                var min_year = related_commits.Min(a => a.AuthorDateTime).Year;

                var i = max_date_month;
                var months = grouped_commit.Select(a => a.key);
                int[] priority_months = new int[14];
                var index = 1;
                for (int j = i; j > 1; j--)
                {

                    if (months.FirstOrDefault(a => a.Year == max_year & a.Month == j) != null)
                    {
                        priority_months[index] = grouped_commit.FirstOrDefault(a => a.key.Year == max_year & a.key.Month == j).count;

                    }
                    else
                    {
                        priority_months[index] = 0;
                    }
                    index++;
                }
                if (max_year != min_year)
                {
                    for (int l = 12; l >= i; l--)
                    {
                        if (months.FirstOrDefault(a => a.Year == min_year & a.Month == l) != null)
                        {
                            priority_months[index] = grouped_commit.FirstOrDefault(a => a.key.Year == min_year & a.key.Month == l).count;

                        }
                        else
                        {
                            priority_months[index] = 0;
                        }
                        index++;
                    }
                }

                for (int p = 1; p < 13; p++)
                {

                    totalCommits += priority_months[p] / Math.Pow(Math.E, (p));
                }
            }


            double? totalReviews = 0.0;
            var reviews = KnowledgeMap.ReviewBasedKnowledgeMap.GetDeveloperReviews(developer);

            var related_reviews = reviews.Where(a => a.CreatedAtDateTime >= from);
            if (related_reviews.Count()  != 0)
            {
                var grouped_r = related_reviews.GroupBy(x => new { x.CreatedAtDateTime?.Year, x.CreatedAtDateTime?.Month }).ToArray();
                var grouped_review = related_reviews.GroupBy(x => new { x.CreatedAtDateTime?.Year, x.CreatedAtDateTime?.Month }).Select(q => new { key = q.Key, count = q.Count() });
                var max_date_month_review = related_reviews.Max(a => a.CreatedAtDateTime)?.Month;
                var max_year_r = related_reviews.Max(a => a.CreatedAtDateTime)?.Year;
                var min_year_r = related_reviews.Min(a => a.CreatedAtDateTime)?.Year;

                var i_r = max_date_month_review ?? 0;
                var months_r = grouped_r.Select(a => a.Key);
                int[] priority_months_r = new int[14];
                var index_r = 1;
                for (int q = i_r; q >= 1; q--)
                {

                    if (months_r.FirstOrDefault(a => a.Year == max_year_r & a.Month == q) != null)
                    {
                        priority_months_r[index_r] = grouped_review.Where(a => a.key.Year == max_year_r & a.key.Month == q).FirstOrDefault().count;

                    }
                    else
                    {
                        priority_months_r[index_r] = 0;
                    }
                    index_r++;
                }
                if (max_year_r != min_year_r)
                {
                    for (int b = 12; b >= i_r; b--)
                    {
                        if (months_r.FirstOrDefault(a => a.Year == min_year_r & a.Month == b) != null)
                        {
                            priority_months_r[index_r] = grouped_review.Where(a => a.key.Month == b & a.key.Year == min_year_r).FirstOrDefault().count;

                        }
                        else
                        {
                            priority_months_r[index_r] = 0;
                        }
                        index_r++;
                    }
                }
                for (int k = 1; k < 13; k++)
                {

                    totalReviews += priority_months_r[k] / Math.Pow(Math.E, (k));
                }

            }

            return (totalReviews, totalCommits);
        }

        private (int TotalReviews, int TotalCommits) GetTotalContributionsBestweenPeriods(DateTime from, DateTime to)
        {
            var totalCommits = 0;
            var committers = KnowledgeMap.CommitBasedKnowledgeMap.GetCommitters();
            var baselineCommit = new Commit() { AuthorDateTime = from };

            foreach (var committer in committers)
            {
                var index = committer.Value.BinarySearch(baselineCommit,_commitComparer);

                if (index < 0)
                    index = ~index;
                totalCommits += committer.Value.Count - index;
            }

            var totalReviews = 0;
            var reviewers = KnowledgeMap.ReviewBasedKnowledgeMap.GetReviewers();
            var baselinePullRequest = new PullRequest() { CreatedAtDateTime = from };

            foreach (var reviewer in reviewers)
            {
                var index = reviewer.Value.BinarySearch(baselinePullRequest,_pullRequestComparer);

                if (index < 0)
                    index = ~index;
                totalReviews += reviewer.Value.Count - index;
            }       

            return (totalReviews, totalCommits);
        }

        public double GetProbabilityOfStay(string reviewer, int numberOfPeriodsForCalculatingProbabilityOfStay)
        {
            var currentPeriodId = PullRequestPeriod.Id;
            var lastYear = PullRequest.CreatedAtDateTime.Value.Subtract(TimeSpan.FromDays(365));

            var commitMonths = KnowledgeMap.CommitBasedKnowledgeMap.GetDeveloperCommits(reviewer)
                .Where(q=>q.AuthorDateTime>=lastYear)
                .Select(q=>q.AuthorDateTime.Month);
            
            var reviewMonths = KnowledgeMap.ReviewBasedKnowledgeMap.GetDeveloperReviews(reviewer)
                .Where(q => q.CreatedAtDateTime >= lastYear)
                .Select(q => q.CreatedAtDateTime.Value.Month);

            var numberOfContributedPeriodsSoFar = commitMonths.Union(reviewMonths).Count();

            return numberOfContributedPeriodsSoFar / (double) 12.0;
        }

        public double _GetProbabilityOfStay(string reviewer, int numberOfPeriodsForCalculatingProbabilityOfStay)
        {
            var currentPeriodId = PullRequestPeriod.Id;
            var lastmonth = PullRequest.CreatedAtDateTime.Value.Subtract(TimeSpan.FromDays(30));

            var commitMonths = KnowledgeMap.CommitBasedKnowledgeMap.GetDeveloperCommits(reviewer)
                .Where(q => q.AuthorDateTime >= lastmonth)
                .Select(q => q.AuthorDateTime.Day);

            var reviewMonths = KnowledgeMap.ReviewBasedKnowledgeMap.GetDeveloperReviews(reviewer)
                .Where(q => q.CreatedAtDateTime >= lastmonth)
                .Select(q => q.CreatedAtDateTime.Value.Day);

            var numberOfContributedPeriodsSoFar = commitMonths.Union(reviewMonths).Count();

            return numberOfContributedPeriodsSoFar / (double)30.0;
        }

        private (int TotalReviews, int TotalCommits) GetTotalContributionsOfPeriod(long periodId)
        {
            if (!_totalContributionsInPeriodDic.ContainsKey(periodId))
            {
                var totalCommits = Developers.Sum(q => q.Value.ContributionsPerPeriod.GetValueOrDefault(periodId)?.TotalCommits ?? 0);
                var totalReviews = Developers.Sum(q => q.Value.ContributionsPerPeriod.GetValueOrDefault(periodId)?.TotalReviews ?? 0);

                _totalContributionsInPeriodDic[periodId] = (totalReviews, totalCommits);
            }

            return _totalContributionsInPeriodDic[periodId];
        }
    }

    public class CommitComparer : IComparer<Commit>
    {
        public int Compare(Commit x, Commit y)
        {
            return x.AuthorDateTime.CompareTo(y.AuthorDateTime);
        }
    }

    public class PullRequestComparer : IComparer<PullRequest>
    {
        public int Compare(PullRequest x, PullRequest y)
        {
            return x.CreatedAtDateTime.Value.CompareTo(y.CreatedAtDateTime);
        }
    }
}
