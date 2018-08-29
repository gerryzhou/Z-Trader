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
	public class Trigger {
		private Strategy instStrategy = null;
		public Trigger(Strategy inst_strategy) {
			this.instStrategy = inst_strategy;
		}
	}
	
	partial class Strategy
    {
		protected string accName = "";
		protected int algoMode = 1;
		protected bool backTest = true; //if it runs for backtesting;
		protected int printOut = 1; // Default setting for PrintOut
		
		protected int tradeDirection = 0; // -1=short; 0-both; 1=long;
		protected int tradeStyle = 0; // -1=counter trend; 1=trend following;
		
		//time=H*10000+M*100+S, S is skipped here;
        protected int timeStartH = 1; //10100 Default setting for timeStart hour
		protected int timeStartM = 1; //10100 Default setting for timeStart minute
		protected int timeStart = -1; //10100 Default setting for timeStart
        protected int timeEndH = 14; // Default setting for timeEnd hour
		protected int timeEndM = 59; // Default setting for timeEnd minute
		protected int timeEnd = -1; // Default setting for timeEnd
		
        protected double enSwingMinPnts = 10; //10 Default setting for EnSwingMinPnts
        protected double enSwingMaxPnts = 35; //16 Default setting for EnSwingMaxPnts
		protected double enPullbackMinPnts = 1; //6 Default setting for EnPullbackMinPnts
        protected double enPullbackMaxPnts = 8; //10 Default setting for EnPullbackMaxPnts
		
		/// <summary>
		/// Check if now is the time allowed to put trade
		/// </summary>
		/// <param name="time_start">start time</param>
		/// <param name="time_end">end time</param>
		/// <param name="session_start">the overnight session start time: 170000 for ES</param>
		/// <returns></returns>
		public bool IsTradingTime(int time_start, int time_end, int session_start) {
			int time_now = ToTime(Time[0]);
			bool isTime= false;
			if(time_start >= session_start) {
				if(time_now >= time_start || time_now <= time_end)
					isTime = true;
			}
			else if (time_now >= time_start && time_now <= time_end) {
				isTime = true;
			}
			return isTime;
		}		
		
		protected virtual void SetTradeContext(PriceAction pa) {
			switch(pa.paType) {
				case PriceActionType.UpTight: //
					tradeStyle = 1;
					tradeDirection = 1;
					break;
				case PriceActionType.UpWide: //wide up channel
					tradeStyle = 2;
					tradeDirection = 1;
					break;
				case PriceActionType.DnTight: //
					tradeStyle = 1;
					tradeDirection = -1;
					break;
				case PriceActionType.DnWide: //wide dn channel
					tradeStyle = 2;
					tradeDirection = -1;
					break;
				case PriceActionType.RngTight: //
					tradeStyle = -1;
					tradeDirection = 0;
					break;
				case PriceActionType.RngWide: //
					tradeStyle = 2;
					tradeDirection = 1;
					break;
				default:
					tradeStyle = 1;
					tradeDirection = 0;
					break;
			}
		}
		
		protected virtual bool PatternMatched()
		{
			//Print("CurrentBar, barsMaxLastCross, barsAgoMaxPbSAREn,=" + CurrentBar + "," + barsAgoMaxPbSAREn + "," + barsSinceLastCross);
//			if (giParabSAR.IsSpvAllowed4PAT(curBarPriceAction.paType) && barsSinceLastCross < barsAgoMaxPbSAREn) 
//				return true;
//			else return false;
			return false;
			//barsAgoMaxPbSAREn Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed
		}
		
		/// <summary>
		/// Check the first reversal bar for the pullback under current ZigZag gap
		/// </summary>
		/// <param name="cur_gap">current ZigZag gap</param>
		/// <param name="tick_size">tick size of the symbol</param>
		/// <param name="n_bars">bar count with the pullback prior to the last reversal bar</param>
		/// <returns>is TBR or not</returns>
		public bool IsTwoBarReversal(double cur_gap, double tick_size, int n_bars) {
			bool isTBR= false;
			
			if(n_bars < 0) return isTBR;
			
			if(cur_gap > 0)
			{
				//Check if the last n_bars are pullback bars (bear bar)
				for(int i=1; i<=n_bars; i++)
				{
					if(Close[i] > Open[i])
						return isTBR;
				}
				if(Close[0]-Open[0] > tick_size && Open[1]-Close[1] >= tick_size) {
					isTBR= true;
				}
			}
			else if(cur_gap < 0)
			{
				//Check if the last n_bars are pullback bars (bull bar)
				for(int i=1; i<=n_bars; i++)
				{
					if(Close[i] < Open[i])
						return isTBR;
				}
				if(Open[0] - Close[0] > tick_size && Close[1] - Open[1] >= tick_size) {
					isTBR= true;
				}
			}
			return isTBR;
		}
		
		/// <summary>
		/// Get the Two Bar Reversal count for the past barsBack
		/// </summary>
		/// <param name="cur_gap">current ZigZag gap</param>
		/// <param name="tick_size">tick size of the symbol</param>
		/// <param name="barsBack">bar count to look back</param>
		/// <returns>pairs count for TBR during the barsBack</returns>
		public int GetTBRPairsCount(double cur_gap, double tick_size, int barsBack) {
			int tbr_count = 0;
			for(int i=0; i<barsBack; i++) {
				if(cur_gap > 0 && Close[i]-Open[i] > tick_size && Open[i+1]-Close[i+1] >= tick_size) {
					tbr_count++;
				}
				if(cur_gap < 0 && Open[i] - Close[i] > tick_size && Close[i+1] - Open[i+1] >= tick_size) {
					tbr_count++;
				}
			}
			return tbr_count;
		}
		
        #region TG Properties
		[Description("Account Name")]
        [GridCategory("Parameters")]
        public string TG_AccName
        {
            get { return accName; }
            set { accName = value; }
        }
		
		[Description("Algo mode")]
        [GridCategory("Parameters")]
        public int TG_AlgoMode
        {
            get { return algoMode; }
            set { algoMode = value; }
        }
		
		[Description("If it runs for backtesting")]
        [GridCategory("Parameters")]
        public bool TG_BackTest
        {
            get { return backTest; }
            set { backTest = value; }
        }
		
		[Description("Print out level: large # print out more")]
        [GridCategory("Parameters")]
        public int TG_PrintOut
        {
            get { return printOut; }
            set { printOut = Math.Max(-1, value); }
        }
		
        [Description("Short, Long or both direction for entry")]
        [GridCategory("Parameters")]
        public int TG_TradeDirection
        {
            get { return tradeDirection; }
            set { tradeDirection = value; }
        }		

        [Description("Trade style: trend following, counter trend, scalp")]
        [GridCategory("Parameters")]
        public int TG_TradeStyle
        {
            get { return tradeStyle; }
            set { tradeStyle = value; }
        }
		
        [Description("Min swing size for entry")]
        [GridCategory("Parameters")]
        public double TG_EnSwingMinPnts
        {
            get { return enSwingMinPnts; }
            set { enSwingMinPnts = Math.Max(1, value); }
        }

        [Description("Max swing size for entry")]
        [GridCategory("Parameters")]
        public double TG_EnSwingMaxPnts
        {
            get { return enSwingMaxPnts; }
            set { enSwingMaxPnts = Math.Max(4, value); }
        }

		[Description("Min pullback size for entry")]
        [GridCategory("Parameters")]
        public double TG_EnPullbackMinPnts
        {
            get { return enPullbackMinPnts; }
            set { enPullbackMinPnts = Math.Max(1, value); }
        }

        [Description("Max pullback size for entry")]
        [GridCategory("Parameters")]
        public double TG_EnPullbackMaxPnts
        {
            get { return enPullbackMaxPnts; }
            set { enPullbackMaxPnts = Math.Max(2, value); }
        }

        [Description("Time start hour")]
        [GridCategory("Parameters")]
        public int TG_TimeStartH
        {
            get { return timeStartH; }
            set { timeStartH = Math.Max(0, value); }
        }
		
        [Description("Time start minute")]
        [GridCategory("Parameters")]
        public int TG_TimeStartM
        {
            get { return timeStartM; }
            set { timeStartM = Math.Max(0, value); }
        }
		
        [Description("Time end hour")]
        [GridCategory("Parameters")]
        public int TG_TimeEndH
        {
            get { return timeEndH; }
            set { timeEndH = Math.Max(0, value); }
        }

        [Description("Time end minute")]
        [GridCategory("Parameters")]
        public int TG_TimeEndM
        {
            get { return timeEndM; }
            set { timeEndM = Math.Max(0, value); }
        }
		
        #endregion
    }
}
