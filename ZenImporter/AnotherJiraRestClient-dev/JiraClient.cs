using AnotherJiraRestClient.JiraModel;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace AnotherJiraRestClient
{
    /// <summary>
    /// Class used for all interaction with the Jira API. See 
    /// http://docs.atlassian.com/jira/REST/latest/ for documentation of the
    /// Jira API.
    /// </summary>
    public class JiraClient
    {
        private readonly RestClient client;

        private string encoded;

        /// <summary>
        /// Constructs a JiraClient.
        /// </summary>
        /// <param name="account">Jira account information</param>
        public JiraClient(JiraAccount account)
        {
            client = new RestClient(account.ServerUrl)
            {
                Authenticator = new HttpBasicAuthenticator(account.User, account.Password)

            };

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", account.User, account.Password));
            encoded = System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Executes a RestRequest and returns the deserialized response. If
        /// the response hasn't got the specified expected response code or if an
        /// exception was thrown during execution a JiraApiException will be 
        /// thrown.
        /// </summary>
        /// <typeparam name="T">Request return type</typeparam>
        /// <param name="request">request to execute</param>
        /// <param name="expectedResponseCode">The expected HTTP response code</param>
        /// <returns>deserialized response of request</returns>
        public T Execute<T>(RestRequest request, HttpStatusCode expectedResponseCode) where T : new()
        {
            int retry = 3;
            // Won't throw exception.
            DoCall:
            var response = client.Execute<T>(request);
      //      var temp = client.Execute(request);
            var keyResponse = JsonConvert.DeserializeObject<T>(response.Content);
            if (keyResponse == null || response.Content.StartsWith("errorMessages"))
            {
                if (response.ErrorMessage != null && response.ErrorMessage == ("The operation has timed out"))
                {
                    if (retry > 0)
                    {
                        retry--;
                        Thread.Sleep(3000);
                        goto DoCall;
                    }
                    else
                    {
                        //timed out and no retries left.
                    }
                }
                else
                throw new JiraApiException(string.Format("RestSharp response status: {0} - HTTP response: {1} - {2} - {3}", response.ResponseStatus, response.StatusCode, response.StatusDescription, response.Content));
            }


            return keyResponse;
        }

        /// <summary>
        /// Returns a comma separated string from the strings in the provided
        /// IEnumerable of strings. Returns an empty string if null is provided.
        /// </summary>
        /// <param name="strings">items to put in the output string</param>
        /// <returns>a comma separated string</returns>
        private static string ToCommaSeparatedString(IEnumerable<string> strings)
        {
            if (strings != null)
                return string.Join(",", strings);
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns the Issue with the specified key. If the fields parameter
        /// is specified only the given field names will be loaded. Issue
        /// contains the availible field names, for example Issue.SUMMARY. Throws
        /// a JiraApiException if the request was unable to execute.
        /// </summary>
        /// <param name="issueKey">Issue key</param>
        /// <param name="fields">Fields to load</param>
        /// <returns>
        /// The issue with the specified key or null if no such issue was found.
        /// </returns>
        public Issue GetIssue(string issueKey, IEnumerable<string> fields = null)
        {
            var fieldsString = ToCommaSeparatedString(fields);
            
            var request = new RestRequest();
            request.Resource = string.Format("{0}?fields={1}", ResourceUrls.IssueByKey(issueKey), fieldsString);
            request.Method = Method.GET;
            
            var issue = Execute<Issue>(request, HttpStatusCode.OK);
            return issue.fields != null ? issue : null;
        }

        /// <summary>
        /// Searches for Issues using JQL. Throws a JiraApiException if the request 
        /// was unable to execute.
        /// </summary>
        /// <param name="jql">a JQL search string</param>
        /// <returns>The search results</returns>
        public Issues GetIssuesByJql(string jql, int startAt, int maxResults, IEnumerable<string> fields = null)
        {
            var request = new RestRequest();
            request.Resource = ResourceUrls.Search();
            request.AddParameter(new Parameter()
                {
                    Name = "jql",
                    Value = jql,
                    Type = ParameterType.GetOrPost
                });
            request.AddParameter(new Parameter()
            {
                Name = "fields",
                Value = ToCommaSeparatedString(fields),
                Type = ParameterType.GetOrPost
            });
            request.AddParameter(new Parameter()
            {
                Name = "startAt",
                Value = startAt,
                Type = ParameterType.GetOrPost
            });
            request.AddParameter(new Parameter()
            {
                Name = "maxResults",
                Value = maxResults,
                Type = ParameterType.GetOrPost
            });
            request.Method = Method.GET;
            return Execute<Issues>(request, HttpStatusCode.OK);
        }

        /// <summary>
        /// Returns the Issues for the specified project.  Throws
        /// a JiraApiException if the request was unable to execute.
        /// </summary>
        /// <param name="projectKey">project key</param>
        /// <returns>the Issues of the specified project</returns>
        public Issues GetIssuesByProject(string projectKey, int startAt, int maxResults, IEnumerable<string> fields = null)
        {
            return GetIssuesByJql("project=" + projectKey, startAt, maxResults, fields);
        }

        /// <summary>
        /// Returns all available projects the current user has permision to view. 
        /// Throws a JiraApiException if the request was unable to execute.
        /// </summary>
        /// <returns>Details of all projects visible to user</returns>
        public List<Project> GetProjects()
        {
            var request = new RestRequest()
            {
                Resource = ResourceUrls.Project(),
                RequestFormat = DataFormat.Json,
                Method = Method.GET
            };

            return Execute<List<Project>>(request, HttpStatusCode.OK);
        }


        public List<Component> GetComponents(string projectKey)
        {
            var request = new RestRequest()
            {
                Resource = ResourceUrls.Component(projectKey),
                RequestFormat = DataFormat.Json,
                Method = Method.GET
            };

            return Execute<List<Component>>(request, HttpStatusCode.OK);
        }

        public Group GetGroup(string groupName)
        {
            var request = new RestRequest()
            {
                Resource = ResourceUrls.Group(groupName),
                RequestFormat = DataFormat.Json,
                Method = Method.GET
            };

            return Execute<Group>(request, HttpStatusCode.OK);
        }

        public List<User> GetUser(string userName)
        {
            var request = new RestRequest()
            {
                Resource = ResourceUrls.User(userName),
                RequestFormat = DataFormat.Json,
                Method = Method.GET
            };

            return Execute<List<User>>(request, HttpStatusCode.OK);
        } 


        /// <summary>
        /// Returns a list of all possible priorities.  Throws
        /// a JiraApiException if the request was unable to execute.
        /// </summary>
        /// <returns></returns>
        public List<Priority> GetPriorities()
        {
            var request = new RestRequest();
            request.Resource = ResourceUrls.Priority();
            request.Method = Method.GET;
            return Execute<List<Priority>>(request, HttpStatusCode.OK);
        }

        /// <summary>
        /// Returns the meta data for creating issues. This includes the 
        /// available projects and issue types, but not fields (fields
        /// are supported in the Jira api but not by this wrapper currently).
        /// </summary>
        /// <param name="projectKey"></param>
        /// <returns>the meta data for creating issues</returns>
        public ProjectMeta GetProjectMeta(string projectKey)
        {
            var request = new RestRequest();
            request.Resource = ResourceUrls.CreateMeta();
            request.AddParameter(new Parameter() 
              { Name = "projectKeys", 
                Value = projectKey, 
                Type = ParameterType.GetOrPost });
            request.Method = Method.GET;
            var createMeta = Execute<IssueCreateMeta>(request, HttpStatusCode.OK);
            if (createMeta.projects[0].key != projectKey || createMeta.projects.Count != 1)
                // TODO: Error message
                throw new JiraApiException();
            return createMeta.projects[0];
        }

        /// <summary>
        /// Returns a list of all possible issue statuses. Throws
        /// a JiraApiException if the request was unable to execute.
        /// </summary>
        /// <returns></returns>
        public List<Status> GetStatuses()
        {
            var request = new RestRequest();
            request.Resource = ResourceUrls.Status();
            request.Method = Method.GET;
            return Execute<List<Status>>(request, HttpStatusCode.OK);
        }

        /// <summary>
        /// Creates a new issue. Throws a JiraApiException if the request was 
        /// unable to execute.
        /// </summary>
        /// <returns>the new issue</returns>
        public BasicIssue CreateIssue(JiraCreateIssue newIssue)
        {
            var request = new RestRequest()
            {
                Resource = ResourceUrls.Issue(),
                RequestFormat = DataFormat.Json,
                Method = Method.POST
            };

            request.AddBody(newIssue);

            return Execute<BasicIssue>(request, HttpStatusCode.Created);
        }

        /// <summary>
        /// Returns the application property with the specified key.
        /// </summary>
        /// <param name="propertyKey">the property key</param>
        /// <returns>the application property with the specified key</returns>
        public ApplicationProperty GetApplicationProperty(string propertyKey)
        {
            var request = new RestRequest()
            {
                Method = Method.GET,
                Resource = ResourceUrls.ApplicationProperties(),
                RequestFormat = DataFormat.Json
            };
            
            request.AddParameter(new Parameter()
            {
                Name = "key",
                Value = propertyKey,
                Type = ParameterType.GetOrPost
            });

            return Execute<ApplicationProperty>(request, HttpStatusCode.OK);
        }

        /// <summary>
        /// Returns the attachment with the specified id.
        /// </summary>
        /// <param name="attachmentId">attachment id</param>
        /// <returns>the attachment with the specified id</returns>
        public Attachment GetAttachment(string attachmentId)
        {
            var request = new RestRequest()
            {
                Method = Method.GET,
                Resource = ResourceUrls.AttachmentById(attachmentId),
                RequestFormat = DataFormat.Json
            };

            return Execute<Attachment>(request, HttpStatusCode.OK);
        }

        /// <summary>
        /// Deletes the specified attachment.
        /// </summary>
        /// <param name="attachmentId">attachment to delete</param>
        public void DeleteAttachment(string attachmentId)
        {
            var request = new RestRequest()
            {
                Method = Method.DELETE,
                Resource = ResourceUrls.AttachmentById(attachmentId)
            };

            var response = client.Execute(request);
            if (response.ResponseStatus != ResponseStatus.Completed || response.StatusCode != HttpStatusCode.NoContent)
                throw new JiraApiException("Failed to delete attachment with id=" + attachmentId);
        }

        //public void LoadAttachments(string attachnemts)
        //{
        //    MultipartEntity 
        //}

        public bool LoadComments(List<PostComment> comments, string issueKey)
        {

            try
            {
                foreach (var item in comments)
                {

                    var request = new RestRequest()
                    {
                        Resource = ResourceUrls.Comment(issueKey),
                        RequestFormat = DataFormat.Json,
                        Method = Method.POST
                    };

                    request.AddBody(item);
                    Execute<BasicIssue>(request, HttpStatusCode.Created);

                }
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        public bool LoadAttachments(List<string> attachments, string jiraKey)
        {
           //Some funky stuff. 
            if (attachments == null || attachments.Count == 0)
                return true;

            List<FileInfo> files = new List<FileInfo>();
            foreach (var f in attachments)
            {
                if (File.Exists(f))
                    files.Add(new FileInfo(f));
            }
            if (files.Count == 0)
                return false;

            return PostMultiPart(@"https://jericho.atlassian.net" + ResourceUrls.Attachments(jiraKey), files);
        }





        private bool PostMultiPart(string restUrl, IEnumerable<FileInfo> filePaths)
        {
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            try
            {
                var boundary = string.Format("----------{0:N}", Guid.NewGuid());
                var content = new MemoryStream();
                var writer = new StreamWriter(content);

                foreach (var filePath in filePaths)
                {
                    var fs = new FileStream(filePath.FullName, FileMode.Open, FileAccess.Read);
                    var data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    fs.Close();

                    writer.WriteLine("--{0}", boundary);
                    writer.WriteLine("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"", filePath.Name);
                    writer.WriteLine("Content-Type: application/octet-stream");
                    writer.WriteLine();
                    writer.Flush();

                    content.Write(data, 0, data.Length);

                    writer.WriteLine();
                }

                writer.WriteLine("--" + boundary + "--");
                writer.Flush();
                content.Seek(0, SeekOrigin.Begin);

                request = WebRequest.Create(restUrl) as HttpWebRequest;
                if (request == null)
                {
                    Console.WriteLine(string.Format("Unable to create REST query: {0}", restUrl));
                    return false;
                }

                request.Method = "POST";
                request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
                request.Accept = "application/json";
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.Headers.Add("X-Atlassian-Token", "nocheck");
                request.ContentLength = content.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    content.WriteTo(requestStream);
                    requestStream.Close();
                }

                using (response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var reader = new StreamReader(response.GetResponseStream());
                        string err = string.Format("The server returned '{0}'\n{1}", response.StatusCode, reader.ReadToEnd());
                        if (err.Contains("Could not rename '"))
                            Console.WriteLine("Name Clash.");
                        else
                        {
                            Console.WriteLine(err);
                            throw new Exception(err);
                        }
                     
                    }

                    return true;
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)wex.Response)
                    {
                        var reader = new StreamReader(errorResponse.GetResponseStream());
                        string err = string.Format("The server returned '{0}'\n{1}).", errorResponse.StatusCode, reader.ReadToEnd());
                        if (err.Contains("Could not rename '"))
                            Console.WriteLine("Name Clash.");
                        else
                        {
                            Console.WriteLine(err);
                            throw new Exception(err);
                        }
                    }
                }

                if (request != null)
                {
                    request.Abort();
                }

                return false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }
    }
}
