﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Carrotware.CMS.Core;
using Carrotware.CMS.UI.Controls;
using Carrotware.Web.UI.Controls;

/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: October 2011
*/

namespace Carrotware.CMS.UI.Admin.c3_admin {

	public partial class SiteSkinEdit : AdminBasePage {
		protected FileDataHelper helpFile = CMSConfigHelper.GetFileDataHelper();
		public string sTemplateFileQS = String.Empty;
		protected string sTemplateFile = String.Empty;
		protected string sFullFilePath = String.Empty;
		protected string sDirectory = String.Empty;
		protected string sEditFile = String.Empty;

		protected void Page_Load(object sender, EventArgs e) {
			Master.ActivateTab(AdminBaseMasterPage.SectionID.ContentSkinEdit);

			if (!String.IsNullOrEmpty(Request.QueryString["path"])) {
				sTemplateFileQS = Request.QueryString["path"].ToString();
				sTemplateFile = CMSConfigHelper.DecodeBase64(sTemplateFileQS);
				sFullFilePath = HttpContext.Current.Server.MapPath(sTemplateFile);
				sEditFile = sFullFilePath;
			}

			if (!String.IsNullOrEmpty(Request.QueryString["alt"])) {
				string sAltFileQS = Request.QueryString["alt"].ToString();
				string sAltFile = CMSConfigHelper.DecodeBase64(sAltFileQS);
				sEditFile = HttpContext.Current.Server.MapPath(sAltFile);
			}

			litSkinFileName.Text = sTemplateFile;

			litEditFileName.Text = sEditFile.Replace(Server.MapPath("~"), @"\");

			if (File.Exists(sEditFile)) {
				if (!IsPostBack) {
					using (StreamReader sr = new StreamReader(sEditFile)) {
						txtPageContents.Text = sr.ReadToEnd();
					}
				}

				litDateMod.Text = File.GetLastWriteTime(sEditFile).ToString();

				if (sFullFilePath.LastIndexOf(@"\") > 0) {
					sDirectory = sFullFilePath.Substring(0, sFullFilePath.LastIndexOf(@"\"));
				} else {
					sDirectory = sFullFilePath.Substring(0, sFullFilePath.LastIndexOf(@"/"));
				}

				SetSourceFiles(sDirectory);
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e) {
			if (File.Exists(sEditFile)) {
				Encoding encode = System.Text.Encoding.Default;

				using (var oWriter = new StreamWriter(sEditFile, false, encode)) {
					oWriter.Write(txtPageContents.Text);
					oWriter.Close();
				}

				Response.Redirect(SiteData.CurrentScriptName + "?" + Request.QueryString.ToString());
			}
		}

		public string EncodePath(string sIn) {
			if (!(sIn.StartsWith(@"\") || sIn.StartsWith(@"/"))) {
				sIn = @"/" + sIn;
			}
			return CMSConfigHelper.EncodeBase64(sIn.ToLowerInvariant());
		}

		protected void SetSourceFiles(string sDir) {
			List<FileData> flsWorking = new List<FileData>();
			List<FileData> fldrWorking = new List<FileData>();

			List<string> lstFileExtensions = new List<string>();
			lstFileExtensions.Add(".css");
			lstFileExtensions.Add(".js");
			lstFileExtensions.Add(".ascx");
			lstFileExtensions.Add(".master");

			if (Directory.Exists(sDir)) {
				string sDirParent = "";

				if (sDir.LastIndexOf(@"\") > 0) {
					sDirParent = sDir.Substring(0, sDir.LastIndexOf(@"\"));
				} else {
					sDirParent = sDir.Substring(0, sDir.LastIndexOf(@"/"));
				}

				FileData skinFolder = helpFile.GetFolderInfo("/", sDir);

				skinFolder.FolderPath = FileDataHelper.MakeWebFolderPath(sDir);

				fldrWorking = helpFile.SpiderDeepFoldersFD(FileDataHelper.MakeWebFolderPath(sDir));

				fldrWorking.Add(skinFolder);

				try {
					if (Directory.Exists(Server.MapPath("~/includes"))) {
						FileData incFolder = helpFile.GetFolderInfo("/", Server.MapPath("~/includes"));
						fldrWorking.Add(incFolder);
					}
					if (Directory.Exists(Server.MapPath("~/js"))) {
						FileData incFolder = helpFile.GetFolderInfo("/", Server.MapPath("~/js"));
						fldrWorking.Add(incFolder);
					}
					if (Directory.Exists(Server.MapPath("~/css"))) {
						FileData incFolder = helpFile.GetFolderInfo("/", Server.MapPath("~/css"));
						fldrWorking.Add(incFolder);
					}
				} catch (Exception ex) { }

				helpFile.IncludeAllFiletypes();

				foreach (FileData f in fldrWorking) {
					List<FileData> fls = helpFile.GetFiles(f.FolderPath);

					flsWorking = (from m in flsWorking.Union(fls).ToList()
								  join e in lstFileExtensions on m.FileExtension.ToLowerInvariant() equals e
								  select m).ToList();
				}

				flsWorking = flsWorking.Where(x => x.MimeType.StartsWith("text") && (x.FolderPath.ToLowerInvariant().StartsWith(SiteData.AdminFolderPath) == false)).ToList();

				GeneralUtilities.BindRepeater(rpFiles, flsWorking.Distinct().OrderBy(x => x.FileName).OrderBy(x => x.FolderPath).ToList());
			}
		}
	}
}