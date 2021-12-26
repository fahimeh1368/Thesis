using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RelationalGit.Data
{
    public class DeveloperReview
    {
        
        public long Id { get; set; }

        public string NormalizedName { get; set; }
        public long SimulationId { get; set; }
        public int PullRequestId { get; set; }
        public DateTime DateTime { get; set; }
        
        //public int CommitNumbers { get; set; }

    }
}

