
namespace cli_repo_program.models
{
    public static class ConstantValues
    {
        public const string OrganizationURL = "[insert URL]";

        public const string PlatformTeamName = "[insert team name]";

        public enum ErrorType
        {
            Unknown,
            IterationPath,
            PersonalAccessToken,
            OutputFileUnavailable,
            OutputFileInvalid,
            URIInvalid
        };
    }
}
