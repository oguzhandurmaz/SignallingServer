using Microsoft.AspNetCore.SignalR;
using SignallingServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SignallingServer.Hubs
{
    public class SignalHub: Hub
    {
        static List<User> userList = new List<User>();
        static List<string> roomList = new List<string>();

        private Random random;

        public SignalHub()
        {
            random = new Random();
        }

        public override Task OnConnectedAsync()
        {
            //Socket'e baglanan kisiyi Sadece Id'si ile kaydet
            userList.Add(new User{
                Id = Context.ConnectionId,
                Name = "",
                RoomName = "",
                IsBusy = false

            });

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var user = userList.Where(x => x.Id == Context.ConnectionId).SingleOrDefault();
            if(user != null)
            {
                userList.Remove(user);
                Clients.All.SendAsync("PeerDisconnected", user);
            }
            
            return base.OnDisconnectedAsync(exception);
        }
        public async Task Connect(string userName)
        {
            var denemeRoom = GenerateRoom();
            var user = JsonSerializer.Deserialize<User>(userName);
            //onConnected da Baglanan User'ı bul ve İsmini ekle
            var isUniq = userList.Where(x => x.Name == user.Name).ToList();
            //Uniq Name ise
            if(isUniq.Count() == 0)
            {
                var userFromList = userList.Where(x => x.Id == Context.ConnectionId).SingleOrDefault();
                if(userFromList != null)
                {
                    userFromList.Name = user.Name;
                    await Clients.All.SendAsync("PeerConnected", JsonSerializer.Serialize(userList));
                }

            }
            else
            {
                await Clients.Caller.SendAsync("Connect", "Please Choose Uniq Name");
            }

        }
        public async Task PeerList()
        {
            await Clients.Caller.SendAsync("PeerList", JsonSerializer.Serialize(userList));
        }
        public async Task Offer(string offer)
        {
            //To - Callee
            //From - Caller
            var offerObject = JsonSerializer.Deserialize<Offer>(offer);

            var callee = userList.Where(x => x.Name == offerObject.To).SingleOrDefault();
            var caller = userList.Where(x => x.Name == offerObject.From).SingleOrDefault();

            //Caller'a Oda Oluşturuluyor
            caller.RoomName = GenerateRoom();

            //Arayan Arama Yaptığı için Mesgul Durumuna Geçiyor
            caller.IsBusy = true;
            if (callee != null && caller != null)
            {
                if (!callee.IsBusy)
                {
                    //Aranan Kişi Müsait Aranıyor
                    await Clients.User(callee.Id).SendAsync("Offer", offer);
                }
                else
                {
                    //Arama Başarısız Olduğu için Arayan Mesgulü İptal Oluyor
                    caller.IsBusy = false;

                    //Arayanın Oda İsmini ve Odayı Geri Sil
                    DeleteRoom(caller.RoomName);
                    caller.RoomName = "";

                    //Aranan kişi Meşgul Aranmıyor Cevabı
                    var answer = new Answer
                    {
                        From = callee.Name,
                        To = offerObject.From,
                        IsBusy = true
                    };
                    await Clients.Caller.SendAsync("Answer", answer);
                }

            }
        }
        public async Task Answer(string answer)
        {

            var answerObject = JsonSerializer.Deserialize<Answer>(answer);
            //To - Caller
            //From - Callee
            var caller = userList.Where(x => x.Name == answerObject.To).SingleOrDefault();
            var callee = userList.Where(x => x.Name == answerObject.From).SingleOrDefault();
            if (caller != null && callee != null)
            {
                //Reddedilmediyse
                if (!answerObject.IsRejected)
                {
                    //Teklifi Kabul Edenin Odasıyla Arayanın Odasnı Aynı Yap
                    callee.RoomName = caller.RoomName;
                }
                else
                {
                    //Reddedildiyse Caller Odasını Kapat
                    DeleteRoom(caller.RoomName);
                    caller.RoomName = "";
                }
                await Clients.User(caller.Id).SendAsync("Answer", answer);
            }
        }

        public async Task IceCandidate(string iceCandidate)
        {
            var iceCandidateObject = JsonSerializer.Deserialize<IceCandidate>(iceCandidate);
            var user = userList.Where(x => x.Name == iceCandidateObject.To).SingleOrDefault();
            if(user != null)
            {
                await Clients.User(user.Id).SendAsync("IceCandidate", iceCandidate);
            }
        }

        public async Task HangOut()
        {
            var closedUser = userList.Where(x => x.Id == Context.ConnectionId).SingleOrDefault();
            var connectedUser = userList.Where(x => x.RoomName == closedUser.RoomName && x.Id != closedUser.Id).SingleOrDefault();

            if(closedUser != null && connectedUser != null)
            {
                //Kapatan Kişinin Odasını Sil ve Mesgulünü Değiştir
                DeleteRoom(closedUser.RoomName);
                closedUser.RoomName = "";
                closedUser.IsBusy = false;

                //Kapatanın Kapattığına Bağlı Olan Kişiye İlet
                await Clients.User(connectedUser.Id).SendAsync("Closed", closedUser.Name);

                //Kapatanın Bağlı Oldugu Kiinin Odasını Sil ve Mesgulünü Değiştir
                connectedUser.RoomName = "";
                closedUser.IsBusy = false;
            }
        }

        private string GenerateRoom()
        {
            var builder = new StringBuilder(20);

            
            const int letterOffset = 26;

            for (var i = 0; i < 20; i++)
            {
                var value = (char)random.Next('a', 'a' + letterOffset);
                builder.Append(value);
            }
            var roomName = builder.ToString();
            var isRoomUniq = roomList.Where(x => x == roomName).SingleOrDefault() is null ? true : false;
            if (!isRoomUniq)
            {
                GenerateRoom();
            }
           
            roomList.Add(roomName);

            return roomName;
        }
        private void DeleteRoom(string roomName)
        {
            roomList.Remove(roomName);
        }

    }
}
