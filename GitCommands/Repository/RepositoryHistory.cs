using System;
using System.Linq;
using System.Xml.Serialization;

namespace GitCommands.Repository
{
    public class RepositoryHistory : RepositoryCategory
    {
        public RepositoryHistory(int maxCount)
        {
            Description = "Recent Repositories";
            MaxCount = maxCount;
        }

        public RepositoryHistory()
            : this(0)
        {
        }


        private int _maxCount;

        [XmlIgnore]
        public int MaxCount
        {
            get
            {
                return _maxCount;
            }
            set
            {
                _maxCount = value;
                while (_maxCount > 0 && Repositories.Count > _maxCount)
                {
                    Repositories.RemoveAt(_maxCount);
                }
            }
        }

        public override void SetIcon()
        {
        }

        public void RemoveRecentRepository(string repo)
        {
            foreach (var recentRepository in Repositories)
            {
                if (!recentRepository.Path.Equals(repo, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                Repositories.Remove(recentRepository);
                break;
            }
        }

        public void AddMostRecentRepository(string repositoryPath)
        {
            repositoryPath = repositoryPath?.Trim();
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                return;
            }

            if (!Repository.PathIsUrl(repositoryPath))
            {
                repositoryPath = repositoryPath.ToNativePath().EnsureTrailingPathSeparator();
            }

            var recentRepository = Repositories.FirstOrDefault(r => r.Path.Equals(repositoryPath, StringComparison.CurrentCultureIgnoreCase));
            if (recentRepository != null)
            {
                Repositories.Remove(recentRepository);
                Repositories.Insert(0, recentRepository);
                recentRepository.Anchor = Repository.RepositoryAnchor.MostRecent;
            }
            else
            {
                Repositories.Insert(0, new Repository(repositoryPath));
            }

            while (MaxCount > 0 && Repositories.Count > MaxCount)
            {
                Repositories.RemoveAt(MaxCount);
            }
        }
    }
}