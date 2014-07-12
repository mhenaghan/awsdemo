using System.IO;

namespace AnotherJiraRestClient
{
    public static class ResourceUrls
    {
        private const string BaseUrl = "/rest/api/2/";

        public static string IssueByKey(string issueKey)
        {
            return Url(string.Format("issue/{0}", issueKey));
        }

        public static string Issue()
        {
            return Url("issue");
        }

        public static string Search()
        {
            return Url("search");
        }

        public static string Priority()
        {
            return Url("priority");
        }

        public static string CreateMeta()
        {
            return Url("issue/createmeta");
        }

        public static string Status()
        {
            return Url("status");
        }

        public static string ApplicationProperties()
        {
            return Url("application-properties");
        }

        public static string AttachmentById(string attachmentId)
        {
            return Url(string.Format("attachment/{0}", attachmentId));
        }

        public static string AttachmentByIssue(string issueKey)
        {
            return "";
        //       r = requests.get(attach['url'], cookies=cookies)
        //if r.headers['content-type'].startswith('text/html') and 'Sign In' in r.content:
        //    print 'Please log into Assembla via browser and copy the'
        //    print '_breakout_session cookie into the file session.cookie.'
        //    raise Exception('Could not download attachment')
        //print '%s attachment: %r %d' % (a_num, attach['name'], len(r.content))
        //resp = jira_client.putFile(
        //    '/issue/%s/attachments' % j_key, attach['name'], r.content)
        //try:
        //    resp.json()
        //except:
        //    print resp.text
        //id_to_name[attach['id']] = attach['name']
        }

        public static string Project()
        {
            return Url("project");
        }

        public static string Component(string projectKey)
        {
            return Url(string.Format("project/{0}/components?expand=name,description",projectKey));
        }

        public static string User(string userName)
        {
            return Url(string.Format("user?username={0}", userName));
        }

        private static string Url(string key)
        {
            return Path.Combine(BaseUrl, key);
        }

        public static string Group(string groupName)
        {
            return Url(string.Format("group?groupname={0}&expand=users",groupName));
        }

        internal static string Comment(string issueKey)
        {
            return Url(string.Format("issue/{0}/comment", issueKey));
        }

       

        internal static string Attachments(string jiraKey)
        {
            return Url(string.Format("issue/{0}/attachments", jiraKey));
        }
    }
}