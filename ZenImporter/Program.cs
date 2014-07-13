using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnotherJiraRestClient;
using AnotherJiraRestClient.JiraModel;
using System.IO;
using System.Xml.Serialization;
using System.Threading;

namespace ZenImporter3
{
    class Program
    {
        static JiraClient jira;
        static ZenJiraBuilder builder;
        static TicketDictionary mappedTickets;
        //want something new
        static double count = 0;
        static double total = 0;

        static void Main(string[] args)
        {



            Dictionary<int, int> userMappings = new Dictionary<int, int>();
            Dictionary<int, string> jiraUsers = new Dictionary<int, string>();
            string mappedTicektsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MappedTickets.xml");
            

            //userMappings.Add(212447715,mm

            var account = new JiraAccount()
            {
                ServerUrl = @"https://jericho.atlassian.net",
                Password = "Oldhead2013",
                User = "myles"
            };

            jira = new JiraClient(account);
            builder = new ZenJiraBuilder(jira, @"C:\Users\Myles\Downloads\ZenImporter3\ZenAttachments");

            StreamReader str = new StreamReader(@"C:\Users\Myles\Downloads\ZenImporter3\ZenTickets\tickets.xml");
            XmlSerializer xSerializer = new XmlSerializer(typeof(tickets));
            tickets theTickets = (tickets)xSerializer.Deserialize(str);


            //Check if we're in downlaod attachment mode.

            //load mapped tickets
            mappedTickets = new TicketDictionary(mappedTicektsFile);
            if (mappedTickets.LoadFromFile())
                Console.WriteLine(string.Format("Loaded mapped tickets from file. Found {0} tickets", mappedTickets.searchData.Count));


            count = mappedTickets.GetHasAttachmentsCount();
            total = theTickets.ticket.Count();
            try
            {

                //Phase 1 - Create all ticket stubs and record created Ids.
                Parallel.ForEach(theTickets.ticket, zenTicket =>
               // foreach (var zenTicket in theTickets.ticket)
	
                {
                    
               //     var token = tokenSource.Token;
                    //Have we made it already?
                    MappingInfo mapping = mappedTickets.FindByZenId(zenTicket.GetNiceId());

                    if (mapping == null)
                    {
                        var j = new JiraClient(account);
                        Console.WriteLine(string.Format("Creating for Zen ID {0}", zenTicket.GetNiceId()));
                        var jiraIssue = builder.MakeJiraFromZen(zenTicket);
                        //log to file the Zen ID and created jira ID.
                        var result = j.CreateIssue(jiraIssue);
                        if (result == null || result.self == null)
                            throw new Exception(string.Format("Something didn't "));
                        mapping = mappedTickets.AddMapping(zenTicket.GetNiceId(), result.key);
                        Console.WriteLine(string.Format("{0} tickets mapped. {1:g2}% complete", mappedTickets.fileData.Count, ((double)mappedTickets.fileData.Count / (double)theTickets.ticket.Count()) * 100));
                    }

                    if (!mapping.CommentsDone)
                        LoadComments(zenTicket, mapping.JiraId);

                    if (!mapping.AttachmentsDone)
                        LoadAttachments(zenTicket, mapping.JiraId);

                } );

                mappedTickets.Write();
                //Phase 2 - loop bacj through and 

              
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message);
                //throw;
            }
            finally
            {
                mappedTickets.Write();
            }

            Console.WriteLine(string.Format("Run Complete {0} tickets mapped. {1:g2}% complete", mappedTickets.searchData.Count, (double)(mappedTickets.searchData.Count / theTickets.ticket.Count())));
        }

        private static void LoadAttachments(ticketsTicket zenTicket, string jiraKey)
        {
            Console.WriteLine(string.Format("Loading Attachments for {0}-{1}", zenTicket.GetNiceId(), jiraKey));
            var attachments = builder.GetAttachmentsFromZen(zenTicket);
            if (jira.LoadAttachments(attachments, jiraKey))
                mappedTickets.SetAttachmentsLoaded(jiraKey);
            count++;
            Console.WriteLine(string.Format("Loaded {0} ticket atttachments, {1:g3}%",count,  (count++ / total)*100));
        }

        private static void LoadComments(ticketsTicket zenTicket, string issueKey)
        {

           Console.WriteLine(string.Format("Loading comments for {0}-{1}", zenTicket.GetNiceId(),issueKey));
           var comments = builder.MakeCommentsFromZen(zenTicket, issueKey);
           if(!jira.LoadComments(comments,issueKey))
               throw new Exception(string.Format("Problem loading comments for {0}",issueKey));
           mappedTickets.SetCommentsLoaded( issueKey);

         Console.WriteLine(string.Format("Loaded {0} comments, {1:g3}%",count++/ total));
        }



        public static object mappedTicketsFile { get; set; }
    }
}
