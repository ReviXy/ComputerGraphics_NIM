using System.Windows.Forms.DataVisualization.Charting;

namespace Testcolors
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.pictureBoxRed = new System.Windows.Forms.PictureBox();
            this.pictureBoxGreen = new System.Windows.Forms.PictureBox();
            this.pictureBoxBlue = new System.Windows.Forms.PictureBox();
            this.chartRed = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartGreen = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartBlue = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.pictureBoxTrue = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTrue)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxRed
            // 
            this.pictureBoxRed.Location = new System.Drawing.Point(337, 12);
            this.pictureBoxRed.Name = "pictureBoxRed";
            this.pictureBoxRed.Size = new System.Drawing.Size(330, 210);
            this.pictureBoxRed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxRed.TabIndex = 0;
            this.pictureBoxRed.TabStop = false;
            // 
            // pictureBoxGreen
            // 
            this.pictureBoxGreen.Location = new System.Drawing.Point(337, 234);
            this.pictureBoxGreen.Name = "pictureBoxGreen";
            this.pictureBoxGreen.Size = new System.Drawing.Size(330, 210);
            this.pictureBoxGreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxGreen.TabIndex = 1;
            this.pictureBoxGreen.TabStop = false;
            // 
            // pictureBoxBlue
            // 
            this.pictureBoxBlue.Location = new System.Drawing.Point(337, 460);
            this.pictureBoxBlue.Name = "pictureBoxBlue";
            this.pictureBoxBlue.Size = new System.Drawing.Size(330, 210);
            this.pictureBoxBlue.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxBlue.TabIndex = 2;
            this.pictureBoxBlue.TabStop = false;
            // 
            // chartRed
            // 
            chartArea1.Name = "ChartArea1";
            this.chartRed.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartRed.Legends.Add(legend1);
            this.chartRed.Location = new System.Drawing.Point(673, 12);
            this.chartRed.Name = "chartRed";
            this.chartRed.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.chartRed.PaletteCustomColors = new System.Drawing.Color[] {
        System.Drawing.Color.Red};
            series1.ChartArea = "ChartArea1";
            series1.CustomProperties = "Title=\'red channel pixels\'";
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            series1.YValuesPerPoint = 2;
            this.chartRed.Series.Add(series1);
            this.chartRed.Size = new System.Drawing.Size(1600, 210);
            this.chartRed.TabIndex = 3;
            this.chartRed.Text = "chart1";
            chartArea1.AxisX.Minimum = 0;
            chartArea1.AxisX.Maximum = 255;
            // 
            // chartGreen
            // 
            chartArea2.Name = "ChartArea1";
            this.chartGreen.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.chartGreen.Legends.Add(legend2);
            this.chartGreen.Location = new System.Drawing.Point(673, 234);
            this.chartGreen.Name = "chartGreen";
            this.chartGreen.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.chartGreen.PaletteCustomColors = new System.Drawing.Color[] {
        System.Drawing.Color.Lime};
            series2.ChartArea = "ChartArea1";
            series2.CustomProperties = "Title=\'green channel pixels\'";
            series2.IsVisibleInLegend = false;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            series2.YValuesPerPoint = 2;
            this.chartGreen.Series.Add(series2);
            this.chartGreen.Size = new System.Drawing.Size(1600, 210);
            this.chartGreen.TabIndex = 4;
            this.chartGreen.Text = "chart2";
            chartArea2.AxisX.Minimum = 0;
            chartArea2.AxisX.Maximum = 255;
            // 
            // chartBlue
            // 
            chartArea3.Name = "ChartArea1";
            this.chartBlue.ChartAreas.Add(chartArea3);
            legend3.LegendStyle = System.Windows.Forms.DataVisualization.Charting.LegendStyle.Column;
            legend3.Name = "Legend1";
            this.chartBlue.Legends.Add(legend3);
            this.chartBlue.Location = new System.Drawing.Point(673, 460);
            this.chartBlue.Name = "chartBlue";
            this.chartBlue.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.chartBlue.PaletteCustomColors = new System.Drawing.Color[] {
        System.Drawing.Color.Blue,
        System.Drawing.Color.Blue};
            series3.ChartArea = "ChartArea1";
            series3.CustomProperties = "Title=\'blue channel pixels\'";
            series3.IsVisibleInLegend = false;
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            series3.YValuesPerPoint = 2;
            this.chartBlue.Series.Add(series3);
            this.chartBlue.Size = new System.Drawing.Size(1600, 210);
            this.chartBlue.TabIndex = 5;
            this.chartBlue.Text = "chart3";
            chartArea3.AxisX.Minimum = 0;
            chartArea3.AxisX.Maximum = 255;
            // 
            // pictureBoxTrue
            // 
            this.pictureBoxTrue.Location = new System.Drawing.Point(1, 234);
            this.pictureBoxTrue.Name = "pictureBoxTrue";
            this.pictureBoxTrue.Size = new System.Drawing.Size(330, 210);
            this.pictureBoxTrue.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxTrue.TabIndex = 6;
            this.pictureBoxTrue.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2300, 720);
            this.Controls.Add(this.pictureBoxTrue);
            this.Controls.Add(this.chartBlue);
            this.Controls.Add(this.chartGreen);
            this.Controls.Add(this.chartRed);
            this.Controls.Add(this.pictureBoxBlue);
            this.Controls.Add(this.pictureBoxGreen);
            this.Controls.Add(this.pictureBoxRed);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTrue)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxRed;
        private System.Windows.Forms.PictureBox pictureBoxGreen;
        private System.Windows.Forms.PictureBox pictureBoxBlue;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartRed;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartGreen;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartBlue;
        private System.Windows.Forms.PictureBox pictureBoxTrue;
    }
}

