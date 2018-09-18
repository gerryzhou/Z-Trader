#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
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
    /// ZigZag backtesting 
    /// </summary>
    [Description("ZigZag backtesting ")]
    public class GSZigZagBT : Strategy
    {
        #region Variables
        // Wizard generated variables
        
        // User defined variables (add any user defined variables below)
		private GIZigZagBase giZigZag = null;
		
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
			indicatorProxy = IndicatorProxy(11,5,9,5);
			Add(indicatorProxy);
			TG_AccName = indicatorProxy.GetAccName();//.GetTsTAccName(Account.Name);
			
			giZigZag = GIZigZagBase(NinjaTrader.Data.DeviationType.Points, 3, true);
            Add(giZigZag);
			
            SetProfitTarget(300);
            SetStopLoss(175, false);

			SetInitParams();
			InitTradeCmd();
			InitTrigger();
			InitTradeMgmt();
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			CheckCmd();
			
			ExecuteCommand();
			
			double curGap = giZigZag.GetCurZZGap();
            // Condition set 1
			Print(CurrentBar + ":" + Time[0] +  ":ZZHi,ZZLo,HiBar,LoBar,trDir,CurZZGap,zzMode=[" + giZigZag.ZigZagHigh[0] + "," + giZigZag.ZigZagLow[0] + "],[" + giZigZag.HighBar(1, 1, CurrentBar-BarsRequired) + "," + giZigZag.LowBar(1, 1, CurrentBar-BarsRequired)+"] " + giZigZag.GetTrendDir() + "," + curGap + "<" + giZigZag.GetBarZZMode(0) + ">");
//            if (GIZigZagBase("", true, DeviationType.Points, 1, true).ZigZagHigh[0] >= Variable0)
//            {
//                EnterShortLimit(DefaultQuantity, 0, "shortLmt");
//            }
			giZigZag.PrintLog(true, false, CurrentBar + "::" + Time[0] +  ":ZZHi,ZZLo,HiBar,LoBar,trDir,CurZZGap,zzMode=[" + giZigZag.ZigZagHigh[0] + "," + giZigZag.ZigZagLow[0] + "],[" + giZigZag.HighBar(1, 1, CurrentBar-BarsRequired) + "," + giZigZag.LowBar(1, 1, CurrentBar-BarsRequired)+"] " + giZigZag.GetTrendDir() + "," + curGap + "<" + giZigZag.GetBarZZMode(0) + ">");
        }
    }
}
