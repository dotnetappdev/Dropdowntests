<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RoadRestrictions.ascx.cs"
    Inherits="DropdownTests.Controls.RoadRestrictions" %>

<div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
  <h3 class="h4 m-0">Route obstructions and restrictions</h3>
  <button type="button" class="btn btn-primary"
      onclick="RestrictionPicker.open('<%= lstRestrictions.ClientID %>', '<%= btnSaveRestrictions.ClientID %>')">
    Add restriction</button>
</div>

<%-- Source list of all restrictions (hidden). Code-behind sets SelectionMode from AllowMultiple
     and pre-selects the attached ones (or marks them "Already added" when choosing one). --%>
<asp:ListBox ID="lstRestrictions" runat="server"
    data-title="Restrictions on this route" Style="display:none" />
<%-- Clicked by the picker's Save button. No validation, so other validators on the page are left alone. --%>
<asp:Button ID="btnSaveRestrictions" runat="server" CausesValidation="false"
    OnClick="btnSaveRestrictions_Click" Style="display:none" />

<div class="grid-box border rounded">
<asp:GridView ID="gvRestrictions" runat="server" AutoGenerateColumns="false"
    CssClass="table table-striped gv" GridLines="None" DataKeyNames="Id" UseAccessibleHeader="true"
    OnRowDataBound="gvRestrictions_RowDataBound"
    OnRowEditing="gvRestrictions_RowEditing"
    OnRowUpdating="gvRestrictions_RowUpdating"
    OnRowCancelingEdit="gvRestrictions_RowCancelingEdit"
    EmptyDataText="No restrictions attached yet. Click Add restriction.">
  <Columns>
    <%-- The only data column. The restriction name is a small heading in the cell so you can
         tell which note is which; delete the rname div if you don't want it. --%>
    <asp:TemplateField HeaderText="Notes" ItemStyle-CssClass="notes">
      <ItemTemplate>
        <div class="rname"><%#: Eval("RestrictionName") %></div>
        <div class="ntext"><%#: Eval("Notes") %></div>
      </ItemTemplate>
      <EditItemTemplate>
        <div class="rname"><%#: Eval("RestrictionName") %></div>
        <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control mt-1"
            Text='<%# Bind("Notes") %>' />
      </EditItemTemplate>
    </asp:TemplateField>

    <asp:TemplateField>
      <ItemTemplate>
        <asp:LinkButton runat="server" CommandName="Edit" Text="Edit" />
      </ItemTemplate>
      <EditItemTemplate>
        <asp:LinkButton runat="server" CommandName="Update" Text="Update" CssClass="me-2" />
        <asp:LinkButton runat="server" CommandName="Cancel" Text="Cancel" CausesValidation="false" />
      </EditItemTemplate>
    </asp:TemplateField>
  </Columns>
</asp:GridView>
</div>
