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
        /// true (default): tick several restrictions; Save attaches the ticked ones and detaches the unticked ones.
        /// false: choose one restriction to add with radio buttons. Restrictions already on the route
        /// are shown as "Already added". Set it in markup, e.g. AllowMultiple="false".
        /// </summary>
        public bool AllowMultiple { get; set; } = true;

        /// <summary>Posted name of the radio group used when AllowMultiple is false.</summary>
        protected string PickName { get { return UniqueID + "$pick"; } }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            btnSaveRestrictions.Text = AllowMultiple ? "Save" : "Add";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
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

        // Save. Multiple: attach what is ticked, detach what was unticked.
        // Single: attach the one chosen restriction.
        protected void btnSaveRestrictions_Click(object sender, EventArgs e)
        {
            if (!AllowMultiple)
            {
                int typeId;
                if (int.TryParse(Request.Form[PickName], out typeId) &&
                    !RouteRestrictions.GetForRoute(RouteId).Any(a => a.RestrictionTypeId == typeId))
                    RouteRestrictions.Attach(RouteId, typeId);
                gvRestrictions.EditIndex = -1;
                BindGrid();
                return;
            }

            var ticked = rptRestrictions.Items.Cast<RepeaterItem>()
                .Where(i => ((CheckBox)i.FindControl("chk")).Checked)
                .Select(i => int.Parse(((HiddenField)i.FindControl("hfId")).Value))
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

        // After the click handlers: rebuild the tick list on every request, ticking what is
        // attached to this route. The page comes back with the picker closed.
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var attached = new HashSet<int>(RouteRestrictions.GetForRoute(RouteId).Select(a => a.RestrictionTypeId));
            string group = null;
            rptRestrictions.DataSource = RestrictionTypes.GetAll()
                .OrderBy(t => t.GroupOrder).ThenBy(t => t.Name)
                .Select(t =>
                {
                    var first = t.GroupName != group;
                    group = t.GroupName;
                    return new
                    {
                        t.Id, t.Name, t.GroupName, t.Description,
                        IsFirstInGroup = first, IsAttached = attached.Contains(t.Id)
                    };
                })
                .ToList();
            rptRestrictions.DataBind();
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
