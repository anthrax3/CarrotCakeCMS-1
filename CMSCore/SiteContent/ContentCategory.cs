﻿using System;
using System.Collections.Generic;
using System.Linq;
using Carrotware.CMS.Data;

/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: October 2011
*/

namespace Carrotware.CMS.Core {

	public class ContentCategory : IContentMetaInfo {

		public ContentCategory() { }

		public Guid ContentCategoryID { get; set; }
		public Guid SiteID { get; set; }
		public string CategoryText { get; set; }
		public string CategorySlug { get; set; }
		public string CategoryURL { get; set; }
		public int? UseCount { get; set; }
		public int? PublicUseCount { get; set; }
		public bool IsPublic { get; set; }
		public DateTime? EditDate { get; set; }

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;

			if (obj is ContentCategory) {
				ContentCategory p = (ContentCategory)obj;
				return (this.SiteID == p.SiteID && this.ContentCategoryID == p.ContentCategoryID);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return this.ContentCategoryID.GetHashCode() ^ this.SiteID.GetHashCode();
		}

		internal ContentCategory(vw_carrot_CategoryCounted c) {
			if (c != null) {
				this.ContentCategoryID = c.ContentCategoryID;
				this.SiteID = c.SiteID;
				this.CategorySlug = ContentPageHelper.ScrubSlug(c.CategorySlug);
				this.CategoryText = c.CategoryText;
				this.UseCount = c.UseCount;
				this.PublicUseCount = 1;
				this.IsPublic = c.IsPublic;

				SiteData site = SiteData.GetSiteFromCache(c.SiteID);
				if (site != null) {
					this.CategoryURL = ContentPageHelper.ScrubFilename(c.ContentCategoryID, String.Format("/{0}/{1}.aspx", site.BlogCategoryPath, c.CategorySlug.Trim()));
				}
			}
		}

		internal ContentCategory(vw_carrot_CategoryURL c) {
			if (c != null) {
				SiteData site = SiteData.GetSiteFromCache(c.SiteID);

				this.ContentCategoryID = c.ContentCategoryID;
				this.SiteID = c.SiteID;
				this.CategoryURL = ContentPageHelper.ScrubFilename(c.ContentCategoryID, c.CategoryUrl);
				this.CategoryText = c.CategoryText;
				this.UseCount = c.UseCount;
				this.PublicUseCount = c.PublicUseCount;
				this.IsPublic = c.IsPublic;

				if (c.EditDate.HasValue) {
					this.EditDate = site.ConvertUTCToSiteTime(c.EditDate.Value);
				}
			}
		}

		internal ContentCategory(carrot_ContentCategory c) {
			if (c != null) {
				this.ContentCategoryID = c.ContentCategoryID;
				this.SiteID = c.SiteID;
				this.CategorySlug = ContentPageHelper.ScrubSlug(c.CategorySlug);
				this.CategoryText = c.CategoryText;
				this.IsPublic = c.IsPublic;
				this.UseCount = 1;
				this.PublicUseCount = 1;

				SiteData site = SiteData.GetSiteFromCache(c.SiteID);
				if (site != null) {
					this.CategoryURL = ContentPageHelper.ScrubFilename(c.ContentCategoryID, String.Format("/{0}/{1}.aspx", site.BlogCategoryPath, c.CategorySlug.Trim()));
				}
			}
		}

		public static ContentCategory Get(Guid CategoryID) {
			ContentCategory _item = null;
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				carrot_ContentCategory query = CompiledQueries.cqGetContentCategoryByID(_db, CategoryID);
				if (query != null) {
					_item = new ContentCategory(query);
				}
			}

			return _item;
		}

		public static ContentCategory GetByURL(Guid SiteID, string requestedURL) {
			ContentCategory _item = null;
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				vw_carrot_CategoryURL query = CompiledQueries.cqGetContentCategoryByURL(_db, SiteID, requestedURL);
				if (query != null) {
					_item = new ContentCategory(query);
				}
			}

			return _item;
		}

		public static int GetSimilar(Guid SiteID, Guid CategoryID, string categorySlug) {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				IQueryable<carrot_ContentCategory> query = CompiledQueries.cqGetContentCategoryNoMatch(_db, SiteID, CategoryID, categorySlug);

				return query.Count();
			}
		}

		public static int GetSiteCount(Guid siteID) {
			int iCt = -1;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				iCt = CompiledQueries.cqGetContentCategoryCountBySiteID(_db, siteID);
			}

			return iCt;
		}

		public static List<ContentCategory> BuildCategoryList(Guid rootContentID) {
			List<ContentCategory> _types = null;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				IQueryable<carrot_ContentCategory> query = CompiledQueries.cqGetContentCategoryByContentID(_db, rootContentID);

				_types = (from d in query.ToList()
						  select new ContentCategory(d)).ToList();
			}

			return _types;
		}

		public void Save() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				bool bNew = false;
				carrot_ContentCategory s = CompiledQueries.cqGetContentCategoryByID(_db, this.ContentCategoryID);

				if (s == null || (s != null && s.ContentCategoryID == Guid.Empty)) {
					s = new carrot_ContentCategory();
					s.ContentCategoryID = Guid.NewGuid();
					s.SiteID = this.SiteID;
					bNew = true;
				}

				s.CategorySlug = ContentPageHelper.ScrubSlug(this.CategorySlug);
				s.CategoryText = this.CategoryText;
				s.IsPublic = this.IsPublic;

				if (bNew) {
					_db.carrot_ContentCategories.InsertOnSubmit(s);
				}

				_db.SubmitChanges();

				this.ContentCategoryID = s.ContentCategoryID;
			}
		}

		public void Delete() {
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				carrot_ContentCategory s = CompiledQueries.cqGetContentCategoryByID(_db, this.ContentCategoryID);

				if (s != null) {
					IQueryable<carrot_CategoryContentMapping> lst = (from m in _db.carrot_CategoryContentMappings
																	 where m.ContentCategoryID == s.ContentCategoryID
																	 select m);

					_db.carrot_CategoryContentMappings.BatchDelete(lst);
					_db.carrot_ContentCategories.DeleteOnSubmit(s);
					_db.SubmitChanges();
				}
			}
		}

		#region IContentMetaInfo Members

		public void SetValue(Guid ContentMetaInfoID) {
			this.ContentCategoryID = ContentMetaInfoID;
		}

		public Guid ContentMetaInfoID {
			get { return this.ContentCategoryID; }
		}

		public string MetaInfoText {
			get { return this.CategoryText; }
		}

		public string MetaInfoURL {
			get { return this.CategoryURL; }
		}

		public bool MetaIsPublic {
			get { return this.IsPublic; }
		}

		public DateTime? MetaDataDate {
			get { return this.EditDate; }
		}

		public int MetaInfoCount {
			get { return this.UseCount == null ? 0 : Convert.ToInt32(this.UseCount); }
		}

		public int MetaPublicInfoCount {
			get { return this.PublicUseCount == null ? 0 : Convert.ToInt32(this.PublicUseCount); }
		}

		#endregion IContentMetaInfo Members
	}
}