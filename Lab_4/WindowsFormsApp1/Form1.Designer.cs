namespace WindowsFormsApp1
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
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.polygonSelectDropDown = new System.Windows.Forms.ComboBox();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.drawPolygonButton = new System.Windows.Forms.Button();
            this.movePolygonButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.turnAroundPointButton = new System.Windows.Forms.Button();
            this.turnAroundCenterButton = new System.Windows.Forms.Button();
            this.scaleRelativeToPointButton = new System.Windows.Forms.Button();
            this.scaleRelativeToCenterButton = new System.Windows.Forms.Button();
            this.findIntersectionButton = new System.Windows.Forms.Button();
            this.convexityCheckButton = new System.Windows.Forms.Button();
            this.positionRelativeToEdgeButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(850, 600);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Black;
            this.pictureBox2.Location = new System.Drawing.Point(850, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(4, 600);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.Black;
            this.pictureBox3.Location = new System.Drawing.Point(0, 600);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(854, 3);
            this.pictureBox3.TabIndex = 2;
            this.pictureBox3.TabStop = false;
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(975, 566);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(100, 25);
            this.clearButton.TabIndex = 3;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // polygonSelectDropDown
            // 
            this.polygonSelectDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.polygonSelectDropDown.FormattingEnabled = true;
            this.polygonSelectDropDown.Location = new System.Drawing.Point(860, 508);
            this.polygonSelectDropDown.Name = "polygonSelectDropDown";
            this.polygonSelectDropDown.Size = new System.Drawing.Size(215, 24);
            this.polygonSelectDropDown.TabIndex = 4;
            // 
            // inputTextBox
            // 
            this.inputTextBox.Location = new System.Drawing.Point(860, 538);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(215, 22);
            this.inputTextBox.TabIndex = 5;
            // 
            // outputTextBox
            // 
            this.outputTextBox.Enabled = false;
            this.outputTextBox.Location = new System.Drawing.Point(860, 342);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.Size = new System.Drawing.Size(215, 160);
            this.outputTextBox.TabIndex = 6;
            // 
            // drawPolygonButton
            // 
            this.drawPolygonButton.Location = new System.Drawing.Point(860, 18);
            this.drawPolygonButton.Name = "drawPolygonButton";
            this.drawPolygonButton.Size = new System.Drawing.Size(215, 30);
            this.drawPolygonButton.TabIndex = 7;
            this.drawPolygonButton.Text = "Draw Polygon";
            this.drawPolygonButton.UseVisualStyleBackColor = true;
            this.drawPolygonButton.Click += new System.EventHandler(this.drawPolygonButton_Click);
            // 
            // movePolygonButton
            // 
            this.movePolygonButton.Location = new System.Drawing.Point(860, 54);
            this.movePolygonButton.Name = "movePolygonButton";
            this.movePolygonButton.Size = new System.Drawing.Size(215, 30);
            this.movePolygonButton.TabIndex = 8;
            this.movePolygonButton.Text = "Move Polygon";
            this.movePolygonButton.UseVisualStyleBackColor = true;
            this.movePolygonButton.Click += new System.EventHandler(this.movePolygonButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(860, 566);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 25);
            this.applyButton.TabIndex = 9;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // turnAroundPointButton
            // 
            this.turnAroundPointButton.Location = new System.Drawing.Point(860, 90);
            this.turnAroundPointButton.Name = "turnAroundPointButton";
            this.turnAroundPointButton.Size = new System.Drawing.Size(215, 30);
            this.turnAroundPointButton.TabIndex = 10;
            this.turnAroundPointButton.Text = "Turn Around Point";
            this.turnAroundPointButton.UseVisualStyleBackColor = true;
            this.turnAroundPointButton.Click += new System.EventHandler(this.turnAroundPointButton_Click);
            // 
            // turnAroundCenterButton
            // 
            this.turnAroundCenterButton.Location = new System.Drawing.Point(860, 126);
            this.turnAroundCenterButton.Name = "turnAroundCenterButton";
            this.turnAroundCenterButton.Size = new System.Drawing.Size(215, 30);
            this.turnAroundCenterButton.TabIndex = 11;
            this.turnAroundCenterButton.Text = "Turn Around Center";
            this.turnAroundCenterButton.UseVisualStyleBackColor = true;
            this.turnAroundCenterButton.Click += new System.EventHandler(this.turnAroundCenterButton_Click);
            // 
            // scaleRelativeToPointButton
            // 
            this.scaleRelativeToPointButton.Location = new System.Drawing.Point(860, 162);
            this.scaleRelativeToPointButton.Name = "scaleRelativeToPointButton";
            this.scaleRelativeToPointButton.Size = new System.Drawing.Size(215, 30);
            this.scaleRelativeToPointButton.TabIndex = 12;
            this.scaleRelativeToPointButton.Text = "Scale Relative To Point";
            this.scaleRelativeToPointButton.UseVisualStyleBackColor = true;
            this.scaleRelativeToPointButton.Click += new System.EventHandler(this.scaleRelativeToPointButton_Click);
            // 
            // scaleRelativeToCenterButton
            // 
            this.scaleRelativeToCenterButton.Location = new System.Drawing.Point(860, 198);
            this.scaleRelativeToCenterButton.Name = "scaleRelativeToCenterButton";
            this.scaleRelativeToCenterButton.Size = new System.Drawing.Size(215, 30);
            this.scaleRelativeToCenterButton.TabIndex = 13;
            this.scaleRelativeToCenterButton.Text = "Scale Relative To Center";
            this.scaleRelativeToCenterButton.UseVisualStyleBackColor = true;
            this.scaleRelativeToCenterButton.Click += new System.EventHandler(this.scaleRelativeToCenterButton_Click);
            // 
            // findIntersectionButton
            // 
            this.findIntersectionButton.Location = new System.Drawing.Point(860, 234);
            this.findIntersectionButton.Name = "findIntersectionButton";
            this.findIntersectionButton.Size = new System.Drawing.Size(215, 30);
            this.findIntersectionButton.TabIndex = 14;
            this.findIntersectionButton.Text = "Find Intersection";
            this.findIntersectionButton.UseVisualStyleBackColor = true;
            this.findIntersectionButton.Click += new System.EventHandler(this.findIntersectionButton_Click);
            // 
            // convexityCheckButton
            // 
            this.convexityCheckButton.Location = new System.Drawing.Point(860, 270);
            this.convexityCheckButton.Name = "convexityCheckButton";
            this.convexityCheckButton.Size = new System.Drawing.Size(215, 30);
            this.convexityCheckButton.TabIndex = 15;
            this.convexityCheckButton.Text = "Convexity Check";
            this.convexityCheckButton.UseVisualStyleBackColor = true;
            this.convexityCheckButton.Click += new System.EventHandler(this.convexityCheckButton_Click);
            // 
            // positionRelativeToEdgeButton
            // 
            this.positionRelativeToEdgeButton.Location = new System.Drawing.Point(860, 306);
            this.positionRelativeToEdgeButton.Name = "positionRelativeToEdgeButton";
            this.positionRelativeToEdgeButton.Size = new System.Drawing.Size(215, 30);
            this.positionRelativeToEdgeButton.TabIndex = 16;
            this.positionRelativeToEdgeButton.Text = "Position Relative To Edge";
            this.positionRelativeToEdgeButton.UseVisualStyleBackColor = true;
            this.positionRelativeToEdgeButton.Click += new System.EventHandler(this.positionRelativeToEdgeButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1082, 603);
            this.Controls.Add(this.positionRelativeToEdgeButton);
            this.Controls.Add(this.convexityCheckButton);
            this.Controls.Add(this.findIntersectionButton);
            this.Controls.Add(this.scaleRelativeToCenterButton);
            this.Controls.Add(this.scaleRelativeToPointButton);
            this.Controls.Add(this.turnAroundCenterButton);
            this.Controls.Add(this.turnAroundPointButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.movePolygonButton);
            this.Controls.Add(this.drawPolygonButton);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.inputTextBox);
            this.Controls.Add(this.polygonSelectDropDown);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.ComboBox polygonSelectDropDown;
        private System.Windows.Forms.TextBox inputTextBox;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Button drawPolygonButton;
        private System.Windows.Forms.Button movePolygonButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button turnAroundPointButton;
        private System.Windows.Forms.Button turnAroundCenterButton;
        private System.Windows.Forms.Button scaleRelativeToPointButton;
        private System.Windows.Forms.Button scaleRelativeToCenterButton;
        private System.Windows.Forms.Button findIntersectionButton;
        private System.Windows.Forms.Button convexityCheckButton;
        private System.Windows.Forms.Button positionRelativeToEdgeButton;
    }
}

