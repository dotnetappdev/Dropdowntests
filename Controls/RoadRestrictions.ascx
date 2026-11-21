<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RoadRestrictions.ascx.cs"
    Inherits="DropdownTests.Controls.RoadRestrictions" %>

<div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
  <h3 class="h4 m-0">Route obstructions and restrictions</h3>
  <%-- AllowMultiple="true": opens the full-screen tick list to add several at once (no postback).
       Its onclick is set in code-behind. --%>
  <button type="button" id="btnPickMany" runat="server" class="btn btn-primary">Add restriction</button>
  <%-- AllowMultiple="false": adds a new row to the grid with a dropdown to choose the restriction --%>
  <asp:Button ID="btnAddRow" runat="server" Text="Add restriction" CssClass="btn btn-primary"
      CausesValidation="false" OnClick="btnAddRow_Click" />
</div>

<%-- AllowMultiple="true" only: the hidden source list for the tick list, and the Save button it clicks --%>
<asp:ListBox ID="lstRestrictions" runat="server" SelectionMode="Multiple"
    data-title="Restrictions on this route" Style="display:none" />
<asp:Button ID="btnSaveRestrictions" runat="server" Text="Save" CausesValidation="false"
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
    <asp:TemplateField HeaderText="Notes" ItemStyle-CssClass="notes">
      <ItemTemplate>
        <div class="rname"><%#: Eval("RestrictionName") %></div>
        <div class="ntext"><%#: Eval("Notes") %></div>
      </ItemTemplate>
      <EditItemTemplate>
        <%-- The old-style dropdown list, filled in code-behind (RowDataBound). Tapping it opens the
             full-screen list; that list's Update and Cancel press this row's Update and Cancel. --%>
        <div class="rp-select" onclick="RestrictionPicker.fromSelect(this.querySelector('select'))">
          <asp:DropDownList ID="ddlRestriction" runat="server" CssClass="form-select"
              data-title="Obstruction / restriction"
              onkeydown="return RestrictionPicker.selectKey(this, event)" />
        </div>
        <asp:Label ID="lblRowError" runat="server" CssClass="text-danger small" EnableViewState="false" />
        <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control mt-2"
            Text='<%# Bind("Notes") %>' />
      </EditItemTemplate>
    </asp:TemplateField>

    <asp:TemplateField>
      <ItemTemplate>
        <asp:LinkButton runat="server" CommandName="Edit" Text="Edit" />
      </ItemTemplate>
      <EditItemTemplate>
        <%-- gv-update and gv-cancel let the full-screen list press these --%>
        <asp:LinkButton runat="server" CommandName="Update" Text="Update" CssClass="gv-update me-2" />
        <asp:LinkButton runat="server" CommandName="Cancel" Text="Cancel" CssClass="gv-cancel" CausesValidation="false" />
      </EditItemTemplate>
    </asp:TemplateField>
  </Columns>
</asp:GridView>
</div>
