using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenImporter3
{
    public class ZenJiraMapper
    {

        Dictionary<int, string> userMappings = new Dictionary<int, string>();
        Dictionary<string, string> componentMappings = new Dictionary<string, string>();
        Dictionary<string, string> supportTypeMappings = new Dictionary<string, string>();



        public ZenJiraMapper()
        {
            userMappings.Add(293814281, "email2jira"); //aliaw@jerichodc.com
            userMappings.Add(22179959, "ckassimatis@jerichodc.com");
            userMappings.Add(233631279, "esada@jerichodc.com");
            userMappings.Add(261941225, "gsalonga");
            userMappings.Add(210285144, "email2jira");
            userMappings.Add(208144299, "email2jira");
            userMappings.Add(230908760, "jjoske");
            userMappings.Add(205478814, "martin@jerichodc.com");
            userMappings.Add(212447715, "mmackie");
            userMappings.Add(230908770, "ogoodman@jerichodc.com");
            userMappings.Add(210285154, "pharrison@jerichodc.com");
            userMappings.Add(217966124, "rchandra@jerichodc.com");
            userMappings.Add(219749210, "rhook@jerichodc.com");
            userMappings.Add(232545499, "Salexandra@jerichodc.com");
            userMappings.Add(205042685, "email2jira");
            userMappings.Add(337553084, "shan.geraldizo@jerichodc.com");
            userMappings.Add(219317534, "soniatay");
            userMappings.Add(252965364, "tdavis@jerichodc.com");
            userMappings.Add(312314249, "traza@jerichodc.com");
            userMappings.Add(274529075, "jennarichards");


            componentMappings.Add("smartmail_pro", "SmartMail");
            componentMappings.Add("insermo", "Insermo");
            componentMappings.Add("esurveys", "eSurveys");
            componentMappings.Add("other", "Other");
            componentMappings.Add("srs", "SRS");
            componentMappings.Add("inspirus", "Inspirus");
            componentMappings.Add("jbilling", "j Billing");
            componentMappings.Add("mailout", "MailOut");

            

            supportTypeMappings.Add("support","General");
            supportTypeMappings.Add("pending_support","General");
            supportTypeMappings.Add("migration","Migration");
            supportTypeMappings.Add("project","General");
            supportTypeMappings.Add("campaign","Campaign");
            supportTypeMappings.Add("template","Template");
            supportTypeMappings.Add("inquiry","Inquiry");
            supportTypeMappings.Add("sign_up","Account Setup");
            supportTypeMappings.Add("billing","Billing");
            supportTypeMappings.Add("internal","Internal");            
            
        }


        public string GetJiraName(string zenId, string fallback = "email2jira")
        {
            if (string.IsNullOrEmpty(zenId))
                return fallback;

            return GetJiraName(int.Parse(zenId), fallback);
        }

        public string GetJiraName(int zenId, string fallback="email2jira")
        {           

            string result;
            if (userMappings.TryGetValue(zenId, out result))
                return result;

            //Not Mapped. return default
            return fallback;
        }

         public string GetJiraComponent(string zenId, string fallback="Other")
        {
            if (string.IsNullOrEmpty(zenId))
                return fallback;

            string result;
            if (componentMappings.TryGetValue(zenId, out result))
                return result;

            //Not Mapped. return default
            return fallback;
         }

        public string GetJiraSupportType(string zenId, string fallback="General")
        {
            if (string.IsNullOrEmpty(zenId))
                return fallback;


            string result;
            if (supportTypeMappings.TryGetValue(zenId, out result))
                return result;

            //Not Mapped. return default
            return fallback;
         }
    }
}
