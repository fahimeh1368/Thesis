using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using RelationalGit.Calculation;

namespace RelationalGit.Commands
{
    public class GetOpenReviewCommand
    {
        private readonly ILogger _logger;

        public GetOpenReviewCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Execute(long actualSimulationId,string analyzeResultPath, string type)
        {
            var analyzer = new AnalyzerOpenReview();
            analyzer.AnalyzeOpenReviewWorkload(actualSimulationId , analyzeResultPath, type);

            
        }
    }
}
