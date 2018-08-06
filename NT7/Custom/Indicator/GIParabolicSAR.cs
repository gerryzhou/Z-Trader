// 
// Copyright (C) 2009, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//

#region Using declarations
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;

#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Parabolic SAR according to Stocks and Commodities magazine V 11:11 (477-479).
    /// </summary>
    [Description("GI Parabolic SAR")]
    public class GIParabolicSAR : Indicator
    {
        #region Variables
		private double acceleration			= 0.002;
		private double accelerationStep		= 0.002;
		private double accelerationMax		= 0.2;

		private bool   longPosition;
		//private DataSeries sarSeries ;		// Save Extreme Price for each bar
		private double xp =0;		// Extreme Price
		private double af					= 0;		// Acceleration factor
		private double todaySAR				= 0;		// SAR value
		private double prevSAR				= 0;
		
		private IntSeries reverseBar;
		//private double reverseValue			= 0;
		private DataSeries reverseValue	;
		
		private int prevBar					= 0;
		//private IntSeries xpBar	; // Bar number of the xp bar for current bar
		private bool afIncreased			= false;
		
		private double curZZGap				= 0;
		private	int pbSARCrossBarsAgo		= 0 ;
		//private int barsSinceLastCross		= 0 ; dup with pbSARCrossBarsAgo
		private Color dotColor				= Color.Orange;
		
		private ILine zigZagLine			= null; // The current ZigZag ended by the last bar of the chart;
		private const string tagZZLine	= "tagZZLine-" ;// the tag of the line object for the current ZZ line;
        private IText curGapText			= null; // The text for the ZZ ended by last bar of the chart; 
		private const string tagCurGapText	= "tagCurGapText-"; // the tag of the line object for curGapText;
		protected int printOut = 3; //0,1,2,3 more print
		protected bool drawTxt = false; //
		protected string accName = "";
		protected string symbol = "";
		protected string log_file = ""; //
		private bool backTest = true; //if it runs for backtesting;
		
		protected List<ZigZagSwing>		zzSwings;
		
		protected PriceAction curPriceAction = new PriceAction(PriceActionType.UnKnown, -1, -1, -1, -1);
		
		#endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
			symbol = Instrument.FullName;
            Add(new Plot(dotColor, PlotStyle.Dot, "GI pbSAR"));
            Overlay					= true;	// Plots the indicator on top of price
			reverseBar = new IntSeries(this, MaximumBarsLookBack.Infinite);
			reverseValue = new DataSeries(this, MaximumBarsLookBack.Infinite);
			zzSwings = new List<ZigZagSwing>();
			//String AccName = GetTsTAccName(Account.Name);//"Sim101";			
			if(backTest)
				ReadSpvPRList();
			else
				ReadSpvFile(@"C:\inetpub\wwwroot\nt_files\pattern\", "ES ##-##");
			log_file = GetFileNameByDateTime(DateTime.Now, @"C:\inetpub\wwwroot\nt_files\log\", AccName, symbol, "log");
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
		/// BarNO started with 0;
        /// </summary>
        protected override void OnBarUpdate()
        {
			//Print("BarNo, Time=" + CurrentBar + "," + Time[0].ToShortTimeString());
			DayWeekMonthCount();
			zzSwings.Add(new ZigZagSwing(-1,-1,0,0));
			if (CurrentBar < 3) 
				return;
			
			else if (CurrentBar == 3)
			{
				// Determine initial position
				longPosition = High[0] > High[1] ? true : false;
				if (longPosition)					
					xp = MAX(High, CurrentBar)[0];				
				else
					xp = MIN(Low, CurrentBar)[0];
				af = AccelerationInit;
				Value.Set(xp + (longPosition ? -1 : 1) * ((MAX(High, CurrentBar)[0] - MIN(Low, CurrentBar)[0]) * af));
				return;
			}
			
			// Reset accelerator increase limiter on new bars
			if (afIncreased && prevBar != CurrentBar)
				afIncreased = false;
			
			// Current event is on a bar not marked as a reversal bar yet
			//if (reverseBar.Get(CurrentBar) != CurrentBar)
			if (!IsReversalBar())
			{
				// SAR = SAR[1] + af * (xp - SAR[1])
				todaySAR = TodaySAR(Value[1] + af * (xp - Value[1]));
				for (int x = 1; x <= 2; x++)
				{
					if (longPosition)
					{
						if (todaySAR > Low[x])
							todaySAR = Low[x];
					}
					else
					{
						if (todaySAR < High[x])
							todaySAR = High[x];
					}
				}
				
				// Reverse position
				if ((longPosition && (Low[0] < todaySAR || Low[1] < todaySAR))
					|| (!longPosition && (High[0] > todaySAR || High[1] > todaySAR))) 
				{
					Value.Set(Reverse());
					return;
				}
				// Holding long position
				else if (longPosition)
				{
					// Process a new SAR value only on a new bar or if SAR value was penetrated.
					if (prevBar != CurrentBar || Low[0] < prevSAR)
					{
						Value.Set(todaySAR);
						prevSAR = todaySAR;
					}
					else
						Value.Set(prevSAR);
					
					if (High[0] > xp)
					{						
						xp = High[0];
						AfIncrease();
					}
				}
				
				// Holding short position
				else if (!longPosition)
				{
					// Process a new SAR value only on a new bar or if SAR value was penetrated.
					if (prevBar != CurrentBar || High[0] > prevSAR)
					{
						Value.Set(todaySAR);
						prevSAR = todaySAR;
					}
					else
						Value.Set(prevSAR);
					
					if (Low[0] < xp)
					{						
						xp = Low[0];
						AfIncrease();
					}
				}
			}
			
			// Current event is on the same bar as the reversal bar
			else
			{
				// Only set new xp values. No increasing af since this is the first bar.
				if (longPosition && High[0] > xp)
					xp = High[0];
				else if (!longPosition && Low[0] < xp)
					xp = Low[0];
				Value.Set(prevSAR);

				// SAR = SAR[1] + af * (xp - SAR[1])
				todaySAR = TodaySAR(longPosition ? Math.Min(reverseValue.Get(CurrentBar), Low[0]) : Math.Max(reverseValue.Get(CurrentBar), High[0]));
			}
			
			prevBar = CurrentBar;
			
			if(IsLastBarOnChart() > 0) {
				if(zigZagLine != null) RemoveDrawObject(zigZagLine);
				//double zzGap;
				zigZagLine = DrawZigZag(CurrentBar, xp, tagZZLine);
				if(curGapText != null) RemoveDrawObject(curGapText);
				curGapText = DrawGapText(curZZGap, tagCurGapText);
				PrintZZSwings(zzSwings, log_file, printOut, 530, 1130);
				//PrintTwoBarRatio();
			}
		}

		protected ILine DrawZigZag(int endBar, double endValue, string tag) {
			//Print("DrawZigZag called");
			ILine zzLine = null;
			int startBar = GetLastReverseBar(endBar);
			//gap = 0;
			if(startBar > 0 && CurrentBar > BarsRequired) {
				if(printOut > 3)
					Print("DrawLine CurrentBar, zzSwings= " + CurrentBar + "," + zzSwings.Count + ", CurrentBar-startBar, Value[CurrentBar-startBar], 0, reverseValue[0]=" + (CurrentBar-startBar) + "," + Value[CurrentBar-startBar] + "," + 0 + "," + reverseValue[0]);
			//DrawLine("My line" + CurrentBar, CurrentBar-startBar, sarSeries[CurrentBar-startBar], 0, sarSeries[0], Color.Blue);
				zzLine = DrawLine(tag + CurrentBar, CurrentBar-startBar, reverseValue[CurrentBar-startBar], 0, endValue, Color.Blue);
				curZZGap = endValue - reverseValue[CurrentBar-startBar] ;
			}
			curPriceAction = GetPriceAction(Time[0]);
			//Print("Time, Pat=" + Time[0] + "," + curPriceActType.ToString());
			SetZZSwing(zzSwings, endBar, startBar, endBar, curZZGap);
			return zzLine;
		}
		
		/// <summary>
		/// Draw Gap from last ZZ to current bar
		/// </summary>
		/// <returns></returns>
		public IText DrawGapText(double zzGap, string tag)
		{
			IText gapText = null;
			double y = 0;
			Color up_color = Color.Green;
			Color dn_color = Color.Red;
			Color sm_color = Color.Black;
			Color draw_color = sm_color;
			if(zzGap > 0) {
				draw_color = up_color;
//				y = double.Parse(Low[0].ToString())-1 ;
				y = prevSAR-1 ;
			}
			else if (zzGap < 0) {
				draw_color = dn_color;
//				y = double.Parse(High[0].ToString())+1 ;
				y = prevSAR+1 ;
			}
			
			gapText = DrawText(tag+CurrentBar.ToString(), GetTimeDate(Time[0], 1)+"\r\n#"+ CurrentBar+"\r\nZ:"+zzGap, 0, y, draw_color);
//			}
			if(gapText != null) gapText.Locked = false;
			//if(printOut > 0)
				//PrintLog(true, log_file, CurrentBar + "::" + this.ToString() + " GaP= " + gap + " - " + Time[0].ToShortTimeString());
			return gapText; 
		}
	
        #region Properties
        /// <summary>
        /// The initial acceleration factor
        /// </summary>
        [Description("The initial acceleration factor")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayNameAttribute("Init Acceleration factor")]
        public double AccelerationInit
        {
            get { return acceleration; }
            set { acceleration = Math.Max(0.00, value); }
        }

        /// <summary>
        /// The acceleration step factor
        /// </summary>
        [Description("The acceleration step factor")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayNameAttribute("Acceleration step")]
        public double AccelerationStep
        {
            get { return accelerationStep; }
            set { accelerationStep = Math.Max(0.00, value); }
        }

		/// <summary>
		/// The maximum acceleration
		/// </summary>
		[Description("The maximum acceleration")]
		[GridCategory("Parameters")]
		[Gui.Design.DisplayNameAttribute("Acceleration max")]
		public double AccelerationMax
		{
			get { return accelerationMax; }
			set { accelerationMax = Math.Max(0.00, value); }
		}
		
		/// <summary>
		/// The maximum acceleration
		/// </summary>
		[Description("The Dot Color")]
		[GridCategory("Parameters")]
		[Gui.Design.DisplayNameAttribute("Dot Color")]
		public Color DotColor
		{
			get { return dotColor; }
			set { dotColor = value; }
		}
		
		/// <summary>
		/// The maximum acceleration
		/// </summary>
		[Description("The maximum acceleration")]
		[GridCategory("Parameters")]
		[Gui.Design.DisplayNameAttribute("Acceleration max")]
		public string AccName
		{
			get { return accName; }
			set { accName = value; }
		}
		
		[Description("If it runs for backtesting")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayNameAttribute("Back Testing")]
        public bool BackTest
        {
            get { return backTest; }
            set { backTest = value; }
        }
		
		#endregion

        #region Other Properties
		
//		public GIParabolicSAR getGIParabolicSAR() {
//			return base.checkGIParabolicSAR;
//		}
		
		public bool GetLongPosition() {
			return longPosition;
		}

		public double GetXp() {
			return xp;
		}

//		public DataSeries GetSarSeries() {
//			return sarSeries;
//		}
//		
//		public IntSeries GetXpBar() {
//			return xpBar;
//		}
		
		public double GetAf() {
			return af;
		}
		
		public double GetTodaySAR() {
			return todaySAR;
		}
		
		public double GetPrevSAR() {
			return prevSAR;
		}
		
		public int GetReverseBar() {
			return reverseBar.Get(CurrentBar);
		}
		
		public int GetLastReverseBar(int curBar) {
			//Print("GetLastReverseBar called");
			for(int i=curBar-1; i>=BarsRequired; i--) {
				//Print("CurrentBar, reverseBar[CurrentBar-i]=" + CurrentBar + "," + reverseBar[CurrentBar-i]);
				if(reverseBar[CurrentBar-i] > 0)
					return reverseBar[CurrentBar-i];
			}
			return -1;
		}
		
		public double GetReverseValue() {
			return reverseValue.Get(CurrentBar);
		}
		
		public bool IsReversalBar() {
			if(CurrentBar <= BarsRequired) 
				return false;
			else
				return reverseBar.Get(CurrentBar) == CurrentBar;
		}
		
		public int GetPrevBar() {
			return prevBar;
		}

		public bool GetAfIncreased() {
			return afIncreased;
		}

		public double GetCurZZGap() {
			return curZZGap;
		}				

		/// <summary>
		/// The gap between current price and last pbSAR reversal
		/// </summary>
		/// <returns></returns>
		public double GetCurGap(){
			double gap  = 0;
			int lastRevBar = GetLastReverseBar(CurrentBar);
			double lastRevVal = lastRevBar>0? reverseValue.Get(lastRevBar) : 0;
			if(lastRevVal>0) {
				gap = longPosition? High[0] - lastRevVal : Low[0] - lastRevVal;
			}
			return gap;
		}
		
		public int GetpbSARCrossBarsAgo() {
			int lastRevBar = GetLastReverseBar(CurrentBar);
			//Print("CurrentBar, lastRevBar=" + CurrentBar + "," + lastRevBar);
			if(IsReversalBar() || lastRevBar<0) return 0;
			else return CurrentBar-lastRevBar;			
		}
		
		public ILine GetCurZZLine() {
			return zigZagLine;
		}
		
		public PriceAction getCurPriceAction() {
			return curPriceAction;
		}
		
		public void setSpvPRBits(int spvPRBits) {
			SpvPRBits = spvPRBits;
		}		
		
		#endregion	
		
		#region Miscellaneous
		// Only raise accelerator if not raised for current bar yet
		private void AfIncrease()
		{
			if (!afIncreased)
			{
				af = Math.Min(AccelerationMax, af + AccelerationStep);
				afIncreased = true;				
			}
			return;
		}
		
		// Additional rule. SAR for today can't be placed inside the bar of day - 1 or day - 2.
		private double TodaySAR(double todaySAR)
		{
			if (longPosition)
			{
				double lowestSAR = Math.Min(Math.Min(todaySAR, Low[0]), Low[1]);
				if (Low[0] > lowestSAR)
					todaySAR = lowestSAR;
				else
					todaySAR = Reverse();
			}
			else
			{
				double highestSAR = Math.Max(Math.Max(todaySAR, High[0]), High[1]);
				if (High[0] < highestSAR)
					todaySAR = highestSAR;
				else
					todaySAR = Reverse();
			}
			return todaySAR;
		}
		
		private double Reverse()
		{
			double todaySAR = xp;
			if ((longPosition && prevSAR > Low[0]) || (!longPosition && prevSAR < High[0]) || prevBar != CurrentBar)
			{
				longPosition = !longPosition;
				reverseBar.Set(CurrentBar);
				//reverseValue = xp;
				reverseValue.Set(todaySAR);
				af = AccelerationInit;				
				xp = longPosition ? High[0] : Low[0];
				//sarSeries.Set(xp);
				//xpBar.Set(CurrentBar);
				prevSAR = todaySAR;
				pbSARCrossBarsAgo = 0;
				//double zzGap ;
				ILine zzLn = DrawZigZag(reverseBar[0], reverseValue[0], tagZZLine);
				if(zzLn != null)
					DrawGapText(curZZGap, tagCurGapText);
			}
			else {
				todaySAR = prevSAR;
				pbSARCrossBarsAgo ++;
			}
			return todaySAR;
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
        private GIParabolicSAR[] cacheGIParabolicSAR = null;

        private static GIParabolicSAR checkGIParabolicSAR = new GIParabolicSAR();

        /// <summary>
        /// GI Parabolic SAR
        /// </summary>
        /// <returns></returns>
        public GIParabolicSAR GIParabolicSAR(double accelerationInit, double accelerationMax, double accelerationStep, string accName, bool backTest, Color dotColor)
        {
            return GIParabolicSAR(Input, accelerationInit, accelerationMax, accelerationStep, accName, backTest, dotColor);
        }

        /// <summary>
        /// GI Parabolic SAR
        /// </summary>
        /// <returns></returns>
        public GIParabolicSAR GIParabolicSAR(Data.IDataSeries input, double accelerationInit, double accelerationMax, double accelerationStep, string accName, bool backTest, Color dotColor)
        {
            if (cacheGIParabolicSAR != null)
                for (int idx = 0; idx < cacheGIParabolicSAR.Length; idx++)
                    if (Math.Abs(cacheGIParabolicSAR[idx].AccelerationInit - accelerationInit) <= double.Epsilon && Math.Abs(cacheGIParabolicSAR[idx].AccelerationMax - accelerationMax) <= double.Epsilon && Math.Abs(cacheGIParabolicSAR[idx].AccelerationStep - accelerationStep) <= double.Epsilon && cacheGIParabolicSAR[idx].AccName == accName && cacheGIParabolicSAR[idx].BackTest == backTest && cacheGIParabolicSAR[idx].DotColor == dotColor && cacheGIParabolicSAR[idx].EqualsInput(input))
                        return cacheGIParabolicSAR[idx];

            lock (checkGIParabolicSAR)
            {
                checkGIParabolicSAR.AccelerationInit = accelerationInit;
                accelerationInit = checkGIParabolicSAR.AccelerationInit;
                checkGIParabolicSAR.AccelerationMax = accelerationMax;
                accelerationMax = checkGIParabolicSAR.AccelerationMax;
                checkGIParabolicSAR.AccelerationStep = accelerationStep;
                accelerationStep = checkGIParabolicSAR.AccelerationStep;
                checkGIParabolicSAR.AccName = accName;
                accName = checkGIParabolicSAR.AccName;
                checkGIParabolicSAR.BackTest = backTest;
                backTest = checkGIParabolicSAR.BackTest;
                checkGIParabolicSAR.DotColor = dotColor;
                dotColor = checkGIParabolicSAR.DotColor;

                if (cacheGIParabolicSAR != null)
                    for (int idx = 0; idx < cacheGIParabolicSAR.Length; idx++)
                        if (Math.Abs(cacheGIParabolicSAR[idx].AccelerationInit - accelerationInit) <= double.Epsilon && Math.Abs(cacheGIParabolicSAR[idx].AccelerationMax - accelerationMax) <= double.Epsilon && Math.Abs(cacheGIParabolicSAR[idx].AccelerationStep - accelerationStep) <= double.Epsilon && cacheGIParabolicSAR[idx].AccName == accName && cacheGIParabolicSAR[idx].BackTest == backTest && cacheGIParabolicSAR[idx].DotColor == dotColor && cacheGIParabolicSAR[idx].EqualsInput(input))
                            return cacheGIParabolicSAR[idx];

                GIParabolicSAR indicator = new GIParabolicSAR();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                indicator.AccelerationInit = accelerationInit;
                indicator.AccelerationMax = accelerationMax;
                indicator.AccelerationStep = accelerationStep;
                indicator.AccName = accName;
                indicator.BackTest = backTest;
                indicator.DotColor = dotColor;
                Indicators.Add(indicator);
                indicator.SetUp();

                GIParabolicSAR[] tmp = new GIParabolicSAR[cacheGIParabolicSAR == null ? 1 : cacheGIParabolicSAR.Length + 1];
                if (cacheGIParabolicSAR != null)
                    cacheGIParabolicSAR.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheGIParabolicSAR = tmp;
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
        /// GI Parabolic SAR
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.GIParabolicSAR GIParabolicSAR(double accelerationInit, double accelerationMax, double accelerationStep, string accName, bool backTest, Color dotColor)
        {
            return _indicator.GIParabolicSAR(Input, accelerationInit, accelerationMax, accelerationStep, accName, backTest, dotColor);
        }

        /// <summary>
        /// GI Parabolic SAR
        /// </summary>
        /// <returns></returns>
        public Indicator.GIParabolicSAR GIParabolicSAR(Data.IDataSeries input, double accelerationInit, double accelerationMax, double accelerationStep, string accName, bool backTest, Color dotColor)
        {
            return _indicator.GIParabolicSAR(input, accelerationInit, accelerationMax, accelerationStep, accName, backTest, dotColor);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// GI Parabolic SAR
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.GIParabolicSAR GIParabolicSAR(double accelerationInit, double accelerationMax, double accelerationStep, string accName, bool backTest, Color dotColor)
        {
            return _indicator.GIParabolicSAR(Input, accelerationInit, accelerationMax, accelerationStep, accName, backTest, dotColor);
        }

        /// <summary>
        /// GI Parabolic SAR
        /// </summary>
        /// <returns></returns>
        public Indicator.GIParabolicSAR GIParabolicSAR(Data.IDataSeries input, double accelerationInit, double accelerationMax, double accelerationStep, string accName, bool backTest, Color dotColor)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.GIParabolicSAR(input, accelerationInit, accelerationMax, accelerationStep, accName, backTest, dotColor);
        }
    }
}
#endregion
