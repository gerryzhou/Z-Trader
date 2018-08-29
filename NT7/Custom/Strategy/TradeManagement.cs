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
    /// This file holds all trade and order management approaches.
    /// </summary>
 
	//public enum SessionBreak {AfternoonClose, EveningOpen, MorningOpen, NextDay};
	public class TradeObj {
		public int tradeDirection;
		public int tradeStyle;
		
		#region Money Mgmt variables
		public double profitTargetAmt = 350; //36 Default(450-650 USD) setting for MM_ProfitTargetAmt
		public double profitTgtIncTic = 6; //8 Default tick Amt for ProfitTarget increase Amt
		public double profitLockMinTic = 16; //24 Default ticks Amt for Min Profit locking
		public double profitLockMaxTic = 30; //80 Default ticks Amt for Max Profit locking
        public double stopLossAmt = 200; //16 Default setting for stopLossAmt
		public double stopLossIncTic = 4; //4 Default tick Amt for StopLoss increase Amt
		public double breakEvenAmt = 150; //150 the profits amount to trigger setting breakeven order
		public double trailingSLAmt = 100; //300 Default setting for trailing Stop Loss Amt
		public double dailyLossLmt = -200; //-300 the daily loss limit amount

		public bool enTrailing = true; //use trailing entry: counter pullback bars or simple enOffsetPnts
		public bool ptTrailing = true; //use trailing profit target every bar
		public bool slTrailing = true; //use trailing stop loss every bar		
		#endregion
		
		#region Trade Mgmt variables
		public double enOffsetPnts = 1.25;//Price offset for entry
		public int enCounterPBBars = 1;//Bar count of pullback for breakout entry setup
		
		public int minutesChkEnOrder = 20; //how long before checking an entry order filled or not
		public int minutesChkPnL = 30; //how long before checking P&L
		
		public int barsHoldEnOrd = 10; // Bars count since en order was issued
        public int barsSincePtSl = 1; // Bar count since last P&L was filled
		public int barsToCheckPL = 2; // Bar count to check P&L since the entry		
		#endregion
		
		#region Order Objects
		public IOrder entryOrder = null;
		public IOrder profitTargetOrder = null;
		public IOrder stopLossOrder = null;
		public double trailingPTTic = 36; //400, tick amount of trailing target
		public double trailingSLTic = 16; // 200, tick amount of trailing stop loss
		public int barsSinceEnOrd = 0; // bar count since the en order issued		
		#endregion
		
		public TradeObj(){
		}
		
	}

    partial class Strategy {
		protected TradeObj tradeObj = null;
		
		#region Trigger Functions
		
		protected virtual void PutTrade(double curGap, bool isRevBar) {
		}
		
		protected bool NewOrderAllowed()
		{
			int bsx = BarsSinceExit();
			int bse = BarsSinceEntry();
			double pnl = CheckAccPnL();//GetAccountValue(AccountItem.RealizedProfitLoss);
			double plrt = CheckAccCumProfit();
			DateTime dayKey = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day);
			TradeCollection tc = (TradeCollection)Performance.AllTrades.ByDay[dayKey];
			if(backTest && tc != null) {
				pnl = tc.TradesPerformance.Currency.CumProfit;
			}
			
			if(printOut > -1) {	
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":(RealizedProfitLoss,RealtimeTrades.CumProfit)=(" + pnl + "," + plrt + ")--" + Get24HDateTime(Time[0]));	
				if(Performance.AllTrades.ByDay.Count == 2) {
					//giParabSAR.PrintLog(true, !backTest, log_file, "Performance.AllTrades.TradesPerformance.Currency.CumProfit is: " + Performance.AllTrades.TradesPerformance.Currency.CumProfit);
					//giParabSAR.PrintLog(true, !backTest, log_file, "Performance.AllTrades.ByDay[dayKey].TradesPerformance.Currency.CumProfit is: " + pnl);
				}
			}
			if((backTest && !Historical) || (!backTest && Historical)) {
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "[backTest,Historical]=" + backTest + "," + Historical + "- NewOrderAllowed=false - " + Get24HDateTime(Time[0]));
				return false;
			}
			if(!backTest && (plrt <= MM_DailyLossLmt || pnl <= MM_DailyLossLmt))
			{
				if(printOut > -1) {
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": dailyLossLmt reached = " + pnl + "," + plrt);
				}
				return false;
			}
			if (backTest && pnl <= MM_DailyLossLmt) {
				if(printOut > 3) {
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": backTest dailyLossLmt reached = " + pnl);
				}
				return false;				
			}
		
			if (IsTradingTime(timeStart, timeEnd, 170000) && Position.Quantity == 0)
			{
				if (tradeObj.entryOrder == null || tradeObj.entryOrder.OrderState != OrderState.Working || MM_EnTrailing)
				{
					if(bsx == -1 || bsx > tradeObj.barsSincePtSl)
					{
						//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "- NewOrderAllowed=true - " + Get24HDateTime(Time[0]));
						return true;
					} //else 
						//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[bsx,barsSincePtSl]" + bsx + "," + barsSincePtSl + " - " + Get24HDateTime(Time[0]));
				} //else
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[entryOrder.OrderState,entryOrder.OrderType]" + entryOrder.OrderState + "," + entryOrder.OrderType + " - " + Get24HDateTime(Time[0]));
			}// else 
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[timeStart,timeEnd,Position.Quantity]" + timeStart + "," + timeEnd + "," + Position.Quantity + " - " + Get24HDateTime(Time[0]));
				
			return false;
		}		
		#endregion Trigger Functions
		
		#region Money Mgmt Functions
		protected bool ChangeSLPT()
		{
			int bse = BarsSinceEntry();
			double timeSinceEn = -1;
			if(bse > 0) {
				timeSinceEn = indicatorProxy.GetMinutesDiff(Time[0], Time[bse]);
			}
			
			double pl = Position.GetProfitLoss(Close[0], PerformanceUnit.Currency);
			 // If not flat print out unrealized PnL
    		if (Position.MarketPosition != MarketPosition.Flat) 
			{
         		//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- Open PnL: " + pl);
				//int nChkPnL = (int)(timeSinceEn/minutesChkPnL);
				double curPTTics = -1;
				double slPrc = tradeObj.stopLossOrder == null ? Position.AvgPrice : tradeObj.stopLossOrder.StopPrice;
				
				if(MM_PTTrailing && pl >= 12.5*(tradeObj.trailingPTTic - 2*MM_ProfitTgtIncTic))
				{
					tradeObj.trailingPTTic = tradeObj.trailingPTTic + MM_ProfitTgtIncTic;
					if(tradeObj.profitTargetOrder != null) {
						curPTTics = Math.Abs(tradeObj.profitTargetOrder.LimitPrice - Position.AvgPrice)/TickSize;
					}
					//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update PT: PnL=" + pl + ",(trailingPTTic, curPTTics, $Amt, $Amt_cur)=(" + trailingPTTic + "," + curPTTics + "," + 12.5*trailingPTTic + "," + 12.5*curPTTics + ")");
					if(tradeObj.profitTargetOrder == null || tradeObj.trailingPTTic > curPTTics)
						SetProfitTarget(CalculationMode.Ticks, tradeObj.trailingPTTic);
				}
				
				if(pl >= MM_BreakEvenAmt) { //setup breakeven order
					//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- setup SL Breakeven: (PnL, posAvgPrc)=(" + pl + "," + Position.AvgPrice + ")");
					slPrc = Position.AvgPrice;
					//SetStopLoss(0);
				}
				
				if(MM_SLTrailing) { // trailing max and min profits then converted to trailing stop after over the max
//					if(trailingSLTic > profitLockMaxTic && pl >= 12.5*(trailingSLTic + 2*profitTgtIncTic)) {
//						trailingSLTic = trailingSLTic + profitTgtIncTic;
//						if(Position.MarketPosition == MarketPosition.Long)
//							slPrc = Position.AvgPrice+TickSize*trailingSLTic;
//						if(Position.MarketPosition == MarketPosition.Short)
//							slPrc = Position.AvgPrice-TickSize*trailingSLTic;
//						Print(AccName + "- update SL over Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");						
//					}
					if(tradeObj.trailingSLTic > MM_ProfitLockMaxTic && pl >= 12.5*(tradeObj.trailingSLTic + 2*MM_ProfitTgtIncTic)) {
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + MM_ProfitTgtIncTic;
						if(tradeObj.stopLossOrder != null)
							CancelOrder(tradeObj.stopLossOrder);
						if(tradeObj.profitTargetOrder != null)
							CancelOrder(tradeObj.profitTargetOrder);
						SetTrailStop(MM_TrailingStopLossAmt);
						//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- SetTrailStop over SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");						
					}
					else if(pl >= 12.5*(MM_ProfitLockMaxTic + 2*MM_ProfitTgtIncTic)) { // lock max profits
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + MM_ProfitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = tradeObj.trailingSLTic > MM_ProfitLockMaxTic ? Position.AvgPrice+TickSize*tradeObj.trailingSLTic : Position.AvgPrice+TickSize*MM_ProfitLockMaxTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = tradeObj.trailingSLTic > MM_ProfitLockMaxTic ? Position.AvgPrice-TickSize*tradeObj.trailingSLTic :  Position.AvgPrice-TickSize*MM_ProfitLockMaxTic;
						//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
					else if(pl >= 12.5*(MM_ProfitLockMinTic + 2*MM_ProfitTgtIncTic)) { //lock min profits
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + MM_ProfitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = Position.AvgPrice+TickSize*MM_ProfitLockMinTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = Position.AvgPrice-TickSize*MM_ProfitLockMinTic;
						//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update SL Min: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
				}
				if(tradeObj.stopLossOrder == null || 
					(Position.MarketPosition == MarketPosition.Long && slPrc > tradeObj.stopLossOrder.StopPrice) ||
					(Position.MarketPosition == MarketPosition.Short && slPrc < tradeObj.stopLossOrder.StopPrice)) 
				{
					SetStopLoss(CalculationMode.Price, slPrc);
				}
			} else {
				SetStopLoss(MM_StopLossAmt);
				SetProfitTarget(MM_ProfitTargetAmt);
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
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": Cum all PnL= " + pl + ", Cum runtime PnL= " + plrt);
			return plrt;
		}		
		#endregion
		
		#region Trade Mgmt Functions
		protected void NewShortLimitOrder(string msg, double zzGap, double curGap)
		{
			double prc = (MM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]+TM_EnOffsetPnts : High[0]+TM_EnOffsetPnts;
			
//			double curGap = giParabSAR.GetCurGap();
//			double todaySAR = giParabSAR.GetTodaySAR();		
//			double prevSAR = giParabSAR.GetPrevSAR();		
//			int reverseBar = giParabSAR.GetReverseBar();		
//			int last_reverseBar = giParabSAR.GetLastReverseBar(CurrentBar);		
//			double reverseValue = giParabSAR.GetReverseValue();
//		
//			if(printOut > 1) {
//				string logText = CurrentBar + "-" + AccName + 
//				":PutOrder-(curGap,todaySAR,prevSAR,zzGap,reverseBar,last_reverseBar,reverseValue)= " 
//				+ curGap + "," + todaySAR + "," + prevSAR + "," + zzGap + "," + reverseBar + "," + last_reverseBar + "," + reverseValue ;
//				giParabSAR.PrintLog(true, !backTest, log_file, logText);
//			}
			//enCounterPBBars
			if(tradeObj.entryOrder == null) {
//				if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg + ", EnterShortLimit called short price=" + prc + "--" + Get24HDateTime(Time[0]));			
			}
			else if (tradeObj.entryOrder.OrderState == OrderState.Working) {
//				if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterShortLimit updated short price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));		
				CancelOrder(tradeObj.entryOrder);
				//entryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			tradeObj.entryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			tradeObj.barsSinceEnOrd = 0;
		}
		
		protected void NewLongLimitOrder(string msg, double zzGap, double curGap)
		{
			double prc = (MM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]-TM_EnOffsetPnts :  Low[0]-TM_EnOffsetPnts;
			
			if(tradeObj.entryOrder == null) {
				tradeObj.entryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
//				if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			else if (tradeObj.entryOrder.OrderState == OrderState.Working) {
//				if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
				CancelOrder(tradeObj.entryOrder);
				tradeObj.entryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			tradeObj.barsSinceEnOrd = 0;
		}
		
		public bool CheckEnOrder(double cur_gap)
        {
            double min_en = -1;

            if (tradeObj.entryOrder != null && tradeObj.entryOrder.OrderState == OrderState.Working)
            {
                min_en = indicatorProxy.GetMinutesDiff(tradeObj.entryOrder.Time, Time[0]);// DateTime.Now);
                //if ( IsTwoBarReversal(cur_gap, TickSize, enCounterPBBars) || (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))
				if ( (TM_BarsHoldEnOrd > 0 && tradeObj.barsSinceEnOrd >= TM_BarsHoldEnOrd) || ( TM_MinutesChkEnOrder > 0 &&  min_en >= TM_MinutesChkEnOrder))	
                {
                    CancelOrder(tradeObj.entryOrder);
                    //giParabSAR.PrintLog(true, !backTest, log_file, "Order cancelled for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					return true;
                }
				else {
					//giParabSAR.PrintLog(true, !backTest, log_file, "Order working for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					tradeObj.barsSinceEnOrd++;
				}
            }
            return false;
        }
		
		public bool CloseAllPositions() 
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, "CloseAllPosition called");
			if(Position.MarketPosition == MarketPosition.Long)
				ExitLong();
			if(Position.MarketPosition == MarketPosition.Short)
				ExitShort();
			return true;
		}
		
		public bool CancelAllOrders() 
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "- CancelAllOrders called");
			if(tradeObj.stopLossOrder != null)
				CancelOrder(tradeObj.stopLossOrder);
			if(tradeObj.profitTargetOrder != null)
				CancelOrder(tradeObj.profitTargetOrder);
			return true;
		}
		
		#endregion
		
		#region Event Handlers
		
		protected override void OnExecution(IExecution execution)
		{
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null && execution.Order.OrderState == OrderState.Filled) {
				//if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + " Exe=" + execution.Name + ",Price=" + execution.Price + "," + execution.Time.ToShortTimeString());
				//if(drawTxt) {
				//	IText it = DrawText(CurrentBar.ToString()+Time[0].ToShortTimeString(), Time[0].ToString().Substring(10)+"\r\n"+execution.Name+":"+execution.Price, 0, execution.Price, Color.Red);
				//	it.Locked = false;
				//}
			}
		}

		protected override void OnOrderUpdate(IOrder order)
		{
		    if (tradeObj.entryOrder != null && tradeObj.entryOrder == order)
		    {
				//giParabSAR.PrintLog(true, !backTest, log_file, order.ToString() + "--" + order.OrderState);
		        if (order.OrderState == OrderState.Cancelled || 
					order.OrderState == OrderState.Filled || 
					order.OrderState == OrderState.Rejected || 
					order.OrderState == OrderState.Unknown)
				{
					tradeObj.barsSinceEnOrd = 0;
					tradeObj.entryOrder = null;
				}
		    }
			
			if (order.OrderState == OrderState.Working || order.OrderType == OrderType.Stop) {
//				if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + order.ToString());
			}
			
			if(tradeObj.profitTargetOrder == null && order.Name == "Profit target" && order.OrderState == OrderState.Working) {
				tradeObj.profitTargetOrder = order;
			}
			if(tradeObj.stopLossOrder == null && order.Name == "Stop loss" && (order.OrderState == OrderState.Accepted || order.OrderState == OrderState.Working)) {
				tradeObj.stopLossOrder = order;
			}
			
			if( order.OrderState == OrderState.Filled || order.OrderState == OrderState.Cancelled) {
				if(order.Name == "Stop loss")
					tradeObj.stopLossOrder = null;
				if(order.Name == "Profit target")
					tradeObj.profitTargetOrder = null;
			}
		}

		protected override void OnPositionUpdate(IPosition position)
		{
			//Print(position.ToString() + "--MarketPosition=" + position.MarketPosition);
			if (position.MarketPosition == MarketPosition.Flat)
			{
				tradeObj.trailingPTTic = MM_ProfitTargetAmt/12.5;
				tradeObj.trailingSLTic = MM_StopLossAmt/12.5;
			}
		}
		
		#endregion
		
		#region TM Properties
        [Description("Offeset points for limit price entry")]
        [GridCategory("Parameters")]
        public double TM_EnOffsetPnts
        {
            get { return tradeObj.enOffsetPnts; }
            set { tradeObj.enOffsetPnts = Math.Max(0, value); }
        }
        [Description("How long to check entry order filled or not")]
        [GridCategory("Parameters")]
        public int TM_MinutesChkEnOrder
        {
            get { return tradeObj.minutesChkEnOrder; }
            set { tradeObj.minutesChkEnOrder = Math.Max(0, value); }
        }
		
        [Description("How long to check P&L")]
        [GridCategory("Parameters")]
        public int TM_MinutesChkPnL
        {
            get { return tradeObj.minutesChkPnL; }
            set { tradeObj.minutesChkPnL = Math.Max(-1, value); }
        }		

        [Description("Bar count since en order issued")]
        [GridCategory("Parameters")]
        public int TM_BarsHoldEnOrd
        {
            get { return tradeObj.barsHoldEnOrd; }
            set { tradeObj.barsHoldEnOrd = Math.Max(1, value); }
        }
		
        [Description("Bar count for en order counter pullback")]
        [GridCategory("Parameters")]
        public int TM_EnCounterPBBars
        {
            get { return tradeObj.enCounterPBBars; }
            set { tradeObj.enCounterPBBars = Math.Max(-1, value); }
        }		
				
		[Description("Bar count since last filled PT or SL")]
        [GridCategory("Parameters")]
        public int TM_BarsSincePtSl
        {
            get { return tradeObj.barsSincePtSl; }
            set { tradeObj.barsSincePtSl = Math.Max(1, value); }
        }
		
		[Description("Bar count before checking P&L")]
        [GridCategory("Parameters")]
        public int TM_BarsToCheckPL
        {
            get { return tradeObj.barsToCheckPL; }
            set { tradeObj.barsToCheckPL = Math.Max(1, value); }
        }
		
		#endregion
		
		#region MM Properties
        [Description("Money amount of profit target")]
        [GridCategory("Parameters")]
        public double MM_ProfitTargetAmt
        {
            get { return tradeObj.profitTargetAmt; }
            set { tradeObj.profitTargetAmt = Math.Max(0, value); }
        }

        [Description("Money amount for profit target increasement")]
        [GridCategory("Parameters")]
        public double MM_ProfitTgtIncTic
        {
            get { return tradeObj.profitTgtIncTic; }
            set { tradeObj.profitTgtIncTic = Math.Max(0, value); }
        }
		
        [Description("Tick amount for min profit locking")]
        [GridCategory("Parameters")]
        public double MM_ProfitLockMinTic
        {
            get { return tradeObj.profitLockMinTic; }
            set { tradeObj.profitLockMinTic = Math.Max(0, value); }
        }

		[Description("Tick amount for max profit locking")]
        [GridCategory("Parameters")]
        public double MM_ProfitLockMaxTic
        {
            get { return tradeObj.profitLockMaxTic; }
            set { tradeObj.profitLockMaxTic = Math.Max(0, value); }
        }
		
        [Description("Money amount of stop loss")]
        [GridCategory("Parameters")]
        public double MM_StopLossAmt
        {
            get { return tradeObj.stopLossAmt; }
            set { tradeObj.stopLossAmt = Math.Max(0, value); }
        }
		
        [Description("Money amount of trailing stop loss")]
        [GridCategory("Parameters")]
        public double MM_TrailingStopLossAmt
        {
            get { return tradeObj.trailingSLAmt; }
            set { tradeObj.trailingSLAmt = Math.Max(0, value); }
        }
		
		[Description("Money amount for stop loss increasement")]
        [GridCategory("Parameters")]
        public double MM_StopLossIncTic
        {
            get { return tradeObj.stopLossIncTic; }
            set { tradeObj.stopLossIncTic = Math.Max(0, value); }
        }
		
        [Description("Break Even amount")]
        [GridCategory("Parameters")]
        public double MM_BreakEvenAmt
        {
            get { return tradeObj.breakEvenAmt; }
            set { tradeObj.breakEvenAmt = Math.Max(0, value); }
        }

		[Description("Daily Loss Limit amount")]
        [GridCategory("Parameters")]
        public double MM_DailyLossLmt
        {
            get { return tradeObj.dailyLossLmt; }
            set { tradeObj.dailyLossLmt = Math.Min(-100, value); }
        }
		[Description("Use trailing entry every bar")]
        [GridCategory("Parameters")]
        public bool MM_EnTrailing
        {
            get { return tradeObj.enTrailing; }
            set { tradeObj.enTrailing = value; }
        }
		
		[Description("Use trailing profit target every bar")]
        [GridCategory("Parameters")]
        public bool MM_PTTrailing
        {
            get { return tradeObj.ptTrailing; }
            set { tradeObj.ptTrailing = value; }
        }
		
		[Description("Use trailing stop loss every bar")]
        [GridCategory("Parameters")]
        public bool MM_SLTrailing
        {
            get { return tradeObj.slTrailing; }
            set { tradeObj.slTrailing = value; }
        }		
		#endregion
	}
}