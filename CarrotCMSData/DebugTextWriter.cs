﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;

/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: October 2011
*/

namespace Carrotware.CMS.Data {

	public class DebugTextWriter : TextWriter {

		public override void Write(char[] buffer, int index, int count) {
			//Debug.Write("--------------------------------------\r\n");
			Debug.Write(new String(buffer, index, count));
		}

		public override void Write(string value) {
			//Debug.Write("--------------------------------------\r\n");
			Debug.Write(value);
		}

		public override Encoding Encoding {
			get { return Encoding.Default; }
		}
	}
}