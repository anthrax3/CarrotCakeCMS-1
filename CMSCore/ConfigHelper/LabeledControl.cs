﻿using System;
using System.Web.UI;

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

	public class LabeledControl {

		public LabeledControl() { }

		public string ControlLabel { get; set; }
		public Control KeyControl { get; set; }

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is LabeledControl) {
				LabeledControl p = (LabeledControl)obj;
				return (ControlLabel.ToLowerInvariant() == p.ControlLabel.ToLowerInvariant());
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return ControlLabel.GetHashCode() ^ KeyControl.GetHashCode();
		}
	}
}