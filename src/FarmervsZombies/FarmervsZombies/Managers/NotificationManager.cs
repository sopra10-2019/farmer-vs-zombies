using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Managers
{
    internal static class NotificationManager
    {
        private static readonly List<Notification> sNotifications = new List<Notification>();
        private static readonly Dictionary<Notification, float> sNotificationTimeLeft = new Dictionary<Notification, float>();
        private const int Width = 500;
        private static Vector2 sPosition = Vector2.Zero;
        private const int Spacing = 10;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            sPosition = new Vector2(graphicsDevice.Viewport.Width - Width - 10, 10);
        }

        public static void AddNotification(string text, float duration)
        {
            var otherNotification = sNotifications.Find(obj => obj.mText == text && obj.mTitle == "");
            if (otherNotification != null)
            {
                sNotificationTimeLeft[otherNotification] = Math.Max(sNotificationTimeLeft[otherNotification], duration);
                return;
            }
            var notification = new Notification(text, Width);
            sNotifications.Insert(0, notification);
            sNotificationTimeLeft.Add(notification, duration);
        }

        public static void AddNotification(string title, string text, float duration)
        {
            var otherNotification = sNotifications.Find(obj => obj.mText == text && obj.mTitle == title);
            if (otherNotification != null)
            {
                sNotificationTimeLeft[otherNotification] = Math.Max(sNotificationTimeLeft[otherNotification], duration);
                return;
            }
            var notification = new Notification(title, text, Width);
            sNotifications.Add(notification);
            sNotificationTimeLeft.Add(notification, duration);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            var current = sPosition;
            foreach (var notification in sNotifications)
            {
                notification.Draw(spriteBatch, current);
                current = new Vector2(current.X, current.Y + notification.Height + Spacing);
            }
        }

        public static void Update(GameTime gameTime)
        {
            var keys = new List<Notification>(sNotificationTimeLeft.Keys);
            foreach (var key in keys)
            {
                sNotificationTimeLeft[key] -= gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (sNotificationTimeLeft[key] <= 0)
                {
                    sNotificationTimeLeft.Remove(key);
                    sNotifications.Remove(key);
                }

            }
        }
    }
}
