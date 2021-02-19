var websocket = require('ws');


var wss = new websocket.Server({port:5500}, callbackInitServer = ()=>{
    console.log("Server is running");
});  // wss = web socket server

var wsList = [];
var roomList = [];
/*
{
    roomName: "xxxx",
    wsList: []
}
*/


wss.on("connection",(ws)=>{

    {
        //LobbyZone
        ws.on("message", (data)=>{
            
            console.log(data);

            /*
            var strSplit = data.split('#');
            //CreateRoom
            if(strSplit[0] == "CreateRoom")
            {
                console.log("Client request CreateRoom");
            }
            if(strSplit[0] == "JoinRooom")
            {
                console.log("Client request JoinRooom");
            }
            */

            var toJson = JSON.parse(data);

            console.log(toJson["eventName"]);
            // หรือพิมพ์แบบนี้ก็ได้ >> console.log(toJson.eventName);

            if(toJson.eventName == "CreateRoom") //CreateRoom
            {
                console.log("Client request CreateRoom ["+toJson.data+"]");
                var isFoundRoom = false;
                for(var i = 0; i<roomList.length;i++)
                {
                    if(roomList[i].roomName == toJson.data)
                    {
                        isFoundRoom = true;
                        break;
                    }
                }
                if(isFoundRoom)
                {
                    console.log("Create room : room is founded");
                    var resultData = {
                        eventName: toJson.eventName,
                        data: "fail",
                    }

                    //var toJonStr = JSON.stringify(resultData);

                    ws.send(JSON.stringify(resultData));
                }
                else
                {
                    var newRoom = {
                        roomName: toJson.data,
                        wsList: []
                    }

                    newRoom.wsList.push(ws)
                    // ws หมายถึง client ที่คอนเน็คเข้ามาในขณะนั้น

                    roomList.push(newRoom);
                    console.log("Create new room : "+ newRoom.roomName);
                    console.log("Create room : room is not founded");

                    var resultData = {
                        eventName: toJson.eventName,
                        data: "success",
                    }

                    //var toJonStr = JSON.stringify(resultData);

                    ws.send(JSON.stringify(resultData));
                }
            }
            else if(toJson.eventName == "JoinRooom")
            {
                console.log("Client request JoinRooom");
            }

            else if(toJson.eventName == "LeaveRoom")
            {
                var isFound = false;
                for (var i = 0; i < roomList.length; i++)
                {
                    for (var i = 0; i < roomList[i].wsList.length; j++)
                    {
                        if(roomList[i].wsList[j] == ws)
                        {
                            roomList[i].wsList.splice(j,1);
                            break;
                        }
                    }
                }
            }

        });
    }
    
    console.log("client connected.");
    wsList.push(ws);

    ws.on("close",()=>{
        console.log("clien disconnected.");
        for(var i = 0; i < wsList.length; i++)
        {
            if(wsList[i] = ws)
            {
                wsList.splice(i,1);
                break;
            }
        }
        
        for (var i = 0; i < roomList.length; i++)
        {
            for (var i = 0; i < roomList[i].wsList.length; j++)
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

function Boardcast(data)
{
    for(var i = 0; i<wsList.length;i++)
    {
        wsList[i].send(data);
    }
}