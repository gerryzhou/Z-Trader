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
		private TradeCommand tradeCommand = null;
		private MarketContext mktContext = null;
		private GIParabolicSAR giParabSAR = null;//new GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan);
		//private Logger logger = null;
		
		private double afAcc = 0.002; // Default setting for AfAcc
        private double afLmt = 0.2; // Default setting for AfLmt

		private int algo_mode = 1;
		
		private double profitTargetAmt = 350; //36 Default(450-650 USD) setting for profitTargetAmt
		private double profitTgtIncTic = 6; //8 Default tick Amt for ProfitTarget increase Amt
		private double profitLockMinTic = 16; //24 Default ticks Amt for Min Profit locking
		private double profitLockMaxTic = 30; //80 Default ticks Amt for Max Profit locking
        private double stopLossAmt = 200; //16 Default setting for stopLossAmt
		private double stopLossIncTic = 4; //4 Default tick Amt for StopLoss increase Amt
		private double breakEvenAmt = 150; //150 the profits amount to trigger setting breakeven order
		private double trailingSLAmt = 100; //300 Default setting for trailing Stop Loss Amt
		private double dailyLossLmt = -200; //-300 the daily loss limit amount
		
		//time=H*10000+M*100+S, S is skipped here;
        private int timeStartH = 1; //10100 Default setting for timeStart hour
		private int timeStartM = 1; //10100 Default setting for timeStart minute
		private int timeStart = -1; //10100 Default setting for timeStart
        private int timeEndH = 14; // Default setting for timeEnd hour
		private int timeEndM = 59; // Default setting for timeEnd minute
		private int timeEnd = -1; // Default setting for timeEnd

		private int minutesChkEnOrder = 20; //how long before checking an entry order filled or not
		private int minutesChkPnL = 30; //how long before checking P&L
		
		private int barsHoldEnOrd = 10; // Bars count since en order was issued
        private int barsSincePtSl = 1; // Bar count since last P&L was filled
		private int barsToCheckPL = 2; // Bar count to check P&L since the entry
		
		private int barsAgoMaxPbSAREn = 6; //Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed
		private int barsMaxLastCross = 24; //Bars count for last PbSAR swing. Enter the maximum bars count of last PbSAR allowed for entry
		//private int barsPullback = 1; // Bars count for pullback
        private double enSwingMinPnts = 10; //10 Default setting for EnSwingMinPnts
        private double enSwingMaxPnts = 35; //16 Default setting for EnSwingMaxPnts
		private double enPullbackMinPnts = 1; //6 Default setting for EnPullbackMinPnts
        private double enPullbackMaxPnts = 8; //10 Default setting for EnPullbackMaxPnts
		private double enOffsetPnts = 1.25;//Price offset for entry
		//private double enOffset2Pnts = 0.5;//Price offset for entry
		private int enCounterPBBars = 1;//Bar count of pullback for breakout entry setup
		private double enResistPrc = 2700; // Resistance price for entry order
		private double enSupportPrc = 2600; // Support price for entry order
		
		private bool enTrailing = true; //use trailing entry: counter pullback bars or simple enOffsetPnts
		private bool ptTrailing = true; //use trailing profit target every bar
		private bool slTrailing = true; //use trailing stop loss every bar
		private bool resistTrailing = false; //track resistance price for entry order
		private bool supportTrailing = false; //track resistance price for entry order
		
		private int tradeDirection = 0; // -1=short; 0-both; 1=long;
		private int tradeStyle = 0; // -1=counter trend; 1=trend following;
		private bool backTest = true; //if it runs for backtesting;
		
		private int printOut = 2; //0,1,2,3 more print

		private bool drawTxt = false; // User defined variables (add any user defined variables below)
		private IText it_gap = null; //the Text draw for gap on current bar
		//private string log_file = ""; //
		
		private int barsSinceLastCross = -1;
		private PriceAction curBarPriceAction = new PriceAction(PriceActionType.UnKnown, -1,-1,-1,-1);//PriceAtionType of current bar
		
		private int spvPRBits = 42;//39,63
		
		/// <summary>
		/// Order handling
		/// </summary>
		
		private IOrder entryOrder = null;
		private IOrder profitTargetOrder = null;
		private IOrder stopLossOrder = null;
		private double trailingPTTic = 36; //400, tick amount of trailing target
		private double trailingSLTic = 16; // 200, tick amount of trailing stop loss
		private int barsSinceEnOrd = 0; // bar count since the en order issued
		
		private string AccName = null;

		#endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
			AccName = GetTsTAccName(Account.Name);
			giParabSAR = GIParabolicSAR(afAcc, afLmt, afAcc, AccName, backTest, Color.Cyan);
			Add(giParabSAR);
			giParabSAR.setSpvPRBits(spvPRBits);
			timeStart = giParabSAR.GetTimeByHM(timeStartH, timeStartM);
			timeEnd = giParabSAR.GetTimeByHM(timeEndH, timeEndM);
			//Add(GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan));
			//Add(GIParabolicSAR(0.001, 0.2, 0.001, Color.Orange));
			EMA(High, 50).Plots[0].Pen.Color = Color.Orange;
			EMA(Low, 50).Plots[0].Pen.Color = Color.Green;

            Add(EMA(High, 50));
            Add(EMA(Low, 50));
			
            SetProfitTarget(300);
            SetStopLoss(150, false);

            CalculateOnBarClose = true;
			DefaultQuantity = 1;

			// Triggers the exit on close function 30 seconds prior to session end
			ExitOnClose = true;
			ExitOnCloseSeconds = 30;
			
			if(!backTest)
				indicatorProxy = new IndicatorProxy(AccName, Instrument.FullName);
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
					indicatorProxy.PrintLog(true, !backTest, CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetCurZZGap,GetCurGap,isReversalBar=" + zz_gap + "," + cur_gap + "," + isReversalBar + ", getCurPriceActType=" + curBarPriceAction.ToString() + ", barsSinceLastCross=" + barsSinceLastCross);//GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetCurZZGap());
			}
			
			if (giParabSAR[0] > 0) //GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Orange)[0] > 0)
            {
                DrawLine("My line" + CurrentBar, 0, 0, 0, 0, Color.Blue);
                //EnterLongLimit(DefaultQuantity, 0, "enLn");
            }			
			
			CheckPerformance();
			//double gap = GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).GetCurZZGap();
			//bool isReversalBar = true;//CurrentBar>BarsRequired?false:GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).IsReversalBar();
			switch(algo_mode) {
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
				string logText = CurrentBar + "-" + AccName
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
