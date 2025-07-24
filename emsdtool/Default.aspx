<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="emsdtool._Default" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Dashboard
</asp:Content>

<asp:Content ID="cHeader" ContentPlaceHolderID="PageHeader" runat="server">
    <!-- Empty (we’ll use the big center title inside the body) -->
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="dashboard-title">Enterprise Management Service Delivery Tool</h1>

    <div class="dashboard-grid">

        <a runat="server" href="~/EmailNotification.aspx" class="tile">
            <i class="fa fa-envelope"></i>
            <h3>Email Notification</h3>
            <p>Send email to app team reminding them to take action on SLTN.</p>
        </a>

    </div>

    <footer class="dashboard-footer">
        © 2024 Neustar, Inc. All rights reserved. Version 1.0.0 (Build 45)
    </footer>

</asp:Content>