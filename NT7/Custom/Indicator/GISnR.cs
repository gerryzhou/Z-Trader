#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Support and resistance indicator
    /// </summary>
    [Description("Support and resistance indicator")]
    public class GISnR : Indicator
    {
        #region Variables
        // Wizard generated variables
            private int startH = 1; // Default setting for StartH
            private int startM = 1; // Default setting for StartM
            private int endH = 8; // Default setting for EndH
            private int endM = 25; // Default setting for EndM
			private int srLinesBack = 2; //how many lines draw, today, ovnight, last-day
			
        // User defined variables (add any user defined variables below)
			private DateTime sessionBegin ;
			private DateTime sessionEnd ;
			private double Price_Spt_LD; //yesterday
			private double Price_Rst_LD;
			private double Price_Spt_ON; //Overnight
			private double Price_Rst_ON;
			private double Price_Spt_TD; //today
			private double Price_Rst_TD;
		
			private Plot plot_Spt_LD;
			private Plot plot_Rst_LD;
			private Plot plot_Spt_ON;
			private Plot plot_Rst_ON;
			private Plot plot_Spt_TD;
			private Plot plot_Rst_TD;		
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
			Pen pen_spt_ld = new Pen(Color.FromKnownColor(KnownColor.DarkGreen), 1.5f);
			pen_spt_ld.DashStyle = DashStyle.Dash;
			Pen pen_rst_ld = new Pen(Color.FromKnownColor(KnownColor.DarkViolet), 1.5f);
			pen_rst_ld.DashStyle = DashStyle.Dash;
			Pen pen_spt_on = new Pen(Color.FromKnownColor(KnownColor.DarkGreen), 2f);
			pen_spt_on.DashStyle = DashStyle.Dot;
			Pen pen_rst_on = new Pen(Color.FromKnownColor(KnownColor.DarkViolet), 2f);
			pen_rst_on.DashStyle = DashStyle.Dot;
			Pen pen_spt_td = new Pen(Color.FromKnownColor(KnownColor.DarkGreen), 3.5f);
			pen_spt_td.DashStyle = DashStyle.Dot;
			Pen pen_rst_td = new Pen(Color.FromKnownColor(KnownColor.DarkViolet), 3.5f);
			pen_rst_td.DashStyle = DashStyle.Dot;
			
			plot_Spt_LD = new Plot(pen_spt_ld, PlotStyle.HLine, "SptLD");
			plot_Rst_LD = new Plot(pen_rst_ld, PlotStyle.HLine, "RstLD");
			plot_Spt_ON = new Plot(pen_spt_on, PlotStyle.HLine, "SptON");
			plot_Rst_ON = new Plot(pen_rst_on, PlotStyle.HLine, "RstON");
			plot_Spt_TD = new Plot(pen_spt_td, PlotStyle.HLine, "SptTD");
			plot_Rst_TD = new Plot(pen_rst_td, PlotStyle.HLine, "RstTD");
			
            Add(plot_Spt_LD);
            Add(plot_Rst_LD);
            Add(plot_Spt_ON);
            Add(plot_Rst_ON);
            Add(plot_Spt_TD);
            Add(plot_Rst_TD);
			
            Overlay				= true;
			PlotsConfigurable = true;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			if(CurrentBar == BarsRequired) {
				Bars.Session.GetNextBeginEnd(Time[0], out sessionBegin, out sessionEnd);
				Print("Bars.Session.TimeZoneInfo=" + Bars.Session.TimeZoneInfo.ToString());

			}
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
//			double prc_spt = Low[LowestBar(Close, Bars.BarsSinceSession - 1)];
//			double prc_rst = High[HighestBar(High, Bars.BarsSinceSession - 1)];

			if(CurrentBar > BarsRequired) {
				int end_H = (Time[0].Hour >= sessionBegin.Hour && EndH < sessionBegin.Hour) ? EndH+24 : EndH;
				if(GetTimeDiffByHM(end_H, EndM, Time[0].Hour, Time[0].Minute) <= 0) {
					Price_Spt_ON = Low[LowestBar(Close, Bars.BarsSinceSession - 1)];
					Price_Rst_ON = High[HighestBar(High, Bars.BarsSinceSession - 1)];
				}
				
				if(Time[1].Hour < sessionBegin.Hour && Time[0].Hour >= sessionBegin.Hour) {
					Price_Spt_LD = PriorDayOHLC().PriorLow[0]; //Bars.GetDayBar(1).Low;
					Price_Rst_LD = PriorDayOHLC().PriorHigh[0]; //Bars.GetDayBar(1).High;
				}
//				else {
//					Price_Spt_LD = CurrentDayOHL().CurrentLow[0];
//					Price_Rst_LD = CurrentDayOHL().CurrentHigh[0];
//				}

				Price_Spt_TD = CurrentDayOHL().CurrentLow[0];
				Price_Rst_TD = CurrentDayOHL().CurrentHigh[0];

				if(srLinesBack >= 3) {
					Spt_LD.Set(Price_Spt_LD);
					Rst_LD.Set(Price_Rst_LD);
				}
				if (srLinesBack >= 2) {
					Spt_ON.Set(Price_Spt_ON);
					Rst_ON.Set(Price_Rst_ON);
				}

				Spt_TD.Set(Price_Spt_TD);
				Rst_TD.Set(Price_Rst_TD);
				
				string text = CurrentBar + "-[" + Time[0] +  "]-[BarsSinceSession,SessionBegin.Hour,sessionEnd.Hour] [Price_Spt,Price_Rst]=[" + Bars.BarsSinceSession + "," + sessionBegin.Hour + "," + sessionEnd.Hour + "], [" + Price_Spt_ON + "," + Price_Rst_ON + "] " ;
				PrintLog(true, false, text);
			}
        }

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Spt_LD
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Rst_LD
        {
            get { return Values[1]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Spt_ON
        {
            get { return Values[2]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Rst_ON
        {
            get { return Values[3]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Spt_TD
        {
            get { return Values[4]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries Rst_TD
        {
            get { return Values[5]; }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int SRLinesBack
        {
            get { return srLinesBack; }
            set { srLinesBack = Math.Max(1, value); }
        }
		
        [Description("")]
        [GridCategory("Parameters")]
        public int StartH
        {
            get { return startH; }
            set { startH = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int StartM
        {
            get { return startM; }
            set { startM = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int EndH
        {
            get { return endH; }
            set { endH = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int EndM
        {
            get { return endM; }
            set { endM = Math.Max(0, value); }
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
        private GISnR[] cacheGISnR = null;

        private static GISnR checkGISnR = new GISnR();

        /// <summary>
        /// Support and resistance indicator
        /// </summary>
        /// <returns></returns>
        public GISnR GISnR(int endH, int endM, int sRLinesBack, int startH, int startM)
        {
            return GISnR(Input, endH, endM, sRLinesBack, startH, startM);
        }

        /// <summary>
        /// Support and resistance indicator
        /// </summary>
        /// <returns></returns>
        public GISnR GISnR(Data.IDataSeries input, int endH, int endM, int sRLinesBack, int startH, int startM)
        {
            if (cacheGISnR != null)
                for (int idx = 0; idx < cacheGISnR.Length; idx++)
                    if (cacheGISnR[idx].EndH == endH && cacheGISnR[idx].EndM == endM && cacheGISnR[idx].SRLinesBack == sRLinesBack && cacheGISnR[idx].StartH == startH && cacheGISnR[idx].StartM == startM && cacheGISnR[idx].EqualsInput(input))
                        return cacheGISnR[idx];

            lock (checkGISnR)
            {
                checkGISnR.EndH = endH;
                endH = checkGISnR.EndH;
                checkGISnR.EndM = endM;
                endM = checkGISnR.EndM;
                checkGISnR.SRLinesBack = sRLinesBack;
                sRLinesBack = checkGISnR.SRLinesBack;
                checkGISnR.StartH = startH;
                startH = checkGISnR.StartH;
                checkGISnR.StartM = startM;
                startM = checkGISnR.StartM;

                if (cacheGISnR != null)
                    for (int idx = 0; idx < cacheGISnR.Length; idx++)
                        if (cacheGISnR[idx].EndH == endH && cacheGISnR[idx].EndM == endM && cacheGISnR[idx].SRLinesBack == sRLinesBack && cacheGISnR[idx].StartH == startH && cacheGISnR[idx].StartM == startM && cacheGISnR[idx].EqualsInput(input))
                            return cacheGISnR[idx];

                GISnR indicator = new GISnR();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                indicator.EndH = endH;
                indicator.EndM = endM;
                indicator.SRLinesBack = sRLinesBack;
                indicator.StartH = startH;
                indicator.StartM = startM;
                Indicators.Add(indicator);
                indicator.SetUp();

                GISnR[] tmp = new GISnR[cacheGISnR == null ? 1 : cacheGISnR.Length + 1];
                if (cacheGISnR != null)
                    cacheGISnR.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheGISnR = tmp;
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
        /// Support and resistance indicator
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.GISnR GISnR(int endH, int endM, int sRLinesBack, int startH, int startM)
        {
            return _indicator.GISnR(Input, endH, endM, sRLinesBack, startH, startM);
        }

        /// <summary>
        /// Support and resistance indicator
        /// </summary>
        /// <returns></returns>
        public Indicator.GISnR GISnR(Data.IDataSeries input, int endH, int endM, int sRLinesBack, int startH, int startM)
        {
            return _indicator.GISnR(input, endH, endM, sRLinesBack, startH, startM);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Support and resistance indicator
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.GISnR GISnR(int endH, int endM, int sRLinesBack, int startH, int startM)
        {
            return _indicator.GISnR(Input, endH, endM, sRLinesBack, startH, startM);
        }

        /// <summary>
        /// Support and resistance indicator
        /// </summary>
        /// <returns></returns>
        public Indicator.GISnR GISnR(Data.IDataSeries input, int endH, int endM, int sRLinesBack, int startH, int startM)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.GISnR(input, endH, endM, sRLinesBack, startH, startM);
        }
    }
}
#endregion
