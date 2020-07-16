#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class GStrategyBaseEx : Strategy
	{
		//protected GIndicatorBase indicatorProxy;
		
		#region Init Functions
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				string ud_dir = NinjaTrader.Core.Globals.UserDataDir;
				Print(this.Name + "set defaults called, NinjaTrader.Core.Globals.UserDataDir=" + ud_dir);
				SetInitParams();
				//	AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.TriangleRight, "CustomPlot1");
				//	AddLine(Brushes.Orange, 1, "CustomLine1");
				//	AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.HLine, "CustomPlot2");
			}
			else if (State == State.Configure)
			{				
				//CurrentTrade.OnCurPositionUpdate += this.OnPositionUpdate;
				//InitTradeMgmt();
				//AddDataSeries("@SPX500", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded)
			{
				//CurrentTrade = new CurrentTradeBase(this);
				//IndicatorProxy = GIndicatorProxy(this);//1, Account.Name);
				//GUtils.UpdateProperties(this, ReadCmdPara(), IndicatorProxy);
				//ReadCmdParaObj();
				//	CurrentTrade.InstStrategy = ;
				//tradeSignal = new TradeSignal();
				//CancelAccountOrders();
				//Account.CancelAllOrders(Instrument);
				//Account.Flatten(new List<Instrument>{Instrument});
			}
			else if (State == State.Terminated) {
//				if(IndicatorProxy != null) {
//					IndicatorProxy.Log2Disk = true;
//					IndicatorProxy.PrintLog(true, true, CurrentBar + ":" + this.Name + " terminated!");
//				}
			}
		}
		
		public void SetInitParams() {
			Print(this.Name + " Set initParams called....");			
			Description									= @"GS Z-Trader base;";
			Name										= "GSZTraderBaseEx";
			Calculate									= Calculate.OnPriceChange;
			//This property does not work for unmanaged order entry;
			EntriesPerDirection							= 2;
			//QuantityType = QuantityType.DefaultQuantity;
			SetOrderQuantity							= SetOrderQuantity.DefaultQuantity;
			DefaultQuantity								= 5;			
			EntryHandling								= EntryHandling.AllEntries;
			//SyncAccountPosition = true;
			IsExitOnSessionCloseStrategy				= true;
			// Triggers the exit on close function 30 seconds prior to session end			
			ExitOnSessionCloseSeconds					= 30;
			IsFillLimitOnTouch							= true;
			MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
			OrderFillResolution							= OrderFillResolution.Standard;
			Slippage									= 0;
			StartBehavior								= StartBehavior.WaitUntilFlatSynchronizeAccount;
			TimeInForce									= TimeInForce.Day;
			TraceOrders									= true;
			RealtimeErrorHandling						= RealtimeErrorHandling.IgnoreAllErrors;
			StopTargetHandling							= StopTargetHandling.ByStrategyPosition;
			BarsRequiredToTrade							= 100;
			// Disable this property for performance gains in Strategy Analyzer optimizations
			// See the Help Guide for additional information
			IsInstantiatedOnEachOptimizationIteration	= true;
			WaitForOcoClosingBracket					= false;
			
			//CustomColor1					= Brushes.Orange;
//			StartH	= DateTime.Parse("08:25", System.Globalization.CultureInfo.InvariantCulture);
			// Use Unmanaged order methods
        	IsUnmanaged = true;
//			AlgoMode = AlgoModeType.Trading;
		}
		#endregion
			
		#region Utilities Functions
		public bool IsLiveTrading() {
			if(State == State.Realtime)
				return true;
			else return false;
		}
		
//		public void SetPrintOut(int i) {
//			PrintOut = IndicatorProxy.PrintOut + i;
//		}
		#endregion
		
		#region Variables for Properties
		
		private string accName = "Sim101";
//		private AlgoModeType algoMode = AlgoModeType.Trading;
		private bool backTest = true; //if it runs for backtesting;
		private bool liveModelUpdate = false; //if it keeps model updated bar by bar
		private string liveUpdateURL = "https://thetradingbook.com/modelupdate"; //model updated URL
		private int printOut = 1; // Default setting for PrintOut
		
		#endregion

	}
}
