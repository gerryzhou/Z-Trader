#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
	public class IndicatorSignal {
		public TrendDirection TrendDir = TrendDirection.UnKnown; //1=up, -1=down, 0=flat/unknown
		public Breakout BreakoutDir = Breakout.UnKnown; //1=bk up, -1=bk down, 0=no bk/unknown
		public Reversal ReversalDir = Reversal.UnKnown; //1=rev up, -1=rev down, 0=no rev/unknown		
	}
	
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("Enter the description of your new custom indicator here")]
    public class IndicatorProxy : Indicator
    {
        #region Variables
        // User defined variables (add any user defined variables below)
            private int startH = 9; // Default setting for StartH
            private int startM = 5; // Default setting for StartM
            private int endH = 11; // Default setting for EndH
            private int endM = 5; // Default setting for EndM
			private string accName = ""; //account name from strategy, extracting simply string for print/log;
			
		#endregion
		
		private List<Indicator> listIndicator = new List<Indicator>();

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Add(new Plot(Color.FromKnownColor(KnownColor.Black), PlotStyle.Block, "StartHM"));
            Add(new Plot(Color.FromKnownColor(KnownColor.DarkBlue), PlotStyle.Block, "EndHM"));
            Overlay				= true;
			accName = GetTsTAccName(Strategy.Account.Name);
			indicatorSignal = new IndicatorSignal();
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
			if(CurrentBar >= BarsRequired) {
				if(GetTimeDiffByHM(StartH, StartM, Time[1]) > 0 && GetTimeDiffByHM(StartH, StartM, Time[0]) <= 0)
            		StartHM.Set(High[0]+0.25);
				if(GetTimeDiffByHM(EndH, EndM, Time[1]) > 0 && GetTimeDiffByHM(EndH, EndM, Time[0]) <= 0)
            		EndHM.Set(Low[0]-0.25);
			}
        }
		
        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries StartHM
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries EndHM
        {
            get { return Values[1]; }
        }

        [Description("Hour of start trading")]
        [GridCategory("Parameters")]
        public int StartH
        {
            get { return startH; }
            set { startH = Math.Max(0, value); }
        }

        [Description("Min of start trading")]
        [GridCategory("Parameters")]
        public int StartM
        {
            get { return startM; }
            set { startM = Math.Max(0, value); }
        }

        [Description("Hour of end trading")]
        [GridCategory("Parameters")]
        public int EndH
        {
            get { return endH; }
            set { endH = Math.Max(0, value); }
        }

        [Description("Min of end trading")]
        [GridCategory("Parameters")]
        public int EndM
        {
            get { return endM; }
            set { endM = Math.Max(0, value); }
        }
        #endregion
		
		#region Methods
		public string GetAccName() {
			return accName;
		}
		
		public void AddIndicator(Indicator i) {
			this.listIndicator.Add(i);
		}

		#endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private IndicatorProxy[] cacheIndicatorProxy = null;

        private static IndicatorProxy checkIndicatorProxy = new IndicatorProxy();

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public IndicatorProxy IndicatorProxy(int endH, int endM, int startH, int startM)
        {
            return IndicatorProxy(Input, endH, endM, startH, startM);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public IndicatorProxy IndicatorProxy(Data.IDataSeries input, int endH, int endM, int startH, int startM)
        {
            if (cacheIndicatorProxy != null)
                for (int idx = 0; idx < cacheIndicatorProxy.Length; idx++)
                    if (cacheIndicatorProxy[idx].EndH == endH && cacheIndicatorProxy[idx].EndM == endM && cacheIndicatorProxy[idx].StartH == startH && cacheIndicatorProxy[idx].StartM == startM && cacheIndicatorProxy[idx].EqualsInput(input))
                        return cacheIndicatorProxy[idx];

            lock (checkIndicatorProxy)
            {
                checkIndicatorProxy.EndH = endH;
                endH = checkIndicatorProxy.EndH;
                checkIndicatorProxy.EndM = endM;
                endM = checkIndicatorProxy.EndM;
                checkIndicatorProxy.StartH = startH;
                startH = checkIndicatorProxy.StartH;
                checkIndicatorProxy.StartM = startM;
                startM = checkIndicatorProxy.StartM;

                if (cacheIndicatorProxy != null)
                    for (int idx = 0; idx < cacheIndicatorProxy.Length; idx++)
                        if (cacheIndicatorProxy[idx].EndH == endH && cacheIndicatorProxy[idx].EndM == endM && cacheIndicatorProxy[idx].StartH == startH && cacheIndicatorProxy[idx].StartM == startM && cacheIndicatorProxy[idx].EqualsInput(input))
                            return cacheIndicatorProxy[idx];

                IndicatorProxy indicator = new IndicatorProxy();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                indicator.EndH = endH;
                indicator.EndM = endM;
                indicator.StartH = startH;
                indicator.StartM = startM;
                Indicators.Add(indicator);
                indicator.SetUp();

                IndicatorProxy[] tmp = new IndicatorProxy[cacheIndicatorProxy == null ? 1 : cacheIndicatorProxy.Length + 1];
                if (cacheIndicatorProxy != null)
                    cacheIndicatorProxy.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheIndicatorProxy = tmp;
                return indicator;
            }
        }
    }
}

// This namespace holds all market analyzer column definitions and is required. Do not change it.
namespace NinjaTrader.MarketAnalyzer
{
    public partial class Column : ColumnBase
    {
        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.IndicatorProxy IndicatorProxy(int endH, int endM, int startH, int startM)
        {
            return _indicator.IndicatorProxy(Input, endH, endM, startH, startM);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.IndicatorProxy IndicatorProxy(Data.IDataSeries input, int endH, int endM, int startH, int startM)
        {
            return _indicator.IndicatorProxy(input, endH, endM, startH, startM);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.IndicatorProxy IndicatorProxy(int endH, int endM, int startH, int startM)
        {
            return _indicator.IndicatorProxy(Input, endH, endM, startH, startM);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.IndicatorProxy IndicatorProxy(Data.IDataSeries input, int endH, int endM, int startH, int startM)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.IndicatorProxy(input, endH, endM, startH, startM);
        }
    }
}
#endregion
