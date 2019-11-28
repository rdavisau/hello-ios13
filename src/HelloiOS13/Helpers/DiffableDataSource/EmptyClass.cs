using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace HelloiOS13.Helpers.DiffableDataSource
{
    public static class DiffableDataSourceExtensions
    {
        // this implementation relies on dictionary key order
        // think hard about whether that's ok for you 
        public static NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>>
            ToDiffableDataSourceSnapshot<TSection, TItem>(this Dictionary<TSection, List<TItem>> dict)
        {
            var snapshot = new NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>>();

            foreach (var kvp in dict)
            {
                var section = IdentifierType.For(kvp.Key);
                var items = kvp.Value.Select(IdentifierType.For).ToArray();

                snapshot.AppendSections(new[] { section });
                snapshot.AppendItems(items);
            }

            return snapshot;
        }

        public static NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>>
            ToDiffableDataSourceSnapshot<TObject, TSection, TItem>(
                this TObject obj,
                Func<TObject, IEnumerable<TSection>> getSections,
                Func<TSection, IEnumerable<TItem>> getItems)
        {
            var snapshot = new NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>>();

            foreach (var section in getSections(obj))
            {
                var items = getItems(section);

                var sectionIdentifier = IdentifierType.For(section);
                var itemIdentifiers = items.Select(IdentifierType.For).ToArray();

                snapshot.AppendSections(new[] { sectionIdentifier });
                snapshot.AppendItems(itemIdentifiers);
            }

            return snapshot;
        }

        public static NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>>
            ToDiffableDataSourceSnapshot<TObject, TSection, TSectionIdentifier, TItem, TItemIdentifier>(
                this TObject obj,
                Func<TObject, IEnumerable<TSection>> getSections, Func<TSection, TSectionIdentifier> getSectionIdentifier,
                Func<TSection, IEnumerable<TItem>> getItems, Func<TItem, TItemIdentifier> getItemIdentifier)
        {
            var snapshot = new NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>>();

            foreach (var section in getSections(obj))
            {
                var items = getItems(section);

                var sectionIdentifier = IdentifierType.For(section, getSectionIdentifier);
                var itemIdentifiers = items.Select(item => IdentifierType.For(item, getItemIdentifier)).ToArray();

                snapshot.AppendSections(new[] { sectionIdentifier });
                snapshot.AppendItems(itemIdentifiers);
            }

            return snapshot;
        }

        public static NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>>
            ToDiffableDataSourceSnapshot<TSection, TSectionIdentifier, TItem, TItemIdentifier>(
                this IEnumerable<TSection> sections, Func<TSection, TSectionIdentifier> getSectionIdentifier,
                Func<TSection, IEnumerable<TItem>> getItems, Func<TItem, TItemIdentifier> getItemIdentifier)
        {
            var snapshot = new NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>>();

            foreach (var section in sections)
            {
                var items = getItems(section);

                var sectionIdentifier = IdentifierType.For(section, getSectionIdentifier);
                var itemIdentifiers = items.Select(item => IdentifierType.For(item, getItemIdentifier)).ToArray();

                snapshot.AppendSections(new[] { sectionIdentifier });
                snapshot.AppendItems(itemIdentifiers);
            }

            return snapshot;
        }
    }

    public static class BetterUITableViewDiffableDataSource
    {
        public static BetterUITableViewDiffableDataSource<TSection, TItem> Create<TSection, TItem>(UITableView tableView, Func<UITableView, NSIndexPath, TItem, UITableViewCell> getCell) => new BetterUITableViewDiffableDataSource<TSection, TItem>(tableView,
                (tv, indexPath, rawIdentifier) => getCell(tv, indexPath, ((IdentifierType<TItem>)rawIdentifier).Item));

        public static BetterUITableViewDiffableDataSource<TSection, TItem> Create<TSection, TItem>(
            UITableView tableView,
            NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>> snapshot,
            Func<UITableView, NSIndexPath, TItem, UITableViewCell> getCell) => new BetterUITableViewDiffableDataSource<TSection, TItem>(tableView,
                (tv, indexPath, rawIdentifier) => getCell(tv, indexPath, ((IdentifierType<TItem>)rawIdentifier).Item));
    }

    public class BetterUITableViewDiffableDataSource<TSection, TItem> : UITableViewDiffableDataSource<IdentifierType<TSection>, IdentifierType<TItem>>
    {
        private Func<UITableView, NSIndexPath, TItem> _getCell;

        public BetterUITableViewDiffableDataSource(
            UITableView tableView, UITableViewDiffableDataSourceCellProvider cellProvider) : base(tableView, cellProvider)
        {

        }
    }

    public static class BestUITableViewDiffableDataSourceQuestionMark
    {
        public static IUITableViewDiffableDataSource<TSection> Create<TSection, TSectionIdentifier, TItem, TItemIdentifier>(
            UITableView tableView,
            List<TSection> items,
            Func<TSection, TSectionIdentifier> getSectionIdentifier,
            Func<TSection, IEnumerable<TItem>> getSectionItems,
            Func<TItem, TItemIdentifier> getItemIdentifier,
            Func<UITableView, NSIndexPath, TItem, UITableViewCell> getCell,
            Func<UITableView, int, TSection, string> titleForHeader = null,
            Func<UITableView, int, TSection, string> titleForFooter = null)
            => new BestUITableViewDiffableDataSourceQuestionMark<TSection, TSectionIdentifier, TItem, TItemIdentifier>
            (
                tableView, getSectionIdentifier, getSectionItems, getItemIdentifier,
                (tv, indexPath, item) => getCell(tv, indexPath, (item as IdentifierType<TItem>).Item),
                titleForHeader, titleForFooter
            );

        public static BetterUITableViewDiffableDataSource<TSection, TItem> Create<TSection, TItem>(
            UITableView tableView,
            NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>> snapshot,
            Func<UITableView, NSIndexPath, TItem, UITableViewCell> getCell) => new BetterUITableViewDiffableDataSource<TSection, TItem>(tableView,
                (tv, indexPath, rawIdentifier) => getCell(tv, indexPath, ((IdentifierType<TItem>)rawIdentifier).Item));
    }

    public class BestUITableViewDiffableDataSourceQuestionMark<TSection, TSectionIdentifier, TItem, TItemIdentifier>
        : UITableViewDiffableDataSource<IdentifierType<TSection>, IdentifierType<TItem>>, IUITableViewDiffableDataSource<TSection>
    {
        Func<TSection, TSectionIdentifier> _getSectionIdentifier;
        Func<TSection, IEnumerable<TItem>> _getSectionItems;
        Func<TItem, TItemIdentifier> _getItemIdentifier;
        Func<UITableView, int, TSection, string> _titleForHeader;
        Func<UITableView, int, TSection, string> _titleForFooter;

        NSDiffableDataSourceSnapshot<IdentifierType<TSection>, IdentifierType<TItem>> _snapshot;

        public BestUITableViewDiffableDataSourceQuestionMark(
            UITableView tableView,
            Func<TSection, TSectionIdentifier> getSectionIdentifier,
            Func<TSection, IEnumerable<TItem>> getSectionItems,
            Func<TItem, TItemIdentifier> getItemIdentifier,
            UITableViewDiffableDataSourceCellProvider cellProvider,
            Func<UITableView, int, TSection, string> titleForHeader = null,
            Func<UITableView, int, TSection, string> titleForFooter = null
            ) : base(tableView, cellProvider)
        {
            _getSectionIdentifier = getSectionIdentifier;
            _getSectionItems = getSectionItems;
            _getItemIdentifier = getItemIdentifier;
            _titleForHeader = titleForHeader;
            _titleForFooter = titleForFooter;
        }

        public void ApplySnapshot(IEnumerable<TSection> sections, bool animatingDifferences)
        {
            _snapshot = sections.ToDiffableDataSourceSnapshot(
                _getSectionIdentifier, _getSectionItems, _getItemIdentifier);

            Debug.WriteLine(" == next snapshot == ");

            var i = 0;
            foreach (var section in _snapshot.SectionIdentifiers)
                Debug.WriteLine($"S{i++} {section.Item} {section.GetNativeHash()}");

            Debug.WriteLine("");

            i = 0;
            foreach (var item in _snapshot.ItemIdentifiers)
                Debug.WriteLine($"I{i++} {item.Item} {item.GetNativeHash()}");

            ApplySnapshot(_snapshot, animatingDifferences);
        }

        public override string TitleForHeader(UITableView tableView, nint section)
            => _titleForHeader?.Invoke(tableView, (int)section, _snapshot.SectionIdentifiers[section].Item);

        public override string TitleForFooter(UITableView tableView, nint section)
            => _titleForFooter?.Invoke(tableView, (int)section, _snapshot.SectionIdentifiers[section].Item);
    }

    public interface IUITableViewDiffableDataSource<TSection>
    {
        void ApplySnapshot(IEnumerable<TSection> sections, bool animatingDifferences);
    }

    public static class IdentifierType
    {
        public static IdentifierType<T> For<T>(T item)
            => new IdentifierType<T>(item);

        public static IdentifierType<T> For<T, TIdentifier>(T item, Func<T, TIdentifier> getIdentifier)
            => new FuncIdentifierType<T, TIdentifier>(item, getIdentifier);
    }

    public class IdentifierType<T> : NSObject
    {
        public readonly T Item;

        public IdentifierType(T item)
        {
            Item = item;
        }

        public override int GetHashCode()
        {
            if (Item is int i)
                return i;

            if (Item is string s)
                return s.ToCharArray().Sum(c => (int)c);

            return Item.GetHashCode();
        }

        public override nuint GetNativeHash()
        {
            var hc = (nuint)GetHashCode();

            return hc;
        }

        public override bool IsEqual(NSObject anObject)
        {
            return GetNativeHash() == anObject.GetNativeHash();
        }

        public override string ToString()
        {
            return Item.ToString();
        }
    }

    public class FuncIdentifierType<T,U> : IdentifierType<T>
    {
        private readonly Func<T, U> _identifierFunc;

        public FuncIdentifierType(T item, Func<T,U> identifierFunc) : base(item)
        {
            _identifierFunc = identifierFunc;
        }

        public override nuint GetNativeHash()
            => (nuint)_identifierFunc(Item).GetHashCode();

        public override bool IsEqual(NSObject anObject)
            => GetNativeHash() == anObject.GetNativeHash();
    }

    public interface IIdentifier
    {
        string Identifier { get; set; }
    }

    public class IIdentifierType<T> : IdentifierType<T>
        where T: IIdentifier
    {
        public IIdentifierType(T item) : base(item) {}

        public override nuint GetNativeHash()
            => (System.nuint)Item.Identifier.GetHashCode();

        public override bool IsEqual(NSObject anObject)
            => GetNativeHash() == anObject.GetNativeHash();
    }
    
    public class Game
    {
        public string GameName { get; set; }
        public List<GameCharacter> Characters { get; set; } = new List<GameCharacter>();


        public override string ToString()
        {
            return GameName.Substring(0,4).ToUpper();
        }
    }

    public class GameCharacter
    {
        public string CharacterName { get; set; }

        public override string ToString()
        {
            return CharacterName.Substring(0,4).ToUpper();
        }
    }
}