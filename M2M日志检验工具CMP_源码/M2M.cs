using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace M2MLogCheck
{
    struct McaData
    {
        public string ICCID;
        public string printData;
        public string filePath;
        public string lineNumber;
    }
    struct LogData
    {
        public string ICCID;
        public string printData;
        public string filePath;
        public string lineNumber;
    }


    class M2M
    {
        /// <summary>
        /// Mca文件需要提取的数据
        /// </summary>
       

        /// <summary>
        /// 合并文件
        /// </summary>
        /// <param name="filePaths">需要合并的问件路径数组</param>
        /// <returns></returns>
        public static string CombineMca(string[] filePaths)
        {
            string combinePath = Path.GetDirectoryName(filePaths[0]) + "\\Combine" + 
                Path.GetExtension(filePaths[0]);
            //if (File.Exists(combinePath))
            //{
            //    File.Delete(combinePath);
            //}

            string currentLine ;

            StreamReader Reader = new StreamReader(filePaths[0], Encoding.Default);
                currentLine=Reader.ReadLine();         
            Reader.Close();
            string[] titleData=currentLine.Split(',');
            List<int> list=new List<int>();
            int ICCIDIndex = 0;
            for(int i=0;i<titleData.Length;i++)
            {
                if(titleData[i]=="ICCID")
                {
                    ICCIDIndex=i;
                }
                else if (titleData[i].IndexOf("打印数据") > -1)
                {
                    list.Add(i);
                }
            }
            int[] printDatas=list.ToArray();
            string temp = null;
            long lineNumber=0;
            StreamWriter sWriter = new StreamWriter(combinePath, false, Encoding.Default);
            sWriter.WriteLine("原始ICCID,打印数据项,文件名,行号");
            foreach (string filePath in filePaths)
            {
                lineNumber=0;
                StreamReader sReader = new StreamReader(filePath,Encoding.Default);
                while ((currentLine = sReader.ReadLine()) != null)
                {
                    lineNumber++;
                    string[] temps = currentLine.Split(',');
                    temp = temps[ICCIDIndex];
                    //如果iccid倒序，需要将其正回来，变成8986开头
                    if (temp.Substring(0, 2) == "98")
                    {
                        temp = Swap(temp);
                    }
                    temp = temp + ",";
                    foreach (int i in printDatas)
                    {
                        temp += temps[i];//((i == printDatas[printDatas.Length-1]) ? "" : ",");
                    }
                    if (temp.IndexOf("ICCID") == -1)
                    {
                        temp += "," + Path.GetFileName(filePath) + "," + lineNumber;
                        sWriter.WriteLine(temp); 
                    }
                                     
                } 
                sReader.Close();
            }
            sWriter.Close();

            return combinePath;
        }
        public static string CombineLog(string[] filePaths)
        {
            string combinePath = Path.GetDirectoryName(filePaths[0]) + "\\Combine" +
                Path.GetExtension(filePaths[0]);
            //if (File.Exists(combinePath))
            //{
            //    File.Delete(combinePath);
            //}

            string currentLine;

            //StreamReader Reader = new StreamReader(filePaths[0], Encoding.Default);
            //currentLine = Reader.ReadLine();
            //Reader.Close();
            //string[] titleData = currentLine.Split(',');
            //List<int> list = new List<int>();
            //int ICCIDIndex = 0;
            //for (int i = 0; i < titleData.Length; i++)
            //{
            //    if (titleData[i] == "ICCID")
            //    {
            //        ICCIDIndex = i;
            //    }
            //    else if (titleData[i].IndexOf("打印数据") > -1)
            //    {
            //        list.Add(i);
            //    }
            //}
            //int[] printDatas = list.ToArray();
            string temp = null;
            long lineNumber=0;
            StreamWriter sWriter = new StreamWriter(combinePath,false,Encoding.Default);
            sWriter.WriteLine("检验ICCID,打印数据,日志文件名,行号");
            foreach (string filePath in filePaths)
            {
                StreamReader sReader = new StreamReader(filePath, Encoding.Default);
                lineNumber=0;
                string iccid = null;
                string printData = null;
                while ((currentLine = sReader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (currentLine.IndexOf("]") > -1)
                    {
                        string[] temps = currentLine.Split(']');
                        iccid=temps[2];      //ICCID为第3列                     
                        iccid = iccid.Replace("[", "");
                        iccid = iccid.Replace(",", "");
                        iccid = iccid.Replace(" =", "");
                        iccid = iccid.Trim();

                        //如果iccid倒序，需要将其正回来，变成8986开头      
                        if (iccid.Substring(0, 2) == "98")
                        {
                            iccid = Swap(iccid);
                        }
                        printData = temps[3]; //打印数据为第4列
                        printData = printData.Replace("[", "");
                        printData = printData.Replace(",", "");
                        printData = printData.Trim();

                        temp = iccid + "," +printData+","+ Path.GetFileName(filePath)+ "," + lineNumber;
                        sWriter.WriteLine(temp);
                    }
                        
                }
                sReader.Close();
            }
            sWriter.Close();

            return combinePath;
        }
        /// <summary>
        /// 获取文件列表
        /// </summary>
        /// <param name="folder">文件夹路径</param>
        /// <param name="exextension">文件扩展名</param>
        /// <returns></returns>
        public static string[] GetFileList(string folder, string exextension)
        {

            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            FileInfo[] fileInfos = directoryInfo.GetFiles(exextension);
            List<string> list = new List<string>();
            foreach (FileInfo file in fileInfos)
            {
                if (file.Name.IndexOf("Combine") == -1)
                {
                    list.Add(file.FullName);
                }
                else
                {
                    File.Delete(file.FullName);
                }
            }
            list.Sort();
            return list.ToArray();
        }
        /// <summary>
        /// 两两交换
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Swap(string str)
        {
            if (str.Length % 2 != 0) return str;  //如果长度不是偶数，则原样返回

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < str.Length / 2; i++)
            {
                builder.Append(str[2 * i + 1]);
                builder.Append(str[2 * i]);
            }
            return builder.ToString();

        }
    }
}
