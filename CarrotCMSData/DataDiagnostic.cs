﻿using System;
using System.Data;
using System.Diagnostics;

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

	public class DataDiagnostic {

		public DataDiagnostic() { }

		private int iDBCounter = -1;

		public DataDiagnostic(CarrotCMSDataContext db) {
			db.Connection.StateChange += DBContextChange;
			db.Log = new DebugTextWriter();
		}

		public DataDiagnostic(CarrotCMSDataContext db, int iCtr) {
			iDBCounter = iCtr;
			db.Connection.StateChange += DBContextChange;
			db.Log = new DebugTextWriter();
		}

		private Stopwatch ThisWatch = new Stopwatch();

		private void DBContextChange(object sender, StateChangeEventArgs e) {
			if (e.OriginalState == ConnectionState.Closed && e.CurrentState == ConnectionState.Open) {
				Debug.WriteLine(iDBCounter + " ================ " + DateTime.UtcNow.ToString() + " ================");
				Debug.WriteLine(iDBCounter + " ~~~~~~~~~~~~~~~~ OPEN ~~~~~~~~~~~~~~~~~~");
				ThisWatch.Reset();
				ThisWatch.Start();
			} else if (e.OriginalState == ConnectionState.Open && e.CurrentState == ConnectionState.Closed) {
				ThisWatch.Stop();
				Debug.WriteLine(string.Format("\t SQL took {0}ms   \r\n", ThisWatch.ElapsedMilliseconds));
				Debug.WriteLine(iDBCounter + " ~~~~~~~~~~~~~~~~ CLOSE ~~~~~~~~~~~~~~~~~~");
			}
		}
	}
}