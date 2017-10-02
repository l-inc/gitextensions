using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitCommands.Utils;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class RecentRepositoriesList : GitExtensionsControl
    {
        private readonly TranslationString _groupMostRecent = new TranslationString("Recent");
        private readonly TranslationString _groupLessRecent = new TranslationString("Older");
        private readonly TranslationString _groupUncategorised = new TranslationString("Uncategorised");
        private readonly TranslationString directoryIsNotAValidRepositoryCaption = new TranslationString("Open");
        private readonly TranslationString directoryIsNotAValidRepository = new TranslationString("The selected item is not a valid git repository.\n\nDo you want to abort and remove it from the recent repositories list?");

        private Font _secondaryFont;
        private static readonly Color DefaultFavouriteColor = Color.DarkGoldenrod;
        private static readonly Color DefaultBranchNameColor = SystemColors.HotTrack;
        private Color _favouriteColor = DefaultFavouriteColor;
        private Color _branchNameColor = DefaultBranchNameColor;
        private Color _hoverColor;
        private Color _headerColor;
        private Color _headerBackColor;
        private Color _mainBackColor;
        private Brush _foreColorBrush;
        private Brush _branchNameColorBrush = new SolidBrush(DefaultBranchNameColor);
        private Brush _favouriteColorBrush = new SolidBrush(DefaultFavouriteColor);
        private Brush _hoverColorBrush = new SolidBrush(SystemColors.InactiveCaption);
        private ListViewItem _prevHoveredItem;
        private readonly ListViewGroup _lvgMostRecent;
        private readonly ListViewGroup _lvgLessRecent;
        private readonly ListViewGroup _lvgUncategorised;
        private RecentRepositoriesGroupBy _groupBy;
        private readonly RecentRepositoryListController _controller = new RecentRepositoryListController();


        public event EventHandler<GitModuleEventArgs> GitModuleChanged;


        public RecentRepositoriesList()
        {
            InitializeComponent();
            Translate();

            mnuTop.DropDownItems.Clear();

            _lvgMostRecent = new ListViewGroup(_groupMostRecent.Text, HorizontalAlignment.Left);
            _lvgLessRecent = new ListViewGroup(_groupLessRecent.Text, HorizontalAlignment.Left);
            _lvgUncategorised = new ListViewGroup(_groupUncategorised.Text, HorizontalAlignment.Left);

            _foreColorBrush = new SolidBrush(base.ForeColor);

            _groupBy = Enum.IsDefined(typeof(RecentRepositoriesGroupBy), AppSettings.RecentReposGroupBy) ?
                                            (RecentRepositoriesGroupBy)AppSettings.RecentReposGroupBy : 0;
            mnuGroupByAccessTime.Tag = RecentRepositoriesGroupBy.Access;
            mnuGroupByCategory.Tag = RecentRepositoriesGroupBy.Category;
            mnuGroupByAccessTime.Checked = _groupBy == RecentRepositoriesGroupBy.Access;
            mnuGroupByCategory.Checked = !mnuGroupByAccessTime.Checked;

            listView1.Items.Clear();
            listView1.ContextMenuStrip = contextMenuStrip;

            ApplyScaling();

            imageList1.Images.Clear();
            imageList1.Images.Add(Resources.folder_git);
            imageList1.Images.Add(Resources.folder_error);
        }


        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "HotTrack")]
        public Color BranchNameColor
        {
            get { return _branchNameColor; }
            set
            {
                if (_branchNameColor == value)
                {
                    return;
                }
                _branchNameColor = value;
                _branchNameColorBrush?.Dispose();
                _branchNameColorBrush = new SolidBrush(value);
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DarkGoldenrod")]
        public Color FavouriteColor
        {
            get { return _favouriteColor; }
            set
            {
                if (_favouriteColor == value)
                {
                    return;
                }
                _favouriteColor = value;
                _favouriteColorBrush?.Dispose();
                _favouriteColorBrush = new SolidBrush(value);
                ShowRecentRepositoriesAsync();
            }
        }

        [Category("Appearance")]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                if (base.ForeColor == value)
                {
                    return;
                }
                base.ForeColor = value;
                listView1.ForeColor = value;
                _foreColorBrush?.Dispose();
                _foreColorBrush = new SolidBrush(value);
                ShowRecentRepositoriesAsync();
            }
        }

        [Category("Appearance")]
        public Color HeaderColor
        {
            get { return _headerColor; }
            set
            {
                if (_headerColor == value)
                {
                    return;
                }
                _headerColor = value;
                lblRecentRepositories.ForeColor = value;
                lblRecentRepositories.Invalidate();
            }
        }

        [Category("Appearance")]
        public Color HeaderBackColor
        {
            get { return _headerBackColor; }
            set
            {
                if (_headerBackColor == value)
                {
                    return;
                }
                _headerBackColor = value;
                pnlHeader.BackColor = value;
                pnlHeader.Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(50)]
        public int HeaderHeight
        {
            get { return pnlHeader.Height; }
            set
            {
                pnlHeader.Height = value;
                pnlHeader.Invalidate();
            }
        }

        [Category("Appearance")]
        public Color HoverColor
        {
            get { return _hoverColor; }
            set
            {
                if (_hoverColor == value)
                {
                    return;
                }
                _hoverColor = value;
                _hoverColorBrush?.Dispose();
                _hoverColorBrush = new SolidBrush(value);
                ShowRecentRepositoriesAsync();
            }
        }

        [Category("Appearance")]
        public Color MainBackColor
        {
            get { return _mainBackColor; }
            set
            {
                if (_mainBackColor == value)
                {
                    return;
                }
                _mainBackColor = value;
                BackColor = value;
                listView1.BackColor = value;
            }
        }


        public async void ShowRecentRepositoriesAsync()
        {
            _secondaryFont = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints - 1.5f * GetScaleFactor());
            lblRecentRepositories.Font = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints + 5.5f);

            IList<RecentRepoInfo> repos;
            using (var graphics = CreateGraphics())
            {
                repos = await _controller.PreRenderRepositoriesAsync(graphics);
            }
            ShowRecentRepositories(repos);
        }


        protected virtual void OnModuleChanged(GitModuleEventArgs args)
        {
            var handler = GitModuleChanged;
            handler?.Invoke(this, args);
        }


        private void ApplyScaling()
        {
            var scaleFactor = GetScaleFactor();
            tableLayoutPanel1.ColumnStyles[1].Width *= scaleFactor;
            ApplyPaddingScaling(pnlHeader, scaleFactor);

            var padding4 = (int)(4 * scaleFactor);
            var padding36 = (int)(36 * scaleFactor);
            listView1.Width = Width - 2 * padding36;
            listView1.Location = new Point(padding36, pnlHeader.Height + padding4);
            listView1.Size = new Size(Width - 2 * padding36, Height - pnlHeader.Height - 2 * padding4);
        }

        private RectangleF DrawText(Graphics g, string text, Font font, Brush brush, int maxTextWidth, PointF location, float spacing)
        {
            var textBounds = TextRenderer.MeasureText(text, font);
            var minWidth = Math.Min(textBounds.Width + spacing, maxTextWidth);
            var bounds = new RectangleF(location, new SizeF(minWidth, textBounds.Height));
            var text1 = Math.Abs(maxTextWidth - minWidth) < float.Epsilon ? ShortenText(text, font, minWidth) : text;
            g.DrawString(text1, font, brush, bounds, StringFormat.GenericTypographic);

            return bounds;
        }

        private static T FindControl<T>(IEnumerable controls, Func<T, bool> predicate) where T : Control
        {
            foreach (Control control in controls)
            {
                var result = control as T;
                if (result != null && predicate(result))
                {
                    return result;
                }
                result = FindControl(control.Controls, predicate);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private string[] GetCategories()
        {
            var categories = listView1.Items.Cast<ListViewItem>()
                                            .Select(lvi => (lvi.Tag as Repository)?.Category)
                                            .Where(category => !string.IsNullOrWhiteSpace(category))
                                            .OrderBy(x => x)
                                            .Distinct()
                                            .ToArray();
            return categories;
        }

        private static Repository GetSelectedRepository(ToolStripItem menuItem)
        {
            // Retrieve the ContextMenuStrip that owns this ToolStripItem
            var contextMenu = menuItem?.Owner as ContextMenuStrip;

            // Get the control that is displaying this context menu
            var selected = contextMenu?.Tag as Repository;
            if (string.IsNullOrWhiteSpace(selected?.Path))
            {
                return null;
            }
            return selected;
        }

        private Repository GetSelectedRepository()
        {
            if (listView1.SelectedItems.Count < 1)
            {
                return null;
            }
            var selected = listView1.SelectedItems[0].Tag as Repository;
            if (string.IsNullOrWhiteSpace(selected?.Path))
            {
                return null;
            }
            return selected;
        }

        private ListViewGroup GetTileGroup(Repository repository)
        {
            switch (_groupBy)
            {
                case RecentRepositoriesGroupBy.Access:
                    {
                        switch (repository.Anchor)
                        {
                            case Repository.RepositoryAnchor.MostRecent:
                                {
                                    return listView1.Groups[0];
                                }
                            default:
                                {
                                    return listView1.Groups[1];
                                }
                        }
                    }

                case RecentRepositoriesGroupBy.Category:
                    {
                        var groups = listView1.Groups.Cast<ListViewGroup>().ToArray();
                        var category = groups.SingleOrDefault(x => x.Header == repository.Category);
                        if (category != null)
                        {
                            return category;
                        }
                        return groups.Last();
                    }

                default:
                    {
                        throw new NotImplementedException("Opps...");
                    }
            }
        }

        private Size GetTileSize(IList<RecentRepoInfo> repositories)
        {
            var scaleFactor = GetScaleFactor();
            var spacing1 = 1 * scaleFactor;
            var spacing2 = 2 * scaleFactor;

            var longestPath = repositories.Select(r =>
                                                  {
                                                      var size = TextRenderer.MeasureText(r.Caption, AppSettings.Font);
                                                      return new
                                                      {
                                                          r.Caption,
                                                          size.Width,
                                                          size.Height
                                                      };
                                                  })
                                          .OrderByDescending(r => r.Width)
                                          .First();
            //var pathTextSize = longestPath.Length;
            var branchTextSize = TextRenderer.MeasureText("A", _secondaryFont);

            var width = AppSettings.RecentReposComboMinWidth;
            if (width < 1)
            {
                width = longestPath.Width;
            }

            var height = longestPath.Height + 2 * branchTextSize.Height +
                /* offset from top and bottom */2 * spacing2 +
                /* twice space between text */2 * spacing1;

            return new Size((int)(width + 50 * scaleFactor), (int)Math.Max(height, 50 * scaleFactor));
        }

        private static void RepositoryContextAction(ToolStripItem menuItem, Action<Repository> action)
        {
            var selected = GetSelectedRepository(menuItem);
            if (selected != null)
            {
                action(selected);
            }
        }

        private void SetupGroups(IEnumerable<string> categories)
        {
            listView1.Groups.Clear();
            switch (_groupBy)
            {
                case RecentRepositoriesGroupBy.Access:
                    {
                        listView1.Groups.AddRange(new[] { _lvgMostRecent, _lvgLessRecent });
                    }
                    break;

                case RecentRepositoriesGroupBy.Category:
                    {
                        categories.ToList().ForEach(c =>
                        {
                            listView1.Groups.Add(c, c);
                        });
                        listView1.Groups.Add(_lvgUncategorised);
                    }
                    break;

                default:
                    {
                        throw new NotImplementedException("Opps...");
                    }
            }
        }

        private static string ShortenText(string text, Font font, float maxWidth)
        {
            while (text.Length > 1)
            {
                var width = TextRenderer.MeasureText(text, font).Width;
                if (width < maxWidth)
                {
                    break;
                }
                text = text.Substring(0, text.Length - 1);
            }
            return text;
        }

        private void ShowRecentRepositories(IList<RecentRepoInfo> repositories)
        {
            listView1.Items.Clear();
            if (repositories == null || repositories.Count < 1 || !Visible)
            {
                return;
            }

            listView1.TileSize = GetTileSize(repositories);
            Debug.WriteLine($"Tile size: {listView1.TileSize}");

            SetupGroups(repositories.Select(repo => repo.Repo.Category)
                                    .Where(c => !string.IsNullOrWhiteSpace(c))
                                    .Distinct()
                                    .OrderBy(x => x));

            foreach (var repository in repositories)
            {
                var item = new ListViewItem(repository.Caption)
                {
                    ForeColor = ForeColor,
                    Font = AppSettings.Font,
                    Group = GetTileGroup(repository.Repo),
                    ImageIndex = _controller.IsValidGitWorkingDir(repository.Repo.Path) ? 0 : 1,
                    UseItemStyleForSubItems = false,
                    Tag = repository.Repo,
                    ToolTipText = repository.Repo.Path
                };
                item.SubItems.Add(_controller.GetCurrentBranchName(repository.Repo.Path), BranchNameColor, BackColor, _secondaryFont);
                item.SubItems.Add(repository.Repo.Category, SystemColors.GrayText, BackColor, _secondaryFont);

                listView1.Items.Add(item);
            }
        }


        private void contextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            ShowRecentRepositoriesAsync();
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var selected = GetSelectedRepository();

            tsmiAddToMostRecent.Visible =
                tsmiRemoveFromMostRecent.Visible =
                    tsmiRemoveFromList.Visible =
                        toolStripMenuItem1.Visible =
                            tsmiCategories.Visible =
                                toolStripMenuItem2.Visible =
                                    tsmiOpenFolder.Visible = selected != null;

            if (selected == null)
            {
                e.Cancel = true;
                return;
            }
            tsmiAddToMostRecent.Visible = selected.Anchor != Repository.RepositoryAnchor.MostRecent;
            tsmiRemoveFromMostRecent.Visible = selected.Anchor == Repository.RepositoryAnchor.MostRecent;

            // address a bug in context menu implementation
            // nested toolstrip items can't get source control
            // http://stackoverflow.com/questions/30534417/
            contextMenuStrip.Tag = selected;
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var scaleFactor = GetScaleFactor();
            var spacing1 = 1 * scaleFactor;
            var spacing2 = 2 * scaleFactor;
            var spacing4 = 4 * scaleFactor;

            var textOffset = spacing2 + imageList1.ImageSize.Width + spacing2;
            int textWidth = AppSettings.RecentReposComboMinWidth > 0 ? AppSettings.RecentReposComboMinWidth : e.Bounds.Width;

            if (e.Item == _prevHoveredItem)
            {
                e.Graphics.FillRectangle(_hoverColorBrush, e.Bounds);
            }
            else
            {
                e.DrawBackground();
            }

            var ptImage = new PointF(e.Bounds.Left + spacing2, e.Bounds.Top + spacing2 * 4);
            // render anchor icon
            if ((e.Item.Tag as Repository)?.Anchor == Repository.RepositoryAnchor.MostRecent)
            {
                var ptImage1 = new PointF(ptImage.X + imageList1.ImageSize.Width - 12, e.Bounds.Top + spacing2);
                e.Graphics.DrawImage(Resources.Star, ptImage1.X, ptImage1.Y, 16, 16);
            }

            // render icon
            e.Graphics.DrawImage(imageList1.Images[e.Item.ImageIndex], ptImage);

            // render path
            var textPadding = new PointF(e.Bounds.Left + spacing2, e.Bounds.Top + spacing2);
            var ptPath = new PointF(textPadding.X + textOffset, textPadding.Y);
            var pathBounds = DrawText(e.Graphics, e.Item.Text, AppSettings.Font, _foreColorBrush, textWidth, ptPath, spacing4 * 2);

            // render branch
            var ptBranch = new PointF(ptPath.X, ptPath.Y + pathBounds.Height + spacing1);
            var branchBounds = DrawText(e.Graphics, e.Item.SubItems[1].Text, _secondaryFont, _branchNameColorBrush, textWidth, ptBranch, spacing4 * 2);

            // render category
            if (!string.IsNullOrWhiteSpace(e.Item.SubItems[2].Text))
            {
                var ptCategory = string.IsNullOrWhiteSpace(e.Item.SubItems[1].Text) ?
                                    ptBranch :
                                    new PointF(ptBranch.X, ptBranch.Y + branchBounds.Height + spacing1);
                DrawText(e.Graphics, e.Item.SubItems[2].Text, _secondaryFont, SystemBrushes.GrayText, textWidth, ptCategory, spacing4 * 2);
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (EnvUtils.IsMonoRuntime() && e.Button == MouseButtons.Right)
            {
                if (listView1.FocusedItem?.Bounds.Contains(e.Location) == true)
                {
                    contextMenuStrip.Show(Cursor.Position);
                    return;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                var selected = GetSelectedRepository();
                if (selected == null)
                {
                    return;
                }


                if (!_controller.IsValidGitWorkingDir(selected.Path))
                {
                    var dialogResult = MessageBox.Show(this, directoryIsNotAValidRepository.Text,
                                                             directoryIsNotAValidRepositoryCaption.Text, MessageBoxButtons.YesNo,
                                                             MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    if (dialogResult == DialogResult.Yes)
                    {
                        _controller.RemoveRepository(selected.Path);
                        listView1.Refresh();
                    }
                    return;
                }

                var module = _controller.GetGitModule(selected.Path);
                OnModuleChanged(new GitModuleEventArgs(module));
            }
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            var item = listView1.GetItemAt(e.X, e.Y);
            _prevHoveredItem = item;
        }

        private void mnuConfigure_Click(object sender, EventArgs e)
        {
            using (var frm = new FormRecentReposSettings())
            {
                var result = frm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    ShowRecentRepositoriesAsync();
                }
            }
        }

        private void mnuGroupBy_Click(object sender, EventArgs e)
        {
            var menu = sender as ToolStripMenuItem;
            if (menu == null)
            {
                return;
            }
            try
            {
                _groupBy = (RecentRepositoriesGroupBy)menu.Tag;
                AppSettings.RecentReposGroupBy = (int)_groupBy;

                mnuGroupBy.DropDownItems.Cast<ToolStripMenuItem>().ForEach(x => x.Checked = false);
                menu.Checked = true;

                ShowRecentRepositoriesAsync();
            }
            catch
            {
                // Do nothing
            }
        }

        private void RecentRepositoriesList_Load(object sender, EventArgs e)
        {
            var form = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == nameof(FormBrowse));
            if (form == null)
            {
                return;
            }

            var menus = new ToolStripItem[] { mnuGroupBy, mnuConfigure };
            var menuStrip = FindControl<MenuStrip>(form.Controls, p => p.Name == "menuStrip1");
            var dashboardMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "dashboardToolStripMenuItem");
            dashboardMenu?.DropDownItems.AddRange(menus);
        }

        private void tsmiAddToMostRecent_Click(object sender, EventArgs e)
        {
            RepositoryContextAction(sender as ToolStripMenuItem, repository =>
            {
                _controller.SetAnchor(repository, Repository.RepositoryAnchor.MostRecent);
                listView1.Refresh();
            });
        }

        private void tsmiCategories_DropDownOpening(object sender, EventArgs e)
        {
            if (sender != tsmiCategories)
            {
                return;
            }

            tsmiCategories.DropDownItems.Clear();

            var categories = GetCategories();
            if (categories.Length > 0)
            {
                tsmiCategories.DropDownItems.Add(tsmiCategoryNone);
                // ReSharper disable once CoVariantArrayConversion
                tsmiCategories.DropDownItems.AddRange(categories.Select(category =>
                {
                    var item = new ToolStripMenuItem(category);
                    item.Tag = category;
                    item.Click += tsmiCategory_Click;
                    return item;
                }).ToArray());
                tsmiCategories.DropDownItems.Add(new ToolStripSeparator());
            }
            tsmiCategories.DropDownItems.Add(tsmiCategoryAdd);

            RepositoryContextAction(tsmiCategories, repository =>
            {
                foreach (ToolStripItem item in tsmiCategories.DropDownItems)
                {
                    item.Enabled = item.Text != repository.Category;
                }
                if (string.IsNullOrWhiteSpace(repository.Category) && tsmiCategories.DropDownItems.Count > 1)
                {
                    tsmiCategories.DropDownItems[0].Enabled = false;
                }
            });
        }

        private void tsmiCategory_Click(object sender, EventArgs e)
        {
            var repository = GetSelectedRepository((sender as ToolStripMenuItem)?.OwnerItem);
            if (repository == null)
            {
                return;
            }
            var category = (sender as ToolStripMenuItem)?.Tag as string;
            _controller.SetCategory(repository, category);
            listView1.Refresh();
        }

        private void tsmiCategoryAdd_Click(object sender, EventArgs e)
        {
            RepositoryContextAction((sender as ToolStripMenuItem)?.OwnerItem, repository =>
            {
                using (var dialog = new FormDashboardCategoryTitle(GetCategories()))
                {
                    if (DialogResult.OK != dialog.ShowDialog(this))
                    {
                        return;
                    }
                    _controller.SetCategory(repository, dialog.Category);
                    ShowRecentRepositoriesAsync();
                }
            });
        }

        private void tsmiOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                RepositoryContextAction(sender as ToolStripMenuItem, repository => Process.Start(repository.Path));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void tsmiRemoveFromList_Click(object sender, EventArgs e)
        {
            RepositoryContextAction(sender as ToolStripMenuItem, repository =>
            {
                _controller.RemoveRepository(repository.Path);
                ShowRecentRepositoriesAsync();
            });
        }

        private void tsmiRemoveFromMostRecent_Click(object sender, EventArgs e)
        {
            RepositoryContextAction(sender as ToolStripMenuItem, repository =>
            {
                _controller.SetAnchor(repository, Repository.RepositoryAnchor.None);
                listView1.Refresh();
            });
        }
    }

    public enum RecentRepositoriesGroupBy
    {
        Access = 0,
        Category
    }
}
