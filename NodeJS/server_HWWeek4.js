var websocket = require('ws');

var wss = new websocket.Server({port:5500}, callbackInitServer = ()=>{
    console.log("Server is running");
});  // wss = web socket server

var wsList = [];
//var lobbyWsList = []; //collect who in lobby to send new roomList change
var roomList = [];
/*
{
    roomName: "xxxx",
    wsList: []
}
*/

// const Login = "Idle";
// const Lobby = "Lobby";
// const ChatRoom = "ChatRoom";
// const RoomJoining = "RoomJoining";
// const RoomCreating = "RoomCreating";

// const Connect = "Connect";
const ChatSystem = "ChatSystem";
// const AllRoom = "AllRoom";
const JoinRoom = "JoinRoom";
const CreateRoom = "CreateRoom";
const LeaveRoom = "LeaveRoom";
const JoinSuccess = "JoinSuccess";
// const CreateSuccess = "CreateSuccess";
// const Disconnect = "Disconnect";
const ErrorAlert = "ErrorAlert";

wss.on("connection",(ws)=>{

    {
        //LobbyZone
        ws.on("message", (data)=>{
            
            console.log(data);

            var toJson = JSON.parse(data);

            console.log(toJson["eventName"]);
            // หรือพิมพ์แบบนี้ก็ได้ >> console.log(toJson.eventName);

            if(toJson.eventName == CreateRoom) //CreateRoom
            {
                console.log("Client request CreateRoom ["+toJson.Room+"]");
                var isFoundRoom = false;
                for(var i = 0; i<roomList.length;i++)
                {
                    if(roomList[i].roomName == toJson.Room)
                    {
                        isFoundRoom = true;
                        break;
                    }
                }
                if(isFoundRoom)
                {
                    console.log("Create room : room is founded");
                    toJson.eventName = "CreateFailed";

                    //var toJonStr = JSON.stringify(resultData);

                    ws.send(JSON.stringify(toJson));
                }
                else
                {
                    var newRoom = {
                        roomName: toJson.Room,
                        wsList: []
                    }

                    newRoom.wsList.push(ws)
                    // ws หมายถึง client ที่คอนเน็คเข้ามาในขณะนั้น

                    roomList.push(newRoom);
                    console.log("Create new room : "+ newRoom.roomName);
                    console.log("Create room : room is not founded");

                    toJson.eventName = "CreateSuccess";

                    //var toJonStr = JSON.stringify(resultData);

                    ws.send(JSON.stringify(toJson));
                }
            }

            else if(toJson.eventName == JoinRoom)
            {
                console.log("Client request CreateRoom ["+toJson.Room+"]");
                var isFoundRoom = false;
                var roomIndex;
                var isFoundUser = false;

                //find roomIndex and kick client from uncorrect room
                for (var i = 0; i < roomList.length; i++)
                {
                    if(roomList[i].roomName == toJson.Room)
                    {
                        isFoundRoom = true;
                        roomIndex = i;
                    }
                    //kick user from uncorrect room
                    for (var j = 0; j < roomList[i].wsList.length; j++)
                    {
                        if(roomList[i].wsList[j] == ws)
                        {
                            roomList[i].wsList.splice(j,1);
                            return;
                        }
                    }
                }
                
                //Join Room
                if(isFoundRoom)
                {
                    console.log("Join room : room is founded");

                    roomList[roomIndex].wsList.push(ws);

                    toJson.eventName = JoinSuccess;
                    //toJson.Room = "";
                    toJson.Sender = "";
                    toJson.data = "";
                    //var toJonStr = JSON.stringify(resultData);

                    console.log("Join Success");
                    ws.send(JSON.stringify(toJson));
                    console.log("Send >>>> " + JSON.stringify(toJson));
                }

                //Join Failed
                else
                {
                    toJson.eventName = "JoinFailed";
                    //toJson.Room = "";
                    toJson.Sender = "";
                    toJson.data = "";

                    ws.send(JSON.stringify(toJson));
                }
            }

            else if(toJson.eventName == LeaveRoom)
            {
                isFoundRoom = false;
                var roomIndex;
                for (var i = 0; i < roomList.length; i++)
                {
                    if(roomList[i].roomName == toJson.Room)
                    {
                        roomIndex = i;
                        isFoundRoom = true;
                    }
                }
                if(isFoundRoom)
                {
                    for (var j = 0; j < roomList[roomIndex].wsList.length; j++)
                    {
                        if(roomList[roomIndex].wsList[j] == ws)
                        {
                            roomList[roomIndex].wsList.splice(j,1);
                            console.log("LeaveRoom Success 1");
                        }
                    }

                    if(roomList[roomIndex].wsList.length > 0)
                    {
                        toJson.eventName = ChatSystem;

                        for (var j = 0; j < roomList[roomIndex].wsList.length; j++)
                        {
                            roomList[roomIndex].wsList[j].send(JSON.stringify(toJson));
                            console.log("tell everyone i leave");
                        }
                    }
                    else
                    {
                        roomList.splice(roomIndex,1);
                    }
                    
                }
                //if room not found kick from every room
                else
                {
                    for (var i = 0; i < roomList.length; i++)
                    {
                        for (var j = 0; j < roomList[i].wsList.length; j++)
                        {
                            if(roomList[i].wsList[j] == ws)
                            {
                                roomList[i].wsList.splice(j,1);
                                console.log("LeaveRoom Success 2");
                            }
                        }
                    }
                }
            }

            else if(toJson.eventName == "AllRoomList"){
                toJson.Room = "";
                toJson.Sender = "";
                toJson.data = "";
                for(var i = 0; i < roomList.length; i++){
                    toJson.data = toJson.data + roomList[i].roomName+"\n";
                }
                ws.send(JSON.stringify(toJson));
            }

            else if(toJson.eventName == ChatSystem){
                Boardcast(toJson,ws);
            }

            //Unknown Event
            else{
                console.log("Unknown Event");
                console.log(data);
            }
        });
    }
    
    console.log("client connected.");
    wsList.push(ws);
    //lobbyWsList.push(ws);

    ws.on("close",()=>{
        console.log("client disconnected.");

        //kick client from wsList
        for(var i = 0; i < wsList.length; i++)
        {
            if(wsList[i] = ws)
            {
                wsList.splice(i,1);
                break;
            }
        }

        //kick client from room
        for (var i = 0; i < roomList.length; i++)
        {
            for (var j = 0; j < roomList[i].wsList.length; j++)
            {
                if(roomList[i].wsList[j] == ws)
                {
                    roomList[i].wsList.splice(j,1);
                    break;
                }
            }
        }
    });
});

