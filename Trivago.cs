using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Microsoft.Bot.Builder.ConnectorEx;

namespace Bot_Application1.Dialogs
{
    [Serializable]
    public class Trivago : IDialog<object>
    {
        int count = 0;
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity.Attachments != null && activity.Attachments.Count > 0 && activity.Attachments[0].ContentType.Contains("image"))
            {
                await context.PostAsync("這是一張圖片");
                return;
            }
            string message = activity.Text;
            DialogQA QA = new DialogQA();
            string ans = QA.answer(message);
            if (ans != null)
                await context.PostAsync(ans);
            // return our reply to the user
            var reply = context.MakeMessage();
            List<string> option = new List<string>();
            option.Add("找飯店");
            option.Add("魯蛇");
            option.Add("難過");
            reply.AddKeyboardCard<string>("你想說什麼？你說了: " + ++count, option);
            await context.PostAsync(reply);
            context.Wait(MessageReceivedAsync);
        }
    }
}