using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitUI.Editor;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class Dashboard : GitModuleControl
    {
        // ReSharper disable InconsistentNaming - can't rename them as they are referenced by translations
        private readonly TranslationString cloneFork = new TranslationString("Clone {0} repository");
        private readonly TranslationString cloneRepository = new TranslationString("Clone repository");
        private readonly TranslationString cloneSvnRepository = new TranslationString("Clone SVN repository");
        private readonly TranslationString createRepository = new TranslationString("Create new repository");
        private readonly TranslationString develop = new TranslationString("Develop");
        private readonly TranslationString donate = new TranslationString("Donate");
        private readonly TranslationString issues = new TranslationString("Issues");
        private readonly TranslationString openRepository = new TranslationString("Open repository");
        private readonly TranslationString translate = new TranslationString("Translate");
        private readonly TranslationString showCurrentBranch = new TranslationString("Show current branch");
        // ReSharper restore InconsistentNaming

        private DashboardTheme _selectedTheme;

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;


        public Dashboard()
        {
            InitializeComponent();
            Translate();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Visible = false;

            // Mono has issues with docked controls, so we need to manually position and size controls
            if (EnvUtils.IsMonoRuntime())
            {
                tableLayoutPanel1.Dock = DockStyle.None;
                tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                pnlLeft.Dock = DockStyle.None;
                pnlLeft.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                flpnlStart.Dock = DockStyle.None;
                flpnlStart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                flpnlContribute.Dock = DockStyle.None;
                flpnlContribute.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

                SizeChanged += dashboard_SizeChanged;
            }
            else
            {
                tableLayoutPanel1.AutoSize = true;
                tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                tableLayoutPanel1.Dock = DockStyle.Fill;
                pnlLeft.Dock = DockStyle.Fill;
                flpnlStart.Dock = DockStyle.Fill;
                flpnlContribute.Dock = DockStyle.Bottom;
                flpnlContribute.SendToBack();
            }

            recentRepositoriesList1.GitModuleChanged += OnModuleChanged;

            ApplyScaling();
        }


        // need this to stop flickering of the background images, nothing else works
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public void RefreshContent()
        {
            InitDashboardLayout();
            ApplyTheme();

            recentRepositoriesList1.ShowRecentRepositoriesAsync();

            if (EnvUtils.IsMonoRuntime())
            {
                Invalidate(true);
            }
        }


        protected virtual void OnModuleChanged(object sender, GitModuleEventArgs e)
        {
            var handler = GitModuleChanged;
            handler?.Invoke(this, e);
        }


        private static void AddLinks(Panel panel, Func<Panel, Control> addLinks, Action<Panel, Control> onLayout)
        {
            panel.SuspendLayout();
            panel.Controls.Clear();

            var lastControl = addLinks(panel);

            panel.ResumeLayout(false);
            panel.PerformLayout();

            onLayout(panel, lastControl);
        }

        private void ApplyScaling()
        {
            var scaleFactor = GetScaleFactor();

            ApplyPaddingScaling(pnlLogo, scaleFactor);
            ApplyPaddingScaling(flpnlStart, scaleFactor);
            ApplyPaddingScaling(flpnlContribute, scaleFactor);

            tableLayoutPanel1.ColumnStyles[1].Width *= scaleFactor;
            recentRepositoriesList1.HeaderHeight = pnlLogo.Height;
        }

        private void ApplyTheme(DashboardTheme theme = null)
        {
            if (theme == null)
            {
                switch (AppSettings.DashboardThemeIndex)
                {
                    case 1: theme = DashboardTheme.Light; break;
                    case 2: theme = DashboardTheme.Dark; break;
                    default:
                        {
                            // select the theme based on the current Windows color scheme
                            // if the default text color is light, then it is likely that
                            // the user runs a dark theme, else set the light theme
                            theme = SystemColors.ControlText.IsLightColor() ? DashboardTheme.Dark : DashboardTheme.Light;
                            break;
                        }
                }
                _selectedTheme = theme;
            }

            BackColor = theme.Primary;
            pnlLogo.BackColor = theme.PrimaryVeryDark;
            flpnlStart.BackColor = theme.PrimaryLight;
            flpnlContribute.BackColor = theme.PrimaryVeryLight;
            lblContribute.ForeColor = theme.SecondaryHeadingText;
            recentRepositoriesList1.BranchNameColor = theme.SecondaryText;
            recentRepositoriesList1.FavouriteColor = theme.AccentedText;
            recentRepositoriesList1.ForeColor = theme.PrimaryText;
            recentRepositoriesList1.HeaderColor = theme.SecondaryHeadingText;
            recentRepositoriesList1.HeaderBackColor = theme.PrimaryDark;
            recentRepositoriesList1.HoverColor = theme.PrimaryLight;
            recentRepositoriesList1.MainBackColor = theme.Primary;
            BackgroundImage = theme.BackgroundImage;

            foreach (var item in flpnlContribute.Controls.OfType<LinkLabel>().Union(flpnlStart.Controls.OfType<LinkLabel>()))
            {
                item.LinkColor = theme.PrimaryText;
            }
        }

        private Control CreateLink(Control container, float scaleFactor, string text, Image icon, EventHandler handler)
        {
            int paddingLeft;
            if (EnvUtils.IsMonoRuntime())
            {
                text = $"      {text}";
                paddingLeft = 12;
            }
            else
            {
                paddingLeft = 24;
            }

            var padding3 = (int)(3 * scaleFactor);
            var linkLabel = new LinkLabel
            {
                AutoSize = true,
                AutoEllipsis = true,
                Font = AppSettings.Font,
                Image = icon,
                ImageAlign = ContentAlignment.MiddleLeft,
                LinkBehavior = LinkBehavior.NeverUnderline,
                Margin = new Padding(padding3, 0, padding3, (int)(8 * scaleFactor)),
                Padding = new Padding((int)(paddingLeft * scaleFactor), padding3, padding3, padding3),
                TabStop = true,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft
            };
            linkLabel.MouseHover += (s, e) => linkLabel.LinkColor = _selectedTheme.AccentedText;
            linkLabel.MouseLeave += (s, e) => linkLabel.LinkColor = _selectedTheme.PrimaryText;

            if (handler != null)
            {
                linkLabel.Click += handler;
            }

            container.Controls.Add(linkLabel);

            return linkLabel;
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

        private void InitDashboardLayout()
        {
            var scaleFactor = GetScaleFactor();

            pnlLeft.SuspendLayout();

            AddLinks(flpnlContribute,
                     panel =>
                     {
                         panel.Controls.Add(lblContribute);
                         lblContribute.Font = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints + 5.5f);

                         CreateLink(panel, scaleFactor, develop.Text, Resources.develop.ToBitmap(), GitHubItem_Click);
                         CreateLink(panel, scaleFactor, donate.Text, Resources.dollar.ToBitmap(), DonateItem_Click);
                         CreateLink(panel, scaleFactor, translate.Text, Resources.EditItem, TranslateItem_Click);
                         var lastControl = CreateLink(panel, scaleFactor, issues.Text, Resources.bug, IssuesItem_Click);
                         return lastControl;
                     },
                     (panel, lastControl) =>
                     {
                         var height = (int)((lastControl.Location.Y + lastControl.Size.Height) * scaleFactor) + panel.Padding.Bottom;
                         panel.Height = height;
                         panel.MinimumSize = new Size(0, height);
                         if (EnvUtils.IsMonoRuntime())
                         {
                             panel.Top = tableLayoutPanel1.Height - panel.Height;
                         }
                     });

            AddLinks(flpnlStart,
                     panel =>
                     {
                         CreateLink(panel, scaleFactor, createRepository.Text, Resources.IconRepoCreate, createItem_Click);
                         CreateLink(panel, scaleFactor, openRepository.Text, Resources.IconRepoOpen, openItem_Click);
                         CreateLink(panel, scaleFactor, cloneRepository.Text, Resources.IconCloneRepoGit, cloneItem_Click);
                         var lastControl = CreateLink(panel, scaleFactor, cloneSvnRepository.Text, Resources.IconCloneRepoSvn, cloneSvnItem_Click);

                         foreach (var gitHoster in RepoHosts.GitHosters)
                         {
                             lastControl = CreateLink(panel, scaleFactor, string.Format(cloneFork.Text, gitHoster.Description), Resources.IconCloneRepoGithub,
                                                      (repoSender, eventArgs) => UICommands.StartCloneForkFromHoster(this, gitHoster, GitModuleChanged));
                         }
                         return lastControl;
                     },
                     (panel, lastControl) =>
                     {
                         var height = (int)((lastControl.Location.Y + lastControl.Size.Height) * scaleFactor) + panel.Padding.Bottom;
                         panel.MinimumSize = new Size(0, height);
                     });

            if (EnvUtils.IsMonoRuntime())
            {
                var maxLinkWidth1 = flpnlStart.Controls.OfType<LinkLabel>().Max(c => c.Width);
                var maxLinkWidth2 = flpnlContribute.Controls.OfType<LinkLabel>().Max(c => c.Width);
                var maxLinkWidth = Math.Max(maxLinkWidth1, maxLinkWidth2);
                if (pnlLeft.Width < maxLinkWidth)
                {
                    var width = maxLinkWidth + flpnlStart.Controls[0].Left * 2;
                    tableLayoutPanel1.ColumnStyles[1].Width = width;
                }
            }

            pnlLeft.ResumeLayout(false);
            pnlLeft.PerformLayout();

            AutoScrollMinSize = new Size(0, pnlLogo.Height + flpnlStart.MinimumSize.Height + flpnlContribute.MinimumSize.Height);
        }


        private void dashboard_ParentChanged(object sender, EventArgs e)
        {
            if (Parent == null)
            {
                Visible = false;
                return;
            }

            //
            // create Show current branch menu item and add to Dashboard menu
            //
            var showCurrentBranchMenuItem = new ToolStripMenuItem(showCurrentBranch.Text);
            showCurrentBranchMenuItem.Click += showCurrentBranchMenuItem_Click;
            showCurrentBranchMenuItem.Checked = AppSettings.DashboardShowCurrentBranch;

            var form = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == nameof(FormBrowse));
            if (form != null)
            {
                var menuStrip = FindControl<MenuStrip>(form.Controls, p => p.Name == "menuStrip1");
                var dashboardMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "dashboardToolStripMenuItem");
                dashboardMenu?.DropDownItems.Add(showCurrentBranchMenuItem);
            }

            Visible = true;
        }

        private void dashboard_SizeChanged(object sender, EventArgs e)
        {
            if (!EnvUtils.IsMonoRuntime())
            {
                return;
            }

            // Mono has issues with docked controls, so we need to manually position and size controls

            flpnlStart.Height = tableLayoutPanel1.Height - pnlLogo.Height - flpnlContribute.Height;
            flpnlContribute.Top = tableLayoutPanel1.Height - flpnlContribute.Height;

            flpnlStart.Width =
                flpnlContribute.Width = (int)tableLayoutPanel1.ColumnStyles[1].Width;

            Invalidate(true);
        }

        private void pbLogo_Click(object sender, EventArgs e)
        {

        }

        private void showCurrentBranchMenuItem_Click(object sender, EventArgs e)
        {
            bool newValue = !AppSettings.DashboardShowCurrentBranch;
            AppSettings.DashboardShowCurrentBranch = newValue;
            ((ToolStripMenuItem)sender).Checked = newValue;
            RefreshContent();
        }

        private static void TranslateItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.transifex.com/git-extensions/git-extensions/translate/");
        }

        private static void GitHubItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://github.com/gitextensions/gitextensions");
        }

        private static void IssuesItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://github.com/gitextensions/gitextensions/issues");
        }

        private void openItem_Click(object sender, EventArgs e)
        {
            GitModule module = FormOpenDirectory.OpenModule(this, currentModule: null);
            if (module != null)
                OnModuleChanged(this, new GitModuleEventArgs(module));
        }

        private void cloneItem_Click(object sender, EventArgs e)
        {
            UICommands.StartCloneDialog(this, null, false, OnModuleChanged);
        }

        private void cloneSvnItem_Click(object sender, EventArgs e)
        {
            UICommands.StartSvnCloneDialog(this, OnModuleChanged);
        }

        private void createItem_Click(object sender, EventArgs e)
        {
            UICommands.StartInitializeDialog(this, Module.WorkingDir, OnModuleChanged);
        }

        private static void DonateItem_Click(object sender, EventArgs e)
        {
            Process.Start(FormDonate.DonationUrl);
        }
    }
}
