using Octokit;

namespace GithubDisplay.Services
{
    public class GithubService
    {
        private static GitHubClient _client;

        private static Repository _repo;

        public GitHubClient Client
        {
            get => _client;
            set => _client = value;
        }

        public Repository Repo
        {
            get => _repo;
            set => _repo = value;
        }
    }
}
