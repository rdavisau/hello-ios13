using System.Collections.Generic;
using ARKitMeetup.Helpers;
using UIKit;

namespace HelloiOS13
{
    public class BaseListViewController<TItem> : BaseViewController
    {
        public UITableView TableView { get; set; }
        public virtual List<TItem> Items { get; set; } = new List<TItem>();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


            TableView = new UITableView
            {
                Source = new InlineTableViewSourceWithoutRowHeight
                {
                    _RowsInSection = (tv, _) => Items.Count,
                    _GetCell = (tv, indexPath) =>
                    {
                        var row = indexPath.Row;
                        var model = Items[row];

                        return GetCell(row, model);
                    }
                }
            };

            View.FillWith(TableView);
        }

        public virtual UITableViewCell GetCell(int row, TItem model)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Default, "cell");

            if (model is UIView v)
                cell.ContentView.FillWith(v, 20, 20);
            else if (model is UIColor c)
                cell.BackgroundColor = c;
            else
                cell.TextLabel.Text = $"{model}";

            return cell;
        }
    }
}

