using Baco.ServerObjects;
using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Baco.Messaging
{
    /// <summary>
    /// Message data
    /// </summary>
    [Serializable]
    public class Message
    {

        public static Dictionary<int, ObservableCollection<Message>> MessageCollection { get; set; }

        public User Sender { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }

        /// <summary>
        /// Creates a chat message
        /// </summary>
        /// <param name="sender">Who is sending</param>
        /// <param name="content">What's the message</param>
        /// <param name="date">Date of the message</param>
        public Message(User sender, string content, DateTime date)
        {
            Sender = sender;
            Content = content;
            Date = date;
        }

        public override string ToString()
        {
            return $"{Date.ToString()} \t {Sender.Nickname}: '{Content}'";
        }

    }
}
