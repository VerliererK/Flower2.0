using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using ImageCaption.Services;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using System.IO;
using System.Net.Http;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.ConnectorEx;

namespace Bot_Application1.Dialogs
{
	[Serializable]
	public class QuestionnaireDialog : IDialog<object>
	{
		private Dictionary<string, string[]> Question = new Dictionary<string, string[]>();
		private Dictionary<string, string[]> Sites = new Dictionary<string, string[]>();

		private Dictionary<string, string[]>.Enumerator enumerator;
		private string[] lastValue;
		private int imgSendCount = 3;
		private Random random = new Random();

		private ICaptionService captionService = new MicrosoftCognitiveCaptionService();

		public QuestionnaireDialog()
		{
			Init();
		}
		private void Init()
		{
			Question.Add("請問您是否有被診斷出以下狀況？", new string[] { "中風偏癱( 左 / 右 )", "脊髓損傷( 頸 / 胸 / 腰 / 肩 )", "腦性麻痺", "發展遲緩", "小兒麻痺", "運動神經元疾病", "下肢骨折或截肢", "關節炎", "心肺功能疾病", "肌肉萎縮症" });
			Question.Add("請問輪椅主要的操作者為？", new string[] { "自己", "照顧者", "我想重新諮詢" });
            
			Question.Add("接下來會詢問您關於身體各部位的狀況，請您依照自己的感受 / 醫生的診斷結果回答。", new string[] { "好", "我想重新諮詢" });
			Question.Add("坐姿平衡，如果不知道應該選擇哪個選項，請拍一張您的坐姿的照片。", new string[] { "良好", "雙手扶持尚可維持平衡", "雙手扶持難以維持平衡", "我想重新諮詢" });
			Question.Add("骨盆，如果不知道應該選擇哪個選項，請拍一張您的骨盆的照片。", new string[] { "正常", "向前 / 後傾", "向左 / 右傾斜", "向左 / 右旋轉", "我想重新諮詢" });
			Question.Add("脊柱，如果不知道應該選擇哪個選項，請拍一張您的背脊的照片。", new string[] { "正常或無明顯變形", "脊柱側彎", "過度前凸(hyperlordosis)", "過度後凸(hyperkyphosis)", "我想重新諮詢" });
			Question.Add("頭部控制", new string[] { "正常", "偶可維持頭部正中位置但控制不佳或耐力不足", "完全無法控制", "我想重新諮詢" });
			Question.Add("肩部，如果不知道應該選擇哪個選項，請拍一張您的雙肩的照片。", new string[] { "正常", "後縮", "前突", "我想重新諮詢" });
			Question.Add("髖部，如果不知道應該選擇哪個選項，請拍一張您的髖部的照片。", new string[] { "正常", "內收", "外展", "風吹式變形", "其他", "我想重新諮詢" });
			Question.Add("膝部，如果不知道應該選擇哪個選項，請拍一張您的膝部的照片。", new string[] { "正常", "屈曲變形", "伸直變形", "我想重新諮詢" });
			Question.Add("踝部，如果不知道應該選擇哪個選項，請拍一張您的腳踝的照片。", new string[] { "正常", "外翻變形", "蹠屈變形", "我想重新諮詢" });
            
			Sites.Add("headquarter", new string[] { "臺北市合宜輔具中心", "25.0703276", "121.5195652", "Email：hoyiatc@gmail.com", "電話：臺北市內請撥1999轉5888轉9，專線電話02-77137760", "臺北市中山區玉門街1號(臺北悠活村合宜輔具中心)" });
			Sites.Add("west", new string[] { "臺北市西區輔具中心", "25.0505644", "121.5185295", "Email：westateden@gmail.com", "電話：02-2523-7902", "地址：臺北市中山區長安西路5巷2號2樓(臺北市政府衛生局舊址後方)" });
			Sites.Add("south", new string[] { "臺北市南區輔具中心", "25.0270075", "121.5673229", "Email：sdat@diyi.org.tw", "電話：02-27207364 或 02-77137533", "地址：臺北市信義區信義路5段150巷310號1樓" });

			enumerator = Question.GetEnumerator();
		}

