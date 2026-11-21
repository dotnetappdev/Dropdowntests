<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RoadRestrictions.ascx.cs"
    Inherits="DropdownTests.Controls.RoadRestrictions" %>

<%-- No JavaScript. The browser's built-in popover opens and closes the picker:
     popovertarget on a button names the element to show. The button and the picker
     can be anywhere on the page; their order doesn't matter. --%>

<div class="uc-toolbar">
  <h3>Route obstructions and restrictions</h3>
  <button type="button" class="btn-primary" popovertarget="<%= ClientID %>_picker">Add restriction</button>
</div>

<div id="<%= ClientID %>_picker" popover class="rp-overlay" role="dialog" aria-label="Restrictions on this route">
  <div class="rp-head">
    <div class="rp-title">Restrictions on this route</div>
    <div class="rp-sub">Tick the restrictions that apply, then Save.</div>
  </div>
  <div class="rp-body">
    <asp:Repeater ID="rptRestrictions" runat="server">
      <ItemTemplate>
        <asp:PlaceHolder runat="server" Visible='<%# Eval("IsFirstInGroup") %>'>
          <div class="rp-group"><%#: Eval("GroupName") %></div>
        </asp:PlaceHolder>
        <label class="rp-item">
          <asp:CheckBox ID="chk" runat="server" Checked='<%# Eval("IsAttached") %>' />
          <asp:HiddenField ID="hfId" runat="server" Value='<%# Eval("Id") %>' />
          <span class="rp-text">
            <span class="rp-name"><%#: Eval("Name") %></span>
            <%# Paragraphs((string)Eval("Description")) %>
          </span>
        </label>
      </ItemTemplate>
    </asp:Repeater>
  </div>
  <div class="rp-bar">
    <button type="button" class="rp-btn" popovertarget="<%= ClientID %>_picker" popovertargetaction="hide">Cancel</button>
    <asp:Button ID="btnSaveRestrictions" runat="server" Text="Save" CssClass="rp-btn rp-primary"
        CausesValidation="false" OnClick="btnSaveRestrictions_Click" />
  </div>
</div>

<div class="grid-box">
<asp:GridView ID="gvRestrictions" runat="server" AutoGenerateColumns="false"
    CssClass="gv" DataKeyNames="Id" UseAccessibleHeader="true"
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
        <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Rows="4"
            Text='<%# Bind("Notes") %>' />
      </EditItemTemplate>
    </asp:TemplateField>

    <asp:TemplateField>
      <ItemTemplate>
        <asp:LinkButton runat="server" CommandName="Edit" Text="Edit" />
      </ItemTemplate>
      <EditItemTemplate>
        <asp:LinkButton runat="server" CommandName="Update" Text="Update" />
        <asp:LinkButton runat="server" CommandName="Cancel" Text="Cancel" CausesValidation="false" />
      </EditItemTemplate>
    </asp:TemplateField>
  </Columns>
</asp:GridView>
</div>
