﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Security;
using System.Web.Services;
using System.Xml.Serialization;
using Carrotware.CMS.Core;
using Carrotware.CMS.Data;
/*
* CarrotCake CMS
* http://carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: October 2011
*/

namespace Carrotware.CMS.UI.Admin {
	/// <summary>
	/// Summary description for CMS
	/// </summary>
	[WebService(Namespace = "http://carrotware.com/cms/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]

	public class CMS : System.Web.Services.WebService {

		protected CarrotCMSDataContext db = new CarrotCMSDataContext();

		private CMSConfigHelper cmsHelper = new CMSConfigHelper();
		private ContentPageHelper pageHelper = new ContentPageHelper();
		private PageWidgetHelper widgetHelper = new PageWidgetHelper();

		private Guid CurrentPageGuid = Guid.Empty;
		private ContentPage filePage = null;

		private List<ContentPage> _pages = null;
		protected List<ContentPage> lstActivePages {
			get {
				if (_pages == null) {
					_pages = pageHelper.GetLatestContentList(SiteData.CurrentSiteID, true);
				}
				return _pages;
			}
		}

		public ContentPage cmsAdminContent {
			get {
				ContentPage c = null;
				try {
					var sXML = GetSerialized("cmsAdminContent");
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContentPage));
					StringReader stringReader = new StringReader(sXML);
					Object genpref = xmlSerializer.Deserialize(stringReader);
					stringReader.Close();
					c = genpref as ContentPage;
				} catch { }
				return c;
			}
			set {
				if (value == null) {
					ClearSerialized("cmsAdminContent");
				} else {
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContentPage));
					StringWriter stringWriter = new StringWriter();
					xmlSerializer.Serialize(stringWriter, value);
					string sXML = stringWriter.ToString();
					stringWriter.Close();
					SaveSerialized("cmsAdminContent", sXML);

				}
			}
		}

		public List<PageWidget> cmsAdminWidget {
			get {
				List<PageWidget> c = null;
				var sXML = GetSerialized("cmsAdminWidget");
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PageWidget>));
				StringReader stringReader = new StringReader(sXML);
				Object genpref = xmlSerializer.Deserialize(stringReader);
				stringReader.Close();
				c = genpref as List<PageWidget>;
				return c;
			}
			set {
				if (value == null) {
					ClearSerialized("cmsAdminWidget");
				} else {
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PageWidget>));
					StringWriter stringWriter = new StringWriter();
					xmlSerializer.Serialize(stringWriter, value);
					string sXML = stringWriter.ToString();
					stringWriter.Close();
					SaveSerialized("cmsAdminWidget", sXML);

				}
			}
		}


		private void SaveSerialized(string sKey, string sData) {
			bool bAdd = false;
			LoadGuids();

			var itm = (from c in db.tblSerialCaches
					   where c.ItemID == CurrentPageGuid
					   && c.EditUserId == SiteData.CurrentUserGuid
					   && c.KeyType == sKey
					   && c.SiteID == SiteData.CurrentSiteID
					   select c).FirstOrDefault();

			if (itm == null) {
				bAdd = true;
				itm = new tblSerialCache();
				itm.SerialCacheID = Guid.NewGuid();
				itm.SiteID = SiteData.CurrentSiteID;
				itm.ItemID = CurrentPageGuid;
				itm.EditUserId = SiteData.CurrentUserGuid;
				itm.KeyType = sKey;
			}

			itm.SerializedData = sData;
			itm.EditDate = DateTime.Now;

			if (bAdd) {
				db.tblSerialCaches.InsertOnSubmit(itm);
			}
			db.SubmitChanges();
		}


		private string GetSerialized(string sKey) {
			string sData = "";
			LoadGuids();

			var itm = (from c in db.tblSerialCaches
					   where c.ItemID == CurrentPageGuid
					   && c.EditUserId == SiteData.CurrentUserGuid
					   && c.KeyType == sKey
					   && c.SiteID == SiteData.CurrentSiteID
					   select c).FirstOrDefault();

			if (itm != null) {
				sData = itm.SerializedData;
			}

			return sData;
		}


		private bool ClearSerialized(string sKey) {
			LoadGuids();

			var itm = (from c in db.tblSerialCaches
					   where c.ItemID == CurrentPageGuid
					   && c.EditUserId == SiteData.CurrentUserGuid
					   && c.KeyType == sKey
					   && c.SiteID == SiteData.CurrentSiteID
					   select c).FirstOrDefault();

			if (itm != null) {
				db.tblSerialCaches.DeleteOnSubmit(itm);
				db.SubmitChanges();
				return true;
			}

			return false;
		}

		private void LoadGuids() {
			if (!string.IsNullOrEmpty(CurrentEditPage)) {
				ContentPageHelper pageHelper = new ContentPageHelper();
				filePage = pageHelper.GetLatestContent(SiteData.CurrentSiteID, null, CurrentEditPage.ToString().ToLower());
				CurrentPageGuid = filePage.Root_ContentID;
			} else {
				if (CurrentPageGuid != Guid.Empty) {
					ContentPageHelper pageHelper = new ContentPageHelper();
					filePage = pageHelper.GetLatestContent(SiteData.CurrentSiteID, CurrentPageGuid);
					CurrentEditPage = filePage.FileName;
				} else {
					filePage = new ContentPage();
				}
			}

		}


		private string CurrentEditPage = "";


		private string ParseEdit(string sContent) {
			string sFixed = "";
			if (!string.IsNullOrEmpty(sContent)) {
				sFixed = sContent;
				var b = "<!-- <#|BEGIN_CARROT_CMS|#> -->";
				var iB = sFixed.IndexOf(b);
				if (iB > 0) {
					sFixed = sFixed.Substring(iB + b.Length);

					var e = "<!-- <#|END_CARROT_CMS|#> -->";
					var iE = sFixed.IndexOf(e);
					sFixed = sFixed.Substring(0, iE);
				}
			}
			return sFixed.Trim();
		}




		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string RecordHeartbeat(string PageID) {
			try {
				CurrentPageGuid = new Guid(PageID);
				LoadGuids();
				var h = (from r in db.tblRootContents
						 where r.Root_ContentID == CurrentPageGuid
						 && r.SiteID == SiteData.CurrentSiteID
						 select r).FirstOrDefault();

				if (h != null) {
					h.Heartbeat_UserId = SiteData.CurrentUserGuid;
					h.EditHeartbeat = DateTime.Now;
					db.SubmitChanges();
					return DateTime.Now.ToString();
				} else {
					return DateTime.MinValue.ToString();
				}

			} catch (Exception ex) {
				return DateTime.MinValue.ToString();
			}
		}


		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string RecordEditorPosition(string ToolbarState, string ToolbarMargin, string ToolbarScroll) {
			try {
				GetSetUserEditState(ToolbarState, ToolbarMargin, ToolbarScroll);

				return "OK";
			} catch (Exception ex) {
				return ex.ToString();
			}
		}

		private void GetSetUserEditState(string ToolbarState, string ToolbarMargin, string ToolbarScroll) {
			UserEditState editor = UserEditState.cmsUserEditState;
			if (editor == null) {
				editor = new UserEditState();
			}

			editor.EditorOpen = string.IsNullOrEmpty(ToolbarState) ? (string.IsNullOrEmpty(editor.EditorOpen) ? "true" : editor.EditorOpen) : ToolbarState.ToLower();
			editor.EditorMargin = string.IsNullOrEmpty(ToolbarMargin) ? (string.IsNullOrEmpty(editor.EditorMargin) ? "L" : editor.EditorMargin) : ToolbarMargin.ToUpper();
			editor.EditorScrollPosition = string.IsNullOrEmpty(ToolbarScroll) ? "0" : ToolbarScroll.ToLower();

			UserEditState.cmsUserEditState = editor;
		}


		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public List<SiteMapOrder> GetChildPages(string PageID, string CurrPageID) {
			Guid? ParentID = Guid.Empty;
			if (!string.IsNullOrEmpty(PageID)) {
				if (PageID.Length > 20) {
					ParentID = new Guid(PageID);
				}
			}

			Guid ContPageID = Guid.Empty;
			if (!string.IsNullOrEmpty(CurrPageID)) {
				if (CurrPageID.Length > 20) {
					ContPageID = new Guid(CurrPageID);
				}
			}

			List<SiteMapOrder> lstSiteMap = new List<SiteMapOrder>();

			var lst = (from ct in db.tblContents
					   join r in db.tblRootContents on ct.Root_ContentID equals r.Root_ContentID
					   orderby ct.NavOrder, ct.NavMenuText
					   where r.SiteID == SiteData.CurrentSiteID
							 && r.Root_ContentID != ContPageID
							 && ct.IsLatestVersion == true
							 && (ct.Parent_ContentID == ParentID
								 || (ct.Parent_ContentID == null && ParentID == Guid.Empty))
					   select new SiteMapOrder {
						   NavLevel = -1,
						   NavMenuText = ct.NavMenuText,
						   NavOrder = ct.NavOrder,
						   PageActive = Convert.ToBoolean(r.PageActive),
						   Parent_ContentID = ct.Parent_ContentID,
						   Root_ContentID = ct.Root_ContentID
					   }).ToList();

			lstSiteMap = (from l in lst
						  orderby l.NavOrder, l.NavMenuText
						  where l.Parent_ContentID != ContPageID || l.Parent_ContentID == null
						  select l).ToList();

			return lstSiteMap;

		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public List<SiteMapOrder> GetPageCrumbs(string PageID, string CurrPageID) {

			Guid? ContentPageID = Guid.Empty;

			if (!string.IsNullOrEmpty(PageID)) {
				if (PageID.Length > 20) {
					ContentPageID = new Guid(PageID);
				}
			}

			Guid ContPageID = Guid.Empty;
			if (!string.IsNullOrEmpty(CurrPageID)) {
				if (CurrPageID.Length > 20) {
					ContPageID = new Guid(CurrPageID);
				}
			}

			List<SiteMapOrder> lstSiteMap = new List<SiteMapOrder>();

			int iLevel = 0;

			int iLenB = 0;
			int iLenA = 1;

			while (iLenB < iLenA) {
				iLenB = lstSiteMap.Count;

				var cont = (from r in db.tblRootContents
							join ct in db.tblContents on r.Root_ContentID equals ct.Root_ContentID
							orderby ct.NavOrder, ct.NavMenuText
							where r.SiteID == SiteData.CurrentSiteID
								&& ct.IsLatestVersion == true
								&& ct.Root_ContentID == ContentPageID

							select new SiteMapOrder {
								NavLevel = iLevel,
								NavMenuText = ct.NavMenuText,
								NavOrder = ct.NavOrder,
								PageActive = Convert.ToBoolean(r.PageActive),
								Parent_ContentID = ct.Parent_ContentID,
								Root_ContentID = ct.Root_ContentID
							}).FirstOrDefault();

				iLevel++;
				if (cont != null) {
					ContentPageID = cont.Parent_ContentID;
					lstSiteMap.Add(cont);
				}

				iLenA = lstSiteMap.Count;
			}

			return lstSiteMap.OrderByDescending(y => y.NavLevel).ToList();
		}



		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string UpdatePageTemplate(string TheTemplate, string ThisPage) {
			try {
				TheTemplate = cmsHelper.DecodeBase64(TheTemplate);
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();

				var c = cmsAdminContent;

				c.TemplateFile = TheTemplate;

				cmsAdminContent = c;

				GetSetUserEditState("", "", "");

				return "OK";
			} catch (Exception ex) {
				return ex.ToString();
			}
		}


		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string ValidateUniqueFilename(string TheFileName, string PageID) {
			try {
				CurrentPageGuid = new Guid(PageID);
				TheFileName = ContentPageHelper.ScrubFilename(CurrentPageGuid, TheFileName);

				var h = (from r in db.tblRootContents
						 where r.Root_ContentID != CurrentPageGuid
						 && r.FileName.ToLower() == TheFileName.ToLower()
						 && r.SiteID == SiteData.CurrentSiteID
						 select r).FirstOrDefault();

				if (h == null) {
					return "PASS";
				} else {
					return "FAIL";
				}
			} catch (Exception ex) {
				return ex.ToString();
			}
		}


		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CacheWidgetUpdate(string WidgetAddition, string ThisPage) {
			try {
				WidgetAddition = cmsHelper.DecodeBase64(WidgetAddition);
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();

				var cacheWidget = cmsAdminWidget;

				List<PageWidget> inputWid = new List<PageWidget>();
				Dictionary<Guid, int> dictOrder = new Dictionary<Guid, int>();
				int iW = 0;

				WidgetAddition = WidgetAddition.Replace("\r\n", "\n");
				WidgetAddition = WidgetAddition.Replace("\r", "\n");
				var arrWidgRows = WidgetAddition.Split('\n');

				foreach (string arrWidgCell in arrWidgRows) {
					if (!string.IsNullOrEmpty(arrWidgCell)) {
						bool bGoodWidget = false;
						var w = arrWidgCell.Split('\t');
						var rWidg = new PageWidget();
						if (w[2].ToLower().EndsWith(".ascx") || w[2].ToLower().StartsWith("class:")) {
							rWidg.ControlPath = w[2];
							rWidg.Root_WidgetID = Guid.NewGuid();
							bGoodWidget = true;
						} else {
							if (w[2].ToString().Length == Guid.Empty.ToString().Length) {
								rWidg.Root_WidgetID = new Guid(w[2]);
								bGoodWidget = true;
							}
						}
						if (bGoodWidget) {
							dictOrder.Add(rWidg.Root_WidgetID, iW);

							rWidg.WidgetDataID = Guid.NewGuid();
							rWidg.PlaceholderName = w[1].Substring(4);
							rWidg.WidgetOrder = int.Parse(w[0]);
							rWidg.Root_ContentID = cmsAdminContent.Root_ContentID;
							rWidg.IsWidgetActive = true;
							rWidg.IsLatestVersion = true;
							rWidg.EditDate = DateTime.Now;
							inputWid.Add(rWidg);
						}
						iW++;
					}
				}

				foreach (var wd in inputWid) {
					var z = (from d in cacheWidget where d.Root_WidgetID == wd.Root_WidgetID select d).FirstOrDefault();
					if (z == null) {
						cacheWidget.Add(wd);
					} else {
						var i = cacheWidget.IndexOf(z);
						cacheWidget[i].WidgetOrder = wd.WidgetOrder;

						int? mainSort = (from entry in dictOrder
										 where entry.Key == wd.Root_WidgetID
										 select entry.Value).FirstOrDefault();
						if (mainSort != null) {
							cacheWidget[i].WidgetOrder = Convert.ToInt32(mainSort);
						}
					}
				}

				cmsAdminWidget = cacheWidget;
				return "OK";
			} catch (Exception ex) {
				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string GetWidgetText(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				var ww = (from w in cmsAdminWidget
						  where w.Root_WidgetID == guidWidget
						  select w).FirstOrDefault();

				if (ww != null) {
					if (string.IsNullOrEmpty(ww.ControlProperties)) {
						return "No Data";
					} else {
						if (ww.ControlProperties.Length < 768) {
							return ww.ControlProperties;
						} else {
							return ww.ControlProperties.Substring(0, 700) + ".........";
						}
					}
				}

				return "OK";
			} catch (Exception ex) {
				return "FAIL";
			}
		}


		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string DeleteWidget(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				var cacheWidget = cmsAdminWidget;

				var ww = (from w in cacheWidget
						  where w.Root_WidgetID == guidWidget
						  select w).ToList();

				if (ww != null) {
					foreach (var w in ww) {
						w.IsWidgetPendingDelete = true;
						w.IsWidgetActive = false;
						w.EditDate = DateTime.Now;
					}
				}

				cmsAdminWidget = cacheWidget;

				return "OK";
			} catch (Exception ex) {
				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string RemoveWidget(string DBKey, string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				var cacheWidget = cmsAdminWidget;

				var ww = (from w in cacheWidget
						  where w.Root_WidgetID == guidWidget
						  select w).ToList();

				if (ww != null) {
					foreach (var w in ww) {
						w.IsWidgetActive = false;
						w.EditDate = DateTime.Now;
					}
				}

				cmsAdminWidget = cacheWidget;

				return "OK";
			} catch (Exception ex) {
				return ex.ToString();
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CacheGenericContent(string ZoneText, string DBKey, string ThisPage) {
			try {
				ZoneText = cmsHelper.DecodeBase64(ZoneText);
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				Guid guidWidget = new Guid(DBKey);

				var cacheWidget = cmsAdminWidget;

				var c = (from w in cacheWidget
						 where w.Root_WidgetID == guidWidget
						 select w).FirstOrDefault();

				c.ControlProperties = ZoneText;
				c.EditDate = DateTime.Now;

				cmsAdminWidget = cacheWidget;

				return "OK";
			} catch (Exception ex) {
				return ex.ToString();
			}
		}




		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CacheContentZoneText(string ZoneText, string Zone, string ThisPage) {
			try {
				ZoneText = cmsHelper.DecodeBase64(ZoneText);
				Zone = cmsHelper.DecodeBase64(Zone);
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				CurrentEditPage = filePage.FileName.ToLower();

				var c = cmsAdminContent;
				c.EditDate = DateTime.Now;
				c.EditUserId = SiteData.CurrentUserGuid;
				c.ContentID = Guid.NewGuid();

				if (Zone.ToLower() == "c")
					c.PageText = ZoneText;

				if (Zone.ToLower() == "l")
					c.LeftPageText = ZoneText;

				if (Zone.ToLower() == "r")
					c.RightPageText = ZoneText;

				cmsAdminContent = c;

				return "OK";
			} catch (Exception ex) {
				return ex.ToString();
			}
		}



		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string PublishChanges(string ThisPage) {
			try {
				CurrentPageGuid = new Guid(ThisPage);
				LoadGuids();
				CurrentEditPage = filePage.FileName.ToLower();

				var pageContents = pageHelper.GetLatestContent(SiteData.CurrentSiteID, null, CurrentEditPage);

				pageContents.IsLatestVersion = false;

				var pageWidgets = widgetHelper.GetWidgets(pageContents.Root_ContentID, true);

				if (cmsAdminContent != null) {

					ContentPage newContent = new ContentPage();
					newContent.ContentID = Guid.NewGuid();
					newContent.Parent_ContentID = cmsAdminContent.Parent_ContentID;
					newContent.Root_ContentID = cmsAdminContent.Root_ContentID;

					newContent.SiteID = SiteData.CurrentSiteID;

					newContent.PageText = ParseEdit(cmsAdminContent.PageText);
					newContent.LeftPageText = ParseEdit(cmsAdminContent.LeftPageText);
					newContent.RightPageText = ParseEdit(cmsAdminContent.RightPageText);

					newContent.IsLatestVersion = true;
					newContent.FileName = cmsAdminContent.FileName;
					newContent.TitleBar = cmsAdminContent.TitleBar;
					newContent.NavMenuText = cmsAdminContent.NavMenuText;
					newContent.NavOrder = cmsAdminContent.NavOrder;
					newContent.PageHead = cmsAdminContent.PageHead;
					newContent.PageActive = cmsAdminContent.PageActive;
					newContent.EditUserId = SiteData.CurrentUserGuid;
					newContent.EditDate = DateTime.Now;

					newContent.TemplateFile = cmsAdminContent.TemplateFile;
					newContent.MetaDescription = cmsAdminContent.MetaDescription;
					newContent.MetaKeyword = cmsAdminContent.MetaKeyword;

					//foreach (var wd in pageWidgets) {
					//    wd.Delete();
					//}
					foreach (var wd in cmsAdminWidget) {
						wd.Save();
					}

					newContent.SavePageEdit();

					cmsAdminWidget = new List<PageWidget>();
					cmsAdminContent = null;
				}

				GetSetUserEditState("true", "", "");

				return "OK";

			} catch (Exception ex) {
				return ex.ToString();
			}
		}




	}
}
