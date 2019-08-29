using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ARKitMeetup.Models;
using UIKit;

namespace HelloiOS13.D1.D2
{
    [DisplayInMenu(DisplayName = "Built-in dynamic colours", DisplayDescription = "All of the dynamic colours provided by apple")]
    public class AllDynamicColoursViewController : BaseListViewController<UILabel>
    {
        public override List<UILabel> Items =>
            typeof(UIColor)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(UIColor) && p.Name.EndsWith("Color", StringComparison.Ordinal))
                .OrderByDescending(x => x.Name.StartsWith("System", StringComparison.Ordinal))
                .ThenBy(x => x.Name)
                .Select(p => Label(p.Name, p.GetValue(null) as UIColor))
                .ToList();

        public UILabel Label(string text, UIColor colour = null)
        {
            var l = new UILabel
            {
                Text = text,
                Font = UIFont.SystemFontOfSize(20),
                BackgroundColor = colour,
                TextColor = UIColor.LabelColor
            };

            return l;
        }

        public override UITableViewCell GetCell(int row, UILabel model)
        {
            var cell = base.GetCell(row, model);

            cell.BackgroundColor = cell.ContentView.Subviews[0].BackgroundColor;

            return cell;
        }
    }
}

