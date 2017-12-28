using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace M2MLogCheck
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (Control control in Controls)
            {
                if (control.GetType() == typeof(TextBox))
                {
                    ((TextBox)control).Text = "";
                }
            }
            btnCheck.Text = "检查日志";
            btnMca.Text = "<<";
            btnLog.Text = "<<";
            progressBar1.Visible = false;
            // 获取当前程序集Assembly的文件名
            this.Text = Assembly.GetExecutingAssembly().GetName().Name;

            //为比对位数下拉列表增加可选参数
            cmbLen.Items.Add("20");
            cmbLen.Items.Add("19");
            cmbLen.Items.Add("18");
            cmbLen.Items.Add("0");
            cmbLen.SelectedIndex = 0;

        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtLog.Text = folderBrowserDialog1.SelectedPath;

            }


        }

        private void btnMca_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtMca.Text = folderBrowserDialog1.SelectedPath;
                //if (txtMca.ToString().LastIndexOf('\\') != txtMca.ToString().Length - 1)
                //{
                //    txtMca.Text = txtMca.Text + "\\";
                //}
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取内外不一致比对长度,0则不比对
                int checkLen = Int32.Parse(cmbLen.Text);

                //获取文件列表
                string[] logFiles = M2M.GetFileList(txtLog.Text, "*.log");
                string[] mcaFiles = M2M.GetFileList(txtMca.Text, "*.mca");

                //合并文件
                string mcaCombinePath = M2M.CombineMca(mcaFiles);
                string logCombinePath = M2M.CombineLog(logFiles);

                StreamReader logReader = new StreamReader(logCombinePath, Encoding.Default);
                StreamReader mcaReader = new StreamReader(mcaCombinePath, Encoding.Default);
                //获取当前系统桌面路径
                string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                //MessageBox.Show(desktopDir);
                //unt-8编码
                StreamWriter sWriter = new StreamWriter(desktopDir + "\\日志比对结果.txt", false, Encoding.UTF8);

                sWriter.WriteLine("----------------------日志重复项------------------------------");
                string logLine = null;
                string mcaLine = null;
                long logNumber = 0;
                long mcaNumber = 0;
                HashSet<string> hashSet = new HashSet<string>();
                List<LogData> repeatList = new List<LogData>();
                List<LogData> inOutDifferetList = new List<LogData>();  //内外信息不一致
                LogData logData;
                string startLogICCID = null;
                string startLogPrint = null;
                string endLogICCID = null;
                string endLogPrint = null;

                //核对日志重复项
                while ((logLine = logReader.ReadLine()) != null)
                {
                    if (logLine.IndexOf("ICCID") == -1)
                    {
                        logNumber++;
                        string[] logFields = logLine.Split(',');
                        logData.ICCID = logFields[0];
                        logData.printData = logFields[1];
                        logData.filePath = logFields[2];
                        logData.lineNumber = logFields[3];
                        if (hashSet.Add(logData.ICCID))
                        {
                            //ok;
                        }
                        else
                        {
                            repeatList.Add(logData);
                        }
                        //截取日志首尾号
                        if (logNumber == 1)
                        {
                            startLogICCID = logData.ICCID;
                            startLogPrint = logData.printData;
                            endLogICCID = logData.ICCID;
                            endLogPrint = logData.printData;

                        }
                        // 获取日志文件中最小的号，作为首号
                        if (String.Compare(logData.ICCID, startLogICCID) < 0)
                        {
                            startLogICCID = logData.ICCID;
                            startLogPrint = logData.printData;
                        }

                        // 获取日志文件中最大的号，作为尾号
                        if (String.Compare(logData.ICCID, endLogICCID) > 0)
                        {
                            endLogICCID = logData.ICCID;
                            endLogPrint = logData.printData;
                        }

                        //比较内外不一致
                        if (checkLen != 0)
                        {
                            LogData log = handleData(logData, checkLen);
                            if (log.ICCID != log.printData)
                            {
                                inOutDifferetList.Add(log);
                            }
                        }

                    }
                }
                logReader.Close();
                sWriter.WriteLine("重复卡号: " + repeatList.Count);
                sWriter.WriteLine("重复ICCID,打印信息,日志文件名,行号");

                for (int i = 0; i < repeatList.Count; i++)
                {
                    sWriter.WriteLine(repeatList[i].ICCID + "," + repeatList[i].printData + "," + repeatList[i].filePath + "," + repeatList[i].lineNumber);
                }
                sWriter.WriteLine();
                sWriter.WriteLine();
                if (checkLen != 0)
                {
                    //--------------输出内外不一致的信息-------------------
                    sWriter.WriteLine("--------------------内外不一致结果汇总---------------------------");
                    sWriter.WriteLine("内外不一致卡号: " + inOutDifferetList.Count);
                    sWriter.WriteLine("不一致ICCID,打印信息,日志文件名,行号");

                    for (int i = 0; i < inOutDifferetList.Count; i++)
                    {
                        sWriter.WriteLine(inOutDifferetList[i].ICCID + "," + inOutDifferetList[i].printData + "," + inOutDifferetList[i].filePath + "," + inOutDifferetList[i].lineNumber);
                    }
                    sWriter.WriteLine();
                    sWriter.WriteLine();
                }
                sWriter.WriteLine("----------------------日志缺失项------------------------------");
                //核对日志缺失
                List<McaData> lackList = new List<McaData>();
                McaData mcaDate;
                string startMcaICCID = null;
                string startMcaPrint = null;
                string endMcaICCID = null;
                string endMcaPrint = null;

                while ((mcaLine = mcaReader.ReadLine()) != null)
                {
                    if (mcaLine.IndexOf("ICCID") == -1)
                    {
                        mcaNumber++;
                        string[] mcaFields = mcaLine.Split(',');
                        mcaDate.ICCID = mcaFields[0];
                        mcaDate.printData = mcaFields[1];
                        mcaDate.filePath = mcaFields[2];
                        mcaDate.lineNumber = mcaFields[3];
                        if (hashSet.Add(mcaDate.ICCID))
                        {

                            lackList.Add(mcaDate);
                        }
                        else
                        {
                            //ok;
                        }
                        if (mcaNumber == 1)
                        {
                            startMcaICCID = mcaDate.ICCID;
                            startMcaPrint = mcaDate.printData;
                            endMcaICCID = mcaDate.ICCID;
                            endMcaPrint = mcaDate.printData;

                        }
                        if (String.Compare(mcaDate.ICCID, startMcaICCID) < 0)
                        {
                            startMcaICCID = mcaDate.ICCID;
                            startMcaPrint = mcaDate.printData;
                        }
                        if (String.Compare(mcaDate.ICCID, endMcaICCID) > 0)
                        {
                            endMcaICCID = mcaDate.ICCID;
                            endMcaPrint = mcaDate.printData;
                        }
                    }
                }
                mcaReader.Close();
                sWriter.WriteLine("缺失卡号: " + lackList.Count);
                sWriter.WriteLine("缺失ICCID,打印信息,日志文件名,行号");

                for (int i = 0; i < lackList.Count; i++)
                {
                    sWriter.WriteLine(lackList[i].ICCID + "," + lackList[i].printData + "," + lackList[i].filePath + "," + lackList[i].lineNumber);
                }
                sWriter.WriteLine();
                sWriter.WriteLine();
                sWriter.WriteLine("--------------------日志比对结果汇总---------------------------");
                sWriter.WriteLine("原始数据: " + mcaNumber + " 条");
                sWriter.WriteLine("日志数据: " + logNumber + " 条");
                sWriter.WriteLine("重复卡号: " + repeatList.Count);
                sWriter.WriteLine("缺失卡号: " + lackList.Count);
                sWriter.WriteLine("内外不一致: " + inOutDifferetList.Count);
                sWriter.WriteLine();
                sWriter.WriteLine();
                sWriter.WriteLine("----------------------日志比对范围-----------------------------");
                sWriter.WriteLine("数据起始号段: " + startMcaICCID + "      打印信息： " + startMcaPrint);
                sWriter.WriteLine("数据结尾号段: " + endMcaICCID + "      打印信息： " + endMcaPrint);
                sWriter.WriteLine();
                sWriter.WriteLine("日志起始号段: " + startLogICCID + "      打印信息： " + startLogPrint);
                sWriter.WriteLine("日志结尾号段: " + endLogICCID + "      打印信息： " + endLogPrint);

                sWriter.WriteLine();
                if (String.Equals(startMcaICCID, startLogICCID) && String.Equals(endMcaICCID, endLogICCID))
                {
                    sWriter.WriteLine("首尾匹配");
                }
                else
                {
                    sWriter.WriteLine("首尾不匹配！！！");
                }

                sWriter.WriteLine();
                //sWriter.WriteLine(DateTime.Now.ToString()); //输出时间
                //sWriter.WriteLine();

                sWriter.Close();

                ////删除临时文件
                if (File.Exists(mcaCombinePath))
                {
                    File.Delete(mcaCombinePath);
                }
                if (File.Exists(logCombinePath))
                {
                    File.Delete(logCombinePath);
                }
                MessageBox.Show("ok!" + "\r\n" + "重复卡号: " + repeatList.Count +
                                            "\r\n" + "缺失卡号: " + lackList.Count +
                                             "\r\n" + "内外不一致: " + inOutDifferetList.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show("出问题了，请检查!");
            }
        }

        /// <summary>
        /// 处理巴基斯坦日志类型
        /// </summary>
        /// <param name="log"></param>
        /// <param name="checklen"></param>
        /// <returns></returns>
        private static LogData handleData(LogData log,int checklen) {
            string print = log.printData;
            string iccid = log.ICCID;

            if (print.Length >= checklen) {
                log.printData = print.Substring(0, checklen);
            }
            if (iccid.Length >= checklen)
            {
                log.ICCID = iccid.Substring(0, checklen);
            }
            return log;

        }
    }
}
