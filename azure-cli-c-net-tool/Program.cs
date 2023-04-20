using cli_repo_program.helpers;
using cli_repo_program.models;
using cli_repo_program.controllers;
using CommandLine;

namespace ConsoleApp
{
    /**
     * Objective: Given an iteration path, fetch all work items, PR's and commits for that iteration
     * 2/8/23
     */
    class Program
    {
        /**
         * To generate report, pass in iteration path, personal access token, output file path.
         **/
        static void Main(string[] args)
        {
            ErrorHandler e = new ErrorHandler();

            // check iteration parameter was passed
            if (args.Length > 0 && args[0] != null)
            {
                // check PAT was passed
                if (args.Length > 1 && args[1] != null)
                {
                    // check output file was passed
                    if (args.Length > 2 && args[2] != null)
                    {
                        string inputtedIteration = args[0];
                        string personalAccessToken = args[1];
                        string outputFilePath = args[2]; // where to write the output CSV

                        // generate the report
                        IterationLogic il = new IterationLogic(personalAccessToken,outputFilePath);
                        il.GetIterationInformation(inputtedIteration);
                    } else
                    {
                        ReportError((int)ConstantValues.ErrorType.IterationPath, e);
                    }
                } else
                {
                    ReportError((int)ConstantValues.ErrorType.PersonalAccessToken, e);
                }
            }
            else
            {
                ReportError((int)ConstantValues.ErrorType.OutputFile, e);
            }
        }

        private static void ReportError(int errorType, ErrorHandler e) 
        {
            ConstantValues.ErrorType enumVal = (ConstantValues.ErrorType)errorType;

            switch(enumVal)
            {
                case ConstantValues.ErrorType.IterationPath:
                    e.HandleError("No iteration path specified as first argument.");
                    break;
                case ConstantValues.ErrorType.PersonalAccessToken:
                    e.HandleError("No personal access token specified as second argument.");
                    break;
                case ConstantValues.ErrorType.OutputFile:
                    e.HandleError("No output file specified as third argument");
                    break;
                default:
                    e.HandleError("An error occured");
                    break;
            }
        }
    }
}
