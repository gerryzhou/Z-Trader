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

    partial class Strategy {
	
		#region Money Mgmt variables
		protected double profitTargetAmt = 350; //36 Default(450-650 USD) setting for profitTargetAmt
		protected double profitTgtIncTic = 6; //8 Default tick Amt for ProfitTarget increase Amt
		protected double profitLockMinTic = 16; //24 Default ticks Amt for Min Profit locking
		protected double profitLockMaxTic = 30; //80 Default ticks Amt for Max Profit locking
        protected double stopLossAmt = 200; //16 Default setting for stopLossAmt
		protected double stopLossIncTic = 4; //4 Default tick Amt for StopLoss increase Amt
		protected double breakEvenAmt = 150; //150 the profits amount to trigger setting breakeven order
		protected double trailingSLAmt = 100; //300 Default setting for trailing Stop Loss Amt
		protected double dailyLossLmt = -200; //-300 the daily loss limit amount

		protected bool enTrailing = true; //use trailing entry: counter pullback bars or simple enOffsetPnts
		protected bool ptTrailing = true; //use trailing profit target every bar
		protected bool slTrailing = true; //use trailing stop loss every bar		
		#endregion
		
		#region Trade Mgmt variables
		protected double enOffsetPnts = 1.25;//Price offset for entry
		protected int enCounterPBBars = 1;//Bar count of pullback for breakout entry setup
		
		protected int minutesChkEnOrder = 20; //how long before checking an entry order filled or not
		protected int minutesChkPnL = 30; //how long before checking P&L
		
		protected int barsHoldEnOrd = 10; // Bars count since en order was issued
        protected int barsSincePtSl = 1; // Bar count since last P&L was filled
		protected int barsToCheckPL = 2; // Bar count to check P&L since the entry		
		#endregion
		
		#region Order Objects
		protected IOrder entryOrder = null;
		protected IOrder profitTargetOrder = null;
		protected IOrder stopLossOrder = null;
		protected double trailingPTTic = 36; //400, tick amount of trailing target
		protected double trailingSLTic = 16; // 200, tick amount of trailing stop loss
		protected int barsSinceEnOrd = 0; // bar count since the en order issued		
		#endregion
		
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
			if(!backTest && (plrt <= dailyLossLmt || pnl <= dailyLossLmt))
			{
				if(printOut > -1) {
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": dailyLossLmt reached = " + pnl + "," + plrt);
				}
				return false;
			}
			if (backTest && pnl <= dailyLossLmt) {
				if(printOut > 3) {
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": backTest dailyLossLmt reached = " + pnl);
				}
				return false;				
			}
		
			if (IsTradingTime(timeStart, timeEnd, 170000) && Position.Quantity == 0)
			{
				if (entryOrder == null || entryOrder.OrderState != OrderState.Working || enTrailing)
				{
					if(bsx == -1 || bsx > barsSincePtSl)
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
				double slPrc = stopLossOrder == null ? Position.AvgPrice : stopLossOrder.StopPrice;
				
				if(ptTrailing && pl >= 12.5*(trailingPTTic - 2*profitTgtIncTic))
				{
					trailingPTTic = trailingPTTic + profitTgtIncTic;
					if(profitTargetOrder != null) {
						curPTTics = Math.Abs(profitTargetOrder.LimitPrice - Position.AvgPrice)/TickSize;
					}
					//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update PT: PnL=" + pl + ",(trailingPTTic, curPTTics, $Amt, $Amt_cur)=(" + trailingPTTic + "," + curPTTics + "," + 12.5*trailingPTTic + "," + 12.5*curPTTics + ")");
					if(profitTargetOrder == null || trailingPTTic > curPTTics)
						SetProfitTarget(CalculationMode.Ticks, trailingPTTic);
				}
				
				if(pl >= breakEvenAmt) { //setup breakeven order
					//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- setup SL Breakeven: (PnL, posAvgPrc)=(" + pl + "," + Position.AvgPrice + ")");
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
						//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- SetTrailStop over SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");						
					}
					else if(pl >= 12.5*(profitLockMaxTic + 2*profitTgtIncTic)) { // lock max profits
						trailingSLTic = trailingSLTic + profitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = trailingSLTic > profitLockMaxTic ? Position.AvgPrice+TickSize*trailingSLTic : Position.AvgPrice+TickSize*profitLockMaxTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = trailingSLTic > profitLockMaxTic ? Position.AvgPrice-TickSize*trailingSLTic :  Position.AvgPrice-TickSize*profitLockMaxTic;
						//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
					else if(pl >= 12.5*(profitLockMinTic + 2*profitTgtIncTic)) { //lock min profits
						trailingSLTic = trailingSLTic + profitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = Position.AvgPrice+TickSize*profitLockMinTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = Position.AvgPrice-TickSize*profitLockMinTic;
						//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update SL Min: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
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
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": Cum all PnL= " + pl + ", Cum runtime PnL= " + plrt);
			return plrt;
		}		
		#endregion
		
		#region Trade Mgmt Functions
		protected void NewShortLimitOrder(string msg, double zzGap, double curGap)
		{
			double prc = (enTrailing && enCounterPBBars>0) ? Close[0]+enOffsetPnts : High[0]+enOffsetPnts;
			
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
			if(entryOrder == null) {
//				if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg + ", EnterShortLimit called short price=" + prc + "--" + Get24HDateTime(Time[0]));			
			}
			else if (entryOrder.OrderState == OrderState.Working) {
//				if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterShortLimit updated short price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));		
				CancelOrder(entryOrder);
				//entryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			entryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			barsSinceEnOrd = 0;
		}
		
		protected void NewLongLimitOrder(string msg, double zzGap, double curGap)
		{
			double prc = (enTrailing && enCounterPBBars>0) ? Close[0]-enOffsetPnts :  Low[0]-enOffsetPnts;
			
			if(entryOrder == null) {
				entryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
//				if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			else if (entryOrder.OrderState == OrderState.Working) {
//				if(printOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
				CancelOrder(entryOrder);
				entryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			barsSinceEnOrd = 0;
		}
		
		public bool CheckEnOrder(double cur_gap)
        {
            double min_en = -1;

            if (entryOrder != null && entryOrder.OrderState == OrderState.Working)
            {
                min_en = indicatorProxy.GetMinutesDiff(entryOrder.Time, Time[0]);// DateTime.Now);
                //if ( IsTwoBarReversal(cur_gap, TickSize, enCounterPBBars) || (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))
				if ( (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))	
                {
                    CancelOrder(entryOrder);
                    //giParabSAR.PrintLog(true, !backTest, log_file, "Order cancelled for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					return true;
                }
				else {
					//giParabSAR.PrintLog(true, !backTest, log_file, "Order working for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					barsSinceEnOrd++;
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
		    if (entryOrder != null && entryOrder == order)
		    {
				//giParabSAR.PrintLog(true, !backTest, log_file, order.ToString() + "--" + order.OrderState);
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
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + order.ToString());
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
		#endregion
	}
}