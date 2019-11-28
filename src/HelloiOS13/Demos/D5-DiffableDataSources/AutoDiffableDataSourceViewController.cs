using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using HelloiOS13.Helpers.DiffableDataSource;
using UIKit;

namespace HelloiOS13.D5.D1
{
    [DisplayInMenu(DisplayName = "Diffable Data Source (auto)", DisplayDescription = "Magic automated diffable data")]
    public class AutoDiffableDataSourceViewController : BaseViewController
    {
        bool gone;

        protected Random R = new Random();
        protected Dictionary<string, List<int>> Data = new Dictionary<string, List<int>>
        {
            ["section 1"] = new List<int> { 100, 200, 300 },
            ["section 2"] = new List<int> { 400, 500, 600 },
            ["section 3"] = new List<int> { 700, 800, 900 },
        };

        protected UITableView TableView;
        protected UITableViewDiffableDataSource<IdentifierType<string>, IdentifierType<int>> Source;

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView = new UITableView(CoreGraphics.CGRect.Empty, UITableViewStyle.Grouped);
            Source = new UITableViewDiffableDataSource<IdentifierType<string>, IdentifierType<int>>(
                TableView, 
                (tv, indexPath, identifier) => 
                {
                    var cell = new UITableViewCell(UITableViewCellStyle.Default, "abc");
                    cell.TextLabel.Text = $"{(identifier as IdentifierType<int>).Item:N0}";

                    return cell;
                }); 

            View.FillWith(TableView);

            await MutateForever();
        }

        public async Task MutateForever()
        {
            while (!gone)
            {
                Data = Mutate(Data);
                Source.ApplySnapshot(Data.ToDiffableDataSourceSnapshot(), true); 

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public Dictionary<string, List<int>> Mutate(Dictionary<string, List<int>> data)
        {
            var nextData = data.ToDictionary(x => x.Key, x => x.Value.ToList());

            foreach (var section in nextData)
            {
                var roll = R.NextDouble();

                if (roll > .65)
                    section.Value.Add(NumberNotContainedInData(nextData));
                else if (roll < .4 && section.Value.Count > 1)
                    section.Value.Remove(RandomItemFrom(section.Value));
            }

            return nextData;
        }
        
        private int NumberNotContainedInData(Dictionary<string, List<int>> data)
        {
            var allValues = Data.Values.SelectMany(x => x).ToHashSet();
            var v = 0;

            while (allValues.Contains(v))
                v = R.Next(0, 1000);

            return v;
        }

        private T RandomItemFrom<T>(ICollection<T> collection)
            => collection.ElementAt(R.Next(0, collection.Count));

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            gone = true;
        }
    }
}

