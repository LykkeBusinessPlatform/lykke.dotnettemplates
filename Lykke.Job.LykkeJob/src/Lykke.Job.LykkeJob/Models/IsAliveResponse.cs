﻿using System.Collections.Generic;

namespace Lykke.Job.LykkeJob.Models
{
    public class IsAliveResponse
    {
        public string Version { get; set; }
        public string Env { get; set; }
        public IEnumerable<IssueIndicator> IssueIndicators { get; set; }

        public class IssueIndicator
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}