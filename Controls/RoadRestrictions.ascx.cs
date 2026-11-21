using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DropdownTests.Controls
{
    public partial class RoadRestrictions : UserControl
    {
        /// <summary>The route whose restrictions are shown. Set by the host page.</summary>
        public int RouteId { get; set; }

        /// <summary>
        /// true (default): tick several restrictions; Save attaches the ticked ones and detaches the unticked ones.
        /// false: choose one restriction to add. Restrictions already on the route are shown as "Already added".
        /// Set it in markup, e.g. &lt;uc:RoadRestrictions runat="server" AllowMultiple="false" /&gt;
        /// </summary>
        public bool AllowMultiple { get; set; } = true;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // Set before the posted selection is loaded, so a single list reads one value
            lstRestrictions.SelectionMode = AllowMultiple ? ListSelectionMode.Multiple : ListSelectionMode.Single;
            btnSaveRestrictions.Text = AllowMultiple ? "Save" : "Add";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Registered by key, so it loads once even if the control is on the page twice
            ScriptManager.RegisterClientScriptInclude(this, typeof(RoadRestrictions),
                "restriction-picker", ResolveUrl("~/Scripts/restriction-picker.js"));

            if (!IsPostBack) BindGrid();
        }

        private void BindGrid()
        {
            gvRestrictions.DataSource = RouteRestrictions.GetForRoute(RouteId);  // the attached ones
            gvRestrictions.DataBind();
        }

        protected void gvRestrictions_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
                e.Row.TableSection = TableRowSection.TableHeader;   // <thead>, for the sticky header
        }

        // Picker's Save. Multiple: attach what is ticked, detach what was unticked.
        // Single: attach the one chosen restriction.
        protected void btnSaveRestrictions_Click(object sender, EventArgs e)
        {
            if (!AllowMultiple)
            {
                int typeId;
                if (int.TryParse(lstRestrictions.SelectedValue, out typeId) &&
                    !RouteRestrictions.GetForRoute(RouteId).Any(a => a.RestrictionTypeId == typeId))
                    RouteRestrictions.Attach(RouteId, typeId);
                gvRestrictions.EditIndex = -1;
                BindGrid();
                return;
            }

            var ticked = lstRestrictions.Items.Cast<ListItem>()
                .Where(li => li.Selected && li.Value != "")
                .Select(li => int.Parse(li.Value))
                .ToList();
            var attached = RouteRestrictions.GetForRoute(RouteId);

            foreach (var typeId in ticked.Except(attached.Select(a => a.RestrictionTypeId)))
                RouteRestrictions.Attach(RouteId, typeId);
            foreach (var a in attached.Where(a => !ticked.Contains(a.RestrictionTypeId)))
                RouteRestrictions.Detach(a.Id);

            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        protected void gvRestrictions_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRestrictions.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void gvRestrictions_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var id = (int)gvRestrictions.DataKeys[e.RowIndex].Value;
            var txtNotes = (TextBox)gvRestrictions.Rows[e.RowIndex].FindControl("txtNotes");
            RouteRestrictions.UpdateNotes(id, txtNotes.Text);
            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        protected void gvRestrictions_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        // After the click handlers: rebuild the source list on every request so the
        // descriptions are always rendered, and pre-select what is attached to this route.
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var attachedIds = new HashSet<string>(RouteRestrictions.GetForRoute(RouteId)
                .Select(a => a.RestrictionTypeId.ToString()));

            lstRestrictions.Items.Clear();
            string group = null;
            foreach (var t in RestrictionTypes.GetAll().OrderBy(t => t.GroupOrder).ThenBy(t => t.Name))
            {
                if (t.GroupName != group)                       // group heading: disabled, empty value
                {
                    var head = new ListItem(t.GroupName, "");
                    head.Attributes["disabled"] = "disabled";
                    lstRestrictions.Items.Add(head);
                    group = t.GroupName;
                }
                var item = new ListItem(t.Name, t.Id.ToString());
                if (AllowMultiple)
                    item.Selected = attachedIds.Contains(item.Value);       // ticked in the picker
                else if (attachedIds.Contains(item.Value))
                    item.Attributes["data-attached"] = "true";              // shown as "Already added"
                if (!string.IsNullOrEmpty(t.Description))
                    item.Attributes["data-desc"] = t.Description;   // blank line = new paragraph
                lstRestrictions.Items.Add(item);
            }
        }
    }
}
