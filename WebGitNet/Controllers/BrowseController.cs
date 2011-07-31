﻿//-----------------------------------------------------------------------
// <copyright file="BrowseController.cs" company="(none)">
//  Copyright © 2011 John Gietzen. All rights reserved.
// </copyright>
// <author>John Gietzen</author>
//-----------------------------------------------------------------------

namespace WebGitNet.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using WebGitNet.ActionResults;
    using WebGitNet.Models;

    public class BrowseController : Controller
    {
        private readonly FileManager fileManager;

        public BrowseController()
        {
            var reposPath = WebConfigurationManager.AppSettings["RepositoriesPath"];
            this.fileManager = new FileManager(reposPath);
        }

        public ActionResult Index()
        {
            var directory = this.fileManager.DirectoryInfo;

            var repos = (from dir in directory.EnumerateDirectories()
                         select dir.Name).ToList();

            return View(repos);
        }

        public ActionResult ViewRepo(string repo)
        {
            var resourceInfo = this.fileManager.GetResourceInfo(repo);
            if (resourceInfo.Type != ResourceType.Directory)
            {
                return HttpNotFound();
            }

            ViewBag.RepoName = resourceInfo.Name;
            ViewBag.LastCommit = GitUtilities.GetLogEntries(resourceInfo.FullPath, 1).FirstOrDefault();
            ViewBag.CurrentTree = GitUtilities.GetTreeInfo(resourceInfo.FullPath, "HEAD");

            return View();
        }

        public ActionResult ViewTree(string repo, string @object, string path)
        {
            var resourceInfo = this.fileManager.GetResourceInfo(repo);
            if (resourceInfo.Type != ResourceType.Directory)
            {
                return HttpNotFound();
            }

            var items = GitUtilities.GetTreeInfo(resourceInfo.FullPath, @object, path);
            ViewBag.RepoName = resourceInfo.Name;
            ViewBag.Tree = @object;
            ViewBag.Path = path ?? string.Empty;

            return View(items);
        }

        public ActionResult ViewBlob(string repo, string @object, string path, bool raw = false)
        {
            var resourceInfo = this.fileManager.GetResourceInfo(repo);
            if (resourceInfo.Type != ResourceType.Directory || string.IsNullOrEmpty(path))
            {
                return HttpNotFound();
            }

            var fileName = Path.GetFileName(path);
            var contentType = MimeUtilities.GetMimeType(fileName);

            if (raw)
            {
                return new GitFileResult(resourceInfo.FullPath, @object, path, contentType);
            }

            ViewBag.RepoName = resourceInfo.Name;
            ViewBag.Tree = @object;
            ViewBag.Path = path;
            ViewBag.FileName = fileName;
            ViewBag.ContentType = contentType;

            return View();
        }
    }
}