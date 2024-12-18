using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Zendesk.Messaging.Android;

namespace AppTesteChat.Droid
{
    [Activity(Label = "ChatActivity")]
    public class ChatActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var channelKey = "";
            var successCallback = new SuccessCallback(this);
            var failureCallback = new FailureCallback();
            var messagingFactory = new DefaultMessagingFactory();

            Zendesk.Android.Zendesk.Initialize(
               context: this,
               channelKey: channelKey,
               successCallback: successCallback,
               failureCallback: failureCallback,
               messagingFactory: messagingFactory
           );
        }
    }

    public class SuccessCallback : Java.Lang.Object, Zendesk.Android.ISuccessCallback
    {
        private readonly Context _context;

        public SuccessCallback(Context context)
        {
            _context = context;
        }

        public void OnSuccess(Java.Lang.Object value)
        {
            var zendesk = (Zendesk.Android.Zendesk)value;
            zendesk.Messaging.SetConversationFields(new Dictionary<string, Java.Lang.Object>
            {
                { "", "teste" }
            });
            zendesk.LoginUser(CreateZendeskToken(), new SuccessCallback2(), new FailureCallback());
            zendesk.Messaging.ShowMessaging(_context);
        }

        private string CreateZendeskToken()
        {
            var headerEncoded = CreateHeader();
            var payloadEncoded = CreatePayload("10000", "Pedro Martins e Sá", "pedro@pedro.com");
            var signatureEncoded = CreateSignature(headerEncoded, payloadEncoded);
            return $"{headerEncoded}.{payloadEncoded}.{signatureEncoded}";
        }

        private string CreateHeader()
        {
            var header = new
            {
                alg = "HS256",
                kid = "",
                typ = "JWT"
            };

            return Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header)));
        }

        private string CreatePayload(string id, string name, string email)
        {
            var body = new
            {
                email = email,
                email_verified = true,
                name = name,
                external_id = id,
                scope = "user",
                iat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                jti = Guid.NewGuid().ToString(),
                exp = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds()
            };

            return Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body)));
        }

        private string CreateSignature(string headerEncoded, string payloadEncoded)
        {
            var privateKey = Encoding.UTF8.GetBytes("");
            using (var hmac = new HMACSHA256(privateKey))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{headerEncoded}.{payloadEncoded}"));
                return Base64UrlEncode(hash);
            }
        }

        private string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split("=")[0];
            output = output.Replace('+', '-');
            output = output.Replace('/', '_');

            return output;
        }
    }

    public class FailureCallback : Java.Lang.Object, Zendesk.Android.IFailureCallback
    {
        public void OnFailure(Java.Lang.Object error)
        { }
    }

    public class SuccessCallback2 : Java.Lang.Object, Zendesk.Android.ISuccessCallback
    {
        public void OnSuccess(Java.Lang.Object value)
        {
            
        }
    }
}