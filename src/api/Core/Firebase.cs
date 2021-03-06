﻿using System;
using System.IO;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace SupercellUilityApi.Core
{
    public class Firebase
    {
        public Firebase()
        {
            if (File.Exists("Resources/firebase.json"))
                FirebaseApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("Resources/firebase.json")
                });
            else
                Logger.Log("Firebase configuration not found!", Logger.ErrorLevel.Error);
        }

        public FirebaseApp FirebaseApp { get; set; }

        public async void SendNotification(string title, string body)
        {
            try
            {
                var message = new Message
                {
                    Notification = new Notification {Title = title, Body = body},
                    Topic = "everyone"
                };

                if (FirebaseMessaging.DefaultInstance == null) return;
                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

                Logger.Log($"Successfully sent notification {response}");
            }
            catch (Exception exception)
            {
                Logger.Log($"Failed to send a notification over firebase! {exception}", Logger.ErrorLevel.Error);
            }
        }
    }
}