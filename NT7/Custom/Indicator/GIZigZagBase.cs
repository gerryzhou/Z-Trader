#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("Enter the description of your new custom indicator here")]
    public class GIZigZagBase : Indicator
    {
        #region Variables
		private double			currentZigZagHigh	= 0;
		private double			currentZigZagLow	= 0;
		private DeviationType	deviationType		= DeviationType.Points;
		private double			deviationValue		= 4;
		private DataSeries		zigZagHighZigZags; 
		private DataSeries		zigZagLowZigZags; 
		private DataSeries		zigZagHighSeries; 
		private DataSeries		zigZagLowSeries; 
		private int				lastSwingIdx		= -1;
		private double			lastSwingPrice		= 0.0;
		private int				trendDir			= 0; // 1 = trend up, -1 = trend down, init = 0
		private bool			useHighLow			= true;

		private IntSeries		barZZMode; //0=noChange, -1=Bear Reversal, 1=Bull Reversal, -2=UpdateLow, 2=UpdateHigh, -3=isOverLowDeviation, 3=isOverHighDeviation;

		protected List<ZigZagSwing>		zzSwings;

		private ILine zigZagLine			= null; // The current ZigZag ended by the last bar of the chart;
		private const string tagZZLine	= "tagZZLine-" ;// the tag of the line object for the current ZZ line;

		private IText curGapText			= null; // The text for the ZZ ended by last bar of the chart; 
		private const string tagCurGapText	= "tagCurGapText-"; // the tag of the line object for curGapText;
		
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
			symbol = Instrument.FullName;
			Add(new Plot(Color.Blue, PlotStyle.Line, "ZigZag"));

			zigZagHighSeries	= new DataSeries(this, MaximumBarsLookBack.Infinite); 
			zigZagHighZigZags	= new DataSeries(this, MaximumBarsLookBack.Infinite); 
			zigZagLowSeries		= new DataSeries(this, MaximumBarsLookBack.Infinite); 
			zigZagLowZigZags	= new DataSeries(this, MaximumBarsLookBack.Infinite); 
			barZZMode			= new IntSeries(this, MaximumBarsLookBack.Infinite);
			
			zzSwings = new List<ZigZagSwing>();
			
			DisplayInDataBox	= true;
            Overlay				= true;
			PaintPriceMarkers	= false;
			AllowRemovalOfDrawObjects = true;
        }


		/// <summary>
		/// Returns the number of bars ago a zig zag low occurred. Returns a value of -1 if a zig zag low is not found within the look back period.
		/// </summary>
		/// <param name="barsAgo"></param>
		/// <param name="instance"></param>
		/// <param name="lookBackPeriod"></param>
		/// <returns></returns>
		public int LowBar(int barsAgo, int instance, int lookBackPeriod) 
		{
			if (instance < 1)
				throw new Exception(GetType().Name + ".LowBar: instance must be greater/equal 1 but was " + instance);
			else if (barsAgo < 0)
				throw new Exception(GetType().Name + ".LowBar: barsAgo must be greater/equal 0 but was " + barsAgo);
			else if (barsAgo >= Count)
				throw new Exception(GetType().Name + ".LowBar: barsAgo out of valid range 0 through " + (Count - 1) + ", was " + barsAgo + ".");

			Update();
			for (int idx = CurrentBar - barsAgo - 1; idx >= CurrentBar - barsAgo - 1 - lookBackPeriod; idx--)
			{
				if (idx < 0)
					return -1;
				if (idx >= zigZagLowZigZags.Count)
					continue;				

				if (zigZagLowZigZags.Get(idx).Equals(0.0))			
					continue;

				if (instance == 1) // 1-based, < to be save
					return CurrentBar - idx;	

				instance--;
			}
	
			return -1;
		}


		/// <summary>
		/// Returns the number of bars ago a zig zag high occurred. Returns a value of -1 if a zig zag high is not found within the look back period.
		/// </summary>
		/// <param name="barsAgo"></param>
		/// <param name="instance"></param>
		/// <param name="lookBackPeriod"></param>
		/// <returns></returns>
		public int HighBar(int barsAgo, int instance, int lookBackPeriod) 
		{
			if (instance < 1)
				throw new Exception(GetType().Name + ".HighBar: instance must be greater/equal 1 but was " + instance);
			else if (barsAgo < 0)
				throw new Exception(GetType().Name + ".HighBar: barsAgo must be greater/equal 0 but was " + barsAgo);
			else if (barsAgo >= Count)
				throw new Exception(GetType().Name + ".HighBar: barsAgo out of valid range 0 through " + (Count - 1) + ", was " + barsAgo + ".");

			Update();
			for (int idx = CurrentBar - barsAgo - 1; idx >= CurrentBar - barsAgo - 1 - lookBackPeriod; idx--)
			{
				if (idx < 0)
					return -1;
				if (idx >= zigZagHighZigZags.Count)
					continue;				

				if (zigZagHighZigZags.Get(idx).Equals(0.0))			
					continue;

				if (instance <= 1) // 1-based, < to be save
					return CurrentBar - idx;	

				instance--;
			}

			return -1;
		}

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			DayWeekMonthCount();
			zzSwings.Add(new ZigZagSwing(-1,-1,0,0));
			
			if (CurrentBar < 2) // need 3 bars to calculate Low/High
			{
				zigZagHighSeries.Set(0);
				zigZagHighZigZags.Set(0);
				zigZagLowSeries.Set(0);
				zigZagLowZigZags.Set(0);
				barZZMode.Set(0);
				return;
			}

			// Initialization
			if (lastSwingPrice == 0.0)
				lastSwingPrice = Input[0];

			IDataSeries highSeries	= High;
			IDataSeries lowSeries	= Low;

			if (!useHighLow)
			{
				highSeries	= Input;
				lowSeries	= Input;
			}

			// Calculation always for 1-bar ago !

			double tickSize = Bars.Instrument.MasterInstrument.TickSize;
			bool isSwingHigh	= highSeries[1] >= highSeries[0] - double.Epsilon 
								&& highSeries[1] >= highSeries[2] - double.Epsilon;
			bool isSwingLow		= lowSeries[1] <= lowSeries[0] + double.Epsilon 
								&& lowSeries[1] <= lowSeries[2] + double.Epsilon;  
			bool isOverHighDeviation	= (deviationType == DeviationType.Percent && IsPriceGreater(highSeries[1], (lastSwingPrice * (1.0 + deviationValue * 0.01))))
										|| (deviationType == DeviationType.Points && IsPriceGreater(highSeries[1], lastSwingPrice + deviationValue));
			bool isOverLowDeviation		= (deviationType == DeviationType.Percent && IsPriceGreater(lastSwingPrice * (1.0 - deviationValue * 0.01), lowSeries[1]))
										|| (deviationType == DeviationType.Points && IsPriceGreater(lastSwingPrice - deviationValue, lowSeries[1]));

			double	saveValue	= 0.0;
			bool	addHigh		= false; 
			bool	addLow		= false; 
			bool	updateHigh	= false; 
			bool	updateLow	= false; 

			zigZagHighZigZags.Set(0);
			zigZagLowZigZags.Set(0);
			barZZMode.Set(0);
			
			if (!isSwingHigh && !isSwingLow)
			{
				zigZagHighSeries.Set(currentZigZagHigh);
				zigZagLowSeries.Set(currentZigZagLow);
				if(trendDir <= 0 && isOverHighDeviation) //Bull reversal, but does not reach a pivot yet;
					barZZMode.Set(3);
				else if(trendDir >= 0 && isOverLowDeviation) //Bear reversal, but does not reach a pivot yet;
					barZZMode.Set(-3);
				return;
			}
			
			if (trendDir <= 0 && isSwingHigh && isOverHighDeviation)
			{	
				saveValue	= highSeries[1];
				addHigh		= true;
				trendDir	= 1;
				barZZMode.Set(1); //bull reversal, ZZLow detected, which is LowBar() ago;
			}	
			else if (trendDir >= 0 && isSwingLow && isOverLowDeviation)
			{	
				saveValue	= lowSeries[1];
				addLow		= true;
				trendDir	= -1;
				barZZMode.Set(-1); //bear reversal, ZZHigh detected, which is HighBar() ago;
			}	
			else if (trendDir == 1 && isSwingHigh && IsPriceGreater(highSeries[1], lastSwingPrice)) 
			{
				saveValue	= highSeries[1];
				updateHigh	= true;
				barZZMode.Set(2); //new high in bull trend detected, which is previous bar;
			}
			else if (trendDir == -1 && isSwingLow && IsPriceGreater(lastSwingPrice, lowSeries[1])) 
			{
				saveValue	= lowSeries[1];
				updateLow	= true;
				barZZMode.Set(-2); //new low in bear trend detected, which is previous bar;
			}

			if (addHigh || addLow || updateHigh || updateLow)
			{
				if (updateHigh && lastSwingIdx >= 0)
				{
					zigZagHighZigZags.Set(CurrentBar - lastSwingIdx, 0);
					Value.Reset(CurrentBar - lastSwingIdx);
				}
				else if (updateLow && lastSwingIdx >= 0)
				{
					zigZagLowZigZags.Set(CurrentBar - lastSwingIdx, 0);
					Value.Reset(CurrentBar - lastSwingIdx);
				}

				if (addHigh || updateHigh)
				{
					zigZagHighZigZags.Set(1, saveValue);
					zigZagHighZigZags.Set(0, 0);

					currentZigZagHigh = saveValue;
					zigZagHighSeries.Set(1, currentZigZagHigh);
					Value.Set(1, currentZigZagHigh);
				}
				else if (addLow || updateLow) 
				{
					zigZagLowZigZags.Set(1, saveValue);
					zigZagLowZigZags.Set(0, 0);

					currentZigZagLow = saveValue;
					zigZagLowSeries.Set(1, currentZigZagLow);
					Value.Set(1, currentZigZagLow);
				}

				lastSwingIdx	= CurrentBar - 1;
				lastSwingPrice	= saveValue;
				
				Print(CurrentBar + "," + Time[0] + ":" + "addHigh,updateHigh,addLow,updateLow=" + addHigh + "," + updateHigh + "," + addLow + "," + updateLow);
			}

			zigZagHighSeries.Set(currentZigZagHigh);
			zigZagLowSeries.Set(currentZigZagLow);
			Print(CurrentBar + "," + Time[0] +  ":ZigZagHigh,ZigZagLow,HighBar,LowBar,trendDir=[" + ZigZagHigh[0] + "," + ZigZagLow[0] + "],[" + HighBar(1, 1, CurrentBar-BarsRequired) + "," + LowBar(1, 1, CurrentBar-BarsRequired) + "] " + trendDir);
			
			if(Math.Abs(barZZMode[0]) == 1) { //add ZigZagSwing object
				int startBar=-1, endBar=-1;
				if(barZZMode[0] == 1) {
					startBar = CurrentBar - HighBar(1, 1, CurrentBar-BarsRequired);
					endBar = CurrentBar - LowBar(1, 1, CurrentBar-BarsRequired);
				}
				if(barZZMode[0] == -1) {
					startBar = CurrentBar - LowBar(1, 1, CurrentBar-BarsRequired);
					endBar = CurrentBar - HighBar(1, 1, CurrentBar-BarsRequired);
				}			
				
				double zzSize = GetZZSwingSize(startBar, endBar);
				Print("startBar,endBar, zzSize=[" + startBar + "," + endBar + "]," + zzSize);
				
				if(zzSize != 0) {
					SetZZSwing(zzSwings, endBar, startBar, endBar, zzSize);
					if(Historical) {
						double y_base = zzSize > 0? Low[CurrentBar-endBar] : High[CurrentBar-endBar];
						DrawGapText(zzSize, tagCurGapText, CurrentBar-endBar, y_base, 0.5);
					}
				}
			}
			
			Print("IsLastBarOnChart()=" + IsLastBarOnChart());
			if(IsLastBarOnChart() > 0) {
				double curGap = GetCurZZGap();
				
				int startBarsAgo = curGap > 0? LowBar(1, 1, CurrentBar-BarsRequired) : HighBar(1, 1, CurrentBar-BarsRequired);
				if(zigZagLine != null && !Historical) RemoveDrawObject(zigZagLine);		
				zigZagLine = DrawZigZagLine(startBarsAgo, 0, curGap, tagZZLine, Color.Blue);
				
				double yBase = curGap > 0? Low[0] : High[0];				
				if(curGapText != null && !Historical) RemoveDrawObject(curGapText);
				curGapText = DrawGapText(curGap, tagCurGapText, 0, yBase, 0.5);
				Print("curGapText=" + curGapText.Y + "," + curGapText.Time + "," + curGapText.Tag + "," + curGapText.BarsAgo + "," + curGapText.Text);

				PrintZZSwings(zzSwings, GetPrintOut(), IsBackTest(), 530, 1130);
			}
        }

		#region Properties
        [Description("Deviation in percent or points regarding on the deviation type")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayName("Deviation value")]
        public double DeviationValue
        {
            get { return deviationValue; }
            set { deviationValue = Math.Max(0.0, value); }
        }

        [Description("Type of the deviation value")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayName("Deviation type")]
        public DeviationType DeviationType
        {
            get { return deviationType; }
            set { deviationType = value; }
        }

        [Description("If true, high and low instead of selected price type is used to plot indicator.")]
        [GridCategory("Parameters")]
		[Gui.Design.DisplayName("Use high and low")]
		[RefreshProperties(RefreshProperties.All)]
        public bool UseHighLow
        {
            get { return useHighLow; }
            set { useHighLow = value; }
        }

		/// <summary>
		/// Gets the ZigZag high points.
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		public DataSeries ZigZagHigh
		{
			get 
			{ 
				Update();
				return zigZagHighSeries; 
			}
		}

		/// <summary>
		/// Gets the ZigZag low points.
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		public DataSeries ZigZagLow
		{
			get 
			{ 
				Update();
				return zigZagLowSeries; 
			}
		}
		
		
		/// <summary>
		/// The account name
		/// </summary>
