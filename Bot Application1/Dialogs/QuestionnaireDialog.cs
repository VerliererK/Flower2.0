using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Bot_Application1.Dialogs
{
    [Serializable]
    public class QuestionnaireDialog : IDialog<object>
    {
        private Dictionary<string, string[]> Question = new Dictionary<string, string[]>();
        private Dictionary<string, string[]>.Enumerator enumerator;
        private string[] lastValue;
        private bool done = false;

        public QuestionnaireDialog()
        {
            ini();
        }
        private void ini()
        {
            Question.Add("請問您現在有輪椅輔具了嗎？", new string[] { "有", "沒有" });
            Question.Add("您使用的是哪一種輪椅呢？", new string[] { "非輕量化量產型輪椅", "輕量化量產型輪椅", "量身訂製型輪椅", "高活動型輪椅", "目前沒有輪椅輔具" });
            Question.Add("目前的輪椅輔具使用情形為？", new string[] { "已損壞不堪修復，需更新", "規格或功能不符使用者現在的需求，需更換", "適合繼續使用，但需要另行購置一部於不同場所使用" });
            Question.Add("請問您是否有被診斷出以下狀況？", new string[] { "中風偏癱( 左 / 右 )", "脊髓損傷( 頸 / 胸 / 腰 / 肩 )", "腦性麻痺", "發展遲緩", "小兒麻痺", "運動神經元疾病", "下肢骨折或截肢", "關節炎", "心肺功能疾病", "肌肉萎縮症" });
            Question.Add("請問輪椅主要的操作者為？", new string[] { "自己", "照顧者" });
            Question.Add("接下來會詢問您關於身體各部位的狀況，請您依照自己的感受 / 醫生的診斷結果回答。", new string[] { "好" });
            Question.Add("坐姿平衡", new string[] { "良好", "雙手扶持尚可維持平衡", "雙手扶持難以維持平衡" });

            Question.Add("骨盆", new string[] { "正常", "向前 / 後傾", "向左 / 右傾斜", "向左 / 右旋轉" });
            Question.Add("坐姿時骨盆經常", new string[] { "向前滑動", "向後滑動", "向左滑動", "向右滑動" });
            Question.Add("脊柱", new string[] { "正常或無明顯變形", "脊柱側彎", "過度前凸(hyperlordosis)", "過度後凸(hyperkyphosis)" });
            Question.Add("頭部控制", new string[] { "正常", "偶可維持頭部正中位置但控制不佳或耐力不足", "完全無法控制" });
            Question.Add("肩部", new string[] { "正常", "後縮", "前突" });
            Question.Add("髖部", new string[] { "正常", "內收", "外展", "風吹式變形", "其他" });
            Question.Add("膝部", new string[] { "正常", "屈曲變形", "伸直變形" });
            Question.Add("踝部", new string[] { "正常", "外翻變形", "蹠屈變形" });

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
            // return our reply to the user
            var reply = context.MakeMessage();
            if (lastValue != null && lastValue.Length > 0 &&
                !stupidCompare(lastValue, message, 0.2f))
            {
                reply.Text = "不要亂回答啦~";
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
                done = true;
                enumerator = Question.GetEnumerator();
                reply.Type = "message";
                reply.Text = "問答都結束囉～以下是我為你推薦的輪椅";
                await context.PostAsync(reply);

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

                reply.Attachments = new List<Attachment>() { card1.ToAttachment(), card2.ToAttachment(), card3.ToAttachment()};
            }
            await context.PostAsync(reply);
            context.Wait(MessageReceivedAsync);
        }

        private bool stupidCompare(string[] texts, string text, float tor)
        {
            float minDis = float.MaxValue;
            if (texts != null)
            {
                foreach (string s in texts)
                {
                    float compare = LevenshteinDistance(s, text) / (float)s.Length;
                    if (minDis > compare)
                        minDis = compare;
                }
            }
            return minDis <= tor;
        }

        private int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
    }
}