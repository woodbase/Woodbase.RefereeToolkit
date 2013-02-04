<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header.ascx.cs" Inherits="Woodbase.RefereeLog.UserControls.Header" %>
<div id="header">
    <div id="logo-container">
        <img id="logo" alt="" src="/Images/Design/logbook.png" /></div>
    <nav>
        <ul id="top-level-menu">
            <li><a href="/Home.aspx">Home</a></li>
            <li><a href="/Pages/Entry.aspx">Entry</a></li>
            <li><a href="/Pages/Stats.aspx">Statistics</a></li>
            <li><a href="/Pages/Others.aspx">Others</a></li>
        </ul>
    </nav>
</div>

<script type="text/javascript">
    $(document).ready(function () {
        $.each($("#top-level-menu a"), function (index, item) {
            if (location.href.indexOf($(item).attr('href')) > 0) {
                $(item).attr('class', 'active');
            } else {
                $(item).removeAttr('class');
            }
        });
    });
</script>
