using System;
using System.Net;
using System.Windows.Forms;
using System.IO;

namespace ParseAvitoAds
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		public static string GetHtml(string url)
		{
			string fullurl = url;

			// create WebClient object
			using (var webClient = new WebClient())
			{
				webClient.Encoding = System.Text.Encoding.UTF8;
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

		private void startButton_Click(object sender, EventArgs e)
		{
			string startUrl = "https://www.avito.ru/moskva?radius=5&geoCoords=55.733939038966945%2C37.6406529148446";
			string parseStr1 = "data-item-url=\"";

			//loading main Avito page with recent ads
			startButton.Text = "require main avito page..";
			Application.DoEvents();
			string html1 = GetHtml(startUrl);
			File.WriteAllText("html.txt", html1);
			startButton.Text = "main avito page is loaded and save";
			Application.DoEvents();

			string adsUrl = "init";
			int ind1, ind2 = 1;
			string[] urls = new string[100];

			//parsing ads urls from main page
			while (adsUrl.Length > 0)
			{
				ind1 = html1.IndexOf(parseStr1);
				html1 = html1.Substring(ind1+ parseStr1.Length, (html1.Length - ind1 - parseStr1.Length));
				ind2 = html1.IndexOf("\"");
				if (ind1 > 0 && ind2 > 0) adsUrl = html1.Substring(0, ind2);
				MessageBox.Show(adsUrl);
			}
		}
	}
}