		public Task StartAsync(IDialogContext context)
		{
			context.Wait(MessageReceivedAsync);
			return Task.CompletedTask;
		}

		private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			string message = activity.Text;
			if (!string.IsNullOrEmpty(message) && message.Contains("重新"))
			{
				lastValue = null;
				enumerator = Question.GetEnumerator();
			}
			if (!string.IsNullOrEmpty(message) && message.Contains("debug"))
			{
				Int32.Parse("debug");
			}
			// return our reply to the user
			var reply = context.MakeMessage();

			if (lastValue != null &&
				activity.Attachments != null &&
				activity.Attachments.Any() &&
				imgSendCount-- >= 0)
			{
				Boolean pass = false;

				try
				{
					var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
					AnalysisResult analysisResult = await this.GetAnalysisResultAsync(activity, connector);
					string[] tags = analysisResult?.Description?.Tags;
					if (tags != null && tags.Contains("person"))
					{
						imgSendCount = 0;
						pass = true;
					}
                    else if (imgSendCount > 0)//不是人在給他一次機會
                    {
						reply.Text = "這不是人阿XD，再給你 " + imgSendCount + " 次機會哦！ \n\n";
						await context.PostAsync(reply);
                        reply.AddKeyboardCard<string>(enumerator.Current.Key, enumerator.Current.Value);
                    }
				}
				catch (ArgumentException e)
				{
					reply.Text = "你確定你有上傳圖片嗎？圖片流量要收錢耶QAQ";
					reply.Text += "系統被你弄得不要不要的，下一題﹍";

					await context.PostAsync(reply);
					enumerator.MoveNext();
					var current = enumerator.Current;
					lastValue = current.Value;
					reply.AddKeyboardCard<string>(current.Key, current.Value);
				}
				catch (Exception e)
				{
					reply.Text = "維大力？";
					reply.Text += "系統被你弄得不要不要的，下一題﹍";

					await context.PostAsync(reply);
					enumerator.MoveNext();
					var current = enumerator.Current;
					lastValue = current.Value;
					reply.AddKeyboardCard<string>(current.Key, current.Value);
				}

				if (imgSendCount == 0)
				{
                    imgSendCount = 3;
                    if (pass)
					{
						reply.Text = "經過分析﹍你是屬於『" + lastValue.ElementAt(random.Next(0, lastValue.Count() - 1)) + "』";
					}
					else
					{
						reply.Text = "你要拍人啦QQ，下一題﹍";
					}

					await context.PostAsync(reply);
					enumerator.MoveNext();
					var current = enumerator.Current;
					lastValue = current.Value;
					reply.AddKeyboardCard<string>(current.Key, current.Value);
				}
			}
			else if (lastValue != null && lastValue.Length > 0 &&
				!StupidCompare(lastValue, message, 0.2f))
			{
				reply.Text = "不要亂回答啦～";

				await context.PostAsync(reply);

				var current = enumerator.Current;
				lastValue = current.Value;
				reply.AddKeyboardCard<string>(current.Key, current.Value);
			}
			else if (enumerator.MoveNext())
			{
				var current = enumerator.Current;
				lastValue = current.Value;
				reply.AddKeyboardCard<string>(current.Key, current.Value);
			}
			else
			{
				lastValue = null;
				enumerator = Question.GetEnumerator();
                //Boolean isLocation = random.Next(0, 2).Equals(0);
                reply.Type = "message";
				reply.Text = "問答都結束囉！以下是我為你推薦的輪椅﹍";
				await context.PostAsync(reply);

				//if (!isLocation)
				//{
					reply = context.MakeMessage();
					reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

					var card1 = new HeroCard()
					{
						Title = "選擇A",
						Subtitle = "價格範圍：NT$40,000 - NT$80,000",
						Images = new List<CardImage>() { new CardImage(url: "http://www.justicemed.com/upfile/2015/6744ccff2a39e998707b71abf90cbd92.jpg ") },
						Text = "骨架型式：折合式" + Environment.NewLine + "後輪型式：軸心可前後調整" + Environment.NewLine + "手推圈型式：批覆橡膠",
						Buttons = new List<CardAction>() { new CardAction()
						{
							Type = "openUrl",
							Title = "選擇A推薦品牌",
							Value = "https://www.microsoft.com/taiwan/events/2017AI-APP-Contest/ "
						}
					}
					};

					var card2 = new HeroCard()
					{
						Title = "選擇B",
						Subtitle = "價格範圍：NT$60,000 - NT$100,000",
						Images = new List<CardImage>() { new CardImage(url: "http://tedxtaipei.com/wp-content/uploads/2013/04/wheelchair-1.jpg ") },
						Text = "骨架型式：固定式" + Environment.NewLine + "後輪型式：實心胎" + Environment.NewLine + "手推圈型式：無",
						Buttons = new List<CardAction>() { new CardAction()
						{
							Type = "openUrl",
							Title = "選擇B推薦品牌",
							Value = "https://www.microsoft.com/taiwan/events/2017AI-APP-Contest/ "
						}
					}
					};

					var card3 = new HeroCard()
					{
						Title = "選擇C",
						Subtitle = "價格範圍：NT$50,000 - NT$70,000",
						Images = new List<CardImage>() { new CardImage(url: "https://cdn0-techbang.pixcdn.tw/system/excerpt_images/18657/inpage/6d9477e2910482cdebb40382fef94705.jpg?1402910275 ") },
						Text = "骨架型式：折合式" + Environment.NewLine + "後輪型式：軸心可前後調整" + Environment.NewLine + "手推圈型式：批覆橡膠",
						Buttons = new List<CardAction>() { new CardAction()
						{
							Type = "openUrl",
							Title = "選擇C推薦品牌",
							Value = "https://www.microsoft.com/taiwan/events/2017AI-APP-Contest/ "
						}
					}
					};

					reply.Attachments = new List<Attachment>() { card1.ToAttachment(), card2.ToAttachment(), card3.ToAttachment() };
				//}
				//else
				//{
                    reply.Text = "若有其他關於輔具評估的問題，請按下方按鈕讓我知道你的位置，也可以拖拉地圖以移動地點，我會告訴你專人的資訊喔！";
					reply.ChannelData = new FacebookMessage
					(
						text: "把你的位置告訴我吧！",
						quickReplies: new List<FacebookQuickReply>
						{
                        // If content_type is location, title and payload are not used
                        // see https://developers.facebook.com/docs/messenger-platform/send-api-reference/quick-replies#fields
                        // for more information.
                        new FacebookQuickReply(
							contentType: FacebookQuickReply.ContentTypes.Location,
							title: default(string),
							payload: default(string)
						)
						}
					);
					await context.PostAsync(reply);
					context.Wait(LocationReceivedAsync);
                    return;
				//}
			}

