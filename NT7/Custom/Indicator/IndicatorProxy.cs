#region Using declarations
using System;
using System.ComponentModel;
using System.IO;
using System.Drawing;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// This file holds logging class.
    /// </summary>
	
	/// <summary>
	/// 
	/// </summary>
	public class IndicatorProxy : Indicator {
		//private string log_file = ""; //
		
		public IndicatorProxy(string acc_name, string symbol) {
			log_file = GetFileNameByDateTime(DateTime.Now, @"C:\inetpub\wwwroot\nt_files\log\", acc_name, symbol, "log");
		}

        protected override void Initialize()
        {
		}
		
        protected override void OnBarUpdate()
        {
		}
		
		public void PrintLog(bool prt_con, bool prt_file, string text) {
			PrintLog(prt_con, prt_file, log_file, text);
		}		
	}
}
