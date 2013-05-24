﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: October 2011
*/

namespace Carrotware.CMS.Core {

	public class CMSAdminModule {
		public CMSAdminModule() {
			PluginMenus = new List<CMSAdminModuleMenu>();
		}
		public Guid PluginID { get; set; }
		public string PluginName { get; set; }
		public List<CMSAdminModuleMenu> PluginMenus { get; set; }
	}

	public class CMSAdminModuleMenu {
		public Guid PluginID { get; set; }
		public int SortOrder { get; set; }
		public string Caption { get; set; }
		public string PluginParm { get; set; }
		public string ControlFile { get; set; }
		public bool UseAjax { get; set; }
		public bool UsePopup { get; set; }
		public bool IsVisible { get; set; }

	}

	public class CMSPlugin {
		public CMSPlugin() {
			this.SortOrder = 1000;
		}

		public int SortOrder { get; set; }
		public string FilePath { get; set; }
		public string Caption { get; set; }
	}

	public class CMSTemplate {
		public string TemplatePath { get; set; }
		public string Caption { get; set; }
		public string EncodedPath { get; set; }
	}

	public class DynamicSite {
		public Guid SiteID { get; set; }
		public string DomainName { get; set; }

	}

	public class CMSFilePath {

		public CMSFilePath() {
			this.DateChecked = DateTime.UtcNow;
			this.FileExists = false;
			this.SiteID = Guid.Empty;
			this.TemplateFile = null;
		}

		public CMSFilePath(string fileName) {
			this.DateChecked = DateTime.UtcNow;
			this.TemplateFile = fileName.ToLower();
			this.SiteID = Guid.Empty;
			this.FileExists = File.Exists(HttpContext.Current.Server.MapPath(this.TemplateFile));
		}

		public CMSFilePath(string fileName, Guid siteID) {
			this.DateChecked = DateTime.UtcNow;
			this.TemplateFile = fileName.ToLower();
			this.SiteID = siteID;
			this.FileExists = File.Exists(HttpContext.Current.Server.MapPath(this.TemplateFile));
		}

		public DateTime DateChecked { get; set; }
		public string TemplateFile { get; set; }
		public bool FileExists { get; set; }
		public Guid SiteID { get; set; }

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is CMSFilePath) {
				CMSFilePath p = (CMSFilePath)obj;
				return (this.TemplateFile.ToLower() == p.TemplateFile.ToLower())
					&& (this.SiteID == p.SiteID);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return TemplateFile.ToLower().GetHashCode() ^ SiteID.GetHashCode();
		}
	}

}
