using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Build.WebApi;
using cli_repo_program.helpers;
using cli_repo_program.models;
using System.Globalization;
using CsvHelper;

namespace cli_repo_program.controllers
{
    /**
     * 2.15.23 AG
     * 
     **/
    public class IterationLogic
    {

        private readonly VssConnection connection;
        private readonly ErrorHandler errorHandler;
        private readonly TeamContext teamContext;
        private readonly CsvWriter csvWriter;

        public IterationLogic(string personalAccessToken, string outputFilePath)
        {
            // setup local error handler
            this.errorHandler = new ErrorHandler();

            // create a connection
            Uri orgUrl = new Uri(ConstantValues.OrganizationURL);
            this.connection = new VssConnection(orgUrl, new VssBasicCredential(string.Empty, personalAccessToken));

            try
            {
                var streamWriter = new StreamWriter(outputFilePath);
                this.csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            }
            // handle I/O errors
            catch (Exception)
            {
                this.errorHandler.ThrowFileOpenException();
                Environment.Exit(0);
            }

            teamContext = new TeamContext(ConstantValues.PlatformTeamName);
        }

        /**
         * This method takes in an iteration path and calls methods to output details from the iteration to a csv file.
         **/
        public void GetIterationInformation(string iterationPath)
        {
            // get and output details for the iteration
            var iteration = GetIterationDetails(iterationPath);

            if (iteration != null)
            {

                PrintCsvHeaders(iteration);
                if (iteration != null)
                {
                    // get the work items for this iteration
                    var iterationWorkItems = GetIterationWorkItems(iteration);

                    if (iterationWorkItems != null)
                    {
                        iterationWorkItems.WorkItemRelations.ForEach(iterationWorkItem =>
                        {
                            // get details for the work item
                            Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem workItem = ShowWorkItemDetails(iterationWorkItem.Target.Id);

                            this.csvWriter.NextRecord();

                            var outputString = "";

                            // output the work item's values (removed for now)
                            /*foreach (var field in workItem.Fields)
                            {
                                outputString = outputString + field.Key + ": " + field.Value + " ";
                            }*/

                            // print out each workItemId
                            outputString = workItem.Id.ToString();
                            this.csvWriter.WriteField(outputString);
                            csvWriter.NextRecord();

                            // get pull requests associated with this workitem
                            var prs = GetPullRequests(connection, iterationWorkItem.Target.Id);

                            prs.ForEach(record =>
                            {
                                // filter out these PR's by target branch of main
                                if (record.TargetBranch != null && record.TargetBranch.Contains("main"))
                                {
                                    // print out each pull request information
                                    csvWriter.NextRecord();
                                    this.csvWriter.WriteField("");
                                    this.csvWriter.WriteField(record.ID);
                                    this.csvWriter.WriteField(FormatReviewers(record.Reviewers));
                                    this.csvWriter.WriteField(record.TargetBranch);

                                    // get all the commits for this pull request (unused for now)
                                    // GetPullRequestCommits(record.Value.Name, record.Key.Id, record.Value.ProjectReference.Name);
                                }
                            });
                            csvWriter.NextRecord();
                        });
                    }
                }
            }
            csvWriter.Flush();
        }

        /**
         * Helper method to output the headers of the csv file.
         **/
        private void PrintCsvHeaders(TeamSettingsIteration iteration)
        {
            // print out the iteration path into csv
            this.csvWriter.WriteField("Iteration: " + iteration.Path);
            this.csvWriter.NextRecord();
            this.csvWriter.NextRecord();

            // print csv headers
            this.csvWriter.WriteField("Work Item");
            this.csvWriter.WriteField("Pull Request");
            this.csvWriter.WriteField("Approver(s)");
            this.csvWriter.WriteField("Target Branch");
            this.csvWriter.NextRecord();
        }

        /**
         * Helper method to format the list of review names as a string. 
         **/
        private string FormatReviewers(List<string> reviewerNames)
        {
            var reviewerString = "";

            reviewerNames.ForEach(name =>
            {
                reviewerString = reviewerString + name + " ";
            });

            return reviewerString;
        }

        /**
         * This returns information about an iteration given an iteration path string.
         **/
        private TeamSettingsIteration? GetIterationDetails(string iterationPath)
        {
            WorkHttpClient workHttpClient = connection.GetClient<WorkHttpClient>();
            TeamSettingsIteration iterationMatch = new TeamSettingsIteration();

            List<TeamSettingsIteration> results = workHttpClient.GetTeamIterationsAsync(teamContext).Result;

            if(results)
            {
                // get the iteration that matches the inputted string iteration path
                iterationMatch = results.FirstOrDefault(x => x.Path == iterationPath);

                if (iterationMatch != null)
                {
                    return iterationMatch;
                }
                else
                {
                    errorHandler.HandleError("No iteration found");
                }
            }

            return iterationMatch;
        }

