﻿using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Carrotware.CMS.Core;

/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: October 2011
*/

namespace Carrotware.CMS.UI.Controls {

	[ToolboxData("<{0}:OpenGraph runat=server></{0}:OpenGraph>")]
	public class OpenGraph : BaseServerControl {

		public enum OpenGraphTypeDef {
			Default,
			Article,
			Blog,
			Website,
			Book,
			Video,
			Movie,
			Profile
		}

		[Category("Appearance")]
		[DefaultValue(false)]
		public override bool EnableViewState {
			get {
				String s = (String)ViewState["EnableViewState"];
				bool b = ((s == null) ? false : Convert.ToBoolean(s));
				base.EnableViewState = b;
				return b;
			}

			set {
				ViewState["EnableViewState"] = value.ToString();
				base.EnableViewState = value;
			}
		}

		[Category("Appearance")]
		[DefaultValue(false)]
		public bool ShowExpirationDate {
			get {
				String s = (String)ViewState["ShowExpirationDate"];
				bool b = ((s == null) ? false : Convert.ToBoolean(s));
				return b;
			}

			set {
				ViewState["ShowExpirationDate"] = value.ToString();
			}
		}

		[Category("Appearance")]
		[DefaultValue("Default")]
		public OpenGraphTypeDef OpenGraphType {
			get {
				String s = (String)ViewState["OpenGraphType"];
				OpenGraphTypeDef c = OpenGraphTypeDef.Default;
				if (!String.IsNullOrEmpty(s)) {
					c = (OpenGraphTypeDef)Enum.Parse(typeof(OpenGraphTypeDef), s, true);
				}
				return c;
			}
			set {
				ViewState["OpenGraphType"] = value.ToString();
			}
		}

		protected override void RenderContents(HtmlTextWriter output) {
			foreach (Control c in this.Controls) {
				c.RenderControl(output);
				output.WriteLine();
			}
		}

		private ControlUtilities cu = new ControlUtilities();

		protected override void OnPreRender(EventArgs e) {
			this.Controls.Clear();

			try {
				ContentPage cp = cu.GetContainerContentPage(this);
				SiteData theSite = SiteData.CurrentSite;

				if (cp != null) {
					HtmlMeta metaSub = new HtmlMeta();
					metaSub.Attributes["property"] = "og:description";
					metaSub.Content = String.IsNullOrEmpty(cp.MetaDescription) ? theSite.MetaDescription : cp.MetaDescription;
					this.Controls.Add(metaSub);

					HtmlMeta metaURL = new HtmlMeta();
					metaURL.Attributes["property"] = "og:url";
					metaURL.Content = theSite.DefaultCanonicalURL;
					this.Controls.Add(metaURL);

					HtmlMeta metaType = new HtmlMeta();
					metaType.Attributes["property"] = "og:type";
					if (this.OpenGraphType == OpenGraphTypeDef.Default) {
						if (cp.ContentType == ContentPageType.PageType.BlogEntry) {
							metaType.Content = OpenGraphTypeDef.Blog.ToString().ToLowerInvariant();
						} else {
							metaType.Content = OpenGraphTypeDef.Article.ToString().ToLowerInvariant();
						}
						if (theSite.Blog_Root_ContentID.HasValue && cp.Root_ContentID == theSite.Blog_Root_ContentID) {
							metaType.Content = OpenGraphTypeDef.Website.ToString().ToLowerInvariant();
						}
					} else {
						metaType.Content = this.OpenGraphType.ToString().ToLowerInvariant();
					}

					this.Controls.Add(metaType);

					if (!String.IsNullOrEmpty(this.Page.Title)) {
						HtmlMeta metaTitle = new HtmlMeta();
						metaTitle.Attributes["property"] = "og:title";
						metaTitle.Content = cp.TitleBar;
						this.Controls.Add(metaTitle);
					}

					if (!String.IsNullOrEmpty(cp.Thumbnail)) {
						HtmlMeta metaTitle = new HtmlMeta();
						metaTitle.Attributes["property"] = "og:image";
						metaTitle.Content = String.Format("{0}/{1}", theSite.MainCanonicalURL, cp.Thumbnail).Replace(@"//", @"/").Replace(@"//", @"/").Replace(@":/", @"://");
						this.Controls.Add(metaTitle);
					}

					if (!String.IsNullOrEmpty(theSite.SiteName)) {
						HtmlMeta metaSite = new HtmlMeta();
						metaSite.Attributes["property"] = "og:site_name";
						metaSite.Content = theSite.SiteName;
						this.Controls.Add(metaSite);
					}

					HtmlMeta metaPubDate = new HtmlMeta();
					metaPubDate.Attributes["property"] = "article:published_time";
					metaPubDate.Content = theSite.ConvertSiteTimeToISO8601(cp.GoLiveDate);
					this.Controls.Add(metaPubDate);

					HtmlMeta metaUpdateDate = new HtmlMeta();
					metaUpdateDate.Attributes["property"] = "article:modified_time";
					metaUpdateDate.Content = theSite.ConvertSiteTimeToISO8601(cp.EditDate);
					this.Controls.Add(metaUpdateDate);

					if (ShowExpirationDate) {
						HtmlMeta metaExpireDate = new HtmlMeta();
						metaExpireDate.Attributes["property"] = "article:expiration_time";
						metaExpireDate.Content = theSite.ConvertSiteTimeToISO8601(cp.RetireDate);
						this.Controls.Add(metaExpireDate);
					}
				}
			} catch (Exception ex) {
			}

			base.OnPreRender(e);
		}
	}
}