//		[Description("The account name")]
//		[GridCategory("Parameters")]
//		[Gui.Design.DisplayNameAttribute("Account name")]
//		public string AccName
//		{
//			get { return accName; }
//			set { accName = value; }
//		}

		
        #endregion
		
		#region Other properties
		/// <summary>
		/// The problem: each reversal was detected at a three bars swing pivot, 
		/// which caused missing the ZZ reversal when consectutive bars without pullback occured;
		/// it happened at range bar chart;
		/// </summary>
		/// <returns></returns>
		public double GetCurZZGap() {
			double cur_gap = 0;
			switch(barZZMode[0]) {
				case 1: case 2: //bull reversal or bull trend continued;
					cur_gap = High[0] - ZigZagLow[0];
					break;
				case -1: case -2: //bear reversal or bear trend continued;
					cur_gap = Low[0] - ZigZagHigh[0];			
					break;
				default: 
					if(trendDir > 0 && ZigZagLow[0] > 0) cur_gap = High[0] - ZigZagLow[0];
					if(trendDir < 0 && ZigZagHigh[0] > 0) cur_gap = Low[0] - ZigZagHigh[0];					
					break;
			}
			return cur_gap;
		}
		
		public int GetTrendDir() {
			return trendDir;
		}
		
		public int GetBarZZMode(int barsAgo) {
			return barZZMode[barsAgo];
		}
		
		#endregion

		#region Miscellaneous

		/// <summary>
		/// #ENS#
		/// </summary>
		/// <param name="chartControl"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public override void GetMinMaxValues(Gui.Chart.ChartControl chartControl, ref double min, ref double max)
		{
			if (BarsArray[0] == null || ChartControl == null)
				return;

			for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
			{
				for (int idx = this.FirstBarIndexPainted; idx <= this.LastBarIndexPainted; idx++)
				{
					if (zigZagHighZigZags.IsValidPlot(idx) && zigZagHighZigZags.Get(idx) != 0)
						max = Math.Max(max, zigZagHighZigZags.Get(idx));
					if (zigZagLowZigZags.IsValidPlot(idx) && zigZagLowZigZags.Get(idx) != 0)
						min = Math.Min(min, zigZagLowZigZags.Get(idx));
				}
			}
		}

		private bool IsPriceGreater(double a, double b)
		{
			if (a > b && a - b > TickSize / 2)
				return true; 
			else 
				return false;
		}

		public override void Plot(Graphics graphics, Rectangle bounds, double min, double max)
		{
			if (Bars == null || ChartControl == null)
				return;

			IsValidPlot(Bars.Count - 1 + (CalculateOnBarClose ? -1 : 0)); // make sure indicator is calculated until last (existing) bar

			int preDiff = 1;
			for (int i = FirstBarIndexPainted - 1; i >= BarsRequired; i--)
			{
				if (i < 0)
					break;

				bool isHigh	= zigZagHighZigZags.IsValidPlot(i) && zigZagHighZigZags.Get(i) > 0;
				bool isLow	= zigZagLowZigZags.IsValidPlot(i) && zigZagLowZigZags.Get(i) > 0;
				
				if (isHigh || isLow)
					break;

				preDiff++;
			}

			int postDiff = 0;
			for (int i = LastBarIndexPainted; i <= zigZagHighZigZags.Count; i++)
			{
				if (i < 0)
					break;

				bool isHigh	= zigZagHighZigZags.IsValidPlot(i) && zigZagHighZigZags.Get(i) > 0;
				bool isLow	= zigZagLowZigZags.IsValidPlot(i) && zigZagLowZigZags.Get(i) > 0;

				if (isHigh || isLow)
					break;

				postDiff++;
			}

			bool linePlotted = false;
			using (GraphicsPath path = new GraphicsPath()) 
			{
				int		barWidth	= ChartControl.ChartStyle.GetBarPaintWidth(Bars.BarsData.ChartStyle.BarWidthUI);

				int		lastIdx		= -1; 
				double	lastValue	= -1; 

				for (int idx = this.FirstBarIndexPainted - preDiff; idx <= this.LastBarIndexPainted + postDiff; idx++)
				{
					if (idx - Displacement < 0 || idx - Displacement >= Bars.Count || (!ChartControl.ShowBarsRequired && idx - Displacement < BarsRequired))
						continue;

					bool isHigh	= zigZagHighZigZags.IsValidPlot(idx) && zigZagHighZigZags.Get(idx) > 0;
					bool isLow	= zigZagLowZigZags.IsValidPlot(idx) && zigZagLowZigZags.Get(idx) > 0;

					if (!isHigh && !isLow)
						continue;
					
					double value = isHigh ? zigZagHighZigZags.Get(idx) : zigZagLowZigZags.Get(idx);
					if (lastValue >= 0)
					{	
						int x0	= ChartControl.GetXByBarIdx(BarsArray[0], lastIdx);
						int x1	= ChartControl.GetXByBarIdx(BarsArray[0], idx);
						int y0	= ChartControl.GetYByValue(this, lastValue);
						int y1	= ChartControl.GetYByValue(this, value);

						path.AddLine(x0, y0, x1, y1);
						linePlotted = true;
					}

					// save as previous point
					lastIdx		= idx; 
					lastValue	= value; 
				}

				SmoothingMode oldSmoothingMode = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.DrawPath(Plots[0].Pen, path);
				graphics.SmoothingMode = oldSmoothingMode;
			}

			if (!linePlotted)
				DrawTextFixed("ZigZagErrorMsg", "ZigZag can't plot any values since the deviation value is too large. Please reduce it.", TextPosition.BottomRight);
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
        private GIZigZagBase[] cacheGIZigZagBase = null;

        private static GIZigZagBase checkGIZigZagBase = new GIZigZagBase();

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public GIZigZagBase GIZigZagBase(DeviationType deviationType, double deviationValue, bool useHighLow)
        {
            return GIZigZagBase(Input, deviationType, deviationValue, useHighLow);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public GIZigZagBase GIZigZagBase(Data.IDataSeries input, DeviationType deviationType, double deviationValue, bool useHighLow)
        {
            if (cacheGIZigZagBase != null)
                for (int idx = 0; idx < cacheGIZigZagBase.Length; idx++)
                    if (cacheGIZigZagBase[idx].DeviationType == deviationType && Math.Abs(cacheGIZigZagBase[idx].DeviationValue - deviationValue) <= double.Epsilon && cacheGIZigZagBase[idx].UseHighLow == useHighLow && cacheGIZigZagBase[idx].EqualsInput(input))
                        return cacheGIZigZagBase[idx];

            lock (checkGIZigZagBase)
            {
                checkGIZigZagBase.DeviationType = deviationType;
                deviationType = checkGIZigZagBase.DeviationType;
                checkGIZigZagBase.DeviationValue = deviationValue;
                deviationValue = checkGIZigZagBase.DeviationValue;
                checkGIZigZagBase.UseHighLow = useHighLow;
                useHighLow = checkGIZigZagBase.UseHighLow;

                if (cacheGIZigZagBase != null)
                    for (int idx = 0; idx < cacheGIZigZagBase.Length; idx++)
                        if (cacheGIZigZagBase[idx].DeviationType == deviationType && Math.Abs(cacheGIZigZagBase[idx].DeviationValue - deviationValue) <= double.Epsilon && cacheGIZigZagBase[idx].UseHighLow == useHighLow && cacheGIZigZagBase[idx].EqualsInput(input))
                            return cacheGIZigZagBase[idx];

                GIZigZagBase indicator = new GIZigZagBase();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                indicator.DeviationType = deviationType;
                indicator.DeviationValue = deviationValue;
                indicator.UseHighLow = useHighLow;
                Indicators.Add(indicator);
                indicator.SetUp();

                GIZigZagBase[] tmp = new GIZigZagBase[cacheGIZigZagBase == null ? 1 : cacheGIZigZagBase.Length + 1];
                if (cacheGIZigZagBase != null)
                    cacheGIZigZagBase.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheGIZigZagBase = tmp;
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
        public Indicator.GIZigZagBase GIZigZagBase(DeviationType deviationType, double deviationValue, bool useHighLow)
        {
            return _indicator.GIZigZagBase(Input, deviationType, deviationValue, useHighLow);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.GIZigZagBase GIZigZagBase(Data.IDataSeries input, DeviationType deviationType, double deviationValue, bool useHighLow)
        {
            return _indicator.GIZigZagBase(input, deviationType, deviationValue, useHighLow);
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
        public Indicator.GIZigZagBase GIZigZagBase(DeviationType deviationType, double deviationValue, bool useHighLow)
        {
            return _indicator.GIZigZagBase(Input, deviationType, deviationValue, useHighLow);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.GIZigZagBase GIZigZagBase(Data.IDataSeries input, DeviationType deviationType, double deviationValue, bool useHighLow)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.GIZigZagBase(input, deviationType, deviationValue, useHighLow);
        }
    }
}
#endregion
