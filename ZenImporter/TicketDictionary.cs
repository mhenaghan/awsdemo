using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ZenImporter3
{
    public class MappingInfo
    {
        public string JiraId { get; set; }
        public string ZenId { get; set; }
        public bool CommentsDone { get; set; }
        public bool AttachmentsDone { get; set; }

    }

   public class TicketDictionary
    {
       public List<MappingInfo> searchData = new List<MappingInfo>();
       public List<MappingInfo> fileData = new List<MappingInfo>();

       XmlSerializer theDude;
       string dataFileName;

       public TicketDictionary(string fileName)
       {
           theDude = new XmlSerializer(typeof(List<MappingInfo>));
           dataFileName = fileName;

       }


       public TicketDictionary()
       { }


       public bool LoadFromFile()
       {
           if (!File.Exists(dataFileName))
               return false;
           using (StreamReader r2 = new StreamReader(dataFileName))
           {
               //try
               //{
               searchData = (List<MappingInfo>)theDude.Deserialize(r2);
               fileData = new List<MappingInfo>(searchData.ToArray());
               //Make a copy for searching only

               return searchData != null;
               
           }
       }

       private static readonly Object obj = new Object();

       public MappingInfo AddMapping(string zenId, string jiraId)
       {
           var entry = new MappingInfo() { ZenId = zenId, JiraId = jiraId, AttachmentsDone = false, CommentsDone = false }; 
           bool lockWasTaken = false;
          
           try
           {
               Monitor.Enter(obj, ref lockWasTaken);
               //There Already?
               //var existing = data.Find(e => e.ZenId == zenId);
               //if (existing != null)
               //    throw new Exception    (string.Format("We have that bugger already: {0} to {1}", existing.ZenId, existing.JiraId));
               fileData.Add(entry);
           }
           finally
           {
               if (lockWasTaken)
               {
                   Monitor.Exit(obj);
               }
           }
           
           
           return entry;
       }

       public MappingInfo FindByZenId (string zenId)
       {
           return searchData.FirstOrDefault( e => e.ZenId == zenId);
       }

       public MappingInfo FindByJiraId(string JiraKey)
       {
           return searchData.FirstOrDefault(e => e.JiraId == JiraKey);
       }

     

       public void Update(MappingInfo mi)
       {
           var i = searchData.FindIndex(e => e.ZenId == mi.ZenId);
           if(i > -1)
            fileData[i] = mi;
       }

       internal void SetCommentsLoaded(string issueKey)
       {
           var entry = FindByJiraId(issueKey);
           if (entry == null)
               return;

           entry.CommentsDone = true;

           Update(entry);
       }

       internal void SetAttachmentsLoaded(string jiraKey)
       {
           var entry = FindByJiraId(jiraKey);
           if (entry == null)
               return;

           entry.AttachmentsDone = true;

           Update(entry);
       }

       internal void Write()
       {           
           StreamWriter w = new StreamWriter(dataFileName);
           theDude.Serialize(w, fileData);
           w.Close();
       }

       internal int GetHasCommentsCount()
       {
           return fileData.Count(m => m.CommentsDone);
       }

       internal int GetHasAttachmentsCount()
       {
           return fileData.Count(m => m.AttachmentsDone );
       }
    }
}