/*
server.listen(process.env.PORT || 5500, ()=>{
    console.log("Server start at port "+server.address().port);
});*/

function ArrayRemove(arr,value)
{
    return arr.filter((element)=>{
        return element != value;
    });
}

function Boardcast(toJson,wsSender)
{
    var roomIndex;
    var isFoundRoom = false;

    for (var i = 0; i < roomList.length; i++)
    {
        if(roomList[i].roomName == toJson.Room){
            isFoundRoom = true;
            roomIndex = i;
            break;
        }
    }

    if(isFoundRoom)
    {
        var isFoundUser = false;
        for(var i = 0; i<roomList[roomIndex].wsList.length;i++)
        {
            if(roomList[roomIndex].wsList[i] == wsSender)
            {
                isFoundUser = true;
            }
        }
        if(isFoundUser)
        {
            console.log("Boardcast: isFoundUser = "+isFoundUser);
            for(var i = 0; i<roomList[roomIndex].wsList.length;i++)
            {
                roomList[roomIndex].wsList[i].send(JSON.stringify(toJson));
            }
        }
        else
        {
            console.log("Boardcast: isFoundUser = "+isFoundUser);
            var resultData = {
                eventName: ErrorAlert,
                Room: "",
                Sender: "",
                data: "Not found user in the room.\nPlease reconnect.",
            }

            wsSender.send(JSON.stringify(resultData));
        }
    }
    else
    {
        console.log("Boardcast: isFoundRoom = "+isFoundRoom);
        var resultData = {
            eventName: ErrorAlert,
            Room: "",
            Sender: "",
            data: "Not found room.\nPlease reconnect.",
        }

        wsSender.send(JSON.stringify(resultData));
    }
}
