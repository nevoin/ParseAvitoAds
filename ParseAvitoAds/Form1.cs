using System;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace ParseAvitoAds
{
	public partial class Form1 : Form
	{
		const string startUrl = "https://www.avito.ru/moskva?radius=5&geoCoords=55.733939038966945%2C37.6406529148446";
		const string parseAllUrls = "data-item-url=\"";
		const string avitoUrlBegin = "https://www.avito.ru";
		const string parseOneAdsTitle1 = "<div class=\"sticky-header-prop sticky-header-title\">";
		const string parseOneAdsTitle2 = "</div>";

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
			//string body = "";
			//string pic = "";
			//string phone = "";
			//string url = "";
		}

		public static string GetHtml(string url)
		{
			string fullurl = url;

			// create WebClient object
			using (var webClient = new WebClient())
			{
				webClient.Encoding = System.Text.Encoding.UTF8;
				webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12");
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
		public void ParseAllUrls(string[] urls, byte urlsCount)
		{
			Random r = new Random();
			int ind1, ind2 = 0;
			byte sleepAntiBan = 0;


			//for (int i=0; i < urlsCount; i++)
			for (int i = 0; i < 5; i++)
			{
				parsedAds[i] = new Ads();
				sleepAntiBan = Convert.ToByte(r.Next(10));
				startButton.Text = "Sleep " + sleepAntiBan.ToString() + " seconds";
				Application.DoEvents();
				Thread.Sleep(sleepAntiBan * 1000); //in seconds
				string html1 = GetHtml(urls[i]);
				ind1 = html1.IndexOf(parseOneAdsTitle1);
				if (ind1>0)
				{
					html1 = html1.Substring(ind1 + parseOneAdsTitle1.Length);
					ind2 = html1.IndexOf(parseOneAdsTitle2);
					if (ind2 > 0) parsedAds[i].title = html1.Substring(0, ind2);
				}
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
			File.WriteAllText("allurls.txt",parsedAds.ToString());
			ParseAllUrls(urls, urlsCount);

			Environment.Exit(0);
		}
	}
}
