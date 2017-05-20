using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ImageCaption.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot_Application1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        IDialog<object> dialog = new Dialogs.QuestionnaireDialog();

		private ICaptionService captionService = new MicrosoftCognitiveCaptionService();

		/// <summary>
		/// POST: api/Messages
		/// Receive a message from a user and reply to it
		/// </summary>
		public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
		{
		    if (activity.Type == ActivityTypes.Message)
		    {
		        await Conversation.SendAsync(activity, () => dialog);
		    }
		    else
		    {
                await HandleSystemMessage(activity);
		    }
		    var response = Request.CreateResponse(HttpStatusCode.OK);
		    return response;
		}

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
				// Greet the user the first time the bot is added to a conversation.
				if (message.MembersAdded.Any(m => m.Id == message.Recipient.Id))
				{
					var connector = new ConnectorClient(new Uri(message.ServiceUrl));

					var response = message.CreateReply();
					response.Text = "您好！歡迎來到扶老2.0 ，我們將提供以下服務:\n\n1.輔具評估\n2.推薦最適合輔具\n\nSay Hi 我們就開始吧～";

					await connector.Conversations.ReplyToActivityAsync(response);
				}
			}
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}
