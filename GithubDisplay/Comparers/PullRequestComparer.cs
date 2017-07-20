using GithubDisplay.Models;
using System.Collections.Generic;

namespace GithubDisplay.Comparers
{
    public class PullRequestComparer : EqualityComparer<PullRequest>
    {
        public override bool Equals(PullRequest x, PullRequest y) { return x.Number == y.Number; }
        public override int GetHashCode(PullRequest obj) { return obj.Number.GetHashCode(); }

        public static PullRequestComparer Create() { return new PullRequestComparer(); }
    }
}
