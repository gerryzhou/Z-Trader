#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Reflection;

using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;

#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// Enter the description of your strategy here
	/// Two questions: 
	/// 1) the volality of the current market
	/// 2) counter swing/trend or follow swing/trend? (Trending, Reversal or Range)
    /// </summary>
    [Description("Enter the description of your strategy here")]
    public class GSpbSARBase : Strategy
    {		
        #region Variables
		
		private Trigger trigger = null;
		private MarketContext mktContext = null;
		private GIParabolicSAR giParabSAR = null;//new GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan);
		//private Logger logger = null;
		
		private double afAcc = 0.002; // Default setting for AfAcc
        private double afLmt = 0.2; // Default setting for AfLmt
		
		private int barsSinceLastCross = -1;
		private PriceAction curBarPriceAction = new PriceAction(PriceActionType.UnKnown, -1,-1,-1,-1);//PriceAtionType of current bar
		
		private int spvPRBits = 42;//39,63
		#endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
			trigger = new Trigger(this);
			tradeObj = new TradeObj(this);
			indicatorProxy = IndicatorProxy(11,5,9,5);
			Add(indicatorProxy);
			
			TG_AccName = indicatorProxy.GetAccName();//.GetTsTAccName(Account.Name);
			indicatorProxy = IndicatorProxy(timeEndH,timeEndM,timeStartH,timeStartM);//accName, Instrument.FullName);
			Add(indicatorProxy);
			
			giParabSAR = GIParabolicSAR(afAcc, afLmt, afAcc, TG_AccName, backTest, Color.Cyan);
			Add(giParabSAR);
			giParabSAR.SpvPRBits = spvPRBits;
			
			//timeStart = indicatorProxy.GetTimeByHM(timeStartH, timeStartM);
			//timeEnd = indicatorProxy.GetTimeByHM(timeEndH, timeEndM);
			//Add(GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan));
			//Add(GIParabolicSAR(0.001, 0.2, 0.001, Color.Orange));
//			EMA(High, 50).Plots[0].Pen.Color = Color.Orange;
//			EMA(Low, 50).Plots[0].Pen.Color = Color.Green;
//
//            Add(EMA(High, 50));
//            Add(EMA(Low, 50));
			
            SetProfitTarget(300);
            SetStopLoss(150, false);
			
			SetInitParams();
			//if(!backTest)
        }

        protected override void OnBarUpdate()
        {
			//Print("-------------" + CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GIParabolicSAR=" + giParabSAR[0] + "-------------");
			//Print("-------------" + CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GIParabolicSAR=" + GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan)[0] + "-------------");
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetAf=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetAf());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetAfIncreased=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetAfIncreased());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetLongPosition=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetLongPosition());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetTodaySAR=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetTodaySAR());
			double zz_gap = giParabSAR.GetCurZZGap(); //GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Orange).GetCurZZGap();
			bool isReversalBar = giParabSAR.IsReversalBar();//GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Orange).IsReversalBar();
			double cur_gap = giParabSAR.GetCurGap();
			curBarPriceAction = giParabSAR.getCurPriceAction();
			barsSinceLastCross = giParabSAR.GetpbSARCrossBarsAgo();
			if(isReversalBar) {				
				SetTradeContext(curBarPriceAction);
				//Print("-------------" + CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GIParabolicSAR=" + GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan)[0] + "-------------");
				if(printOut > 3)
					indicatorProxy.PrintLog(true, !backTest, CurrentBar + "-" + indicatorProxy.Get24HDateTime(Time[0]) + "-GetCurZZGap,GetCurGap,isReversalBar=" + zz_gap + "," + cur_gap + "," + isReversalBar + ", getCurPriceActType=" + curBarPriceAction.ToString() + ", barsSinceLastCross=" + barsSinceLastCross);//GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetCurZZGap());
			}
			
			if (giParabSAR[0] > 0) //GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Orange)[0] > 0)
            {
                DrawLine("My line" + CurrentBar, 0, 0, 0, 0, Color.Blue);
                //EnterLongLimit(DefaultQuantity, 0, "enLn");
            }			
			//Print("indicatorProxy.Strategy.Account=" + indicatorProxy.Strategy.Account);
			CheckPerformance();
			//double gap = GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).GetCurZZGap();
			//bool isReversalBar = true;//CurrentBar>BarsRequired?false:GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).IsReversalBar();
			switch(algoMode) {
				case 0: //liquidate
					CloseAllPositions();
					break;
				case 1: //trading
					ChangeSLPT();
					CheckEnOrder(zz_gap);
								
					if(NewOrderAllowed() && PatternMatched())
					{
						indicatorProxy.PrintLog(true, !backTest, "----------------PutTrade, isReversalBar=" + isReversalBar + ",giParabSAR.IsSpvAllowed4PAT(curBarPat)=" + giParabSAR.IsSpvAllowed4PAT(curBarPriceAction.paType));
						PutTrade(zz_gap, cur_gap, isReversalBar);
					}
					break;
				case 2: //cancel order
					CancelAllOrders();
					break;
				case -1: //stop trading
					//indicatorProxy.PrintLog(true, !backTest, log_file, CurrentBar + "- Stop trading cmd:" + Get24HDateTime(Time[0]));
					break;
			}			
        }

		#region	Trade Functions

		protected void PutTrade(double zzGap, double curGap, bool isRevBar) {
			double zzGapAbs = Math.Abs(zzGap);
			double curGapAbs = Math.Abs(curGap);
			//double todaySAR = giParabSAR.GetTodaySAR();		
			//double prevSAR = giParabSAR.GetPrevSAR();		
			//int reverseBar = giParabSAR.GetReverseBar();		
			int last_reverseBar = giParabSAR.GetLastReverseBar(CurrentBar);		
			//double reverseValue = giParabSAR.GetReverseValue();
		
			if(printOut > 1) {
				string logText = CurrentBar + "-" + TG_AccName
				//":PutOrder-(curGap,todaySAR,prevSAR,zzGap,reverseBar,last_reverseBar,reverseValue)= " 
				//+ curGap + "," + todaySAR + "," + prevSAR + "," + zzGap + "," + reverseBar + "," + last_reverseBar + "," + reverseValue ;
				+ ":PutOrder-(curGap,zzGap,last_reverseBar)= " 
				+ curGap + "," + zzGap + "," + last_reverseBar ;
				indicatorProxy.PrintLog(true, !backTest, logText);
			}
			
//			double lastZZ = GetLastZZ();
//			double lastZZAbs = Math.Abs(lastZZ);
//			if(entryOrder == null)
				//indicatorProxy.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":PutOrder-(tradeStyle,tradeDirection,gap,enSwingMinPnts,enSwingMaxPnts,enPullbackMinPnts,enPullbackMaxPnts, entryOrder)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + enSwingMinPnts + "," + enSwingMaxPnts + "," + enPullbackMinPnts + "," + enPullbackMaxPnts + "--null");
//			else
				//indicatorProxy.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":PutOrder-(tradeStyle,tradeDirection,gap,enSwingMinPnts,enSwingMaxPnts,enPullbackMinPnts,enPullbackMaxPnts, entryOrder)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + enSwingMinPnts + "," + enSwingMaxPnts + "," + enPullbackMinPnts + "," + enPullbackMaxPnts + "--" + entryOrder.ToString());
			if(tradeStyle == 0) // scalping, counter trade the pullbackMinPnts
			{
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
					if(curGapAbs >= enPullbackMinPnts && curGapAbs < enPullbackMaxPnts)
						NewLongLimitOrder("scalping long", zzGap, curGap);
				}
				 
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
//					if(gap < 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts)
					if(curGapAbs >= enPullbackMinPnts && curGapAbs < enPullbackMaxPnts)
						NewShortLimitOrder("scalping short", zzGap, curGap);
				}
			}
			else if(tradeStyle < 0) //counter trend trade, , counter trade the swingMinPnts
			{
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
//					if(gap < 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
					if(curGapAbs >= enPullbackMinPnts && curGapAbs < enPullbackMaxPnts)
						NewLongLimitOrder("counter trade long", zzGap, curGap);
				}
				
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
//					if(gap > 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
					if(curGapAbs >= enPullbackMinPnts && curGapAbs < enPullbackMaxPnts)
						NewShortLimitOrder("counter trade short", zzGap, curGap);
				}
			}
			// tradeStyle > 0, trend following, tradeStyle=1:entry at breakout; tradeStyle=2:entry at pullback;
			else if(tradeStyle == 1) //entry at breakout
			{
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
//					if(gap > 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
//						if(enCounterPBBars < 0 || IsTwoBarReversal(gap, TickSize, enCounterPBBars))
					if(curGapAbs <= enPullbackMinPnts)
							NewLongLimitOrder("trend follow long entry at breakout", zzGap, curGap);
				}
				
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
//					if(gap < 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
//						if(enCounterPBBars < 0 || IsTwoBarReversal(gap, TickSize, enCounterPBBars))
					if(curGapAbs <= enPullbackMinPnts)
							NewShortLimitOrder("trend follow short entry at breakout", zzGap, curGap);
				}
			}
			else if(tradeStyle == 2) //entry at pullback, wide channel: UpWide, DnWide, RngWide
			{
				//indicatorProxy.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":PutOrder(tradeStyle,tradeDirection,gap,lastZZs[0],lastZZAbs,enPullbackMinPnts,enPullbackMaxPnts)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + lastZZ + "," + lastZZAbs + "," + enPullbackMinPnts + "," + enPullbackMaxPnts);
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
//					if(gap < 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts)
					if(curGapAbs >= enPullbackMinPnts && curGapAbs < enPullbackMaxPnts)
						NewLongLimitOrder("trend follow long entry at pullback", zzGap, curGap);
				}
				
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
//					if(gap > 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts)
					if(curGapAbs >= enPullbackMinPnts && curGapAbs < enPullbackMaxPnts)
						NewShortLimitOrder("trend follow short entry at pullback", zzGap, curGap);
				}
			}
			else {
				//indicatorProxy.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":PutOrder no-(tradeStyle,tradeDirection,gap,enSwingMinPnts,enSwingMaxPnts,enPullbackMinPnts,enPullbackMaxPnts)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + enSwingMinPnts + "," + enSwingMaxPnts + "," + enPullbackMinPnts + "," + enPullbackMaxPnts);
			}
		}		

		
		#endregion
		
		#region Order and P&L Functions
		
	
		#endregion	
		        
		[Description("AfAcc")]
        [GridCategory("Parameters")]
        public double GI_AfAcc
        {
            get { return afAcc; }
            set { afAcc = Math.Max(0.001, value); }
        }

        [Description("AfLmt")]
        [GridCategory("Parameters")]
        public double GI_AfLmt
        {
            get { return afLmt; }
            set { afLmt = Math.Max(0.2, value); }
        }

    }
}
