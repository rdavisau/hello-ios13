using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using Foundation;
using HelloiOS13.Helpers.DiffableDataSource;
using UIKit;

namespace HelloiOS13.D5.D2
{
    [DisplayInMenu(DisplayName = "Diffable Data Source (manual)", DisplayDescription = "Magic manual diffable data")]
    public class ManualDiffableDataSourceViewController : BaseViewController
    {
        protected UIButton ApplyButton;
        protected UITableView TableView;
        protected IUITableViewDiffableDataSource<Game> TableViewDataSource;

        // look ma, no manual diffs!
        private void ApplyButtonClicked(object sender, EventArgs e)
            => TableViewDataSource.ApplySnapshot(Games, animatingDifferences: true);

        protected List<Game> Games = new List<Game>
        {

            new Game
            {
                GameName = "Super Smash Brothers",
                Characters =
                {
                    new GameCharacter { CharacterName = "Mario" },
                    new GameCharacter { CharacterName = "Luigi" },
                    new GameCharacter { CharacterName = "Peach" },
                }
            },
            new Game
            {
                GameName = "The Legend of Zelda",
                Characters =
                {
                    new GameCharacter { CharacterName = "Link" },
                    new GameCharacter { CharacterName = "Navi" },
                    new GameCharacter { CharacterName = "Zelda" },
                }
            },
            new Game
            {
                GameName = "EarthBound",
                Characters =
                {
                    new GameCharacter { CharacterName = "Paula" },
                    new GameCharacter { CharacterName = "Lucas" },
                    new GameCharacter { CharacterName = "Ness" },
                }
            },

        };

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView = WH.GetOrSet("tableView", GetTableView);
            TableViewDataSource = WH.GetOrSet("dataSource", GetDiffableDataSource);

            ApplyButton = GetApplyButton();

            View.FillWith(TableView);
            View.InsertSubviewAbove(ApplyButton, TableView); 

            ApplyButton.TouchUpInside += ApplyButtonClicked;
        }

        public UITableView GetTableView()
            =>  new UITableView(CoreGraphics.CGRect.Empty, UITableViewStyle.Grouped);

        public IUITableViewDiffableDataSource<Game> GetDiffableDataSource()
        {
            var source =
                BestUITableViewDiffableDataSourceQuestionMark.Create(
                    TableView,                                                      // in diffable, source assigns tableview
                    items: Games,                                                   // used for type inference, can be null at the time of the call
                    getSectionIdentifier: game => game.GameName,                    // return key for section
                    getSectionItems:      game => game.Characters,                  // return items for section
                    getItemIdentifier:    character => character.CharacterName,     // return key for item
                    getCell: (tv, indexPath, character) =>                          // standard GetCell, but includes typed model parameter (character)
                    {
                        var cell = new UITableViewCell(UITableViewCellStyle.Default, "cell");
                        cell.TextLabel.Text = character.CharacterName;

                        return cell;
                    },
                    titleForHeader: (tv, sectionIndex, section) => section.GameName);
            
            source.ApplySnapshot(Games, false);
            
            return source;
        }

        private UIButton GetApplyButton()
        {
            var button = new UIButton(UIButtonType.System) { TranslatesAutoresizingMaskIntoConstraints = false};

            View.AddSubview(button); 

            button.SetTitle("Apply Changes", UIControlState.Normal);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(button, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(button, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1, -30),
            });
             
            return button; 
        }
    }
}

