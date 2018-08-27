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
    /// This file holds all user defined strategy classes.
    /// </summary>
 
	//public enum SessionBreak {AfternoonClose, EveningOpen, MorningOpen, NextDay};

    public class TradeCommand {
		private int algo_mode = 1;
		private string AccName = null;
		
		private int tradeDirection = 0; // -1=short; 0-both; 1=long;
		private int tradeStyle = 0; // -1=counter trend; 1=trend following;
		private bool backTest = true; //if it runs for backtesting;
		
		public TradeCommand () {
		}
		
        #region Properties
		

		
//        [Description("ZigZag retrace points")]
//        [GridCategory("Parameters")]
//        public double RetracePnts
//        {
//            get { return retracePnts; }
//            set { retracePnts = Math.Max(4, value); }
//        }
/*
		[Description("Algo mode")]
        [GridCategory("Parameters")]
        public int AlgoMode
        {
            get { return algo_mode; }
            set { algo_mode = value; }
        }
		
		[Description("Supervised PR Bits")]
        [GridCategory("Parameters")]
        public int SpvPRBits
        {
            get { return spvPRBits; }
            set { spvPRBits = Math.Max(0, value); }
        }		
		*/
//		[Description("ZigZag retrace points")]
//        [GridCategory("Parameters")]
//        public double RetracePnts
//        {
//            get { return retracePnts; }
//            set { retracePnts = Math.Max(1, value); }
//        }


/*		
        [Description("Time start hour")]
        [GridCategory("Parameters")]
        public int TimeStartH
        {
            get { return timeStartH; }
            set { timeStartH = Math.Max(0, value); }
        }
		
        [Description("Time start minute")]
        [GridCategory("Parameters")]
        public int TimeStartM
        {
            get { return timeStartM; }
            set { timeStartM = Math.Max(0, value); }
        }
		
        [Description("Time end hour")]
        [GridCategory("Parameters")]
        public int TimeEndH
        {
            get { return timeEndH; }
            set { timeEndH = Math.Max(0, value); }
        }

        [Description("Time end minute")]
        [GridCategory("Parameters")]
        public int TimeEndM
        {
            get { return timeEndM; }
            set { timeEndM = Math.Max(0, value); }
        }
		
        [Description("How long to check entry order filled or not")]
        [GridCategory("Parameters")]
        public int MinutesChkEnOrder
        {
            get { return minutesChkEnOrder; }
            set { minutesChkEnOrder = Math.Max(0, value); }
        }
		
        [Description("How long to check P&L")]
        [GridCategory("Parameters")]
        public int MinutesChkPnL
        {
            get { return minutesChkPnL; }
            set { minutesChkPnL = Math.Max(-1, value); }
        }		

        [Description("Bar count since en order issued")]
        [GridCategory("Parameters")]
        public int BarsHoldEnOrd
        {
            get { return barsHoldEnOrd; }
            set { barsHoldEnOrd = Math.Max(1, value); }
        }
		
        [Description("Bar count for en order counter pullback")]
        [GridCategory("Parameters")]
        public int EnCounterPullBackBars
        {
            get { return enCounterPBBars; }
            set { enCounterPBBars = Math.Max(-1, value); }
        }		
				
		[Description("Bar count since last filled PT or SL")]
        [GridCategory("Parameters")]
        public int BarsSincePtSl
        {
            get { return barsSincePtSl; }
            set { barsSincePtSl = Math.Max(1, value); }
        }
		
		[Description("Bar count before checking P&L")]
        [GridCategory("Parameters")]
        public int BarsToCheckPL
        {
            get { return barsToCheckPL; }
            set { barsToCheckPL = Math.Max(1, value); }
        }

		[Description("Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed")]
        [GridCategory("Parameters")]
        public int BarsAgoMaxPbSAREn
        {
            get { return barsAgoMaxPbSAREn; }
            set { barsAgoMaxPbSAREn = Math.Max(1, value); }
        }

		[Description("Bars count for last PbSAR swing. Enter the maximum bars count of last PbSAR allowed for entry")]
        [GridCategory("Parameters")]
        public int BarsMaxLastCross
        {
            get { return barsMaxLastCross; }
            set { barsMaxLastCross = Math.Max(1, value); }
        }		

        [Description("Min swing size for entry")]
        [GridCategory("Parameters")]
        public double EnSwingMinPnts
        {
            get { return enSwingMinPnts; }
            set { enSwingMinPnts = Math.Max(1, value); }
        }

        [Description("Max swing size for entry")]
        [GridCategory("Parameters")]
        public double EnSwingMaxPnts
        {
            get { return enSwingMaxPnts; }
            set { enSwingMaxPnts = Math.Max(4, value); }
        }

		[Description("Min pullback size for entry")]
        [GridCategory("Parameters")]
        public double EnPullbackMinPnts
        {
            get { return enPullbackMinPnts; }
            set { enPullbackMinPnts = Math.Max(1, value); }
        }

        [Description("Max pullback size for entry")]
        [GridCategory("Parameters")]
        public double EnPullbackMaxPnts
        {
            get { return enPullbackMaxPnts; }
            set { enPullbackMaxPnts = Math.Max(2, value); }
        }
		
        [Description("Offeset points for limit price entry")]
        [GridCategory("Parameters")]
        public double EnOffsetPnts
        {
            get { return enOffsetPnts; }
            set { enOffsetPnts = Math.Max(0, value); }
        }
*/		
//        [Description("Offeset points for limit price entry, pullback entry")]
//        [GridCategory("Parameters")]
//        public double EnOffset2Pnts
//        {
//            get { return enOffset2Pnts; }
//            set { enOffset2Pnts = Math.Max(0, value); }
//        }
/*		
		[Description("Use trailing entry every bar")]
        [GridCategory("Parameters")]
        public bool EnTrailing
        {
            get { return enTrailing; }
            set { enTrailing = value; }
        }
		
		[Description("Use trailing profit target every bar")]
        [GridCategory("Parameters")]
        public bool PTTrailing
        {
            get { return ptTrailing; }
            set { ptTrailing = value; }
        }
		
		[Description("Use trailing stop loss every bar")]
        [GridCategory("Parameters")]
        public bool SLTrailing
        {
            get { return slTrailing; }
            set { slTrailing = value; }
        }
		
        [Description("Short, Long or both direction for entry")]
        [GridCategory("Parameters")]
        public int TradeDirection
        {
            get { return tradeDirection; }
            set { tradeDirection = value; }
        }		

        [Description("Trade style: trend following, counter trend, scalp")]
        [GridCategory("Parameters")]
        public int TradeStyle
        {
            get { return tradeStyle; }
            set { tradeStyle = value; }
        }
		
		[Description("If it runs for backtesting")]
        [GridCategory("Parameters")]
        public bool BackTest
        {
            get { return backTest; }
            set { backTest = value; }
        }
		
		[Description("Print out level: large # print out more")]
        [GridCategory("Parameters")]
        public int PrintOut
        {
            get { return printOut; }
            set { printOut = Math.Max(-1, value); }
        }
*/		
        #endregion		
	}
}