			await context.PostAsync(reply);
			context.Wait(MessageReceivedAsync);
		}

		public async Task LocationReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
		{
			var msg = await argument;
           
            var location = msg.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();
			context.Done(location);

            var geo = (location.Geo as JObject)?.ToObject<GeoCoordinates>();
            var reply = context.MakeMessage();
            reply.Text = "服務您所選地點的輔具中心是...";
            if (geo != null)
            {
                Dictionary<string, string> site = ChatUtil.GetNearestLocation(Convert.ToDouble(geo.Latitude), Convert.ToDouble(geo.Longitude), Sites);
                double siteLat = Convert.ToDouble(site["lat"]);
                double siteLon = Convert.ToDouble(site["lon"]);

                reply.Attachments.Add(new HeroCard
                {
                    Title = site["name"],
                    Subtitle = site["address"],
                    Text = site["phone"]+ Environment.NewLine + site["email"],
                    Buttons = new List<CardAction> {
                            new CardAction
                            {
                                Title = "帶我去" + site["name"],
                                Type = ActionTypes.OpenUrl,
                                Value = "https://www.google.com.tw//maps/place/" + site["name"] + $"/@{siteLat},{siteLon},15z",
                                //Value = $"https://www.bing.com/maps/?v=2&cp={siteLat}~{siteLon}&lvl=16&dir=0&sty=c&sp=point.{siteLat}_{siteLon}_You%20are%20here&ignoreoptin=1"
                            }
                        }

                }.ToAttachment());
            }
            else
            {
                reply.Text = "這是哪裡阿﹍";
            }
            await context.PostAsync(reply);
        }

