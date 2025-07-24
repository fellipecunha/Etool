using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using emsdtool;

namespace emsdtool
{
    public partial class EmailNotification : System.Web.UI.Page
    {
        /* ------------ DB COLUMN NAMES ------------ */
        private const string COL_SECTOR = "Funding_Sector";
        private const string COL_STAGE = "u_stage";
        private const string COL_SUBMIT = "u_submission_date";
        private const string COL_SYSID = "sys_id";
        private const string COL_SLTN = "number";
        private const string COL_OWNER = "sys_created_by";
        private const string COL_PROJ = "short_description";
        private const string COL_STATE = "state";
        private const string COL_GROUP = "assignment_group";
        private const string COL_STATUS = "u_tech_dev_head_approval_status";

        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        /* ---------------- PAGE LIFE ---------------- */
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindFundingSectors();
                BindStatuses();
                pnlNoRecords.Visible = false;
                pnlTabsWrapper.Visible = false;
                kpiRow.Visible = false;
                phModals.Controls.Clear();
                return;
            }

            // Detect the custom postback from sendEmail()
            string eventArg = Request["__EVENTARGUMENT"];
            if (eventArg == "SEND_EMAIL")
            {
                HandleEmailSend();
                // After sending, reload data so UI stays in sync
                LoadData();
                return;
            }
        }

        protected void btnLoad_Click(object sender, EventArgs e)
        {
            var sectorsSelected = cblSectors.Items.Cast<ListItem>().Any(i => i.Selected);
            var statusesSelected = cblStatuses.Items.Cast<ListItem>().Any(i => i.Selected);

            if (!sectorsSelected || !statusesSelected)
            {
                // simple alert; replace with a nicer toast if you want
                ScriptManager.RegisterStartupScript(this, GetType(), "valmsg",
                    "alert('Please select at least one item in both dropdowns.');", true);
                return;
            }

            LoadData();
        }

        /* --------------- FILTERS --------------- */
        private void BindFundingSectors()
        {
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                $"SELECT DISTINCT {COL_SECTOR} AS Sector FROM STLN_Core_Details " +
                $"WHERE {COL_SECTOR} IS NOT NULL AND {COL_SECTOR} <> '' ORDER BY {COL_SECTOR}", con))
            {
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    cblSectors.DataSource = rdr;
                    cblSectors.DataTextField = "Sector";
                    cblSectors.DataValueField = "Sector";
                    cblSectors.DataBind();
                }
            }
        }
        private void BindStatuses()
        {
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(
                $"SELECT DISTINCT {COL_STATUS} AS StatusVal FROM STLN_Core_Details " +
                $"WHERE {COL_STATUS} IS NOT NULL AND {COL_STATUS} <> '' ORDER BY {COL_STATUS}", con))
            {
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    cblStatuses.DataSource = rdr;
                    cblStatuses.DataTextField = "StatusVal";
                    cblStatuses.DataValueField = "StatusVal";
                    cblStatuses.DataBind();
                }
            }
        }

        /* --------------- MAIN LOAD --------------- */
        private void LoadData()
        {
            // ----- 1. Read filters -----
            var selectedSectors = cblSectors.Items.Cast<ListItem>()
                                     .Where(i => i.Selected)
                                     .Select(i => i.Value)
                                     .ToList();

            var selectedStatuses = cblStatuses.Items.Cast<ListItem>()
                            .Where(i => i.Selected)
                            .Select(i => i.Value)
                            .ToList();

            int maxDays = 30;
            int.TryParse(txtMaxDays.Text, out maxDays);

            // ----- 2. Query -----
            var dt = GetRecords(maxDays, selectedSectors, selectedStatuses);
            Session["LastRecords"] = dt;

            if (dt == null || dt.Rows.Count == 0)
            {
                pnlNoRecords.Visible = true;
                pnlTabsWrapper.Visible = false;
                kpiRow.Visible = false;

                rptSectorTabsNav.DataSource = null; rptSectorTabsNav.DataBind();
                rptSectorTabsContent.DataSource = null; rptSectorTabsContent.DataBind();
                phModals.Controls.Clear();

                // clear literals safely
                SetLiteralText("litTotalFlagged", "0");
                SetLiteralText("litSectorCount", "0");
                SetLiteralText("litStageCount", "0");
                SetLiteralText("litAllCount", "0");
                return;
            }

            // ----- 3. Grouping -----
            var stageGroups = dt.AsEnumerable()
                .GroupBy(r => GetStr(r, COL_STAGE).NullIfEmpty() ?? "Unknown")
                .Select(g => new GroupDTO { Key = g.Key, KeySlug = Slug(g.Key), Count = g.Count(), Rows = g.ToList() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var sectorTabs = dt.AsEnumerable()
                .GroupBy(r => GetStr(r, COL_SECTOR).NullIfEmpty() ?? "Unknown")
                .Select(g => new SectorTabDTO
                {
                    Key = g.Key,
                    KeySlug = Slug(g.Key),
                    Count = g.Count(),
                    Rows = g.ToList(),
                    StageBreakdown = g.GroupBy(r => GetStr(r, COL_STAGE).NullIfEmpty() ?? "Unknown")
                                      .Select(st => new StageCountDTO
                                      {
                                          Stage = st.Key,
                                          StageSlug = Slug(st.Key),
                                          Count = st.Count()
                                      })
                                      .OrderByDescending(x => x.Count)
                                      .ToList()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // ----- 4. KPIs & visibility -----
            pnlNoRecords.Visible = false;
            pnlTabsWrapper.Visible = true;
            kpiRow.Visible = true;

            SetLiteralText("litTotalFlagged", dt.Rows.Count.ToString());
            SetLiteralText("litSectorCount", sectorTabs.Count.ToString());
            SetLiteralText("litStageCount", stageGroups.Count.ToString());
            SetLiteralText("litAllCount", dt.Rows.Count.ToString());

            // ----- 5. Bind tabs -----
            rptSectorTabsNav.DataSource = sectorTabs;
            rptSectorTabsNav.DataBind();

            rptSectorTabsContent.DataSource = sectorTabs;
            rptSectorTabsContent.DataBind();

            // ----- 6. Build modals -----
            phModals.Controls.Clear();

            // Funding sector modals
            foreach (var fg in sectorTabs)
            {
                phModals.Controls.Add(new LiteralControl(
                    BuildModalHtml($"Funding Sector - {fg.Key}", $"fundingModal_{fg.KeySlug}", fg.Rows, maxDays)));
            }

            // Stage modals
            foreach (var sg in stageGroups)
            {
                phModals.Controls.Add(new LiteralControl(
                    BuildModalHtml($"Phase Details - {sg.Key}", $"phaseModal_{sg.KeySlug}", sg.Rows, maxDays)));
            }

            // ALL modal
            phModals.Controls.Add(new LiteralControl(
                BuildModalHtml("All Flagged Records", "allModal", dt.AsEnumerable().ToList(), maxDays)));
        }

        /* ---------- helpers ---------- */
        private void SetLiteralText(string id, string value)
        {
            var lit = FindControlRecursive(Page, id) as Literal;
            if (lit != null) lit.Text = value;
        }

        private static Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id) return root;
            foreach (Control c in root.Controls)
            {
                var t = FindControlRecursive(c, id);
                if (t != null) return t;
            }
            return null;
        }
        private static string CleanLabel(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            // remove a trailing "name" (case-insensitive) and any extra spaces
            return System.Text.RegularExpressions.Regex.Replace(s, @"\s*name\s*$", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }


        /* --------------- DB --------------- */
        private DataTable GetRecords(int maxDays, List<string> sectors, List<string> statuses)
        {
            var sql = new StringBuilder($@"
        SELECT *
        FROM STLN_Core_Details
        WHERE {COL_SUBMIT} IS NOT NULL
          AND DATEDIFF(DAY, {COL_SUBMIT}, GETDATE()) > @MaxDays");

            if (sectors != null && sectors.Count > 0)
            {
                sql.Append(" AND " + COL_SECTOR + " IN (");
                sql.Append(string.Join(",", sectors.Select((s, i) => "@sec" + i)));
                sql.Append(")");
            }

            if (statuses != null && statuses.Count > 0)
            {
                sql.Append(" AND " + COL_STATUS + " IN (");
                sql.Append(string.Join(",", statuses.Select((s, i) => "@st" + i)));
                sql.Append(")");
            }

            var dt = new DataTable();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql.ToString(), con))
            {
                cmd.Parameters.AddWithValue("@MaxDays", maxDays);

                if (sectors != null)
                    for (int i = 0; i < sectors.Count; i++)
                        cmd.Parameters.AddWithValue("@sec" + i, sectors[i]);

                if (statuses != null)
                    for (int i = 0; i < statuses.Count; i++)
                        cmd.Parameters.AddWithValue("@st" + i, statuses[i]);

                con.Open();
                dt.Load(cmd.ExecuteReader());
            }
            return dt;
        }

        /* --------------- REPEATERS --------------- */
        protected void rptSectorTabsContent_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var sector = (SectorTabDTO)e.Item.DataItem;
            var inner = (Repeater)e.Item.FindControl("rptStagesInside");
            if (inner != null)
            {
                inner.DataSource = sector.StageBreakdown;
                inner.DataBind();
            }
        }

        /* --------------- MODAL HTML --------------- */
        private static string BuildModalHtml(string title, string modalId, List<DataRow> rows, int maxDays)
        {
            var tableId = "recordsTable_" + modalId;
            var searchId = "sltnSearch_" + modalId;
            var selectAllId = "selectAllVisible_" + modalId;

            var sb = new StringBuilder();

            sb.AppendLine($@"
                    <div class='modal fade' id='{modalId}' tabindex='-1' role='dialog' aria-modal='true'>
                      <div class='modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable' role='document'>
                        <div class='modal-content shadow-lg border-0'>
                          <div class='modal-header bg-primary text-white'>
                            <h5 class='modal-title'>{HttpUtility.HtmlEncode(title)}</h5>
                            <button type='button' class='btn-close btn-close-white' data-bs-dismiss='modal' aria-label='Close'></button>
                          </div>

                          <div class='modal-body' style='max-height:70vh; overflow-y:auto;'>
                            <div class='mb-3' style='max-width:700px;'>
                              <input id='{searchId}' type='text' class='form-control w-100'
                                     placeholder='Filter by SLTN numbers (comma or space separated)'
                                     autocomplete='off'
                                     oninput='filterRows(this,""{tableId}"",""{selectAllId}"")' />
                            </div>

                            <table id='{tableId}' class='table table-bordered table-striped table-hover align-middle'>
                              <thead class='table-light sticky-top'>
                                <tr>
                                  <th style=""width:32px;"">
                                    <input type='checkbox' id='{selectAllId}' onclick='toggleAllVisible(this,""{tableId}"")' />
                                  </th>
                                  <th>SLTN</th>
                                  <th>Project Name</th>
                                  <th>State</th>
                                  <th>Stage</th>
                                  <th>Assigned Group</th>
                                  <th>Created By</th>
                                  <th>Approval Status</th>      <!-- new column -->
                                  <th>Days in Phase</th>
                                  <th>Days &gt; {maxDays}</th>
                                </tr>
                              </thead>
                              <tbody>");

                                foreach (var r in rows)
                                {
                                    string sltn = GetStr(r, COL_SLTN);
                                    string proj = GetStr(r, COL_PROJ);
                                    string state = GetStr(r, COL_STATE);
                                    string stage = CleanLabel(GetStr(r, COL_STAGE));
                                    string group = CleanLabel(GetStr(r, COL_GROUP));
                                    string owner = GetStr(r, COL_OWNER);
                                    string status = GetStr(r, COL_STATUS);               // pull the status field
                                    DateTime sub = GetDate(r, COL_SUBMIT) ?? DateTime.MinValue;

                                    int daysInPhase = sub == DateTime.MinValue
                                                      ? 0
                                                      : (int)(DateTime.Now - sub).TotalDays;
                                    int over = Math.Max(0, daysInPhase - maxDays);
                                    string overCls = over > 0 ? "fw-bold text-danger" : "";

                                    sb.AppendLine($@"
                                <tr>
                                  <td><input type='checkbox' name='SelectedRecordNumbers' value='{HttpUtility.HtmlAttributeEncode(sltn)}' /></td>
                                  <td class='sltn-number'>{HttpUtility.HtmlEncode(sltn)}</td>
                                  <td>{HttpUtility.HtmlEncode(proj)}</td>
                                  <td>{HttpUtility.HtmlEncode(state)}</td>
                                  <td>{HttpUtility.HtmlEncode(stage)}</td>
                                  <td>{HttpUtility.HtmlEncode(group)}</td>
                                  <td>{HttpUtility.HtmlEncode(owner)}</td>
                                  <td>{HttpUtility.HtmlEncode(status)}</td>          <!-- render status -->
                                  <td>{daysInPhase}</td>
                                  <td class='{overCls}'>{over}</td>
                                </tr>");
                                }

                                sb.AppendLine($@"
                              </tbody>
                            </table>
                          </div>
                          <div class='modal-footer bg-light d-flex justify-content-between'>
                            <button type='button' class='btn btn-secondary' data-bs-dismiss='modal'>Close</button>
                            <button type='button' class='btn btn-success' onclick='sendEmail(""{modalId}"")'>
                              📧 Send Email to Selected
                            </button>
                          </div>
                        </div>
                      </div>
                    </div>");

            return sb.ToString();
        }


        /* --------------- HELPERS --------------- */
        private static string GetStr(DataRow r, string col)
            => r.Table.Columns.Contains(col) && r[col] != DBNull.Value ? r[col].ToString() : "";

        private static DateTime? GetDate(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return null;
            return Convert.ToDateTime(r[col]);
        }

        private static string Slug(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "unknown";
            var cleaned = Regex.Replace(s.ToLower(), @"[^a-z0-9]+", "-");
            return cleaned.Trim('-');
        }
        private void HandleEmailSend()
        {
            var raw = hfSelectedIds.Value;
            if (string.IsNullOrWhiteSpace(raw)) return;

            var selected = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => s.Trim())
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList();
            if (selected.Count == 0) return;

            // Retrieve last results from session (or re-query)
            var dt = Session["LastRecords"] as DataTable;
            if (dt == null) return;

            var records = dt.AsEnumerable()
                        .Where(r => selected.Contains(GetStr(r, COL_SLTN), StringComparer.OrdinalIgnoreCase))
                        .Select(r => new
                        {
                            number = GetStr(r, COL_SLTN),
                            stage = GetStr(r, COL_STAGE),
                            owner = GetStr(r, COL_OWNER),
                            status = GetStr(r, COL_STATUS)   // ← add this
                        })
                        .ToList();

            int sent = SendEmails(records);
            // Show a toast / alert
            ScriptManager.RegisterStartupScript(this, GetType(), "emailsent",
                $"alert('Emails sent to {sent} recipient(s).');", true);
        }
        private int SendEmails(IEnumerable<dynamic> records)
        {
            var mxServers = GetMXServers();
            int count = 0;

            foreach (var rec in records)
            {
                try
                {
                    string user = rec.owner?.Trim();
                    if (string.IsNullOrWhiteSpace(user))
                        continue;

                    string toEmail = user + "@company.com";
                    string sltn = rec.number;
                    string stage = rec.stage;
                    // assume your dynamic has the status field named u_tech_dev_head_approval_status
                    string status = (rec.status ?? "").Trim();

                    // get the HTML body from your EmailTemplates class
                    string bodyHtml = EmailTemplates.GetBody(sltn, stage, status, user);

                    using (var mail = new System.Net.Mail.MailMessage())
                    {
                        mail.From = new System.Net.Mail.MailAddress("no-reply@yourdomain.com");
                        mail.To.Add(toEmail);
                        mail.Subject = $"Reminder for SLTN {sltn}";
                        mail.Body = bodyHtml;
                        mail.IsBodyHtml = true;

                        using (var client = new System.Net.Mail.SmtpClient())
                        {
                            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                            client.UseDefaultCredentials = true;

                            bool sentThis = false;
                            foreach (var host in mxServers.Keys)
                            {
                                try
                                {
                                    client.Host = host;
                                    client.Send(mail);
                                    sentThis = true;
                                    break;
                                }
                                catch
                                {
                                    // try next MX host
                                }
                            }
                            if (sentThis)
                                count++;
                        }
                    }
                }
                catch
                {
                    // TODO: log the exception
                }
            }

            return count;
        }

        private Dictionary<string, string> GetMXServers()
        {
            return new Dictionary<string, string>
            {
                { "smtp.company.com", "" }  // add more if you have backups
            };
        }
        protected void btnSendEmails_Click(object sender, EventArgs e)
        {
            HandleEmailSend();
            LoadData();
        }

        private class GroupDTO
        {
            public string Key { get; set; }
            public string KeySlug { get; set; }
            public int Count { get; set; }
            public List<DataRow> Rows { get; set; }
        }

        private class SectorTabDTO
        {
            public string Key { get; set; }
            public string KeySlug { get; set; }
            public int Count { get; set; }
            public List<DataRow> Rows { get; set; }
            public List<StageCountDTO> StageBreakdown { get; set; }
        }

        private class StageCountDTO
        {
            public string Stage { get; set; }
            public string StageSlug { get; set; }
            public int Count { get; set; }
        }
    }

    internal static class StringExt
    {
        public static string NullIfEmpty(this string s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
