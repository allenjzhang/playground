using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using projectutil;
using System.Reflection.Metadata.Ecma335;

const string authToken = "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";
const string AzureOrg = "Azure";
const string AzureCadlRepo = "cadl-azure";
const string MicrosoftOrg = "Microsoft";
const string MicrosoftCadlRepo = "cadl";
const string CadlProjectId = "PN_kwDOAGhwUs4ABJ0D";
//var cadl_azureRepoId = 92878788;
//var cadl_RepoId = 381857226;

var graphQLClient = new GraphQLHttpClient("https://api.github.com/graphql", new NewtonsoftJsonSerializer());
graphQLClient.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", authToken);
graphQLClient.HttpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("TestApp", "1.0"));

//await InitialImportAllIssues(AzureOrg, AzureCadlRepo);
  await ImportNewIssues(AzureOrg, AzureCadlRepo);
//await InitialImportAllIssues(MicrosoftOrg, MicrosoftCadlRepo);

async Task InitialImportAllIssues(string org, string repo)
{
    var allOpenIssues = graphQLClient.GetRepoIssuesAsync(org, repo);
    await foreach (var issue in allOpenIssues)
    {
        Console.WriteLine($"Processing issue {issue.Url}");

        // Skip importing epic
        if (issue.Labels.Contains(Label.Epic))
        {
            continue;
        }

        var itemId = await graphQLClient.AddIssueToProjectAsync(CadlProjectId, issue.Id);
        await ProcessProjectItem(issue, itemId);
        break;
    }
}

async Task ImportNewIssues(string org, string repo)
{
    var allOpenIssues = graphQLClient.GetNewRepoIssuesAsync(org, repo, DateTime.Now.AddDays(-5));
    await foreach (var issue in allOpenIssues)
    {
        Console.WriteLine($"Processing issue {issue.Url}");

        // Skip importing epic
        if (issue.Labels.Contains(Label.Epic))
        {
            continue;
        }

        Console.WriteLine($"Adding new issue: {issue.Url}");
        var itemId = await graphQLClient.AddIssueToProjectAsync(CadlProjectId, issue.Id);
        await ProcessProjectItem(issue, itemId);
    }
}

async Task ProcessProjectItem(Issue issue, string projectItemId)
{
    if (issue.State == "OPEN")
    {
        if (issue.MileStone == MileStone.May2022)
            await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Status, Status.InProgress);
        else
            await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Status, Status.Ready);

        _ = issue.MileStone switch
        {
            MileStone.May2022 => await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Sprint, Sprint.May2022),
            MileStone.June2022 => await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Sprint, Sprint.June2022),
            MileStone.July2022 => await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Sprint, Sprint.July2022),
            MileStone.August2022 => await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Sprint, Sprint.August2022),
            MileStone.September2022 => await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Sprint, Sprint.September2022),
            MileStone.October2022 => await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Sprint, Sprint.October2022),
            _ => "",
        };
    }
    else
    {
        await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Status, Status.Done);
    }

    if (issue.Labels.Contains(Label.Bug))
    {
        await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.FeatureArea, Feature.Bug);
    }
    if (issue.Labels.Contains(Label.CADLDesign))
    {
        await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.FeatureArea, Feature.Design);
    }
    if (issue.Labels.Contains(Label.V1Preview))
    {
        await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Semester, Semester.Nickle);
    }
    if (issue.Labels.Contains(Label.IDE))
    {
        await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.FeatureArea, Feature.IDE);
    }
    if (issue.Labels.Contains(Label.Copper))
    {
        await graphQLClient.UpdateProjectItemAsync(CadlProjectId, projectItemId, ProjectField.Semester, Semester.Copper);
    }
}