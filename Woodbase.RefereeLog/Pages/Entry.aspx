<%@ Page Title="Create new entry" Language="C#" MasterPageFile="/MasterPages/Default.Master" %>

<asp:Content runat="server" ID="Head" ContentPlaceHolderID="head">
</asp:Content>
<asp:Content runat="server" ID="ContentPlaceHolder1" ContentPlaceHolderID="ContentPlaceHolder1">
    <h1>
        Create a new entry</h1>
    <div id="entry-form">
        <div class="label">
            Match day:</div>
        <input type="datetime" name="matchDay" /><br />
        <div class="label">
            Home team</div>
        <input type="text" name="homeTeam" /><br />
        <div class="label">
            Away team</div>
        <input type="text" name="awayteam" /><br />
        <div class="label">
            Result</div>
        <input type="number" name="homeScore" class="score-field" min="0" />
        -
        <input type="number" name="awayScore" min="0" class="score-field" /><br />
        <div class="label">
            Grade</div>
        <input type="number" name="grade" class="score-field" /><br />
        <div class="label">
            Log entry</div>
        <div class="full-width">
            <CKEditor:CKEditorControl Width="1024" ToolbarCanCollapse="True" ID="CKEditorControl1"
                runat="server" BasePath="/ckEditor"></CKEditor:CKEditorControl></div>
        <div>
            <input type="submit" name="Save" /></div>
    </div>
    <script type="text/javascript">
        $(document).ready(function () {
            //            $('#entry-form input[type="text"]').autocomplete({
            //                serviceUrl: '/WebServices/RefereeLogService.asmx/LookupTeam',
            //                type:'POST',
            //                //lookup: countries,
            //                onSelect: function (suggestion) {
            //                    alert('You selected: ' + suggestion.value + ', ' + suggestion.data);
            //                }
            //            });
            $('#entry-form input[type="text"]').autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "/WebServices/RefereeLogService.asmx/LookupTeam",
                        dataType: "json",
                        type: 'POST',
                        contentType: 'application/json; charset=utf-8',
                        data: {

                    },
                    success: function (data) {
                        response($.map(data.d, function (item) {
                            return {
                                label: item.Name,
                                value: item.Name
                            };
                        }));
                    }
                });
            },
            minLength: 2,
            open: function () {
                $(this).removeClass("ui-corner-all").addClass("ui-corner-top");
            },
            close: function () {
                $(this).removeClass("ui-corner-top").addClass("ui-corner-all");
            }
        });
    });
                
    </script>
</asp:Content>
