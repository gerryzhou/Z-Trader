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

#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// Enter the description of your strategy here
    /// </summary>
    [Description("Enter the description of your strategy here")]
    public class GSpbSAR : Strategy
    {
        #region Variables
        // Wizard generated variables
        private double afAcc = 0.002; // Default setting for AfAcc
        private double afLmt = 0.2; // Default setting for AfLmt
        // User defined variables (add any user defined variables below)
		protected int algo_mode = 1;
		protected double profitTargetAmt = 350; //36 Default(450-650 USD) setting for profitTargetAmt
		protected double profitTgtIncTic = 6; //8 Default tick Amt for ProfitTarget increase Amt
		protected double profitLockMinTic = 16; //24 Default ticks Amt for Min Profit locking
		protected double profitLockMaxTic = 30; //80 Default ticks Amt for Max Profit locking
        protected double stopLossAmt = 200; //16 Default setting for stopLossAmt
		protected double stopLossIncTic = 4; //4 Default tick Amt for StopLoss increase Amt
		protected double breakEvenAmt = 150; //150 the profits amount to trigger setting breakeven order
		protected double trailingSLAmt = 100; //300 Default setting for trailing Stop Loss Amt
		protected double dailyLossLmt = -200; //-300 the daily loss limit amount
		
        protected int timeStart = 10100; //93300 Default setting for timeStart
        protected int timeEnd = 145900; // Default setting for timeEnd
		protected int minutesChkEnOrder = 10; //how long before checking an entry order filled or not
		protected int minutesChkPnL = 30; //how long before checking P&L
		
		protected int barsHoldEnOrd = 2; // Bars count since en order was issued
        protected int barsSincePtSl = 1; // Bar count since last P&L was filled
		protected int barsToCheckPL = 2; // Bar count to check P&L since the entry
		//protected int barsPullback = 1; // Bars count for pullback
        protected double enSwingMinPnts = 11; //10 Default setting for EnSwingMinPnts
        protected double enSwingMaxPnts = 16; //16 Default setting for EnSwingMaxPnts
		protected double enPullbackMinPnts = 5; //6 Default setting for EnPullbackMinPnts
        protected double enPullbackMaxPnts = 9; //10 Default setting for EnPullbackMaxPnts
		protected double enOffsetPnts = 0.5;//Price offset for entry
		//protected double enOffset2Pnts = 0.5;//Price offset for entry
		protected int enCounterPBBars = 1;//Bar count of pullback for breakout entry setup
		protected double enResistPrc = 2700; // Resistance price for entry order
		protected double enSupportPrc = 2600; // Support price for entry order
		
		protected bool enTrailing = true; //use trailing entry: counter pullback bars or simple enOffsetPnts
		protected bool ptTrailing = true; //use trailing profit target every bar
		protected bool slTrailing = true; //use trailing stop loss every bar
		protected bool resistTrailing = false; //track resistance price for entry order
		protected bool supportTrailing = false; //track resistance price for entry order
		
		protected int tradeDirection = 0; // -1=short; 0-both; 1=long;
		protected int tradeStyle = 1; // -1=counter trend; 1=trend following;
		protected bool backTest = false; //if it runs for backtesting;
		
		protected int printOut = 1; //0,1,2,3 more print
		protected bool drawTxt = false; // User defined variables (add any user defined variables below)
		protected IText it_gap = null; //the Text draw for gap on current bar
		protected string log_file = ""; //

		protected IOrder entryOrder = null;
		protected IOrder profitTargetOrder = null;
		protected IOrder stopLossOrder = null;
		protected double trailingPTTic = 36; //400, tick amount of trailing target
		protected double trailingSLTic = 16; // 200, tick amount of trailing stop loss
		protected int barsSinceEnOrd = 0; // bar count since the en order issued
		
		protected string AccName = null;
		
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
			Add(GIParabolicSAR(0.002, 0.2, 0.002, Color.Cyan));
			//Add(GIParabolicSAR(0.001, 0.2, 0.001, Color.Orange));
			EMA(High, 50).Plots[0].Pen.Color = Color.Orange;
			EMA(Low, 50).Plots[0].Pen.Color = Color.Green;

            Add(EMA(High, 50));
            Add(EMA(Low, 50));
			
            SetProfitTarget(300);
            SetStopLoss(150, false);

            CalculateOnBarClose = true;
			AccName = GetTsTAccName(Account.Name);
			log_file = GetFileNameByDateTime(DateTime.Now, @"C:\inetpub\wwwroot\nt_files\log\", AccName, "log");
        }

		protected double GetLastZZ(){
			double zz = 0;
			//if(latestZZs.Length > 0)
			//	zz = latestZZs[latestZZs.Length-1].Size;
			return zz;
		}
			
        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			Print("-------------" + CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GIParabolicSAR=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange)[0] + "-------------");
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetAf=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetAf());
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetAfIncreased=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetAfIncreased());
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetLongPosition=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetLongPosition());
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetTodaySAR=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetTodaySAR());
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetPrevBar=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetPrevBar());
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetPrevSAR=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetPrevSAR());
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetReverseBar=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetReverseBar());
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetReverseValue=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetReverseValue());
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetXp=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetXp());
			Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetCurZZGap=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetCurZZGap());
			//Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetXpBar[0]=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetXpBar()[0]);
			//Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetXpSeries[0]=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetSarSeries()[0]);
			//Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetXpBar[1]=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetXpBar()[1]);
			//Print(CurrentBar + "-" + Get24HDateTime(Time[0]) + "-GetXpSeries[1]=" + GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange).GetSarSeries()[1]);			
            // Condition set 1
            if (GIParabolicSAR(0.002, 0.2, 0.002, Color.Orange)[0] > 0)
            {
                DrawLine("My line" + CurrentBar, 0, 0, 0, 0, Color.Blue);
                //EnterLongLimit(DefaultQuantity, 0, "enLn");
            }			
			
			CheckPerformance();
			double gap = GIParabolicSAR(0.002, 0.2, 0.002, Color.Cyan).GetCurZZGap();
			switch(algo_mode) {
				case 0: //liquidate
					CloseAllPositions();
					break;
				case 1: //trading
					ChangeSLPT();
					CheckEnOrder(gap);
								
					if(NewOrderAllowed())
					{
						PutTrade(gap);
					}
					break;
				case 2: //cancel order
					CancelAllOrders();
					break;
				case -1: //stop trading
					PrintLog(true, log_file, CurrentBar + "- Stop trading cmd:" + Get24HDateTime(Time[0]));
					break;
			}			
        }
		
		protected void PutTrade(double gap) {
			double gapAbs = Math.Abs(gap);			
			double lastZZ = GetLastZZ();
			double lastZZAbs = Math.Abs(lastZZ);
			if(entryOrder == null)
				PrintLog(true, log_file, CurrentBar + "-" + AccName + ":PutOrder-(tradeStyle,tradeDirection,gap,enSwingMinPnts,enSwingMaxPnts,enPullbackMinPnts,enPullbackMaxPnts, entryOrder)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + enSwingMinPnts + "," + enSwingMaxPnts + "," + enPullbackMinPnts + "," + enPullbackMaxPnts + "--null");
			else
				PrintLog(true, log_file, CurrentBar + "-" + AccName + ":PutOrder-(tradeStyle,tradeDirection,gap,enSwingMinPnts,enSwingMaxPnts,enPullbackMinPnts,enPullbackMaxPnts, entryOrder)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + enSwingMinPnts + "," + enSwingMaxPnts + "," + enPullbackMinPnts + "," + enPullbackMaxPnts + "--" + entryOrder.ToString());
			if(tradeStyle == 0) // scalping, counter trade the pullbackMinPnts
			{
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
					if(gap < 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts)
						NewLongLimitOrder("scalping long");
				}
				 
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
					if(gap > 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts)
						NewShortLimitOrder("scalping short");
				}
			}
			else if(tradeStyle < 0) //counter trend trade, , counter trade the swingMinPnts
			{
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
					if(gap < 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
						NewLongLimitOrder("counter trade long");
				}
				
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
					if(gap > 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
						NewShortLimitOrder("counter trade short");
				}
			}
			 // tradeStyle > 0, trend following, tradeStyle=1:entry at breakout; tradeStyle=2:entry at pullback;
			else if(tradeStyle == 1) //entry at breakout
			{
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
					if(gap > 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
						if(enCounterPBBars < 0 || IsTwoBarReversal(gap, TickSize, enCounterPBBars))
							NewLongLimitOrder("trend follow long entry at breakout");
				}
				
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
					if(gap < 0 && gapAbs >= enSwingMinPnts && gapAbs < enSwingMaxPnts)
						if(enCounterPBBars < 0 || IsTwoBarReversal(gap, TickSize, enCounterPBBars))
							NewShortLimitOrder("trend follow short entry at breakout");
				}
			}
			else if(tradeStyle == 2) //entry at pullback
			{
				PrintLog(true, log_file, CurrentBar + "-" + AccName + ":PutOrder(tradeStyle,tradeDirection,gap,lastZZs[0],lastZZAbs,enPullbackMinPnts,enPullbackMaxPnts)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + lastZZ + "," + lastZZAbs + "," + enPullbackMinPnts + "," + enPullbackMaxPnts);
				if(tradeDirection >= 0) //1=long only, 0 is for both;
				{
					if(gap < 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts && lastZZ > 0 && lastZZAbs >= enSwingMinPnts && lastZZAbs <= enSwingMaxPnts)
						NewLongLimitOrder("trend follow long entry at pullback");
				}
				
				if(tradeDirection <= 0) //-1=short only, 0 is for both;
				{
					if(gap > 0 && gapAbs >= enPullbackMinPnts && gapAbs < enPullbackMaxPnts && lastZZ < 0 && lastZZAbs >= enSwingMinPnts && lastZZAbs <= enSwingMaxPnts)
						NewShortLimitOrder("trend follow short entry at pullback");
				}
			}
			else {
				PrintLog(true, log_file, CurrentBar + "-" + AccName + ":PutOrder no-(tradeStyle,tradeDirection,gap,enSwingMinPnts,enSwingMaxPnts,enPullbackMinPnts,enPullbackMaxPnts)= " + tradeStyle + "," + tradeDirection + "," + gap + "," + enSwingMinPnts + "," + enSwingMaxPnts + "," + enPullbackMinPnts + "," + enPullbackMaxPnts);
			}
		}

		protected void NewShortLimitOrder(string msg)
		{
			double prc = (enTrailing && enCounterPBBars>0) ? Close[0]+enOffsetPnts : High[0]+enOffsetPnts;
			//enCounterPBBars
			if(entryOrder == null) {
				if(printOut > -1)
					PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + msg + ", EnterShortLimit called short price=" + prc + "--" + Get24HDateTime(Time[0]));			
			}
			else if (entryOrder.OrderState == OrderState.Working) {
				if(printOut > -1)
					PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterShortLimit updated short price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));		
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
				if(printOut > -1)
					PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			else if (entryOrder.OrderState == OrderState.Working) {
				if(printOut > -1)
					PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
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
				PrintLog(true, log_file, CurrentBar + "-" + AccName + ":(RealizedProfitLoss,RealtimeTrades.CumProfit)=(" + pnl + "," + plrt + ")--" + Get24HDateTime(Time[0]));	

			if((backTest && !Historical) || (!backTest && Historical)) {
				PrintLog(true, log_file, CurrentBar + "-" + AccName + "[backTest,Historical]=" + backTest + "," + Historical + "- NewOrderAllowed=false - " + Get24HDateTime(Time[0]));
				return false;
			}
			if(!backTest && (plrt <= dailyLossLmt || pnl <= dailyLossLmt))
			{
				if(printOut > -1) {
					PrintLog(true, log_file, CurrentBar + "-" + AccName + ": dailyLossLmt reached = " + pnl + "," + plrt);
				}
				return false;
			}
		
			if (IsTradingTime(timeStart, timeEnd, 170000) && Position.Quantity == 0)
			{
				if (entryOrder == null || entryOrder.OrderState != OrderState.Working || enTrailing)
				{
					if(bsx == -1 || bsx > barsSincePtSl)
					{
						PrintLog(true, log_file, CurrentBar + "-" + AccName + "- NewOrderAllowed=true - " + Get24HDateTime(Time[0]));
						return true;
					} else 
						PrintLog(true, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[bsx,barsSincePtSl]" + bsx + "," + barsSincePtSl + " - " + Get24HDateTime(Time[0]));
				} else
					PrintLog(true, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[entryOrder.OrderState,entryOrder.OrderType]" + entryOrder.OrderState + "," + entryOrder.OrderType + " - " + Get24HDateTime(Time[0]));
			} else 
				PrintLog(true, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[timeStart,timeEnd,Position.Quantity]" + timeStart + "," + timeEnd + "," + Position.Quantity + " - " + Get24HDateTime(Time[0]));
				
			return false;
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
         		PrintLog(true, log_file, AccName + "- Open PnL: " + pl);
				//int nChkPnL = (int)(timeSinceEn/minutesChkPnL);
				double curPTTics = -1;
				double slPrc = stopLossOrder == null ? Position.AvgPrice : stopLossOrder.StopPrice;
				
				if(ptTrailing && pl >= 12.5*(trailingPTTic - 2*profitTgtIncTic))
				{
					trailingPTTic = trailingPTTic + profitTgtIncTic;
					if(profitTargetOrder != null) {
						curPTTics = Math.Abs(profitTargetOrder.LimitPrice - Position.AvgPrice)/TickSize;
					}
					PrintLog(true, log_file, AccName + "- update PT: PnL=" + pl + ",(trailingPTTic, curPTTics, $Amt, $Amt_cur)=(" + trailingPTTic + "," + curPTTics + "," + 12.5*trailingPTTic + "," + 12.5*curPTTics + ")");
					if(profitTargetOrder == null || trailingPTTic > curPTTics)
						SetProfitTarget(CalculationMode.Ticks, trailingPTTic);
				}
				
				if(pl >= breakEvenAmt) { //setup breakeven order
					PrintLog(true, log_file, AccName + "- setup SL Breakeven: (PnL, posAvgPrc)=(" + pl + "," + Position.AvgPrice + ")");
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
						PrintLog(true, log_file, AccName + "- SetTrailStop over SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");						
					}
					else if(pl >= 12.5*(profitLockMaxTic + 2*profitTgtIncTic)) { // lock max profits
						trailingSLTic = trailingSLTic + profitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = trailingSLTic > profitLockMaxTic ? Position.AvgPrice+TickSize*trailingSLTic : Position.AvgPrice+TickSize*profitLockMaxTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = trailingSLTic > profitLockMaxTic ? Position.AvgPrice-TickSize*trailingSLTic :  Position.AvgPrice-TickSize*profitLockMaxTic;
						PrintLog(true, log_file, AccName + "- update SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
					else if(pl >= 12.5*(profitLockMinTic + 2*profitTgtIncTic)) { //lock min profits
						trailingSLTic = trailingSLTic + profitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = Position.AvgPrice+TickSize*profitLockMinTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = Position.AvgPrice-TickSize*profitLockMinTic;
						PrintLog(true, log_file, AccName + "- update SL Min: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
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
			PrintLog(true, log_file, CurrentBar + "-" + AccName + ": Cum all PnL= " + pl + ", Cum runtime PnL= " + plrt);
			return plrt;
		}
		
		public bool CheckEnOrder(double cur_gap)
        {
            double min_en = -1;

            if (entryOrder != null && entryOrder.OrderState == OrderState.Working)
            {
                min_en = GetMinutesDiff(entryOrder.Time, Time[0]);// DateTime.Now);
                if ( IsTwoBarReversal(cur_gap, TickSize, enCounterPBBars) || (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))
                {
                    CancelOrder(entryOrder);
                    PrintLog(true, log_file, "Order cancelled for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					return true;
                }
				else {
					PrintLog(true, log_file, "Order working for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					barsSinceEnOrd++;
				}
            }
            return false;
        }
		
		public bool CloseAllPositions() 
		{
			PrintLog(true, log_file, "CloseAllPosition called");
			if(Position.MarketPosition == MarketPosition.Long)
				ExitLong();
			if(Position.MarketPosition == MarketPosition.Short)
				ExitShort();
			return true;
		}
		
		public bool CancelAllOrders() 
		{
			PrintLog(true, log_file, CurrentBar + "- CancelAllOrders called");
			if(stopLossOrder != null)
				CancelOrder(stopLossOrder);
			if(profitTargetOrder != null)
				CancelOrder(profitTargetOrder);
			return true;
		}
		
		protected override void OnExecution(IExecution execution)
		{
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null && execution.Order.OrderState == OrderState.Filled) {
				if(printOut > -1)
					PrintLog(true, log_file, CurrentBar + "-" + AccName + " Exe=" + execution.Name + ",Price=" + execution.Price + "," + execution.Time.ToShortTimeString());
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
				PrintLog(true, log_file, order.ToString() + "--" + order.OrderState);
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
				if(printOut > -1)
					PrintLog(true, log_file, CurrentBar + "-" + AccName + ":" + order.ToString());
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
		
        #region Properties
        [Description("")]
        [GridCategory("Parameters")]
        public double AfAcc
        {
            get { return afAcc; }
            set { afAcc = Math.Max(0.001, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public double AfLmt
        {
            get { return afLmt; }
            set { afLmt = Math.Max(0.2, value); }
        }
        #endregion
    }
}
