
namespace WinForms_REST_Server
{
    partial class MyForm1
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
            this.myRichTextBox1 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // myRichTextBox1
            // 
            this.myRichTextBox1.Location = new System.Drawing.Point(12, 12);
            this.myRichTextBox1.Name = "myRichTextBox1";
            this.myRichTextBox1.ReadOnly = true;
            this.myRichTextBox1.Size = new System.Drawing.Size(646, 426);
            this.myRichTextBox1.TabIndex = 0;
            this.myRichTextBox1.Text = "";
            // 
            // MyForm1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.myRichTextBox1);
            this.Name = "MyForm1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MyForm1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox myRichTextBox1;
    }
}

