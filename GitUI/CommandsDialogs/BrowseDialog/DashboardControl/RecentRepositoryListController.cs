using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GitCommands;
using GitCommands.Repository;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public sealed class RecentRepositoryListController
    {
        public string GetCurrentBranchName(string path)
        {
            if (!AppSettings.DashboardShowCurrentBranch || GitModule.IsBareRepository(path))
            {
                return string.Empty;
            }
            return GitModule.GetSelectedBranchFast(path);
        }

        public GitModule GetGitModule(string path)
        {
            var module = new GitModule(path);
            Repositories.AddMostRecentRepository(module.WorkingDir);
            return module;
        }

        public bool IsValidGitWorkingDir(string path)
        {
            return GitModule.IsValidGitWorkingDir(path);
        }

        public async Task<IList<RecentRepoInfo>> PreRenderRepositoriesAsync(Graphics g)
        {
            return await Task.Run(() =>
            {
                var mostRecentRepos = new List<RecentRepoInfo>();
                var lessRecentRepos = new List<RecentRepoInfo>();

                var splitter = new RecentRepoSplitter
                {
                    Graphics = g,
                    MeasureFont = AppSettings.Font,

                    MaxRecentRepositories = AppSettings.MaxMostRecentRepositories,
                    RecentReposComboMinWidth = AppSettings.RecentReposComboMinWidth,
                    ShorteningStrategy = AppSettings.ShorteningRecentRepoPathStrategy,
                    SortLessRecentRepos = AppSettings.SortLessRecentRepos,
                    SortMostRecentRepos = AppSettings.SortMostRecentRepos
                };
                // NB: ToList'ing to avoid cross-thread access
                splitter.SplitRecentRepos(Repositories.RepositoryHistory.Repositories.ToList(), mostRecentRepos, lessRecentRepos);

                var repositories = mostRecentRepos.Union(lessRecentRepos).ToList();

                Repositories.RepositoryCategories.ForEach(c =>
                {
                    c.Repositories.ForEach(r =>
                    {
                        var repo = repositories.FirstOrDefault(rp =>  rp.Repo.Path == r.Path);
                        if (repo != null && !string.IsNullOrWhiteSpace(c.Description))
                        {
                            repo.Repo.Category = c.Description;
                        }
                    });
                });

                return repositories;
            });
        }

        public void RemoveRepository(string path)
        {
            Repositories.RepositoryHistory.RemoveRecentRepository(path);
            Repositories.SaveSettings();
        }

        public void SetAnchor(Repository repository, Repository.RepositoryAnchor anchor)
        {
            repository.Anchor = anchor;
            Repositories.SaveSettings();
        }

        public void SetCategory(Repository repository, string category)
        {
            MigrateCategory(repository);
            repository.Category = category;
            Repositories.SaveSettings();
        }


        // Migration logic from earlier versions - originally categories were stored in Repositories.RepositoryCategories structure
        // Now category information is a property of a repository and it is stored with the repository in Repositories.RepositoryHistory structure
        // Whenever a repository is now assined to a category we need to ensure it is deleted from the old data structure, if it was stored there
        private static void MigrateCategory(Repository repository)
        {
            bool saveRequired = false;
            Repositories.RepositoryCategories.ForEach(c =>
            {
                var repo = c.Repositories.FirstOrDefault(r => r.Path == repository.Path);
                if (repo != null)
                {
                    c.Repositories.Remove(repo);
                    saveRequired = true;
                }
            });
            if (saveRequired)
            {
                Repositories.SaveSettings();
            }
        }
    }
}