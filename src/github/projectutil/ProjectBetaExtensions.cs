using projectutil;
using GraphQL;
using GraphQL.Client.Http;
using Newtonsoft.Json.Linq;

static class ProjectBetaExtensions
{
    public static async Task<string> GetProjectNodeIdAsync(this GraphQLHttpClient client, string owner, int projectNumber)
    {
        var request = new GraphQLRequest
        {
            Query = @"query ($organization: String!, $projectNumber: Int!) {
                        organization(login: $organization) {
                            projectNext(number: $projectNumber) {
                                id
                                title
                            }
                        }
                    }",
            Variables = new { organization = owner, projectNumber = projectNumber }
        };

        var res = await client.SendQueryAsync<JObject>(request);
        CheckErrorResponse(res);

        return (((JObject)res.Data["organization"]["projectNext"]).First as JProperty).Value.ToString();
    }

    public static async Task<string> AddIssueToProjectAsync(this GraphQLHttpClient client, string projectId, string issueId)
    {
        var request = new GraphQLRequest
        {
            Query = @"mutation ($projectId: ID!, $contentId: ID!) {
                      addProjectNextItem(input: {projectId: $projectId, contentId: $contentId}) {
                        projectNextItem {
                          id
                        }
                      }
                    }",
            Variables = new { projectId = projectId, contentId = issueId }
        };

        var res = await client.SendMutationAsync<JObject>(request);
        CheckErrorResponse(res);

        return res.Data["addProjectNextItem"]["projectNextItem"]["id"].Value<string>();
    }

    public static async IAsyncEnumerable<Issue> GetRepoIssuesAsync(this GraphQLHttpClient client, string repoOrg, string repoName, int top = 100, string state = "OPEN")
    {
        var HasMoreData = true;
        var request = new GraphQLRequest
        {
            Query = @"query ($org: String!, $reponame: String!, $top: Int!, $state: [IssueState!]) {
                      repository(owner: $org, name: $reponame) {
                        issues(first:$top, states:$state) {
                            pageInfo {
                                endCursor
                                startCursor
                                hasNextPage
                                hasPreviousPage
                            }
                          nodes {
                            repository {
                              name
                            }
                            id
                            title
                            url
                            state
                            author {
                              login
                            }
                            milestone {
                              title
                            }
                            labels(first:100) {
                              nodes {
                                name
                              }
                            }
                            assignees(first: 10) {
                              nodes {
                                login
                              }
                            }
                          }
                        }
                      }
                    }",
            Variables = new { org = repoOrg, reponame = repoName, top = top, state = new[] { state } }
        };

        while (HasMoreData)
        {
            var res = await client.SendQueryAsync<JObject>(request);
            CheckErrorResponse(res);
            foreach (var issue in res.Data["repository"]["issues"]["nodes"])
            {
                yield return new Issue
                {
                    Id = issue["id"].ToString(),
                    Author = issue["author"]["login"].ToString(),
                    Title = issue["title"].ToString(),
                    Url = issue["url"].ToString(),
                    State = issue["state"].ToString(),
                    MileStone = issue["milestone"].HasValues ? issue["milestone"]["title"].ToString() : null,
                    Labels = (issue["labels"]["nodes"] as JArray).Select(x => x["name"].ToString()).ToArray(),
                    Assignees = (issue["assignees"]["nodes"] as JArray).Select(x => x["login"].ToString()).ToArray(),
                    Repository = repoName
                };
            }
            HasMoreData = res.Data["repository"]["issues"]["pageInfo"]["hasNextPage"].Value<Boolean>();
            var EndCursor = res.Data["repository"]["issues"]["pageInfo"]["endCursor"].Value<string>();
            if (HasMoreData)
            {
                request = new GraphQLRequest
                {
                    Query = @"query ($org: String!, $reponame: String!, $top: Int!, $state: [IssueState!], $endCursor: String!) {
                      repository(owner: $org, name: $reponame) {
                        issues(first:$top, after:$endCursor, states:$state) {
                            pageInfo {
                                endCursor
                                startCursor
                                hasNextPage
                                hasPreviousPage
                            }
                          nodes {
                            repository {
                              name
                            }
                            id
                            title
                            url
                            state
                            author {
                              login
                            }
                            milestone {
                              title
                            }
                            labels(first:100) {
                              nodes {
                                name
                              }
                            }
                            assignees(first: 10) {
                              nodes {
                                login
                              }
                            }
                          }
                        }
                      }
                    }",
                    Variables = new { org = repoOrg, reponame = repoName, top = top, state = new[] { state }, endCursor = EndCursor }
                };
            }
        }
    }

    public static async IAsyncEnumerable<Issue> GetNewRepoIssuesAsync(this GraphQLHttpClient client, string repoOrg, string repoName, DateTime createdAfter, int top = 100, string state = "OPEN")
    {
        var HasMoreData = true;
        var request = new GraphQLRequest
        {
            Query = @"query ($org: String!, $reponame: String!, $top: Int!, $state: [IssueState!], $createdAfter: DateTime!) {
                      repository(owner: $org, name: $reponame) {
                        issues(first:$top, states:$state, filterBy: {since: $createdAfter}) {
                            totalCount
                            pageInfo {
                                endCursor
                                startCursor
                                hasNextPage
                                hasPreviousPage
                            }
                          nodes {
                            projectNext(number: 142) {
                              id
                            }
                            repository {
                              name
                            }
                            id
                            title
                            url
                            state
                            author {
                              login
                            }
                            milestone {
                              title
                            }
                            labels(first:100) {
                              nodes {
                                name
                              }
                            }
                            assignees(first: 10) {
                              nodes {
                                login
                              }
                            }
                          }
                        }
                      }
                    }",
            Variables = new { 
                org = repoOrg,
                reponame = repoName,
                top = top,
                state = new[] { state },
                createdAfter = createdAfter.ToUniversalTime().ToString("o") }
        };

        while (HasMoreData)
        {
            var res = await client.SendQueryAsync<JObject>(request);
            CheckErrorResponse(res);
            foreach (var issue in res.Data["repository"]["issues"]["nodes"])
            {
                if (issue["projectNext"] is not null && issue["projectNext"].HasValues)
                    continue; // Skip already created issues.

                yield return new Issue
                {
                    Id = issue["id"].ToString(),
                    Author = issue["author"]["login"].ToString(),
                    Title = issue["title"].ToString(),
                    Url = issue["url"].ToString(),
                    State = issue["state"].ToString(),
                    MileStone = issue["milestone"].HasValues ? issue["milestone"]["title"].ToString() : null,
                    Labels = (issue["labels"]["nodes"] as JArray).Select(x => x["name"].ToString()).ToArray(),
                    Assignees = (issue["assignees"]["nodes"] as JArray).Select(x => x["login"].ToString()).ToArray(),
                    Repository = repoName
                };
            }
            HasMoreData = res.Data["repository"]["issues"]["pageInfo"]["hasNextPage"].Value<Boolean>();
            var EndCursor = res.Data["repository"]["issues"]["pageInfo"]["endCursor"].Value<string>();
            if (HasMoreData)
            {
                request = new GraphQLRequest
                {
                    Query = @"query ($org: String!, $reponame: String!, $top: Int!, $state: [IssueState!], $endCursor: String!, $createdAfter: DateTime!) {
                      repository(owner: $org, name: $reponame) {
                        issues(first:$top, after:$endCursor, states:$state, filterBy: {since: $createdAfter}) {
                            totalCount
                            pageInfo {
                                endCursor
                                startCursor
                                hasNextPage
                                hasPreviousPage
                            }
                          nodes {
                            projectNext(number: 142) {
                              id
                            }
                            repository {
                              name
                            }
                            id
                            title
                            url
                            state
                            author {
                              login
                            }
                            milestone {
                              title
                            }
                            labels(first:100) {
                              nodes {
                                name
                              }
                            }
                            assignees(first: 10) {
                              nodes {
                                login
                              }
                            }
                          }
                        }
                      }
                    }",
                    Variables = new {
                        org = repoOrg,
                        reponame = repoName,
                        top = top,
                        state = new[] { state },
                        endCursor = EndCursor,
                        createdAfter = createdAfter.ToUniversalTime().ToString("o")
                    }
                };
            }
        }
    }

    public static async Task<ProjectItem> GetProjectItemAsync(this GraphQLHttpClient client, string itemId)
    {
        var request = new GraphQLRequest
        {
            Query = @"query ($itemId: ID!) {
                          node(id: $itemId) {
                            ... on ProjectNextItem {
                              id
                              title
                              project {
                                id
                              }
                              fieldValues(first: 100) {
                                nodes {
                                  value
                                  projectField {
                                    id
                                  }
                                }
                              }
                              content {
                                ... on Issue {
                                  id
                                  labels(first:20) {
                                    nodes {
                                      name
                                    }
                                  }
                                }
                              }
                            }
                          }
                        }",
            Variables = new { itemId = itemId }
        };

        var res = await client.SendQueryAsync<JObject>(request);
        CheckErrorResponse(res);

        var item = new ProjectItem
        {
            Id = res.Data["node"]["id"].ToString(),
            Title = res.Data["node"]["title"].ToString(),
            ProjectId = res.Data["node"]["project"]["id"].ToString(),
        };
        if (res.Data["node"]["content"]["labels"]["nodes"].HasValues)
        {
            item.Labels = (res.Data["node"]["content"]["labels"]["nodes"] as JArray).Select(l => l["name"].ToString()).ToArray();
        };

        foreach (var field in res.Data["node"]["fieldValues"]["nodes"])
        {
            item.Fields.Add(field["projectField"]["id"].ToString(), field["value"].ToString());
        }

        return item;
    }

    public static ProjectItem GetProjectItemByIssueId(this GraphQLHttpClient client, int issueId)
    {
        return null;
    }

    public static async Task<string> UpdateProjectItemAsync(this GraphQLHttpClient client, string projectId, string itemId, string field, object value)
    {
        var request = new GraphQLRequest
        {
            Query = @"mutation ($input: UpdateProjectNextItemFieldInput!)
                    {
                        updateProjectNextItemField(input: $input) {
                            projectNextItem {
                              id
                            }
                        }
                     }",
            Variables = new
            {
                input = new
                {
                    projectId = projectId,
                    itemId = itemId,
                    fieldId = field,
                    value = value
                }
            }
        };
        var res = await client.SendQueryAsync<JObject>(request);
        CheckErrorResponse(res);
        return res.Data["updateProjectNextItemField"]["projectNextItem"]["id"].Value<string>();
    }

    private static void CheckErrorResponse(IGraphQLResponse response)
    {
        if (response.Errors != null && response.Errors.Any())
        {
            throw new ApplicationException(message: response.Errors.FirstOrDefault()?.Message);
        }
    }
}