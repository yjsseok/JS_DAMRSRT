namespace JS_DAMRSRT
{
    partial class MainFrm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.listStatus = new System.Windows.Forms.ListBox();
            this.Btn_code_load = new System.Windows.Forms.Button();
            this.Btn_Load_FR = new System.Windows.Forms.Button();
            this.Btn_Load_Dam = new System.Windows.Forms.Button();
            this.Btn_Load_AR = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listStatus
            // 
            this.listStatus.FormattingEnabled = true;
            this.listStatus.ItemHeight = 12;
            this.listStatus.Location = new System.Drawing.Point(12, 55);
            this.listStatus.Name = "listStatus";
            this.listStatus.Size = new System.Drawing.Size(728, 592);
            this.listStatus.TabIndex = 0;
            // 
            // Btn_code_load
            // 
            this.Btn_code_load.Location = new System.Drawing.Point(12, 26);
            this.Btn_code_load.Name = "Btn_code_load";
            this.Btn_code_load.Size = new System.Drawing.Size(109, 23);
            this.Btn_code_load.TabIndex = 1;
            this.Btn_code_load.Text = "코드 불러오기";
            this.Btn_code_load.UseVisualStyleBackColor = true;
            this.Btn_code_load.Click += new System.EventHandler(this.Btn_Check_code);
            // 
            // Btn_Load_FR
            // 
            this.Btn_Load_FR.Location = new System.Drawing.Point(137, 26);
            this.Btn_Load_FR.Name = "Btn_Load_FR";
            this.Btn_Load_FR.Size = new System.Drawing.Size(144, 23);
            this.Btn_Load_FR.TabIndex = 2;
            this.Btn_Load_FR.Text = "유량";
            this.Btn_Load_FR.UseVisualStyleBackColor = true;
            this.Btn_Load_FR.Click += new System.EventHandler(this.Btn_Load_FR_Click);
            // 
            // Btn_Load_Dam
            // 
            this.Btn_Load_Dam.Location = new System.Drawing.Point(296, 26);
            this.Btn_Load_Dam.Name = "Btn_Load_Dam";
            this.Btn_Load_Dam.Size = new System.Drawing.Size(144, 23);
            this.Btn_Load_Dam.TabIndex = 3;
            this.Btn_Load_Dam.Text = "댐저수율";
            this.Btn_Load_Dam.UseVisualStyleBackColor = true;
            this.Btn_Load_Dam.Click += new System.EventHandler(this.Btn_Load_Dam_Click);
            // 
            // Btn_Load_AR
            // 
            this.Btn_Load_AR.Location = new System.Drawing.Point(455, 26);
            this.Btn_Load_AR.Name = "Btn_Load_AR";
            this.Btn_Load_AR.Size = new System.Drawing.Size(144, 23);
            this.Btn_Load_AR.TabIndex = 4;
            this.Btn_Load_AR.Text = "농업용저수지 저수량";
            this.Btn_Load_AR.UseVisualStyleBackColor = true;
            this.Btn_Load_AR.Click += new System.EventHandler(this.Btn_Load_AR_Click);
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 785);
            this.Controls.Add(this.Btn_Load_AR);
            this.Controls.Add(this.Btn_Load_Dam);
            this.Controls.Add(this.Btn_Load_FR);
            this.Controls.Add(this.Btn_code_load);
            this.Controls.Add(this.listStatus);
            this.Name = "MainFrm";
            this.Text = "MainFrm";
            this.Load += new System.EventHandler(this.MainFrm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listStatus;
        private System.Windows.Forms.Button Btn_code_load;
        private System.Windows.Forms.Button Btn_Load_FR;
        private System.Windows.Forms.Button Btn_Load_Dam;
        private System.Windows.Forms.Button Btn_Load_AR;
    }
}

