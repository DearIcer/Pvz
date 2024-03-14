namespace PVZCheat
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            InfiniteSunshine = new CheckBox();
            SunshineTimer = new System.Windows.Forms.Timer(components);
            KillZombie = new CheckBox();
            Init = new System.Windows.Forms.Timer(components);
            Refresh = new CheckBox();
            RefreshTimer = new System.Windows.Forms.Timer(components);
            Overlap = new CheckBox();
            AutomaticCollection = new CheckBox();
            PlantsInvincible = new CheckBox();
            SuspendLayout();
            // 
            // InfiniteSunshine
            // 
            InfiniteSunshine.AutoSize = true;
            InfiniteSunshine.Location = new Point(17, 13);
            InfiniteSunshine.Name = "InfiniteSunshine";
            InfiniteSunshine.Size = new Size(91, 24);
            InfiniteSunshine.TabIndex = 0;
            InfiniteSunshine.Text = "无限阳光";
            InfiniteSunshine.UseVisualStyleBackColor = true;
            InfiniteSunshine.CheckedChanged += InfiniteSunshine_CheckedChanged;
            // 
            // SunshineTimer
            // 
            SunshineTimer.Interval = 1000;
            SunshineTimer.Tick += SunshineTimer_Tick;
            // 
            // KillZombie
            // 
            KillZombie.AutoSize = true;
            KillZombie.Location = new Point(125, 13);
            KillZombie.Name = "KillZombie";
            KillZombie.Size = new Size(91, 24);
            KillZombie.TabIndex = 1;
            KillZombie.Text = "秒杀僵尸";
            KillZombie.UseVisualStyleBackColor = true;
            KillZombie.CheckedChanged += KillZombie_CheckedChanged;
            // 
            // Init
            // 
            Init.Enabled = true;
            Init.Tick += Init_Tick;
            // 
            // Refresh
            // 
            Refresh.AutoSize = true;
            Refresh.Location = new Point(227, 12);
            Refresh.Name = "Refresh";
            Refresh.Size = new Size(76, 24);
            Refresh.TabIndex = 2;
            Refresh.Text = "无冷却";
            Refresh.UseVisualStyleBackColor = true;
            Refresh.CheckedChanged += Refresh_CheckedChanged;
            // 
            // RefreshTimer
            // 
            RefreshTimer.Tick += RefreshTimer_Tick;
            // 
            // Overlap
            // 
            Overlap.AutoSize = true;
            Overlap.Location = new Point(325, 14);
            Overlap.Name = "Overlap";
            Overlap.Size = new Size(91, 24);
            Overlap.TabIndex = 3;
            Overlap.Text = "植物重叠";
            Overlap.UseVisualStyleBackColor = true;
            Overlap.CheckedChanged += Overlap_CheckedChanged;
            // 
            // AutomaticCollection
            // 
            AutomaticCollection.AutoSize = true;
            AutomaticCollection.Location = new Point(434, 14);
            AutomaticCollection.Name = "AutomaticCollection";
            AutomaticCollection.Size = new Size(91, 24);
            AutomaticCollection.TabIndex = 4;
            AutomaticCollection.Text = "自动收集";
            AutomaticCollection.UseVisualStyleBackColor = true;
            AutomaticCollection.CheckedChanged += AutomaticCollection_CheckedChanged;
            // 
            // PlantsInvincible
            // 
            PlantsInvincible.AutoSize = true;
            PlantsInvincible.Location = new Point(541, 13);
            PlantsInvincible.Name = "PlantsInvincible";
            PlantsInvincible.Size = new Size(91, 24);
            PlantsInvincible.TabIndex = 5;
            PlantsInvincible.Text = "植物无敌";
            PlantsInvincible.UseVisualStyleBackColor = true;
            PlantsInvincible.CheckedChanged += PlantsInvincible_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(660, 264);
            Controls.Add(PlantsInvincible);
            Controls.Add(AutomaticCollection);
            Controls.Add(Overlap);
            Controls.Add(Refresh);
            Controls.Add(KillZombie);
            Controls.Add(InfiniteSunshine);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox InfiniteSunshine;
        private System.Windows.Forms.Timer SunshineTimer;
        private CheckBox KillZombie;
        private System.Windows.Forms.Timer Init;
        private CheckBox Refresh;
        private System.Windows.Forms.Timer RefreshTimer;
        private CheckBox Overlap;
        private CheckBox AutomaticCollection;
        private CheckBox PlantsInvincible;
    }
}
