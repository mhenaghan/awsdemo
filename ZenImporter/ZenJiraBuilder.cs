using AnotherJiraRestClient;
using AnotherJiraRestClient.JiraModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZenImporter3
{
    class ZenJiraBuilder
    {
        ZenJiraMapper mapper = new ZenJiraMapper();
        string projectKey = "HDAU";
        string issueType = "15"; // support Ticket
        string priority = "1"; //normal?
        string attachmentsDir;

        JiraClient jira;
        List<Component> projectComponents;
        Users groupUsers;
        public ZenJiraBuilder(JiraClient theClient, string downloadDir)
        {
            jira = theClient;
            attachmentsDir = downloadDir;
            projectComponents = jira.GetComponents(projectKey);
        }
        public JiraCreateIssue MakeJiraFromZen(ticketsTicket zenTicket)
        {

          var summary = string.Format("#{0} {1}",zenTicket.niceid[0].Value, zenTicket.subject);
          var description = zenTicket.description;
          var reporterName = GetJiraUser(int.Parse(zenTicket.requesterid[0].Value));
            
          
            
          //Populate initial fields
          var result  = new JiraCreateIssue("HDAU", summary, description, issueType, priority,reporterName, null);
            //Populate secondary fields
          result.AddField("customfield_10204", "Unknown - Imported ZenDesk"); //account name
          result.AddField("customfield_10205", new {value = "AU"}); //account region
            
          //Load Component
          var component = GetComponent(GetFieldValue(20966279,zenTicket)); //Component Field                
          if (component == null)
          { 
          }
                
          result.AddField("components",new Component[]{component});

          //Pick first comment author as sponser.
          if (zenTicket.comments != null)
          {
              var firstCommentAuthor = int.Parse(zenTicket.comments.First().comment.First().authorid[0].Value);
              
              var aUser = GetJiraUser(firstCommentAuthor);
              if (aUser == null)
                  throw new Exception(string.Format("Muppet Detected {0}?",  mapper.GetJiraName(firstCommentAuthor,"aHole")));

              result.AddField("customfield_10700", new { name = aUser });
          }

          result.AddField("resolution", new { name = "Done" });
        //  result.AddField("status", new { name = "Closed" });
       
            return result;
        }




        private string GetFieldValue(int fieldId, ticketsTicket zenTicket)
        {
            if (zenTicket.ticketfieldentries[0].ticketfieldentry == null)
                return null;
            for (int i = 0; i < zenTicket.ticketfieldentries[0].ticketfieldentry.Length; i++)
            {
                var e = zenTicket.ticketfieldentries[0].ticketfieldentry[i];
                if (int.Parse(e.ticketfieldid[0].Value) == fieldId)
                {
                    return e.value;
                }
            }
            return null;
        }
        
        
        Component GetComponent(string componentName)
        {          

            string jiraComponentName = mapper.GetJiraComponent(componentName);

            return projectComponents.FirstOrDefault(c => c.name.ToLower() == jiraComponentName.ToLower());

        }

        internal string GetJiraUser (int zenAuthorId)
        {
            var mappedName = mapper.GetJiraName(zenAuthorId);
            
                        if( groupUsers == null)
            groupUsers = jira.GetGroup("jira-users").users;

            //Check if we have an actual usr by that name. 
            var actual = groupUsers.items.FirstOrDefault( u => u.name.ToLower() == mappedName.ToLower());
            if(actual != null)
                return mappedName;

           //Not there,try by email
            var byEmail = groupUsers.items.FirstOrDefault(u => u.emailAddress == mappedName);
            if (byEmail != null)
                return byEmail.name;

            return "email2jira";            

        }

        internal List<PostComment> MakeCommentsFromZen(ticketsTicket zenTicket, string issueKey)
        {
            /*
             * Need author, body, date
             * 
             * Can't re-create the as is comment,so add original author and time to top of body:
             */


            var result = new List<PostComment>();

            foreach (var c in zenTicket.comments)
            {
                foreach (var cc in c.comment)
                {
                    var author = mapper.GetJiraName(cc.authorid[0].Value);                    
                    var when = cc.createdat[0].Value;
                    result.Add(new PostComment() { body = string.Format(@"[{0} on {1}]  {2}", author, when, cc.value) });
                    
                }
            }

            return result;

        }

        internal List<string> GetAttachmentsFromZen(ticketsTicket zenTicket)
        {
            var results = new List<string>();
            foreach (var c in zenTicket.comments)
            {
                foreach (var cc in c.comment)
                {
                    if (cc.attachments.Length == 0)
                        continue;

                    foreach (var a in cc.attachments)
                    {
                        if (a.attachment == null)
                            continue;
                        foreach (var aa in a.attachment)
                        {
                            Console.WriteLine(string.Format("Downloading for {0}. File: {1}", zenTicket.GetNiceId(), aa.url));
                            var fileName = DownloadFile(aa.url, aa.contenttype, zenTicket.GetNiceId());
                            results.Add(fileName);                        
                        }
                    }

                }
            }
            return results;
        }

        private string DownloadFile(string url, string contentType, string zenTicketId)
        {
            WebClient webClient = new WebClient();
            ///var addr = new Uri(url);
            var fileName = Path.GetFileName(url).Split(new string[]{"?name="},StringSplitOptions.RemoveEmptyEntries)[0];
            //Prob got a ?name= bit.
            
            var downloadPath = Path.Combine(attachmentsDir,zenTicketId);

            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);
            downloadPath = Path.Combine(downloadPath, fileName);
            if (File.Exists(downloadPath))
                return downloadPath; //Been there done that. 
            int retry = 3;

            download:
            try
            {
               
                webClient.DownloadFile(url, downloadPath);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("timed out") && retry > 0)
                {
                    retry--;
                    goto download;
                }
                    
                throw;
            }
            

            return downloadPath;
        }
    }
}
