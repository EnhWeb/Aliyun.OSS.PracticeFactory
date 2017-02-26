using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aliyun.OSS.PracticeFactory.HelloWord
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            string filepath = this.openFileDialog1.ShowDialog() == DialogResult.OK ? this.openFileDialog1.FileName : "";
            if(filepath!="")
            {
                this.textBox2.Text = filepath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //要上传的文件路径
            string filePath = this.textBox2.Text;

            var fileinfo = new System.IO.FileInfo(filePath);
            if (!fileinfo.Exists)
            {
                MessageBox.Show("文件不存在，请重新选择文件！");
            }

            this.textBox3.Text += WebEnhAliyunOss.WebEnhOssUploadsHelper.UploadFileToOSS(filePath);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
