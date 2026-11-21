<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RoadRestrictions.ascx.cs"
    Inherits="DropdownTests.Controls.RoadRestrictions" %>

<%-- No JavaScript. The browser's built-in popover opens and closes the picker:
     popovertarget on a button names the element to show. The button and the picker
     can be anywhere on the page; their order doesn't matter. --%>

<div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
  <h3 class="h4 m-0">Route obstructions and restrictions</h3>
  <button type="button" class="btn btn-primary" popovertarget="<%= ClientID %>_picker">Add restriction</button>
</div>

<div id="<%= ClientID %>_picker" popover class="rp-overlay" role="dialog" aria-label="Restrictions on this route">
  <div class="rp-head">
    <h5 class="rp-title">Restrictions on this route</h5>
    <div class="rp-sub"><%= AllowMultiple ? "Tick the restrictions that apply, then Save." : "Choose one restriction to add, then press Add." %></div>
  </div>
  <div class="rp-body">
    <asp:Repeater ID="rptRestrictions" runat="server">
      <ItemTemplate>
        <asp:PlaceHolder runat="server" Visible='<%# Eval("IsFirstInGroup") %>'>
          <div class="rp-group"><%#: Eval("GroupName") %></div>
        </asp:PlaceHolder>
        <label class="rp-item">
          <%-- AllowMultiple="true": a tick box, ticked when attached --%>
          <asp:CheckBox ID="chk" runat="server" Checked='<%# Eval("IsAttached") %>' Visible='<%# AllowMultiple %>' />
          <%-- AllowMultiple="false": one radio group (plain HTML, so the radios share a name).
               Restrictions already on the route can't be chosen. --%>
          <asp:PlaceHolder runat="server" Visible='<%# !AllowMultiple %>'>
            <input type="radio" name="<%# PickName %>" value="<%# Eval("Id") %>" <%# (bool)Eval("IsAttached") ? "disabled" : "" %> />
          </asp:PlaceHolder>
          <asp:HiddenField ID="hfId" runat="server" Value='<%# Eval("Id") %>' />
          <span class="rp-text">
            <span class="rp-name"><%#: Eval("Name") %></span>
            <asp:PlaceHolder runat="server" Visible='<%# !AllowMultiple && (bool)Eval("IsAttached") %>'>
              <span class="rp-tag">Already added</span>
            </asp:PlaceHolder>
            <%# Paragraphs((string)Eval("Description")) %>
          </span>
        </label>
      </ItemTemplate>
    </asp:Repeater>
  </div>
  <div class="rp-bar">
    <button type="button" class="btn btn-outline-secondary" popovertarget="<%= ClientID %>_picker" popovertargetaction="hide">Cancel</button>
    <asp:Button ID="btnSaveRestrictions" runat="server" CssClass="btn btn-primary"
        CausesValidation="false" OnClick="btnSaveRestrictions_Click" />
  </div>
</div>

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
