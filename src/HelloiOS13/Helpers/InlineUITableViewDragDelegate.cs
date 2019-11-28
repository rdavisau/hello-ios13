using System;
using Foundation;
using UIKit;

namespace ARKitMeetup.Helpers
{
    public class InlineUITableViewDragDelegate : UITableViewDragDelegate
    {
        public Func<UITableView, IUIDragSession, NSIndexPath, UIDragItem[]> _GetItemsForBeginningDragSession { get; set; }

        public Func<UITableView, IUIDragSession, bool> _DragSessionIsRestrictedToDraggingApplication { get; set; }

        public override UIDragItem[] GetItemsForBeginningDragSession(UITableView tableView, IUIDragSession session, NSIndexPath indexPath)
            => _GetItemsForBeginningDragSession?.Invoke(tableView, session, indexPath);

        public override bool DragSessionIsRestrictedToDraggingApplication(UIKit.UITableView tableView, UIKit.IUIDragSession session) =>
            _DragSessionIsRestrictedToDraggingApplication(tableView, session);
    }
}