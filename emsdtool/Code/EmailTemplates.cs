using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace emsdtool
{
    public class EmailTemplates
    {
        public static string GetBody(string sltn, string stage, string status, string owner)
        {
            // If it's explicitly rejected, show the rejected template
            if (string.Equals(status, "rejected", StringComparison.OrdinalIgnoreCase))
                return GetRejectedBody(sltn, stage, owner);

            // Otherwise approved—or any other status—uses the approved template
            return GetApprovedBody(sltn, stage, owner);
        }

        private static string GetApprovedBody(string sltn, string stage, string owner)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><body style=\"font-family:Segoe UI,Arial,sans-serif;\">");
            sb.AppendLine($"  <h2 style=\"color:green; margin-bottom:16px;\">SLTN {sltn} Approved</h2>");
            sb.AppendLine($"  <p>Hi {owner},</p>");
            sb.AppendLine($"  <p>Your request <strong>{sltn}</strong> has been <strong>approved</strong> at the <em>{stage}</em> stage.</p>");
            sb.AppendLine("  <p>You may now proceed with the next steps.</p>");
            sb.AppendLine("  <hr style=\"margin:24px 0;\" />");
            sb.AppendLine("  <p style=\"font-size:0.85em;color:#555;\">This is an automated notification. Please do not reply.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string GetRejectedBody(string sltn, string stage, string owner)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><body style=\"font-family:Segoe UI,Arial,sans-serif;\">");
            sb.AppendLine($"  <h2 style=\"color:red; margin-bottom:16px;\">SLTN {sltn} Rejected</h2>");
            sb.AppendLine($"  <p>Hi {owner},</p>");
            sb.AppendLine($"  <p>We’re sorry to inform you that your request <strong>{sltn}</strong> was <strong>rejected</strong> at the <em>{stage}</em> stage.</p>");
            sb.AppendLine("  <p>Please review the details or contact support for assistance.</p>");
            sb.AppendLine("  <hr style=\"margin:24px 0;\" />");
            sb.AppendLine("  <p style=\"font-size:0.85em;color:#555;\">This is an automated notification. Please do not reply.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string GetPendingBody(string sltn, string stage, string owner)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><body style=\"font-family:Segoe UI,Arial,sans-serif;\">");
            sb.AppendLine($"  <h2 style=\"color:orange; margin-bottom:16px;\">SLTN {sltn} Pending Approval</h2>");
            sb.AppendLine($"  <p>Hi {owner},</p>");
            sb.AppendLine($"  <p>Your request <strong>{sltn}</strong> is currently <strong>pending</strong> at the <em>{stage}</em> stage.</p>");
            sb.AppendLine("  <p>We will notify you once a decision is made.</p>");
            sb.AppendLine("  <hr style=\"margin:24px 0;\" />");
            sb.AppendLine("  <p style=\"font-size:0.85em;color:#555;\">This is an automated notification. Please do not reply.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }
    }
}