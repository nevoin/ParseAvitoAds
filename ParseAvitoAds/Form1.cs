using System;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Diagnostics;


// Copyright(C) 2019 Dmitriy Slabnov

namespace ParseAvitoAds
{

	public partial class Form1 : Form
	{
		const string startUrl = "https://www.avito.ru/moskva?radius=5&geoCoords=55.733939038966945%2C37.6406529148446";
		const string parseAllUrls = "data-item-url=\"";
		const string avitoUrlBegin = "https://www.avito.ru";
		const string parseOneAdsTitle1 = "<div class=\"sticky-header-prop sticky-header-title\">";
		const string parseOneAdsTitle2 = "</div>";
		const string parseOneAdsBody1 = "<div class=\"item-description-text\" itemprop=\"description\">";
		const string parseOneAdsBody2 = "</div>";
		const string parseOneAdsPic1 = "avito.item.image = '";
		const string parseOneAdsPic2 = "';";
		const byte trimPerc = 14;//in percents - trim Avito logo
		const string link2Phone = "#login?s=p";

		string[] urls = new string[100];
		Ads[] parsedAds = new Ads[100];
		byte urlsCount = 0;

		public Form1()
		{
			InitializeComponent();
		}

		public class Ads
		{
			public string title="";
			public string body = "";
			public string pic = "";
			//string phone = "";
		}

		public void ExecProg(string path)
		{
			Process iStartProcess = new Process(); 
			iStartProcess.StartInfo.FileName = @path; // path+name
			//iStartProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; // hidden start
			iStartProcess.Start(); 
			//iStartProcess.WaitForExit(30 * 1000); // wait in ms
		}

		public static string GetHtml(string url)
		{
			string fullurl = url;

			// create WebClient object
			using (var webClient = new WebClient())
			{
				webClient.Encoding = System.Text.Encoding.UTF8;
				webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12");
				webClient.Headers.Add("Referer", "https://yandex.ru/");
				try
				{
					return webClient.DownloadString(fullurl);
				}
				catch (Exception)
				{
					return "Some Error";
				}
			}
		}
		
		//parse each ads - title/body/phone/pic
		//wrong parse for Company or Vip ads?
		public void ParseAllUrls(string[] urls, byte urlsCount)
		{
			Random r = new Random();
			int ind1, ind2 = 0;
			byte sleepAntiBan = 0;
			string html, tmpHtml = "";


			//for (int i=0; i < urlsCount; i++)
			for (int i = 0; i < 10; i++)
			{
				parsedAds[i] = new Ads();

				sleepAntiBan = Convert.ToByte(r.Next(10));
				startButton.Text = "Sleep " + sleepAntiBan.ToString() + " seconds, "+(urlsCount-i).ToString()+" urls left";
				Application.DoEvents();
				Thread.Sleep(sleepAntiBan * 1000); //in seconds

				html=tmpHtml = GetHtml(urls[i]);
				//parse title
				ind1 = tmpHtml.IndexOf(parseOneAdsTitle1);
				if (ind1>0)
				{
					tmpHtml = tmpHtml.Substring(ind1 + parseOneAdsTitle1.Length);
					ind2 = tmpHtml.IndexOf(parseOneAdsTitle2);
					if (ind2 > 0) parsedAds[i].title = tmpHtml.Substring(2, ind2-4);
				}

				//parse body
				ind1 = tmpHtml.IndexOf(parseOneAdsBody1);
				if (ind1 > 0)
				{
					tmpHtml = tmpHtml.Substring(ind1 + parseOneAdsBody1.Length);
					ind2 = tmpHtml.IndexOf(parseOneAdsBody2);
					if (ind2 > 0) parsedAds[i].body = tmpHtml.Substring(3, ind2 - 4);
				}

				//parse pic url
				ind1 = html.IndexOf(parseOneAdsPic1);
				if (ind1 > 0)
				{
					tmpHtml = html.Substring(ind1 + parseOneAdsPic1.Length);
					ind2 = tmpHtml.IndexOf(parseOneAdsPic2);
					if (ind2 > 0) parsedAds[i].pic = "https:"+tmpHtml.Substring(0, ind2).Replace("208x156", "640x480");
				}

				//do not save ads with empty body
				if (parsedAds[i].body.Length>0) File.WriteAllText("ads"+i.ToString()+".txt", parsedAds[i].title+ "\n\n" + parsedAds[i].body+"\n");

				//trim avito logo and save
				pictureBox1.Load(parsedAds[i].pic);
				Application.DoEvents();
				Bitmap bmp = pictureBox1.Image as Bitmap;
				Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height-(bmp.Height/100* trimPerc));
				if (bmp != null)
				{
					Bitmap cropBmp = bmp.Clone(rect, bmp.PixelFormat);
					if (cropBmp != null)
					{
						cropBmp.Save("ads" + i.ToString() + ".bmp");
					}
				}

				//parse phone
				ExecProg(urls[i]); // open page in current browser (tested  in Chrome)
				Thread.Sleep(20000); //waiting until page load

				for (byte c=0;c<35;c++)
				{
					SendKeys.SendWait("{TAB}");
				}
				SendKeys.SendWait("{ENTER}");				
			}
		}

		public void ParseMainPage(ref string [] urls, ref byte urlsCount)
		{
			//loading main Avito page with recent ads
			startButton.Text = "require main avito page..";
			Application.DoEvents();
			string html1 = GetHtml(startUrl);
			startButton.Text = "main avito page is loaded";
			Application.DoEvents();

			string adsUrl = "init";
			int ind1, ind2 = 1;
			bool goParse = true;

			//parsing ads urls from main page
			while (goParse)
			{
				ind1 = html1.IndexOf(parseAllUrls);
				if (ind1 > 0)
				{
					html1 = html1.Substring(ind1 + parseAllUrls.Length, (html1.Length - ind1 - parseAllUrls.Length));
					ind2 = html1.IndexOf("\"");
					if (ind2 > 0)
					{
						adsUrl = avitoUrlBegin + html1.Substring(0, ind2);
						urls[urlsCount] = adsUrl;
						urlsCount++;
					}
				}
				else goParse = false;
			}

		}
 
		private void startButton_Click(object sender, EventArgs e)
		{
			ParseMainPage(ref urls, ref urlsCount);
			ParseAllUrls(urls, urlsCount);

			Environment.Exit(0);
		}
	}
}
