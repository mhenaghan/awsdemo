﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnotherJiraRestClient.JiraModel
{
    /// <summary>
    /// Class used to create a new issue in the Jira API.
    /// 
    /// Contains only a dictionary because that allows us to (easily) add any custom fields
    /// without having trouble with serialization and deserialization. That's why Issue is not used
    /// for creating issues.
    /// </summary>
    public class JiraCreateIssue
    {
        public readonly Dictionary<string, object> fields;

        public JiraCreateIssue(
            string projectKey,
            string summary,
            string description,
            string issueTypeId,
            string priorityId,
            string reporter,
            IEnumerable<string> labels)
        {
            fields = new Dictionary<string, object>
            {
                { "project", new { key = projectKey } },
                { "summary", summary },
                { "description", description },
                { "issuetype", new { id = issueTypeId } },
                { "priority", new { id = priorityId } },
                { "reporter", new { name = reporter}}
               
              
            };
            if (labels != null)
                fields["labels"] = labels; 

        }

        public void AddField(string fieldName, object value)
        {
            fields.Add(fieldName, value);
        }
        
    }
}
