#region Using declarations
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;

using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// This file holds all user defined strategy methods.
    /// </summary>
 
	//public enum SessionBreak {AfternoonClose, EveningOpen, MorningOpen, NextDay};
	
	partial class Strategy
    {
		protected string AccName = "";
		protected int printOut = 1; // Default setting for PrintOut
		private int algo_mode = 1;
		
		private int tradeDirection = 0; // -1=short; 0-both; 1=long;
		private int tradeStyle = 0; // -1=counter trend; 1=trend following;
		private bool backTest = true; //if it runs for backtesting;
		
		//time=H*10000+M*100+S, S is skipped here;
        private int timeStartH = 1; //10100 Default setting for timeStart hour
		private int timeStartM = 1; //10100 Default setting for timeStart minute
		private int timeStart = -1; //10100 Default setting for timeStart
        private int timeEndH = 14; // Default setting for timeEnd hour
		private int timeEndM = 59; // Default setting for timeEnd minute
		private int timeEnd = -1; // Default setting for timeEnd


		

		
		protected bool PatternMatched()
		{
			//Print("CurrentBar, barsMaxLastCross, barsAgoMaxPbSAREn,=" + CurrentBar + "," + barsAgoMaxPbSAREn + "," + barsSinceLastCross);
//			if (giParabSAR.IsSpvAllowed4PAT(curBarPriceAction.paType) && barsSinceLastCross < barsAgoMaxPbSAREn) 
//				return true;
//			else return false;
			return false;
			//barsAgoMaxPbSAREn Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed
		}
		
        #region Properties

        #endregion		
	

    }
}
