using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

namespace VoiceChat.Networking
{
    public class VoiceChatUserListMessage : MessageBase
    {
        private List<User> users;
        private int numUsers;

        private List<ProxyIdUser> onlineUsers;
        private int numOnlineUsers;

        public List<User> Users
        {
            get
            {
                return users;
            }
        }

        public List<ProxyIdUser> OnlineUsers
        {
            get
            {
                return onlineUsers;
            }
        }

        public VoiceChatUserListMessage(List<User> users, List<ProxyIdUser> onlineUsers)
        {
            this.users = users;
            this.onlineUsers = onlineUsers;
            numUsers = users.Count;
            numOnlineUsers = onlineUsers.Count;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(numUsers);
            writer.Write(numOnlineUsers);
            foreach (var user in users)
            {
                writer.Write(user.Username);
                writer.Write(user.Role);
            }
            foreach (var onlineUser in onlineUsers)
            {
                writer.Write(onlineUser.ProxyId);
                writer.Write(onlineUser.User.Username);
                writer.Write(onlineUser.User.Role);
            }

        }

        public override void Deserialize(NetworkReader reader)
        {
            numUsers = reader.ReadInt32();
            numOnlineUsers = reader.ReadInt32();
            users = new List<User>();
            foreach (int index in Enumerable.Range(0, numUsers - 1))
            {
                string username = reader.ReadString();
                string role = reader.ReadString();
                var user = new User(username, role);
                users.Add(user);
            }
            onlineUsers = new List<ProxyIdUser>();
            foreach (int index in Enumerable.Range(0, numOnlineUsers - 1))
            {
                int proxyId = reader.ReadInt32();
                string username = reader.ReadString();
                string role = reader.ReadString();
                var onlineUser = new User(username, role);
                onlineUsers.Add(new ProxyIdUser(proxyId, onlineUser));
            }
        }
    }
}