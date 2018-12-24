using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.Models
{
    public class EmailModel
    {
        public string SenderCode { get; set; }
        public int MinistryID { get; set; }
        public int MemberID { get; set; }
        public string MinistryName { get; set; }
        public string MemberName { get; set; }
        public string MemberEmail { get; set; }
        public string To { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string Subject { get; set; }
        public string EmailMessage { get; set; }
        public string TextMessage { get; set; }
        public string[] EmailString { get; set; }
        public string ChurchName { get; set; }

        public IEnumerable<HttpPostedFileBase> files { get; set; }
    }
}