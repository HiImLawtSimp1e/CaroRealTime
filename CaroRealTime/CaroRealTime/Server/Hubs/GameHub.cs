﻿using CaroRealTime.Shared.Entities;
using Microsoft.AspNetCore.SignalR;

namespace CaroRealTime.Server.Hubs
{
    public class GameHub : Hub
    {
        private static readonly List<GameRoom> _rooms = new List<GameRoom>();
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Player with Id {Context.ConnectionId} connected");

            await Clients.Caller.SendAsync("Rooms", _rooms.OrderBy(r => r.RoomName));
        }

        public async Task<GameRoom> CreateRoom(string name, string playerName)
        {
            var roomId = Guid.NewGuid().ToString();
            var room = new GameRoom(roomId, name);
            _rooms.Add(room);

            var newPlayer = new Player(Context.ConnectionId, playerName);
            room.TryAddPlayer(newPlayer);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.All.SendAsync("Rooms", _rooms.OrderBy(r => r.RoomName));

            return room;
        }

        public async Task<GameRoom?> JoinRoom(string roomId, string playerName)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if(room != null)
            {
                var newPlayer = new Player(Context.ConnectionId, playerName);
                if (room.TryAddPlayer(newPlayer))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                    await Clients.Group(roomId).SendAsync("PlayerJoined", newPlayer);

                    return room;
                }
            }
            return null;
        }
        
        public async Task StartGame(string roomId)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if(room != null)
            {
                room.Game.StartGame();
                await Clients.Group(roomId).SendAsync("UpdateGame", room);
            }
        }

        public async Task MakeMove(string roomId, int row, int col,string playerId)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if(room != null && room.Game.MakeMove(row,col, playerId) ) 
            {
                room.Game.Winner = room.Game.CheckWinner();
                room.Game.IsDraw = room.Game.CheckDraw();
                if (!string.IsNullOrEmpty(room.Game.Winner) || room.Game.IsDraw)
                {
                    room.Game.GameOver = true;
                }
                await Clients.Group(roomId).SendAsync("UpdateGame", room);
            }
        }
    }
}
