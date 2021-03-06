﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace booruReader.Model
{
    /// <summary>
    /// This is an offline posts fetcher for the favorites
    /// </summary>
    public class FavoriteHandler
    {
        private List<BasePost> _favoritesList;
        private string _favoritesFolder;
        private string _favoritesThumbFolder;

        public FavoriteHandler()
        {
            FolderSetup();
            _favoritesList = new List<BasePost>();
            //Load in favorites xml that contains tag info and file detail
            PopulateLit();
        }

        private void PopulateLit()
        {
            if (File.Exists(GlobalSettings.Instance.ApplicationFolder + @"\Favorites.xml"))
            {
                TextReader textReader = new StreamReader(GlobalSettings.Instance.ApplicationFolder + @"\Favorites.xml");
                XmlSerializer deserializer = new XmlSerializer(_favoritesList.GetType());
                _favoritesList = (List<BasePost>)deserializer.Deserialize(textReader);
                textReader.Close();
            }
        }

        #region Public functions

        public List<BasePost> FetchFavorites(string tagsToMatch)
        {
            PopulateLit();

            List<BasePost> returnList;
            if (string.IsNullOrEmpty(tagsToMatch))
                returnList = _favoritesList;
            else
            {
                returnList = new List<BasePost>();

                Parallel.ForEach<string>(tagsToMatch.Split(' '), (tag) =>
                {
                    foreach (BasePost post in _favoritesList.Where(x => x.Tags.Contains(tag)))
                    {
                        if (!returnList.Contains(post))
                            returnList.Add(post);
                    }
                });
            }

            return returnList;
        }

        public void AddToFavorites(BasePost webPost, string fileToCopy)
        {
            // Replace http paths with the drive paths to the images, ideally caching should solve this problem, but this needs testing
            BasePost favPost = new BasePost(webPost);

            string thumbPath = string.Empty;
            string bigPath = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(webPost.PreviewURL))
                {
                    thumbPath = Path.Combine(_favoritesThumbFolder, Path.GetFileName(webPost.PreviewURL));
                    File.Copy(webPost.PreviewURL, thumbPath, true);
                    favPost.PreviewURL = thumbPath;
                    favPost.URLStore = thumbPath;
                }
                else if (!string.IsNullOrEmpty(webPost.URLStore))
                {
                    thumbPath = Path.Combine(_favoritesThumbFolder, Path.GetFileName(webPost.URLStore));
                    File.Copy(webPost.URLStore, thumbPath, true);
                    favPost.PreviewURL = thumbPath;
                    favPost.URLStore = thumbPath;
                }
            }
            catch
            {
                //
            }

            try
            {
                bigPath = Path.Combine(_favoritesFolder, Path.GetFileName(fileToCopy));
                File.Copy(fileToCopy, bigPath, true);
                favPost.FullPictureURL = bigPath;
            }
            catch
            {
                //
            }

            //We can ignore the fails, file access for thumbnails isnt always instantly freed if same file is being added/removed 
            if (!string.IsNullOrEmpty(thumbPath) && !string.IsNullOrEmpty(bigPath) && _favoritesList.FirstOrDefault(x => x.PreviewURL == thumbPath && x.FullPictureURL == bigPath) == null)
            {
                _favoritesList.Add(favPost);

                UpdateFavoritesConfigFile();
            }
        }

        public void RemoveFromFavorites(BasePost post)
        {
            //Delete files off the drive and remove entry from list/xml file
            var favPost = _favoritesList.FirstOrDefault(x => x.FileMD == post.FileMD);

            if (favPost != null)
                _favoritesList.Remove(favPost);

            UpdateFavoritesConfigFile();
        }

        /// <summary>
        /// MD5 based filename builder
        /// </summary>
        private string FormFilename(string md5, string FullPictureURL)
        {
            string filename;

            if (FullPictureURL.ToLowerInvariant().Contains("jpg") || FullPictureURL.ToLowerInvariant().Contains("jpeg"))
                filename = ".jpg";
            else if (FullPictureURL.ToLowerInvariant().Contains("png"))
                filename = ".png";
            else if (FullPictureURL.ToLowerInvariant().Contains("gif"))
                filename = ".gif";
            else // This shouldn't happen
                filename = null;

            return md5 + filename;
        }
        #endregion

        #region Private functions

        private void FolderSetup()
        {
            _favoritesFolder = Path.Combine(GlobalSettings.Instance.ApplicationFolder, "Favorites");
            _favoritesThumbFolder = Path.Combine(_favoritesFolder, "Thumbs");

            if (!Directory.Exists(_favoritesFolder))
                Directory.CreateDirectory(_favoritesFolder);

            if (!Directory.Exists(_favoritesThumbFolder))
                Directory.CreateDirectory(_favoritesThumbFolder);
        }

        private void UpdateFavoritesConfigFile()
        {
            TextWriter textWriter = new StreamWriter(GlobalSettings.Instance.ApplicationFolder + @"\Favorites.xml");

            XmlSerializer x = new XmlSerializer(_favoritesList.GetType());
            x.Serialize(textWriter, _favoritesList);
            textWriter.Close();
        }

        #endregion

        internal void CheckForRemovedFavorites()
        {

            DirectoryInfo Dir = new DirectoryInfo(_favoritesThumbFolder);
            FileInfo[] FileList = Dir.GetFiles();

            foreach (FileInfo file in FileList)
            {
                try
                {
                    if (_favoritesList.FirstOrDefault(x => Path.GetFileName(x.PreviewURL) == Path.GetFileName(file.FullName)) == null)
                        file.Delete();
                }
                catch { }
            }

            Dir = new DirectoryInfo(_favoritesFolder);
            FileList = Dir.GetFiles();

            foreach (FileInfo file in FileList)
            {
                try
                {
                    if (_favoritesList.FirstOrDefault(x => Path.GetFileName(x.FullPictureURL) == Path.GetFileName(file.FullName)) == null)
                        file.Delete();
                }
                catch { }
            }
        }

    }
}
