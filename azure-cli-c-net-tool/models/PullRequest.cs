namespace cli_repo_program.models
{
    public class PullRequest
    {
        public List<string> Reviewers { get; set; } = new List<string>();

        public string ID { get; set; } = "";

        public string TargetBranch { get; set; } = "";

        public string Description { get; set; } = "";

        public string TargetRepository { get; set; } = "";

        public int WorkItemId { get; set; }

    }
}
