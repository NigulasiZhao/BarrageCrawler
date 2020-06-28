using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BarrageCrawler.Models;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using System.IO.Compression;
using JiebaNet.Segmenter.Common;
using JiebaNet.Segmenter;
using System.Drawing;

namespace BarrageCrawler.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public string DM()
        {
            #region MyRegion
            string filePath = @"./WordText/cn_stopwords.txt";

            string[] Text = System.IO.File.ReadAllText(filePath).Split('\n');
            #endregion

            string url = "https://api.bilibili.com/x/v1/dm/list.so?oid=201056987";
            string asda = "";
            using (var client = new WebClient())
            {

                var response = client.DownloadData(url);
                asda = DecompressString(response);
                var responseString = Encoding.Default.GetString(response);
            }
            var segmenter = new JiebaSegmenter();
            XmlDocument doc = new XmlDocument();//新建一个XML文档
            doc.LoadXml(asda);//将字符串转换成XML文档
            List<string> list = new List<string>();
            List<string> listC = new List<string>();
            List<int> listPV = new List<int>();
            XmlNode idNodes = doc.SelectSingleNode("i");
            StringBuilder stringall = new StringBuilder("");
            foreach (XmlNode node1 in idNodes.ChildNodes)
            {
                stringall.Append(node1.InnerText);
                //foreach (var item in segmenter.Cut(node1.InnerText))
                //{
                //    list.Add(item);
                //}

            }
            var freqs = new Counter<string>(segmenter.Cut(stringall.ToString(), cutAll: true));
            foreach (var pair in freqs.MostCommon(200))
            {
                if (!Text.Contains(pair.Key))
                {
                    listC.Add(pair.Key);
                    listPV.Add(pair.Value);
                }
            }
            var wc = new WordCloud.WordCloud(2500, 2500, true);

            Image ig = wc.Draw(listC, listPV);
            //ig.Save(DateTime.Now.ToString("yyyy-MM-dd") + ".png", System.Drawing.Imaging.ImageFormat.Png);
            ig.Save("./wwwroot/WordCloudPicture/" + DateTime.Now.ToString("yyyy-MM-dd") + Guid.NewGuid() + ".png");
            //list.Add(segmenter.Cut(node1.InnerText, cutAll: true).Join(''));
            XmlNodeList weather_nodes = doc.GetElementsByTagName("weather");//读取XML文档的父节点
            XmlNodeList wind_nodes = doc.GetElementsByTagName("wind");
            XmlNodeList temperature_nodes = doc.GetElementsByTagName("temperature");
            XmlNodeList desNodeList = doc.GetElementsByTagName("des");

            XmlNode today_weather_node = weather_nodes[0];//读取某一父节点下的子节点
            XmlNode today_wind_node = wind_nodes[0];
            XmlNode today_temperature_node = temperature_nodes[0];
            XmlNode desNode1 = desNodeList[0];
            XmlNode desNode2 = desNodeList[5];
            return today_weather_node.InnerText + " " + today_wind_node.InnerText + " " +
                today_temperature_node.InnerText + "。" + desNode1.InnerText +
                desNode2.InnerText;
        }
        /// <summary>  
        /// 解压字符串  
        /// </summary>  
        /// <param name="str"></param>  
        /// <returns></returns>  
        public static string DecompressString(byte[] str)
        {
            return Encoding.UTF8.GetString(DecompressBytes(str));
        }
        public static byte[] DecompressBytes(byte[] str)
        {
            var ms = new MemoryStream(str) { Position = 0 };
            var outms = new MemoryStream();
            using (var deflateStream = new DeflateStream(ms, CompressionMode.Decompress, true))
            {
                var buf = new byte[1024];
                int len;
                while ((len = deflateStream.Read(buf, 0, buf.Length)) > 0)
                    outms.Write(buf, 0, len);
            }
            //string asdas = System.Text.Encoding.UTF8.GetString(outms.ToArray(), 0, outms.ToArray().Length);
            return outms.ToArray();
        }
    }
}
