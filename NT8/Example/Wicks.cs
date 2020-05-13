#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    public class Wicks : Indicator
    {
        #region Variables
        private double UpperWick = 0;
        private double LowerWick = 0;
        private double UpperWickCalc = 0;
        private double LowerWickCalc = 0;
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        private void Initialize()
        {
            AddPlot(new Stroke(Brushes.Green), PlotStyle.Bar, "UpperWickPercent");
            AddPlot(new Stroke(Brushes.Red), PlotStyle.Bar, "LowerWickPercent");
            AddLine(Brushes.Black, 0, "ZeroLine");
            IsOverlay = false;
        }

        protected override void OnStateChange()
        {
            switch (State)
            {
                case State.SetDefaults:
                    Name = "Wicks";
                    Description = "Shows % of Range for upper and lower wicks.";
                    Initialize();
                    break;
             }
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			if (
				Open[0] > Close[0]	//Down bar
				)
			{
				UpperWick = High[0] - Open[0];
				LowerWick = Low[0] - Close[0];
			}
			if (
				Open[0] < Close[0]	//Up bar
				)
			{
				UpperWick = High[0] - Close[0];
				LowerWick = Low[0] - Open[0];
			}
			if (
				Open[0] == Close[0]	//Doji bar
				)
			{
				UpperWick = High[0] - Close[0];
				LowerWick = Low[0] - Open[0];
			}
			if (
				Range()[0] > 0
				)
			{
        		UpperWickCalc = (UpperWick/Range()[0]) * 100;
        		LowerWickCalc = (LowerWick/Range()[0]) * 100;
			}
			else
				{
        		UpperWickCalc = 0;
        		LowerWickCalc = 0;
				}
			
            UpperWickPercent[0] = UpperWickCalc;
            LowerWickPercent[0] = LowerWickCalc;
        }

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> UpperWickPercent
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> LowerWickPercent
        {
            get { return Values[1]; }
        }

        #endregion
    }
}

