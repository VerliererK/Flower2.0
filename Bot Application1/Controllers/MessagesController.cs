using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot_Application1
{
	[BotAuthentication]
	public class MessagesController : ApiController
	{
		/// <summary>
		/// POST: api/Messages
		/// Receive a message from a user and reply to it
		/// </summary>
		public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
		{
			if (activity.Type == ActivityTypes.Message)
			{
				await Conversation.SendAsync(activity, () => new Dialogs.QuestionnaireDialog());
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
			switch (message.Type)
			{
				case ActivityTypes.DeleteUserData:
					break;
				case ActivityTypes.ConversationUpdate:
					// Greet the user the first time the bot is added to a conversation.
					if (message.MembersAdded.Any(m => m.Id == message.Recipient.Id))
					{
						var connector = new ConnectorClient(new Uri(message.ServiceUrl));
						var response = message.CreateReply();
						response.Text = "您好！歡迎來到扶老2.0，我們將提供以下服務:\n\n\n1.身體狀況評估\n\n2.推薦最適合輔具\n\n\n你說「Hi」我們就開始吧～";
						await connector.Conversations.ReplyToActivityAsync(response);
					}
					break;
				case ActivityTypes.ContactRelationUpdate:
					break;
				case ActivityTypes.Typing:
					break;
				case ActivityTypes.Ping:
					break;
			}

			return null;
		}
	}
}
