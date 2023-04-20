**Overview:**
This project is a C# and .NET application that takes in a TFS iteration path and returns a report of TFS work items, Git pull requests and other details from that iteration. This report is generated as a .csv file with columns for WorkItem, Pull Request, Reviewers, and Target Branch. 

**To use this program:**
  1. In the ConstantValues.cs file, replace the 'OrganizationURL' and 'PlatformTeamName' with values from your own Team Foundation instance.
  2. Create a personal access token for using the Azure Devops Service .NET SDK (see 
     https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=Windows for more information)
  3. This program is run from the command line. From the command line and in the directory for this project, run "**dotnet run [full-iteration-path] [personal-access-token] [output-file-path.csv]**", replacing the bracketed values with your unique iteration path, personal access token and desired output file path.