		private bool StupidCompare(string[] texts, string text, float tor)
		{
			float minDis = float.MaxValue;
			if (texts != null)
			{
				foreach (string s in texts)
				{
					float compare = ChatUtil.LevenshteinDistance(s, text) / (float)s.Length;
					if (minDis > compare)
						minDis = compare;
				}
			}
			return minDis <= tor;
		}

		private static async Task<Stream> GetImageStream(ConnectorClient connector, Attachment imageAttachment)
		{
			using (var httpClient = new HttpClient())
			{
				// The Skype attachment URLs are secured by JwtToken,
				// you should set the JwtToken of your bot as the authorization header for the GET request your bot initiates to fetch the image.
				// https://github.com/Microsoft/BotBuilder/issues/662
				var uri = new Uri(imageAttachment.ContentUrl);
				if (uri.Host.EndsWith("skype.com", StringComparison.Ordinal) && uri.Scheme == "https")
				{
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync(connector));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
				}

				return await httpClient.GetStreamAsync(uri);
			}
		}

		/// <summary>
		/// Gets the href value in an anchor element.
		/// </summary>
		///  Skype transforms raw urls to html. Here we extract the href value from the url
		/// <param name="text">Anchor tag html.</param>
		/// <param name="url">Url if valid anchor tag, null otherwise</param>
		/// <returns>True if valid anchor element</returns>
		private static bool TryParseAnchorTag(string text, out string url)
		{
			var regex = new Regex("^<a href=\"(?<href>[^\"]*)\">[^<]*</a>$", RegexOptions.IgnoreCase);
			url = regex.Matches(text).OfType<Match>().Select(m => m.Groups["href"].Value).FirstOrDefault();
			return url != null;
		}

		/// <summary>
		/// Gets the JwT token of the bot.
		/// </summary>
		/// <param name="connector"></param>
		/// <returns>JwT token of the bot</returns>
		private static async Task<string> GetTokenAsync(ConnectorClient connector)
		{
			if (connector.Credentials is MicrosoftAppCredentials credentials)
			{
				return await credentials.GetTokenAsync();
			}

			return null;
		}

		/// <summary>
		/// Gets the caption asynchronously by checking the type of the image (stream vs URL)
		/// and calling the appropriate caption service method.
		/// </summary>
		/// <param name="activity">The activity.</param>
		/// <param name="connector">The connector.</param>
		/// <returns>The AnalysisResult if found</returns>
		/// <exception cref="ArgumentException">The activity doesn't contain a valid image attachment or an image URL.</exception>
		private async Task<AnalysisResult> GetAnalysisResultAsync(Activity activity, ConnectorClient connector)
		{
			var imageAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType.Contains("image"));
			if (imageAttachment != null)
			{
				using (var stream = await GetImageStream(connector, imageAttachment))
				{
					return await this.captionService.GetAnalysisResultAsync(stream);
				}
			}

			if (TryParseAnchorTag(activity.Text, out string url))
			{
				return await this.captionService.GetAnalysisResultAsync(url);
			}

			if (Uri.IsWellFormedUriString(activity.Text, UriKind.Absolute))
			{
				return await this.captionService.GetAnalysisResultAsync(activity.Text);
			}

			// If we reach here then the activity is neither an image attachment nor an image URL.
			throw new ArgumentException("The activity doesn't contain a valid image attachment or an image URL.");
		}
	}
}
