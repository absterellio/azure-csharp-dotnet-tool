using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cli_repo_program.helpers
{
    /**
      * 2.15.23 AG
      **/
    public class ErrorHandler : IErrorHandler
    {
        public void HandleError(string error)
        {
            Console.WriteLine("\nAn error occurred: " + error);
        }

        public void ThrowFileOpenException()
        {
            Console.WriteLine("\nFile open exception. Make sure the output file is not already in use and that the output path is valid.");
        }

        public void ThrowURIException()
        {
            Console.WriteLine("\nURI format exception. Please ensure the OrganizationURL specified in ConstantValues.cs is valid.");
        }

        public void ShowWorkItemIssue(int workItemId)
        {
            Console.WriteLine($"\nUnable to retrieve pull requests for work item {workItemId}");
        }
    }
}
