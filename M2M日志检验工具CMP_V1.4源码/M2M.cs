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
           public string  ICCID;
            public string  filePath;
            public string  lineNumber;
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
                    temp = temps[ICCIDIndex]+",";
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
            sWriter.WriteLine("检验ICCID,日志文件名,行号");
            foreach (string filePath in filePaths)
            {
                StreamReader sReader = new StreamReader(filePath, Encoding.Default);
                lineNumber=0;
                while ((currentLine = sReader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (currentLine.IndexOf("]") > -1)
                    {
                        string[] temps = currentLine.Split(']');
                        temp=temps[2];      //ICCID为第3列
                        temp = temp.Replace("[", "");
                        temp = temp.Replace(",", "");
                        temp = temp.Replace(" =", "");
                        temp = temp.Trim();

                       
                        temp = temp  + "," + Path.GetFileName(filePath)+ "," + lineNumber;
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


       
    }
}
