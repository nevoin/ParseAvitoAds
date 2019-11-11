﻿using System;
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

		public class Ads
		{
			string title = "";
			string body = "";
			string pic = "";
			string phone = "";
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

		private void startButton_Click(object sender, EventArgs e)
		{
			string startUrl = "https://www.avito.ru/moskva?radius=5&geoCoords=55.733939038966945%2C37.6406529148446";
			string parseStr1 = "data-item-url=\"";
			string avitoUrlStart = "https://www.avito.ru";

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
			byte urlsCount = 0;
			bool goParse = true;

			//parsing ads urls from main page
			while (goParse)
			{
				ind1 = html1.IndexOf(parseStr1);
				if (ind1 > 0)
				{
					html1 = html1.Substring(ind1 + parseStr1.Length, (html1.Length - ind1 - parseStr1.Length));
					ind2 = html1.IndexOf("\"");
					if (ind2 > 0)
					{
						adsUrl = avitoUrlStart + html1.Substring(0, ind2);
						urls[urlsCount] = adsUrl;
						urlsCount++;
					}
				}
				else goParse = false;
			}

			File.WriteAllLines("html.txt", urls);
			Environment.Exit(0);
		}
	}
}
