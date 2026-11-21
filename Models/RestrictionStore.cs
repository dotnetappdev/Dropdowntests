using System.Collections.Generic;
using System.Linq;

namespace DropdownTests.Models
{
    // Sample data so the page runs on its own. Replace RestrictionTypes and RestrictionStore
    // with your own data access (for example an EF Core DbContext).

    public class RestrictionType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public int GroupOrder { get; set; }
        /// <summary>Long description. A blank line starts a new paragraph.</summary>
        public string Description { get; set; }
    }

    public class RouteRestriction
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int RestrictionTypeId { get; set; }
        public string RestrictionName { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>The source list of every restriction.</summary>
    public static class RestrictionTypes
    {
        private static readonly List<RestrictionType> All = Build();

        public static IList<RestrictionType> GetAll() { return All; }

        public static RestrictionType Get(int id) { return All.FirstOrDefault(t => t.Id == id); }

        private static List<RestrictionType> Build()
        {
            var groups = new[]
            {
                new { Name = "Height", Items = new[] { "Low bridge", "Overhead power cables", "Overhead telecom cables", "Overhanging trees", "Footbridge", "Gantry / overhead sign", "Height-restricted tunnel" } },
                new { Name = "Width", Items = new[] { "Narrow road", "Width restriction", "Narrow bridge", "Traffic island / central reserve", "Bollards", "Street furniture (signs, lamp posts)", "Parked vehicles", "Road works / barriers" } },
                new { Name = "Weight", Items = new[] { "Weight-restricted bridge", "Weak culvert", "Weight limit on road", "Cellar or void under road" } },
                new { Name = "Length and turning", Items = new[] { "Tight bend", "Hairpin bend", "Small roundabout", "Mini roundabout", "Sharp junction" } },
                new { Name = "Ground clearance", Items = new[] { "Hump-back bridge", "Level crossing", "Speed humps / cushions", "Steep gradient", "Severe camber" } },
                new { Name = "Movement conditions", Items = new[] { "Police escort required", "Time-of-day restriction", "Convoy / low speed only", "Contraflow required", "Toll or barrier" } }
            };
            var descriptions = new Dictionary<string, string>
            {
                ["Low bridge"] = "Any bridge where the headroom is less than the load height plus a safety margin. Check the signed height and the measured height on both carriageways, because headroom often differs between lanes and at the kerb.\n\nIf the load cannot clear the bridge, record the diversion in Notes and confirm it with the highway authority before the movement order is issued.",
                ["Overhead power cables"] = "Overhead lines cross many rural routes and are usually lowest at mid-span, not at the poles. Measure the lowest point over the carriageway, not the verge.\n\nIf the clearance is below the safe distance for the voltage, the network operator must lift the line or switch it off. Book this well ahead and put their reference number in Notes.",
                ["Weight-restricted bridge"] = "A structure with a signed or assessed weight limit below the gross weight or axle loads of the movement.\n\nThe structure owner must be notified. They may require the load to cross alone, at walking pace and on the centre line, or refuse the crossing so that a diversion is needed.",
                ["Small roundabout"] = "The swept path of the vehicle is wider than the roundabout allows.\n\nCommon solutions are overrunning the central island, going the wrong way round under escort, or using rear steer.",
                ["Level crossing"] = "Low loaders and long vehicles can ground on the crossing or take too long to clear it. Where the crossing has a phone, the driver must call the signaller and get permission before crossing."
            };

            var list = new List<RestrictionType>();
            for (var g = 0; g < groups.Length; g++)
                foreach (var name in groups[g].Items)
                {
                    string desc;
                    descriptions.TryGetValue(name, out desc);
                    list.Add(new RestrictionType { Id = list.Count + 1, Name = name, GroupName = groups[g].Name, GroupOrder = g, Description = desc });
                }
            return list;
        }
    }

    /// <summary>
    /// Restrictions attached to routes (what the grid shows). In-memory sample only,
    /// registered as a singleton in Startup.
    /// </summary>
    public class RestrictionStore
    {
        private readonly object _sync = new object();
        private readonly List<RouteRestriction> _rows = new List<RouteRestriction>();
        private int _nextId = 1;

        public RestrictionStore()
        {
            // Sample restrictions on route 42
            Attach(42, IdOf("Low bridge"), "A38 Station Road rail bridge, 4.65 m. Load is 4.9 m high.\n\nDivert via A4019.");
            Attach(42, IdOf("Weight-restricted bridge"), "B4632 Mill Brook bridge, 18 t. Bridge assessment requested.");
            Attach(42, IdOf("Small roundabout"), "A449 / Worcester Rd. Overrun the central island with escort holding traffic.");
        }

        private static int IdOf(string name)
        {
            return RestrictionTypes.GetAll().First(t => t.Name == name).Id;
        }

        public IList<RouteRestriction> GetForRoute(int routeId)
        {
            lock (_sync) return _rows.Where(r => r.RouteId == routeId).ToList();
        }

        public void Attach(int routeId, int restrictionTypeId, string notes = "")
        {
            var type = RestrictionTypes.Get(restrictionTypeId);
            if (type == null) return;
            lock (_sync)
            {
                if (_rows.Any(r => r.RouteId == routeId && r.RestrictionTypeId == restrictionTypeId)) return;
                _rows.Add(new RouteRestriction
                {
                    Id = _nextId++, RouteId = routeId, RestrictionTypeId = restrictionTypeId,
                    RestrictionName = type.Name, Notes = notes ?? ""
                });
            }
        }

        /// <summary>Changes a row's restriction and notes (editing a grid row).</summary>
        public void Update(int id, int restrictionTypeId, string notes)
        {
            var type = RestrictionTypes.Get(restrictionTypeId);
            if (type == null) return;
            lock (_sync)
            {
                var row = _rows.FirstOrDefault(r => r.Id == id);
                if (row == null) return;
                if (_rows.Any(r => r.Id != id && r.RouteId == row.RouteId && r.RestrictionTypeId == restrictionTypeId)) return;
                row.RestrictionTypeId = restrictionTypeId;
                row.RestrictionName = type.Name;
                row.Notes = notes ?? "";
            }
        }

        /// <summary>Removes (unattaches) a restriction from its route.</summary>
        public void Detach(int id)
        {
            lock (_sync) _rows.RemoveAll(r => r.Id == id);
        }
    }
}
