using System;
using System.Json;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using Android.OS;
using System.Net;
using System.IO;
using System.Xml;

namespace Xamarin.Auth.Sample.Android
{
	[Activity (Label = "Xamarin.Auth Sample (Android)", MainLauncher = true)]
	public class MainActivity : Activity
	{
		public string token;
		public string userId;

		void LoginToVk ()
		{
			var auth = new OAuth2Authenticator (
				clientId: "",  // put your id here
				scope: "friends,video,groups",
				authorizeUrl: new Uri ("https://oauth.vk.com/authorize"),
				redirectUrl: new Uri ("https://oauth.vk.com/blank.html"));

			auth.AllowCancel = true;
			auth.Completed += (s, ee) => {
				if (!ee.IsAuthenticated) {
					var builder = new AlertDialog.Builder (this);
					builder.SetMessage ("Not Authenticated");
					builder.SetPositiveButton ("Ok", (o, e) => { });
					builder.Create().Show();
					return;
				}
				else
				{
					token = ee.Account.Properties ["access_token"].ToString ();
					userId = ee.Account.Properties ["user_id"].ToString ();	
					GetInfo();
				}
			};
			var intent = auth.GetUI (this);
			StartActivity (intent);
		}

		void GetInfo()
		{
			// создаем xml-документ
			XmlDocument xmlDocument = new XmlDocument ();
			// делаем запрос на получение имени пользователя
			WebRequest webRequest = WebRequest.Create ("https://api.vk.com/method/users.get.xml?&access_token=" + token);
			WebResponse webResponse = webRequest.GetResponse ();
			Stream stream = webResponse.GetResponseStream ();
			xmlDocument.Load (stream);
			string name =  xmlDocument.SelectSingleNode ("response/user/first_name").InnerText;
			// делаем запрос на проверку, 
			webRequest = WebRequest.Create ("https://api.vk.com/method/groups.isMember.xml?group_id=20629724&access_token=" + token);
			webResponse = webRequest.GetResponse ();
			stream = webResponse.GetResponseStream ();
			xmlDocument.Load (stream);
			bool habrvk = (xmlDocument.SelectSingleNode ("response").InnerText =="1");
			// выводим диалоговое окно
			var builder = new AlertDialog.Builder (this);
			// пользователь состоит в группе "хабрахабр"?
			if (!habrvk) {
				builder.SetMessage ("Привет, "+name+"!\r\n\r\nТы не состоишь в группе habrahabr.Не хочешь вступить?");
				builder.SetPositiveButton ("Да", (o, e) => {
					// уточнив, что пользователь желает вступить, отправим запрос
					webRequest = WebRequest.Create ("https://api.vk.com/method/groups.join.xml?group_id=20629724&access_token=" + token);
					 webResponse = webRequest.GetResponse ();
				});
				builder.SetNegativeButton ("Нет", (o, e) => {
				});
			} else {
				builder.SetMessage ("Привет, "+name+"!\r\n\r\nОтлично! Ты состоишь в группе habrahabr.");
				builder.SetPositiveButton ("Ок", (o, e) => {
				});
			}
			builder.Create().Show();
		}


		private static readonly TaskScheduler UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
			var vk = FindViewById<Button> (Resource.Id.VkButton);			
			vk.Click += delegate { LoginToVk();};
		}
	}
}