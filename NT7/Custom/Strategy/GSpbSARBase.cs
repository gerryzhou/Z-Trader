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
		
        private int timeStart = 10100; //93300 Default setting for timeStart
        private int timeEnd = 145900; // Default setting for timeEnd
		private int minutesChkEnOrder = 20; //how long before checking an entry order filled or not
		private int minutesChkPnL = 30; //how long before checking P&L
		
		private int barsHoldEnOrd = 10; // Bars count since en order was issued
        private int barsSincePtSl = 1; // Bar count since last P&L was filled
		private int barsToCheckPL = 2; // Bar count to check P&L since the entry
		
		private int barsAgoMaxPbSAREn = 5; //Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed
		private int barsMaxLastCross = 54; //Bars count for last PbSAR swing. Enter the maximum bars count of last PbSAR allowed for entry
		//private int barsPullback = 1; // Bars count for pullback
        private double enSwingMinPnts = 10; //10 Default setting for EnSwingMinPnts
        private double enSwingMaxPnts = 35; //16 Default setting for EnSwingMaxPnts
		private double enPullbackMinPnts = 5; //6 Default setting for EnPullbackMinPnts
        private double enPullbackMaxPnts = 29; //10 Default setting for EnPullbackMaxPnts
		private double enOffsetPnts = 0.75;//Price offset for entry
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
		
		private int printOut = 1; //0,1,2,3 more print

		private bool drawTxt = false; // User defined variables (add any user defined variables below)
		private IText it_gap = null; //the Text draw for gap on current bar
		private string log_file = ""; //
		
		private GIParabolicSAR giParabSAR = null;//new GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan);
		private int barsSinceLastCross = -1;
		private PriceActionType curBarPat = PriceActionType.UnKnown;//PriceAtionType of current bar
		
		private int spvPRBits = 0;
		
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
			giParabSAR = GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan);
			Add(giParabSAR);
			giParabSAR.setSpvPRBits(spvPRBits);
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
						
			//log_file = GetFileNameByDateTime(DateTime.Now, @"C:\inetpub\wwwroot\nt_files\log\", AccName, "log");
        }

        protected override void OnBarUpdate()
        {
			//Print("-------------" + CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GIParabolicSAR=" + giParabSAR[0] + "-------------");
			//Print("-------------" + CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GIParabolicSAR=" + GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan)[0] + "-------------");
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetAf=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetAf());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetAfIncreased=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetAfIncreased());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetLongPosition=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetLongPosition());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetTodaySAR=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetTodaySAR());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetPrevBar=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetPrevBar());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetPrevSAR=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetPrevSAR());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetReverseBar=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetReverseBar());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetReverseValue=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetReverseValue());
//			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetXp=" + GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetXp());
			double gap = giParabSAR.GetCurZZGap(); //GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Orange).GetCurZZGap();
			bool isReversalBar = giParabSAR.IsReversalBar();//GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Orange).IsReversalBar();
			curBarPat = giParabSAR.getCurPriceActType();
			barsSinceLastCross = giParabSAR.GetpbSARCrossBarsAgo();
			if(isReversalBar) {				
				SetTradeContext(curBarPat);
				//Print("-------------" + CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GIParabolicSAR=" + GIParabolicSAR(afAcc, afLmt, afAcc, AccName, Color.Cyan)[0] + "-------------");
				Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetCurZZGap,isReversalBar=" + gap + "," + isReversalBar + ", getCurPriceActType=" + curBarPat.ToString() + ", barsSinceLastCross=" + barsSinceLastCross);//GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Orange).GetCurZZGap());
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
					CheckEnOrder(gap);
								
					if(NewOrderAllowed() && PatternMatched())
					{
						Print("----------------PutTrade, isReversalBar=" + isReversalBar + ",giParabSAR.IsSpvAllowed4PAT(curBarPat)=" + giParabSAR.IsSpvAllowed4PAT(curBarPat));
						PutTrade(gap, isReversalBar);
					}
					break;
				case 2: //cancel order
					CancelAllOrders();
					break;
				case -1: //stop trading
					//PrintLog(true, log_file, CurrentBar + "- Stop trading cmd:" + Get24HDateTime(Time[0]));
					break;
			}			
        }

		#region	Trade Functions
		
		protected void SetTradeContext(PriceActionType pat) {
			switch(pat) {
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
		
		protected void PutTrade(double gap, bool isRevBar) {
			double gapAbs = Math.Abs(gap);
//			double lastZZ = GetLastZZ();
//			double lastZZAbs = Math.Abs(lastZZ);
//			if(entryOrder == null)
				//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":PutOrder-(tradeStyle,tradeDirection,gap,enSwingMinPnts,enSwingMaxPnts,enPullbackMinPnts,enPullbackMaxPnts, entryOrder)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + enSwingMinPnts + "," + enSwingMaxPnts + "," + enPullbackMinPnts + "," + enPullbackMaxPnts + "--null");
//			else
				//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":PutOrder-(tradeStyle,tradeDirection,gap,enSwingMinPnts,enSwingMaxPnts,enPullbackMinPnts,enPullbackMaxPnts, entryOrder)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + enSwingMinPnts + "," + enSwingMaxPnts + "," + enPullbackMinPnts + "," + enPullbackMaxPnts + "--" + entryOrder.ToString());
			if(tradeStyle == 0) // scalping, counter trade the pullbackMinPnts
			{
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
//					if(gap > 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts)
						NewLongLimitOrder("scalping long");
				}
				 
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
//					if(gap < 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts)
						NewShortLimitOrder("scalping short");
				}
			}
			else if(tradeStyle < 0) //counter trend trade, , counter trade the swingMinPnts
			{
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
//					if(gap < 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
						NewLongLimitOrder("counter trade long");
				}
				
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
//					if(gap > 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
						NewShortLimitOrder("counter trade short");
				}
			}
			// tradeStyle > 0, trend following, tradeStyle=1:entry at breakout; tradeStyle=2:entry at pullback;
			else if(tradeStyle == 1) //entry at breakout
			{
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
//					if(gap > 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
//						if(enCounterPBBars < 0 || IsTwoBarReversal(gap, TickSize, enCounterPBBars))
							NewLongLimitOrder("trend follow long entry at breakout");
				}
				
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
//					if(gap < 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
//						if(enCounterPBBars < 0 || IsTwoBarReversal(gap, TickSize, enCounterPBBars))
							NewShortLimitOrder("trend follow short entry at breakout");
				}
			}
			else if(tradeStyle == 2) //entry at pullback, wide channel: UpWide, DnWide, RngWide
			{
				//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":PutOrder(tradeStyle,tradeDirection,gap,lastZZs[0],lastZZAbs,enPullbackMinPnts,enPullbackMaxPnts)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + lastZZ + "," + lastZZAbs + "," + enPullbackMinPnts + "," + enPullbackMaxPnts);
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
//					if(gap < 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts)
						NewLongLimitOrder("trend follow long entry at pullback");
				}
				
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
//					if(gap > 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts)
						NewShortLimitOrder("trend follow short entry at pullback");
				}
			}
			else {
				//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":PutOrder no-(tradeStyle,tradeDirection,gap,enSwingMinPnts,enSwingMaxPnts,enPullbackMinPnts,enPullbackMaxPnts)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + enSwingMinPnts + "," + enSwingMaxPnts + "," + enPullbackMinPnts + "," + enPullbackMaxPnts);
			}
		}
		
		#endregion
		
		#region Order and P&L Functions
		
		protected void NewShortLimitOrder(string msg)
		{
			double prc = (enTrailing && enCounterPBBars>0) ? Close[0]+enOffsetPnts : High[0]+enOffsetPnts;
			//enCounterPBBars
			if(entryOrder == null) {
//				if(printOut > -1)
					//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + msg + ", EnterShortLimit called short price=" + prc + "--" + Get24HDateTime(Time[0]));			
			}
			else if (entryOrder.OrderState == OrderState.Working) {
//				if(printOut > -1)
					//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterShortLimit updated short price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));		
				CancelOrder(entryOrder);
				//entryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			entryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			barsSinceEnOrd = 0;
		}
		
		protected void NewLongLimitOrder(string msg)
		{
			double prc = (enTrailing && enCounterPBBars>0) ? Close[0]-enOffsetPnts :  Low[0]-enOffsetPnts;

			if(entryOrder == null) {
				entryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
//				if(printOut > -1)
					//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			else if (entryOrder.OrderState == OrderState.Working) {
//				if(printOut > -1)
					//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
				CancelOrder(entryOrder);
				entryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			barsSinceEnOrd = 0;
		}

		protected bool NewOrderAllowed()
		{
			int bsx = BarsSinceExit();
			int bse = BarsSinceEntry();
			double pnl = CheckAccPnL();//GetAccountValue(AccountItem.RealizedProfitLoss);
			double plrt = CheckAccCumProfit();
			if(printOut > -1)				
				//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":(RealizedProfitLoss,RealtimeTrades.CumProfit)=(" + pnl + "," + plrt + ")--" + Get24HDateTime(Time[0]));	

			if((backTest && !Historical) || (!backTest && Historical)) {
				//PrintLog(true, log_file, CurrentBar + "-" + AccName + "[backTest,Historical]=" + backTest + "," + Historical + "- NewOrderAllowed=false - " + Get24HDateTime(Time[0]));
				return false;
			}
			if(!backTest && (plrt <= dailyLossLmt || pnl <= dailyLossLmt))
			{
				if(printOut > -1) {
					//PrintLog(true, log_file, CurrentBar + "-" + AccName + ": dailyLossLmt reached = " + pnl + "," + plrt);
				}
				return false;
			}
		
			if (IsTradingTime(timeStart, timeEnd, 170000) && Position.Quantity == 0)
			{
				if (entryOrder == null || entryOrder.OrderState != OrderState.Working || enTrailing)
				{
					if(bsx == -1 || bsx > barsSincePtSl)
					{
						//PrintLog(true, log_file, CurrentBar + "-" + AccName + "- NewOrderAllowed=true - " + Get24HDateTime(Time[0]));
						return true;
					} //else 
						//PrintLog(true, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[bsx,barsSincePtSl]" + bsx + "," + barsSincePtSl + " - " + Get24HDateTime(Time[0]));
				} //else
					//PrintLog(true, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[entryOrder.OrderState,entryOrder.OrderType]" + entryOrder.OrderState + "," + entryOrder.OrderType + " - " + Get24HDateTime(Time[0]));
			}// else 
				//PrintLog(true, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[timeStart,timeEnd,Position.Quantity]" + timeStart + "," + timeEnd + "," + Position.Quantity + " - " + Get24HDateTime(Time[0]));
				
			return false;
		}

		protected bool PatternMatched()
		{
			Print("CurrentBar, barsMaxLastCross, barsAgoMaxPbSAREn,=" + CurrentBar + "," + barsAgoMaxPbSAREn + "," + barsSinceLastCross);
			if (giParabSAR.IsSpvAllowed4PAT(curBarPat) && barsSinceLastCross < barsAgoMaxPbSAREn) 
				return true;
			else return false;
			//barsAgoMaxPbSAREn Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed
		}
		
		protected bool ChangeSLPT()
		{
			int bse = BarsSinceEntry();
			double timeSinceEn = -1;
			if(bse > 0) {
				timeSinceEn = GetMinutesDiff(Time[0], Time[bse]);
			}
			
			double pl = Position.GetProfitLoss(Close[0], PerformanceUnit.Currency);
			 // If not flat print out unrealized PnL
    		if (Position.MarketPosition != MarketPosition.Flat) 
			{
         		//PrintLog(true, log_file, AccName + "- Open PnL: " + pl);
				//int nChkPnL = (int)(timeSinceEn/minutesChkPnL);
				double curPTTics = -1;
				double slPrc = stopLossOrder == null ? Position.AvgPrice : stopLossOrder.StopPrice;
				
				if(ptTrailing && pl >= 12.5*(trailingPTTic - 2*profitTgtIncTic))
				{
					trailingPTTic = trailingPTTic + profitTgtIncTic;
					if(profitTargetOrder != null) {
						curPTTics = Math.Abs(profitTargetOrder.LimitPrice - Position.AvgPrice)/TickSize;
					}
					//PrintLog(true, log_file, AccName + "- update PT: PnL=" + pl + ",(trailingPTTic, curPTTics, $Amt, $Amt_cur)=(" + trailingPTTic + "," + curPTTics + "," + 12.5*trailingPTTic + "," + 12.5*curPTTics + ")");
					if(profitTargetOrder == null || trailingPTTic > curPTTics)
						SetProfitTarget(CalculationMode.Ticks, trailingPTTic);
				}
				
				if(pl >= breakEvenAmt) { //setup breakeven order
					//PrintLog(true, log_file, AccName + "- setup SL Breakeven: (PnL, posAvgPrc)=(" + pl + "," + Position.AvgPrice + ")");
					slPrc = Position.AvgPrice;
					//SetStopLoss(0);
				}
				
				if(slTrailing) { // trailing max and min profits then converted to trailing stop after over the max
//					if(trailingSLTic > profitLockMaxTic && pl >= 12.5*(trailingSLTic + 2*profitTgtIncTic)) {
//						trailingSLTic = trailingSLTic + profitTgtIncTic;
//						if(Position.MarketPosition == MarketPosition.Long)
//							slPrc = Position.AvgPrice+TickSize*trailingSLTic;
//						if(Position.MarketPosition == MarketPosition.Short)
//							slPrc = Position.AvgPrice-TickSize*trailingSLTic;
//						Print(AccName + "- update SL over Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");						
//					}
					if(trailingSLTic > profitLockMaxTic && pl >= 12.5*(trailingSLTic + 2*profitTgtIncTic)) {
						trailingSLTic = trailingSLTic + profitTgtIncTic;
						if(stopLossOrder != null)
							CancelOrder(stopLossOrder);
						if(profitTargetOrder != null)
							CancelOrder(profitTargetOrder);
						SetTrailStop(trailingSLAmt);
						//PrintLog(true, log_file, AccName + "- SetTrailStop over SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");						
					}
					else if(pl >= 12.5*(profitLockMaxTic + 2*profitTgtIncTic)) { // lock max profits
						trailingSLTic = trailingSLTic + profitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = trailingSLTic > profitLockMaxTic ? Position.AvgPrice+TickSize*trailingSLTic : Position.AvgPrice+TickSize*profitLockMaxTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = trailingSLTic > profitLockMaxTic ? Position.AvgPrice-TickSize*trailingSLTic :  Position.AvgPrice-TickSize*profitLockMaxTic;
						//PrintLog(true, log_file, AccName + "- update SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
					else if(pl >= 12.5*(profitLockMinTic + 2*profitTgtIncTic)) { //lock min profits
						trailingSLTic = trailingSLTic + profitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = Position.AvgPrice+TickSize*profitLockMinTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = Position.AvgPrice-TickSize*profitLockMinTic;
						//PrintLog(true, log_file, AccName + "- update SL Min: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
				}
				if(stopLossOrder == null || 
					(Position.MarketPosition == MarketPosition.Long && slPrc > stopLossOrder.StopPrice) ||
					(Position.MarketPosition == MarketPosition.Short && slPrc < stopLossOrder.StopPrice)) 
				{
					SetStopLoss(CalculationMode.Price, slPrc);
				}
			} else {
				SetStopLoss(stopLossAmt);
				SetProfitTarget(profitTargetAmt);
			}

			return false;
		}
		
		public double CheckAccPnL() {
			double pnl = GetAccountValue(AccountItem.RealizedProfitLoss);
			//Print(CurrentBar + "-" + AccName + ": GetAccountValue(AccountItem.RealizedProfitLoss)= " + pnl + " -- " + Time[0].ToString());
			return pnl;
		}
		
		public double CheckAccCumProfit() {
			double plrt = Performance.RealtimeTrades.TradesPerformance.Currency.CumProfit;
			//Print(CurrentBar + "-" + AccName + ": Cum runtime PnL= " + plrt);
			return plrt;
		}
		
		public double CheckPerformance()
		{
			double pl = Performance.AllTrades.TradesPerformance.Currency.CumProfit;
			double plrt = Performance.RealtimeTrades.TradesPerformance.Currency.CumProfit;
			//PrintLog(true, log_file, CurrentBar + "-" + AccName + ": Cum all PnL= " + pl + ", Cum runtime PnL= " + plrt);
			return plrt;
		}
		
		public bool CheckEnOrder(double cur_gap)
        {
            double min_en = -1;

            if (entryOrder != null && entryOrder.OrderState == OrderState.Working)
            {
                min_en = GetMinutesDiff(entryOrder.Time, Time[0]);// DateTime.Now);
                //if ( IsTwoBarReversal(cur_gap, TickSize, enCounterPBBars) || (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))
				if ( (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))	
                {
                    CancelOrder(entryOrder);
                    //PrintLog(true, log_file, "Order cancelled for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					return true;
                }
				else {
					//PrintLog(true, log_file, "Order working for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					barsSinceEnOrd++;
				}
            }
            return false;
        }
		
		#endregion
		
		#region Position Functions
		
		public bool CloseAllPositions() 
		{
			//PrintLog(true, log_file, "CloseAllPosition called");
			if(Position.MarketPosition == MarketPosition.Long)
				ExitLong();
			if(Position.MarketPosition == MarketPosition.Short)
				ExitShort();
			return true;
		}
		
		public bool CancelAllOrders() 
		{
			//PrintLog(true, log_file, CurrentBar + "- CancelAllOrders called");
			if(stopLossOrder != null)
				CancelOrder(stopLossOrder);
			if(profitTargetOrder != null)
				CancelOrder(profitTargetOrder);
			return true;
		}
		
		#endregion
		
		#region Event Handlers
		
		protected override void OnExecution(IExecution execution)
		{
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null && execution.Order.OrderState == OrderState.Filled) {
				if(printOut > -1)
					//PrintLog(true, log_file, CurrentBar + "-" + AccName + " Exe=" + execution.Name + ",Price=" + execution.Price + "," + execution.Time.ToShortTimeString());
				if(drawTxt) {
					IText it = DrawText(CurrentBar.ToString()+Time[0].ToShortTimeString(), Time[0].ToString().Substring(10)+"\r\n"+execution.Name+":"+execution.Price, 0, execution.Price, Color.Red);
					it.Locked = false;
				}
			}
		}

		protected override void OnOrderUpdate(IOrder order)
		{
		    if (entryOrder != null && entryOrder == order)
		    {
				//PrintLog(true, log_file, order.ToString() + "--" + order.OrderState);
		        if (order.OrderState == OrderState.Cancelled || 
					order.OrderState == OrderState.Filled || 
					order.OrderState == OrderState.Rejected || 
					order.OrderState == OrderState.Unknown)
				{
					barsSinceEnOrd = 0;
					entryOrder = null;
				}
		    }
			
			if (order.OrderState == OrderState.Working || order.OrderType == OrderType.Stop) {
//				if(printOut > -1)
					//PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + order.ToString());
			}
			
			if(profitTargetOrder == null && order.Name == "Profit target" && order.OrderState == OrderState.Working) {
				profitTargetOrder = order;
			}
			if(stopLossOrder == null && order.Name == "Stop loss" && (order.OrderState == OrderState.Accepted || order.OrderState == OrderState.Working)) {
				stopLossOrder = order;
			}
			
			if( order.OrderState == OrderState.Filled || order.OrderState == OrderState.Cancelled) {
				if(order.Name == "Stop loss")
					stopLossOrder = null;
				if(order.Name == "Profit target")
					profitTargetOrder = null;
			}
		}

		protected override void OnPositionUpdate(IPosition position)
		{
			//Print(position.ToString() + "--MarketPosition=" + position.MarketPosition);
			if (position.MarketPosition == MarketPosition.Flat)
			{
				trailingPTTic = profitTargetAmt/12.5;
				trailingSLTic = stopLossAmt/12.5;
			}
		}
		
		#endregion
		
        #region Properties
		
        [Description("AfAcc")]
        [GridCategory("Parameters")]
        public double AfAcc
        {
            get { return afAcc; }
            set { afAcc = Math.Max(0.001, value); }
        }

        [Description("AfLmt")]
        [GridCategory("Parameters")]
        public double AfLmt
        {
            get { return afLmt; }
            set { afLmt = Math.Max(0.2, value); }
        }
		
//        [Description("ZigZag retrace points")]
//        [GridCategory("Parameters")]
//        public double RetracePnts
//        {
//            get { return retracePnts; }
//            set { retracePnts = Math.Max(4, value); }
//        }

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
		
//		[Description("ZigZag retrace points")]
//        [GridCategory("Parameters")]
//        public double RetracePnts
//        {
//            get { return retracePnts; }
//            set { retracePnts = Math.Max(1, value); }
//        }

        [Description("Money amount of profit target")]
        [GridCategory("Parameters")]
        public double ProfitTargetAmt
        {
            get { return profitTargetAmt; }
            set { profitTargetAmt = Math.Max(0, value); }
        }

        [Description("Money amount for profit target increasement")]
        [GridCategory("Parameters")]
        public double ProfitTgtIncTic
        {
            get { return profitTgtIncTic; }
            set { profitTgtIncTic = Math.Max(0, value); }
        }
		
        [Description("Tick amount for min profit locking")]
        [GridCategory("Parameters")]
        public double ProfitLockMinTic
        {
            get { return profitLockMinTic; }
            set { profitLockMinTic = Math.Max(0, value); }
        }

		[Description("Tick amount for max profit locking")]
        [GridCategory("Parameters")]
        public double ProfitLockMaxTic
        {
            get { return profitLockMaxTic; }
            set { profitLockMaxTic = Math.Max(0, value); }
        }
		
        [Description("Money amount of stop loss")]
        [GridCategory("Parameters")]
        public double StopLossAmt
        {
            get { return stopLossAmt; }
            set { stopLossAmt = Math.Max(0, value); }
        }
		
        [Description("Money amount of trailing stop loss")]
        [GridCategory("Parameters")]
        public double TrailingStopLossAmt
        {
            get { return trailingSLAmt; }
            set { trailingSLAmt = Math.Max(0, value); }
        }
		
		[Description("Money amount for stop loss increasement")]
        [GridCategory("Parameters")]
        public double StopLossIncTic
        {
            get { return stopLossIncTic; }
            set { stopLossIncTic = Math.Max(0, value); }
        }
		
        [Description("Break Even amount")]
        [GridCategory("Parameters")]
        public double BreakEvenAmt
        {
            get { return breakEvenAmt; }
            set { breakEvenAmt = Math.Max(0, value); }
        }

		[Description("Daily Loss Limit amount")]
        [GridCategory("Parameters")]
        public double DailyLossLmt
        {
            get { return dailyLossLmt; }
            set { dailyLossLmt = Math.Min(-100, value); }
        }
		
        [Description("Time start")]
        [GridCategory("Parameters")]
        public int TimeStart
        {
            get { return timeStart; }
            set { timeStart = Math.Max(0, value); }
        }

        [Description("Time end")]
        [GridCategory("Parameters")]
        public int TimeEnd
        {
            get { return timeEnd; }
            set { timeEnd = Math.Max(0, value); }
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
		
//        [Description("Offeset points for limit price entry, pullback entry")]
//        [GridCategory("Parameters")]
//        public double EnOffset2Pnts
//        {
//            get { return enOffset2Pnts; }
//            set { enOffset2Pnts = Math.Max(0, value); }
//        }
		
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
		
        #endregion
    }
}
