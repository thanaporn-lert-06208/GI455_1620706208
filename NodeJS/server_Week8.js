const sqlite = require('sqlite3').verbose();
var websocket = require('ws');

var wss = new websocket.Server({port:5500}, callbackInitServer = ()=>{
    console.log("Server is running");
});  // wss = web socket server

var wsList = [];
//var lobbyWsList = []; //collect who in lobby to send new roomList change
//var roomList = [];
/*
{
    roomName: "xxxx",
    wsList: []
}
*/

var roomMap = new Map();

const Login = "Login";
const Register = "Register";

// const Login = "Idle";
// const Lobby = "Lobby";
// const ChatRoom = "ChatRoom";
const LeaveRoom = "LeaveRoom";

// const Connect = "Connect";
const ChatSystem = "ChatSystem";
// const AllRoom = "AllRoom";
const Success = "Success";
const Failed = "Failed";
// const Disconnect = "Disconnect";
const ErrorAlert = "ErrorAlert";

wss.on("connection",(ws)=>{

    {
        //LobbyZone
        ws.on("message", (data)=>{
            
            console.log("Receive data: "+data);

            var toJson = JSON.parse(data);

            console.log(toJson["eventName"]);
            // หรือพิมพ์แบบนี้ก็ได้ >> console.log(toJson.eventName);

            if(toJson.eventName == "CreateRoom") //CreateRoom
            {
                CreateRoom(ws, toJsonObj.roomOption);
            }

            else if(toJson.eventName == "JoinRoom")
            {
                JoinRoom(ws, toJsonObj.roomOption);
            }

            else if(toJson.eventName == "LeaveRoom")
            {
                LeaveRoom(ws, (status)=>{
                    let callbackMsg = {
                        eventName: "LeaveRoom",
                        status: true,
                    }
                    if(status === false)
                    {
                        callbackMsg.status = false;
                    }
                    ws.send(JSON.stringify(callbackMsg));

                    if(roomMap.get(roomKey).wsList.size <= 0){
                        roomMap.delete(roomKey);
                    }
                });
                
            }

            /*
            else if(toJson.eventName == "AllRoomList"){
                toJson.Room = "";
                toJson.Sender = "";
                toJson.data = "";
                for(var i = 0; i < roomList.length; i++){
                    toJson.data = toJson.data + roomList[i].roomName+"\n";
                }
                ws.send(JSON.stringify(toJson));
            }
            */

            else if(toJson.eventName == ChatSystem){
                Boardcast(toJson,ws);
            }

            else if(toJson.eventName == Login){
                let db = new sqlite.Database('./db/chatDbHw.db', sqlite.OPEN_CREATE | sqlite.OPEN_READWRITE, (err)=>{
    
                    if(err){
                        console.log("Login Error");
                        console.log(err);
                    }
                    else{
                        console.log('Connected to database.');
                        var sqlSelect = "SELECT * FROM UserData WHERE UserID='"+toJson.iD+"' AND Password='"+toJson.data1+"'";
                        db.all(sqlSelect, (err,rows)=>{
                            if(err) {
                                console.log(err);
                            }
                            else{
                                if(rows.length > 1){
                                    var resultData = {
                                        eventName: Login,
                                        iD: "",
                                        data1: Failed,
                                        data2: "Error UserID\nPlease Contact Admin",
                                    }
                                    ws.send(JSON.stringify(resultData));
                                }
                                else if(rows.length > 0){
                                    var resultData = {
                                        eventName: Login,
                                        iD: "",
                                        data1: Success,
                                        data2: rows[0].Name,
                                    }
                                    ws.send(JSON.stringify(resultData));
                                }
                                else
                                {
                                    var resultData = {
                                        eventName: Login,
                                        iD: "",
                                        data1: Failed,
                                        data2: "Not Found Data",
                                    }
                                    ws.send(JSON.stringify(resultData));
                                }
                            }
                    
                            console.log(rows);
                        });
                    }
                });
            }

            else if(toJson.eventName == Register){
                let db = new sqlite.Database('./db/chatDbHw.db', sqlite.OPEN_CREATE | sqlite.OPEN_READWRITE, (err)=>{
    
                    if(err){
                        console.log("Register Error");
                        console.log(err);
                    }
                    else{
                        console.log('Connected to database.');
                        console.log("Check Same UserID");
                        var sqlSelect = "SELECT * FROM UserData WHERE UserID='"+toJson.iD+"'";
                        db.all(sqlSelect, (err,rows)=>{
                            if(err) {
                                console.log(err);
                            }
                            else{
                                if(rows.length > 0){
                                    var resultData = {
                                        eventName: Register,
                                        iD: "",
                                        data1: Failed,
                                        data2: "Already have this UserID",
                                    }
                                    console.log(resultData.data2);
                                    ws.send(JSON.stringify(resultData));
                                }
                                else
                                {
                                    var sqlInsert = "INSERT INTO UserData (UserID, Password, Name) VALUES ('"+toJson.iD+"','"+toJson.data1+"','"+toJson.data2+"')";
                                    db.all(sqlInsert, (err,rows)=>{
                                        if(err) {
                                            console.log(err);
                                            var resultData = {
                                                eventName: Register,
                                                iD: "",
                                                data1: Failed,
                                                data2: "Register Error\nPlease Contact Admin",
                                            }
                                        }
                                        else{
                                            var resultData = {
                                                eventName: Register,
                                                iD: "",
                                                data1: Success,
                                                data2: "",
                                            }
                                            console.log("Register Success");
                                            ws.send(JSON.stringify(resultData));
                                        }
                                
                                        console.log(rows);
                                    });
                                }
                            }
                    
                            console.log(rows);
                        });
                    }
                });
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
        LeaveRoom(ws, (status)=>{
            let callbackMsg = {
                eventName: "LeaveRoom",
                status: true,
            }
            if(status === false)
            {
                callbackMsg.status = false;
            }
            ws.send(JSON.stringify(callbackMsg));

            if(roomMap.get(roomKey).wsList.size <= 0){
                roomMap.delete(roomKey);
            }
        });
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

function Boardcast(messageObj,wsSender)
{
    console.log("--------Boardcast--------");
    var findRoom = FindRoom(messageObj.Room);
    if(findRoom.isFoundRoom) //Found Room
    {
        var findUserInRoom = FindUserInRoom(findRoom.roomIndex,wsSender);
        if(findUserInRoom.isFoundUser)
        {
            console.log("Boardcast: isFoundUser = "+findUserInRoom.isFoundUser);
            for(var i = 0; i<roomList[findRoom.roomIndex].wsList.length;i++)
            {
                roomList[findRoom.roomIndex].wsList[i].send(JSON.stringify(messageObj));
            }
        }
        else //Not Found User in Room
        {
            console.log("Boardcast: isFoundUser = "+findUserInRoom.isFoundUser);
            var resultData = {
                eventName: ErrorAlert,
                Room: "",
                Sender: "",
                data: "Not found user in the room.\nPlease reconnect.",
            }

            wsSender.send(JSON.stringify(resultData));
        }
    }
    else //Not Found Room
    {
        console.log("Boardcast: isFoundRoom = "+findRoom.isFoundRoom);
        var resultData = {
            eventName: ErrorAlert,
            Room: "",
            Sender: "",
            data: "Not found room.\nPlease reconnect.",
        }

        wsSender.send(JSON.stringify(resultData));
    }
}

function FindRoom(roomName){
    var findRoom = {
        isFoundRoom: false,
        roomIndex: -1,
    }
    if(roomList.length>0){
        for (var i = 0; i < roomList.length; i++)
        {
            if(roomList[i].roomName == roomName){
                findRoom.isFoundRoom = true;
                findRoom.roomIndex = i;
                break;
            }
        }
    }
    return findRoom;
}

function FindUserInRoom(roomIndex,ws){
    var findUserInRoom = {
        isFoundUser: false,
        userIndex: -1,
    }
    for(var i = 0; i<roomList[roomIndex].wsList.length;i++)
    {
        if(roomList[roomIndex].wsList[i] == ws)
        {
            findUserInRoom.isFoundUser = true;
            findUserInRoom.userIndex = i;
        }
    }
    return findUserInRoom;
}

function CreateRoom(ws, roomOption){
    /*
        roomOption = 
        {
            roomName: "xxx",
            maxPlayer: 4,
            Map: "Untitle",
        }
    */

    var isFoundRoom = roomMap.has(roomOption.roomName);
    if(isFoundRoom===true) // เท่ากับ 3 อัน หมายถึงต้องเป็น true ที่เป็น boolean ไม่ใช่สตริงที่เป็น 'true' หรือ 0 == '0' เป็นจริงแต่ 0 === '0' เป็นเท็จ 
    {
        var callbackMsg = {
            eventName: "CreateRoom",
            status: false,
        }
        var toJsonStr = JSON.stringify(callbackMsg);
        ws.send(toJsonStr);
    }
    else
    {
        let roomName = roomOption.roomName;
        roomMap.set(roomName,{
            wsList: new Map()
        });

        roomMap.get(roomName).set(ws,{});

        var callbackMsg = {
            eventName: "CreateRoom",
            status: true,
            data: JSON.stringify(roomOption),
        }
        var toJsonStr = JSON.stringify(callbackMsg);
        ws.send(toJsonStr);
    }

    // console.log("Client request CreateRoom ["+toJson.Room+"]");

    // //Find Room
    // var findRoom = FindRoom(toJson.Room);
    // if(findRoom.isFoundRoom) 
    // {
    //     //Can't Create room same name with other
    //     console.log("Create room : room is founded");
    //     toJson.data = Failed;
    //     ws.send(JSON.stringify(toJson));
    // }
    // else
    // {
    //     //CreateRoom
    //     var newRoom = {
    //         roomName: toJson.Room,
    //         wsList: []
    //     }
    //     newRoom.wsList.push(ws)
    //     roomList.push(newRoom);
    //     console.log("Create new room : "+ newRoom.roomName);
    //     toJson.data = Success;
    //     ws.send(JSON.stringify(toJson));
    // }
}

function JoinRoom(ws,roomOption){

    let roomName = roomOption.roomName;
    let isFoundRoom = roomMap.has(roomName);
    
    let callbackMsg = {
        eventName: "JoinRoom",
        status: false,
    }

    if(isFoundRoom === true){
        callbackMsg.status = false;
    }
    else
    {
        let isFoundClientInRoom = roomMap.get(roomName).wsList.has(ws);
        if(isFoundClientInRoom === true)
        {
            callbackMsg.status = false;
        }
        else
        {
            roomMap.get(roomName).wsList.set(ws,{});
            callbackMsg.status: true;
            callbackMsg.roomOption = JSON.stringify(roomList[indexRoom].roomOption);
        }
    }
    ws.send(JSON.stringify(callbackMsg));

    // console.log("Client request CreateRoom ["+toJson.Room+"]");
    // var isFoundRoom = false;
    // var roomIndex;

    // //find roomIndex and kick client from uncorrect room
    // for (var i = 0; i < roomList.length; i++)
    // {
    //     if(roomList[i].roomName == toJson.Room)
    //     {
    //         isFoundRoom = true;
    //         roomIndex = i;
    //     }
    //     //kick user from uncorrect room
    //     for (var j = 0; j < roomList[i].wsList.length; j++)
    //     {
    //         if(roomList[i].wsList[j] == ws)
    //         {
    //             roomList[i].wsList.splice(j,1);
    //             return;
    //         }
    //     }
    // }
    
    // //Join Room
    // if(isFoundRoom)
    // {
    //     console.log("Join room : "+toJson.Room);

    //     roomList[roomIndex].wsList.push(ws);
    //     toJson.data = Success;
    //     console.log("Join Success");
    //     ws.send(JSON.stringify(toJson));
    //     console.log("Send >>>> " + JSON.stringify(toJson));
    // }

    // //Join Failed
    // else
    // {
    //     toJson.data = Failed;
    //     ws.send(JSON.stringify(toJson));
    // }
}

function LeaveRoom(ws,callback){

    //ลอกมาไม่ครบนะเออไปเช็คด้วยตรง callback
    for(let roomKey of roomMap.keys())
    {
        if(roomMap.get(roomKey).wsList.has(ws))
        {
            if(roomMap.get(roomKey).wsList.delete(ws))
            {
                callback(roomMap.get(roomKey).wsList.delete(ws));
                return;
            }
        }
    }

    callback(false);
    return;

    /*
    var findRoom = FindRoom(toJson.Room);
    if(findRoom.isFoundRoom)
    {
        for (var j = 0; j < roomList[findRoom.roomIndex].wsList.length; j++)
        {
            if(roomList[findRoom.roomIndex].wsList[j] == ws)
            {
                roomList[findRoom.roomIndex].wsList.splice(j,1);
                console.log("LeaveRoom Success 1");
            }
        }

        if(roomList[findRoom.roomIndex].wsList.length > 0)
        {
            toJson.eventName = ChatSystem;

            for (var j = 0; j < roomList[findRoom.roomIndex].wsList.length; j++)
            {
                roomList[findRoom.roomIndex].wsList[j].send(JSON.stringify(toJson));
                console.log("tell everyone i leave");
            }
        }
        else
        {
            roomList.splice(findRoom.roomIndex,1);
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
    */
}