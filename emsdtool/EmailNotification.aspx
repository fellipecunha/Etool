<%@ Page Title="STLN Notification Dashboard" Language="C#" MasterPageFile="~/Site.master"
    AutoEventWireup="true" CodeBehind="EmailNotification.aspx.cs"
    Inherits="emsdtool.EmailNotification" %>

<asp:Content ID="HeadStuff" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>

    <style>
        .page-title-bar{margin-bottom:1.25rem;padding:12px 0;border-bottom:1px solid #e5e7eb;}
        .page-title-bar h1{margin:0;font-size:1.55rem;font-weight:600;color:#1f2937;}

        .filter-toggle{margin-bottom:1rem;}
        .filter-card{border:1px solid #e5e7eb;border-radius:10px;background:#fafafa;}
        .filter-card .card-body{padding:22px 26px;}

        .dropdown-menu .form-check{margin-bottom:.35rem;}
        #sectorBtn .sector-summary{overflow:hidden;text-overflow:ellipsis;white-space:nowrap;flex:1 1 auto;}
        .sector-chip{background:#e0e7ff;color:#1e3a8a;border-radius:12px;padding:2px 8px;font-size:.75rem;margin-right:4px;display:inline-block;}

        .kpi-row{margin-bottom:1.25rem;}
        .kpi{background:#fff;border:1px solid #e5e7eb;border-radius:8px;padding:14px 18px;text-align:center;box-shadow:0 1px 2px rgba(0,0,0,.03);}
        .kpi .num{display:block;font-size:1.4rem;font-weight:700;color:#2563eb;line-height:1;}
        .kpi .lbl{font-size:.8rem;color:#6b7280;margin-top:4px;}

        .list-group-item:hover{text-decoration:none;background:#f9fafb;}
        .empty-state{padding:28px 0;text-align:center;color:#6b7280;font-size:.9rem;}

        .sticky-top{position:sticky;top:0;z-index:2;}
        .modal-body .form-control{box-shadow:none;}

        .loader-overlay{position:fixed;inset:0;background:rgba(255,255,255,.78);display:flex;align-items:center;justify-content:center;z-index:2000;}
        .d-none{display:none!important;}
    </style>
</asp:Content>

<asp:Content ID="HeaderStuff" ContentPlaceHolderID="PageHeader" runat="server">
        <h1>STLN Notification Dashboard</h1>
</asp:Content>

<asp:Content ID="MainStuff" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Mobile filter toggle -->
    <div class="filter-toggle d-md-none">
        <button class="btn btn-outline-secondary w-100" type="button"
                data-bs-toggle="collapse" data-bs-target="#filtersCollapse"
                aria-expanded="false" aria-controls="filtersCollapse">
            Filters
        </button>
    </div>

    <!-- Filters -->
    <div id="filtersCollapse" class="collapse show">
        <div class="card filter-card mb-4 shadow-sm border-0">
            <div class="card-body">
                <div class="row gy-3 gx-4 align-items-start">

                    <!-- Funding Sector (multi-select dropdown) -->
                    <div class="col-md-4">
                        <label class="form-label fw-semibold mb-2">Sector</label>
                        <div class="dropdown w-100" data-bs-auto-close="outside">
                            <button class="btn btn-outline-secondary w-100 text-start d-flex justify-content-between align-items-center"
                                    type="button" id="sectorBtn" data-bs-toggle="dropdown" aria-expanded="false">
                                <span id="sectorSummary" class="sector-summary text-muted">Select sectors…</span>
                                <i class="fa fa-chevron-down ms-2 small"></i>
                            </button>

                            <div class="dropdown-menu p-3 w-100" style="max-height:220px; overflow-y:auto;">
                                <asp:CheckBoxList ID="cblSectors" runat="server"
                                                  CssClass="form-check sector-list" RepeatLayout="Flow" />
                                <div class="d-flex justify-content-between mt-2">
                                    <button type="button" class="btn btn-sm btn-link p-0"
                                            onclick="selectAllInList('<%= cblSectors.ClientID %>', 'sectorSummary', true)">Select all</button>
                                    <button type="button" class="btn btn-sm btn-link text-danger p-0"
                                            onclick="selectAllInList('<%= cblSectors.ClientID %>', 'sectorSummary', false)">Clear</button>
                                    <button type="button" class="btn btn-sm btn-primary" data-bs-toggle="dropdown">Done</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- Tech Dev Head Approval Status (multi-select dropdown) -->
                    <div class="col-md-4">
                        <label class="form-label fw-semibold mb-2">Approval Status</label>
                        <div class="dropdown w-100" data-bs-auto-close="outside">
                            <button class="btn btn-outline-secondary w-100 text-start d-flex justify-content-between align-items-center"
                                    type="button" id="statusBtn" data-bs-toggle="dropdown" aria-expanded="false">
                                <span id="statusSummary" class="sector-summary text-muted">Select status…</span>
                                <i class="fa fa-chevron-down ms-2 small"></i>
                            </button>

                            <div class="dropdown-menu p-3 w-100" style="max-height:220px; overflow-y:auto;">
                                <asp:CheckBoxList ID="cblStatuses" runat="server"
                                                  CssClass="form-check status-list" RepeatLayout="Flow" />
                                <div class="d-flex justify-content-between mt-2">
                                    <button type="button" class="btn btn-sm btn-link p-0"
                                            onclick="selectAllInList('<%= cblStatuses.ClientID %>', 'statusSummary', true)">Select all</button>
                                    <button type="button" class="btn btn-sm btn-link text-danger p-0"
                                            onclick="selectAllInList('<%= cblStatuses.ClientID %>', 'statusSummary', false)">Clear</button>
                                    <button type="button" class="btn btn-sm btn-primary" data-bs-toggle="dropdown">Done</button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Max Days -->
                    <div class="col-md-3 col-lg-2">
                        <label class="form-label fw-semibold mb-2">Max Allowed Days</label>
                        <asp:TextBox ID="txtMaxDays" runat="server" CssClass="form-control" Text="30" TextMode="Number" />
                    </div>

                    <!-- Load -->
                    <div class="col-md-3 col-lg-2 d-grid align-self-end">
                        <asp:Button ID="btnLoad" runat="server"
                                    CssClass="btn btn-primary btn-lg"
                                    Text="Load Data"
                                    OnClick="btnLoad_Click"
                                    UseSubmitBehavior="false"
                                    OnClientClick="return startLoaderPostback();" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- KPI row -->
    <div class="row kpi-row justify-content-center" runat="server" id="kpiRow" visible="false">
        <div class="col-4 col-md-2">
            <div class="kpi text-center">
                <span class="num"><asp:Literal ID="litTotalFlagged" runat="server" /></span>
                <span class="lbl">Total Flagged</span>
            </div>
        </div>
        <div class="col-4 col-md-2">
            <div class="kpi text-center">
                <span class="num"><asp:Literal ID="litSectorCount" runat="server" /></span>
                <span class="lbl">Sectors</span>
            </div>
        </div>
        <div class="col-4 col-md-2">
            <div class="kpi text-center">
                <span class="num"><asp:Literal ID="litStageCount" runat="server" /></span>
                <span class="lbl">Stages</span>
            </div>
        </div>
    </div>

    <!-- Tabs + View ALL -->
    <asp:Panel ID="pnlTabsWrapper" runat="server" Visible="false">

        <div class="d-flex justify-content-end mb-2">
            <button type="button"
                    class="btn btn-sm btn-outline-dark"
                    data-bs-toggle="modal"
                    data-bs-target="#allModal">
                View ALL (<asp:Literal ID="litAllCount" runat="server" />)
            </button>
        </div>

        <ul class="nav nav-tabs mb-3" id="sectorTabs" role="tablist">
            <asp:Repeater ID="rptSectorTabsNav" runat="server">
                <ItemTemplate>
                    <li class="nav-item" role="presentation">
                        <button class='nav-link <%# Container.ItemIndex==0 ? "active" : "" %>'
                                id='tab-btn-<%# Eval("KeySlug") %>'
                                data-bs-toggle="tab"
                                data-bs-target='#tab-<%# Eval("KeySlug") %>'
                                type="button" role="tab">
                            <%# Eval("Key") %>
                            <span class="badge bg-secondary ms-1"><%# Eval("Count") %></span>
                        </button>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>

        <div class="tab-content" id="sectorTabsContent">
            <asp:Repeater ID="rptSectorTabsContent" runat="server" OnItemDataBound="rptSectorTabsContent_ItemDataBound">
                <ItemTemplate>
                    <div class='tab-pane fade <%# Container.ItemIndex==0 ? "show active" : "" %>'
                         id='tab-<%# Eval("KeySlug") %>' role='tabpanel'
                         aria-labelledby='tab-btn-<%# Eval("KeySlug") %>'>

                        <div class="d-flex justify-content-end mb-2">
                            <button type="button"
                                    class="btn btn-sm btn-outline-primary"
                                    data-bs-toggle="modal"
                                    data-bs-target='#fundingModal_<%# Eval("KeySlug") %>'>
                                View all (<%# Eval("Count") %>)
                            </button>
                        </div>

                        <asp:Repeater ID="rptStagesInside" runat="server">
                            <HeaderTemplate><div class="list-group shadow-sm mb-4"></HeaderTemplate>
                            <ItemTemplate>
                                <a href="#"
                                   class="list-group-item list-group-item-action d-flex justify-content-between align-items-center"
                                   data-bs-toggle="modal"
                                   data-bs-target='#phaseModal_<%# Eval("StageSlug") %>'>
                                    <span><%# Eval("Stage") %></span>
                                    <span class="badge bg-danger rounded-pill"><%# Eval("Count") %></span>
                                </a>
                            </ItemTemplate>
                            <FooterTemplate></div></FooterTemplate>
                        </asp:Repeater>

                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>

    <!-- Empty state -->
    <asp:Panel ID="pnlNoRecords" runat="server" Visible="false" CssClass="empty-state">
        ✅ No flagged records found.
    </asp:Panel>

    <!-- Modals -->
    <asp:PlaceHolder ID="phModals" runat="server" />

    <footer class="mt-5 text-muted small text-center">
        © <%: DateTime.Now.Year %> EMSD Web Portal
    </footer>

    <!-- Spinner overlay -->
    <div id="loaderOverlay" class="loader-overlay d-none">
        <div class="text-center">
            <div class="spinner-border" role="status" style="width:3rem;height:3rem;"></div>
            <div class="mt-3 fw-semibold">Loading…</div>
        </div>
    </div>

    <asp:HiddenField ID="hfSelectedIds" runat="server" />
    <asp:Button 
        ID="btnSendEmails" 
        runat="server" 
        OnClick="btnSendEmails_Click" 
        Style="display:none;" 
    />
    <script>
        /* -------- Loader -------- */
        var loaderStart = 0;
        const MIN_SPIN_MS = 2000;

        function showOverlay() {
            document.getElementById('loaderOverlay').classList.remove('d-none');
            loaderStart = Date.now();
        }
        function hideOverlay() {
            document.getElementById('loaderOverlay').classList.add('d-none');
        }

        function hasSelection(listClientId) {
            return document.querySelectorAll('#' + listClientId + ' input[type="checkbox"]:checked').length > 0;
        }

        function startLoaderPostback() {
            // --- validate both dropdowns ---
            if (!hasSelection("<%= cblSectors.ClientID %>")) {
            alert("Please select at least one Funding Sector.");
            return false;
        }
        if (!hasSelection("<%= cblStatuses.ClientID %>")) {
            alert("Please select at least one Tech Dev Head Approval Status.");
            return false;
        }

        // ok, show spinner and fire postback after minimum delay
        showOverlay();
        setTimeout(function () {
            __doPostBack('<%= btnLoad.UniqueID %>', '');
        }, MIN_SPIN_MS);
        return false;
    }

    // If using UpdatePanel, uncomment:
    /*
    (function(){
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_beginRequest(function () { showOverlay(); });
            prm.add_endRequest(function () {
                var elapsed = Date.now() - loaderStart;
                var wait = Math.max(0, MIN_SPIN_MS - elapsed);
                setTimeout(hideOverlay, wait);
            });
        }
    })();
    */

        /* -------- Multi-select summaries (both dropdowns) -------- */
        (function () {
            const sectorsId = "<%= cblSectors.ClientID %>";
    const statusesId = "<%= cblStatuses.ClientID %>";

    document.addEventListener('change', function (e) {
        if (e.target.closest('#' + sectorsId)) updateSummary(sectorsId, "sectorSummary");
        if (e.target.closest('#' + statusesId)) updateSummary(statusesId, "statusSummary");
    });

    document.addEventListener('DOMContentLoaded', function () {
        updateSummary(sectorsId, "sectorSummary");
        updateSummary(statusesId, "statusSummary");
    });

    // expose globally so Select All buttons can call it
    window.updateSummary = function (listId, spanId) {
        const span = document.getElementById(spanId);
        if (!span) return;

        const container = document.getElementById(listId);
        const boxes = container ? container.querySelectorAll('input[type="checkbox"]') : [];

        const selected = [];
        boxes.forEach(cb => {
            if (cb.checked) {
                const lbl = container.querySelector('label[for="' + cb.id + '"]');
                selected.push(lbl ? lbl.textContent.trim() : cb.value);
            }
        });

        if (selected.length === 0) {
            span.textContent = 'Select…';
            span.classList.add('text-muted');
        } else {
            span.classList.remove('text-muted');
            span.innerHTML = selected.map(t => `<span class="sector-chip">${t}</span>`).join('');
        }
    };

    // also global
    window.selectAllInList = function (listId, spanId, check) {
        const boxes = document.querySelectorAll('#' + listId + ' input[type="checkbox"]');
        boxes.forEach(b => b.checked = check);
        updateSummary(listId, spanId);   // refresh chips immediately
    };
})();

        /* -------- Modal table helpers -------- */
        function filterRows(input, tableId, selectAllId) {
            const val = input.value.toLowerCase().trim();
            const tokens = val.split(/[\s,]+/).filter(Boolean);
            const rows = document.querySelectorAll("#" + tableId + " tbody tr");

            rows.forEach(tr => {
                const sltnCell = tr.querySelector(".sltn-number");
                const sltn = sltnCell ? sltnCell.textContent.toLowerCase() : "";
                const match = tokens.length === 0 || tokens.some(t => sltn.indexOf(t) !== -1);
                tr.style.display = match ? "" : "none";
                if (!match) {
                    const chk = tr.querySelector("input[type='checkbox']");
                    if (chk) chk.checked = false;
                }
            });

            const visibleSel = "#" + tableId + " tbody tr:not([style*='display: none']) input[type='checkbox']";
            const vis = document.querySelectorAll(visibleSel);
            const visOn = document.querySelectorAll(visibleSel + ":checked");
            const selAll = document.getElementById(selectAllId);
            if (selAll) selAll.checked = (vis.length > 0 && vis.length === visOn.length);
        }

        function toggleAllVisible(cb, tableId) {
            const checks = document.querySelectorAll("#" + tableId + " tbody tr:not([style*='display: none']) input[type='checkbox']");
            checks.forEach(chk => chk.checked = cb.checked);
        }

        function sendEmail(modalId) {
            alert("Implement email logic for modal: " + modalId);
        }

        function selectAllInList(listId, spanId, check) {
            const boxes = document.querySelectorAll('#' + listId + ' input[type="checkbox"]');
            boxes.forEach(b => b.checked = check);
            // reuse existing updater
            if (typeof updateSummary === 'function') {
                updateSummary(listId, spanId);
            }
        }

    </script>

    <script>
        function sendEmail(modalId) {
            // 1) collect all checked IDs in that modal
            var selector = "#recordsTable_" + modalId + " tbody input[type='checkbox']:checked";
            var ids = Array.from(document.querySelectorAll(selector)).map(chk => chk.value);
            if (ids.length === 0) {
                alert("Please select at least one record to email.");
                return;
            }

            // 2) stash them into the hidden field
            document.getElementById("<%= hfSelectedIds.ClientID %>").value = ids.join(",");

    // 3) trigger the hidden server button
    __doPostBack('<%= btnSendEmails.UniqueID %>', '');
        }
    </script>


</asp:Content>
