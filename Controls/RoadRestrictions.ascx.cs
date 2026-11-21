using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DropdownTests.Controls
{
    public partial class RoadRestrictions : UserControl
    {
        /// <summary>The route whose restrictions are shown. Set by the host page.</summary>
        public int RouteId { get; set; }

        /// <summary>
        /// true (default): Add restriction opens a full-screen tick list to add or remove several at once.
        /// false: Add restriction adds a new row to the grid with a dropdown to choose one restriction.
        /// Either way, editing a row shows the restriction as a dropdown that opens full screen.
        /// Set it in markup, e.g. AllowMultiple="false".
        /// </summary>
        public bool AllowMultiple { get; set; } = true;

        /// <summary>True while the grid shows a new, unsaved row (Add restriction with AllowMultiple="false").</summary>
        private bool Adding
        {
            get { return (bool?)ViewState["Adding"] ?? false; }
            set { ViewState["Adding"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindGrid();
        }

        private void BindGrid()
        {
            var rows = RouteRestrictions.GetForRoute(RouteId).ToList();   // the attached ones
            if (Adding)
            {
                rows.Add(new RouteRestriction { Id = 0, RouteId = RouteId, Notes = "" });   // the new, unsaved row
                gvRestrictions.EditIndex = rows.Count - 1;
            }
            gvRestrictions.DataSource = rows;
            gvRestrictions.DataBind();
        }

        /// <summary>Posted name of a grid row's radio group.</summary>
        private static string RadioName(GridViewRow row) { return row.UniqueID + "$pick"; }

        // AllowMultiple="false": add a new row in edit mode
        protected void btnAddRow_Click(object sender, EventArgs e)
        {
            Adding = true;
            BindGrid();
        }

        protected void gvRestrictions_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
                e.Row.TableSection = TableRowSection.TableHeader;   // <thead>, for the sticky header

            // Row in edit mode: fill its full-screen list of restrictions
            if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState & DataControlRowState.Edit) > 0)
            {
                var row = (RouteRestriction)e.Row.DataItem;

                // Restrictions already on another row can't be chosen again
                var taken = new HashSet<int>(RouteRestrictions.GetForRoute(RouteId)
                    .Where(a => a.Id != row.Id).Select(a => a.RestrictionTypeId));

                var rpt = (Repeater)e.Row.FindControl("rptPick");
                var radioName = RadioName(e.Row);
                rpt.DataSource = ListItems((item, t) =>
                {
                    item.IsCurrent = row.Id != 0 && t.Id == row.RestrictionTypeId;
                    item.IsTaken = taken.Contains(t.Id);
                    item.RadioName = radioName;
                });
                rpt.DataBind();
            }
        }

        protected void gvRestrictions_RowEditing(object sender, GridViewEditEventArgs e)
        {
            Adding = false;                     // editing another row drops an unsaved new row
            gvRestrictions.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void gvRestrictions_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var gridRow = gvRestrictions.Rows[e.RowIndex];
            var id = (int)gvRestrictions.DataKeys[e.RowIndex].Value;
            var txtNotes = (TextBox)gridRow.FindControl("txtNotes");

            int typeId;
            if (!int.TryParse(Request.Form[RadioName(gridRow)], out typeId))
            {
                ((Label)gridRow.FindControl("lblRowError")).Text = "Choose a restriction.";
                e.Cancel = true;                // stay in edit mode
                return;
            }

            if (id == 0)
            {
                RouteRestrictions.Attach(RouteId, typeId, txtNotes.Text);
                Adding = false;
            }
            else
            {
                RouteRestrictions.Update(id, typeId, txtNotes.Text);
            }

            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        protected void gvRestrictions_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            Adding = false;
            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        // AllowMultiple="true": the tick list's Save. Attach what is ticked, detach what was unticked.
        protected void btnSaveRestrictions_Click(object sender, EventArgs e)
        {
            var ticked = rptRestrictions.Items.Cast<RepeaterItem>()
                .Where(i => ((CheckBox)i.FindControl("chk")).Checked)
                .Select(i => int.Parse(((HiddenField)i.FindControl("hfId")).Value))
                .ToList();
            var attached = RouteRestrictions.GetForRoute(RouteId);

            foreach (var typeId in ticked.Except(attached.Select(a => a.RestrictionTypeId)))
                RouteRestrictions.Attach(RouteId, typeId);
            foreach (var a in attached.Where(a => !ticked.Contains(a.RestrictionTypeId)))
                RouteRestrictions.Detach(a.Id);

            Adding = false;
            gvRestrictions.EditIndex = -1;
            BindGrid();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            phPickManyButton.Visible = phPickMany.Visible = AllowMultiple;
            btnAddRow.Visible = !AllowMultiple;
            if (!AllowMultiple) return;

            // Rebuild the tick list on every request, ticking what is attached to this route.
            // The page comes back with the list closed.
            var attached = new HashSet<int>(RouteRestrictions.GetForRoute(RouteId).Select(a => a.RestrictionTypeId));
            rptRestrictions.DataSource = ListItems((item, t) => item.IsAttached = attached.Contains(t.Id));
            rptRestrictions.DataBind();
        }

        /// <summary>One row of a full-screen list (bound to the Repeaters).</summary>
        public class PickItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string GroupName { get; set; }
            public string Description { get; set; }
            public bool IsFirstInGroup { get; set; }   // show the group heading before this item
            public bool IsAttached { get; set; }       // tick list: ticked
            public bool IsCurrent { get; set; }        // row list: the row's saved restriction
            public bool IsTaken { get; set; }          // row list: already on another row
            public string RadioName { get; set; }      // row list: the row's radio group
        }

        /// <summary>Every restriction in group order, with <paramref name="fill"/> setting the extra flags.</summary>
        private static List<PickItem> ListItems(Action<PickItem, RestrictionType> fill)
        {
            var list = new List<PickItem>();
            string group = null;
            foreach (var t in RestrictionTypes.GetAll().OrderBy(t => t.GroupOrder).ThenBy(t => t.Name))
            {
                var item = new PickItem
                {
                    Id = t.Id, Name = t.Name, GroupName = t.GroupName, Description = t.Description,
                    IsFirstInGroup = t.GroupName != group
                };
                group = t.GroupName;
                fill(item, t);
                list.Add(item);
            }
            return list;
        }

        /// <summary>Description text to HTML-encoded paragraphs (a blank line starts a new one).</summary>
        protected string Paragraphs(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return string.Concat(Regex.Split(text.Trim(), @"\r?\n\s*\r?\n")
                .Select(p => "<span class=\"rp-desc\">" + HttpUtility.HtmlEncode(p.Trim()) + "</span>"));
        }
    }
}