        /**
         * This method pulls and outputs the work items associated with a given TFS iteration.
         **/
        private IterationWorkItems GetIterationWorkItems(TeamSettingsIteration iteration)
        {
            var workItems = new IterationWorkItems();

            try
            {
                // fetch all work items for an iteration
                WorkHttpClient workHttpClient = this.connection.GetClient<WorkHttpClient>();
                workItems = workHttpClient.GetIterationWorkItemsAsync(teamContext, iteration.Id).Result;

            } catch (Exception ex)
            {
                errorHandler.HandleError(ex.Message);
            }

            return workItems;
        }

        /**
         * This methods retrieves and outputs the pull requests associated with a given work item. Builds
         * and returns a list of PullRequest models for future use.
         * 
         * References: https://stackoverflow.com/questions/62266401/retrieve-pull-request-from-work-item-in-azure-devops-webapi
         **/
        private List<cli_repo_program.models.PullRequest> GetPullRequests(VssConnection connection, int workItemId)
        {
            // create clients to use
            var witClient = connection.GetClient<WorkItemTrackingHttpClient>();
            var gitHttpClient = connection.GetClient<GitHttpClient>();

            // gets the work item based on the id
            var workItem = witClient.GetWorkItemAsync(workItemId, expand: Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemExpand.Relations).Result;
            var pullRequests = new List<cli_repo_program.models.PullRequest>();

            if (workItem.Relations != null)
            {
                foreach (var relation in workItem.Relations)
                {
                    // for each Pull Request associated with this work item, map the properties to a new Pull Request object
                    if ((string)relation.Attributes["name"] == "Pull Request")
                    {
                        var segment = relation.Url.Split("/").Last();
                        var ids = segment.Split("%2F");

                        // unused for now
                        // var repo = gitHttpClient.GetRepositoryAsync(ids[1]).Result;
                        // Console.WriteLine(repo.Name);

                        // get further PR details based on the pull request id
                        var gitPullRequest = gitHttpClient.GetPullRequestByIdAsync(Int32.Parse(ids[2]));
                        var pullRequestResult = gitPullRequest.Result;

                        // grab the reviewers for this PR
                        var reviewers = new List<string>();
                        pullRequestResult.Reviewers.ForEach(r =>
                        {
                            reviewers.Add(r.DisplayName);
                        });

                        var prObj = new models.PullRequest
                        {
                            WorkItemId = workItemId,
                            ID = pullRequestResult.PullRequestId.ToString(),
                            Reviewers = reviewers,
                            TargetBranch = pullRequestResult.TargetRefName,
                            Description = pullRequestResult.Description,
                        };

                        pullRequests.Add(prObj);
                    }
                }
            }
            else
            {
                errorHandler.ShowWorkItemIssue(workItemId);
            }
            return pullRequests;
        }

        /**
         * This outputs the details for a work item into a csv file.
         * 
         * References: https://github.com/microsoft/azure-devops-dotnet-samples
         **/
        private Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem ShowWorkItemDetails(int workItemId)
        {
            // create an instance of the work item tracking client
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            // specify which fields to returns 
            string[] fields = { "System.Id", "System.Title", "Work Item Type" };
            Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem workItem = new Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem();

            try
            {
                // get the specified work item
                workItem = witClient.GetWorkItemAsync(workItemId, fields).Result;
            }
            catch (AggregateException aex)
            {
                errorHandler.HandleError(aex.Message);
            }

            return workItem;
        }

        /**
         * This method pulls and outputs the details of a commit to the csv file.
         **/
        private void GetPullRequestCommits(string repoName, string pullRequestId, string projectName)
        {
            var gitClient = connection.GetClient<GitHttpClient>();

            try
            {
                var prId = Int32.Parse(pullRequestId);

                // get commits for this pull request based on the id, project and repository
                var commits = gitClient.GetPullRequestCommitsAsync(projectName, repoName, prId).Result;

                // print commit details
                if (commits != null && commits.Count > 0)
                {
                    this.csvWriter.NextRecord();
                    this.csvWriter.WriteField("");
                    this.csvWriter.WriteField("COMMITS FOR PR " + pullRequestId + ":");

                    foreach (var commit in commits)
                    {
                        this.csvWriter.NextRecord();
                        this.csvWriter.WriteField("");
                        this.csvWriter.WriteField("");
                        this.csvWriter.WriteField("\t\tCommit Id: " + commit.CommitId + ", Commit Comment: " + commit.Comment);
                    }
                }
            }
            catch (Exception e)
            {
                errorHandler.HandleError(e.Message);
            }
        }
    }
}
