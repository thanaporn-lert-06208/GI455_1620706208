// const app = require('express')();
// const server = require('http').Server(app);
const websocket = require('ws');
//const wss = new websocket.Server({server});
const sqlite = require('sqlite3').verbose();
const uuid = require('uuid'); // ไว้ generate ตัว uid ของ object ไม่ให้ซ้ำกัน

var database = new sqlite.Database('.chatDB.db', sqlite.OPEN_CREATE | sqlite.OPEN_READWRITE, (err)=>{
    if(err) throw err;

    console.log("Connect to database.");
});

// server.listen(procrss.env.PORT || 8080, ()=>{
//     console.log("Server start at port "+server.address().port);
// });
/*
server.listen(process.env.PORT || 5500, ()=>{
    console.log("Server start at port "+server.address().port);
});*/

var wss = new websocket.Server({port:5500}, callbackInitServer = ()=>{
    console.log("Server is running");
});  // wss = web socket server

/*
//var lobbyWsList = []; //collect who in lobby to send new roomList change
//var roomList = [];
{
    roomName: "xxxx",
    wsList: []
}
*/

var roomMap = new Map();
var wsList = [];

wss.on("connection",(ws)=>{
    {
        console.log("client connected.");
        ws.on("message", (data)=>{

            var toJsonObj = JSON.parse(data);

            if(toJsonObj.eventName == "CreateRoom") //CreateRoom
            {
                CreateRoom(ws, toJsonObj.roomOption);
            }
            else if(toJsonObj.eventName == "JoinRoom")
            {
                JoinRoom(ws, toJsonObj.roomOption);
            }
            else if(toJsonObj.eventName == "LeaveRoom")
            {
                LeaveRoom(ws, (status,roomKey)=>{
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
            else if(toJsonObj.eventName == "Login"){
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
            else if(toJsonObj.eventName == "Register"){
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
            else if(toJsonObj.eventName == "RequestUIDObject"){
                RequestUIDObject(ws);
            }
            else if(toJsonObj.eventName == "ReplicateData"){
                ReplicateData(ws, toJsonObj.roomName, toJsonObj.data);
            }
            //Unknown Event case
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
        LeaveRoom(ws, (status,roomKey)=>{
            if(status === true){
                if(roomMap.get(roomKey).wsList.size <= 0){
                    roomMap.delete(roomKey);
                }
            }
        });
    });
});



function ClientConnect(ws){
    ws.uid = uuid.v1();
    let callbackMsg = {
        eventName: "Connect",
        data: ws.uid
    }
    ws.send(JSON.stringify(callbackMsg));
}

function Boardcast()
{
    //console.log("Boardcast");
    //Loop ทุกห้อง
    for(let keyRoom of roomMap.keys())
    {
        //เก็บ ws ทุกคนที่อยู่ในห้อง
        let wsList = roomMap.get(keyRoom).wsList;

        //Loop ทุกคนที่อยู่ในห้อง
        for(let keyClient of wsList){
            for(let keyOtherClient of wsList.keys())
            {
                //Loop เพื่อส่งข้อมูลให้ Client อื่นๆ
                let otherWs = keyOtherClient;
                let replicateData = wsList.get(keyClient).replicateData;

                if(replicateData != undefined && replicateData != ""){
                    let callbackMsg = {
                        eventName: "ReplicateData",
                        data: replicateData
                    }
                    otherWs.send(JSON.stringify(callbackMsg));
                }
                console.log(replicateData);
            }
        }
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
    /*  roomOption = 
        {
            roomName: "xxx",
            maxPlayer: 4,
            Map: "untitle",
        }
    */
    
    var isFoundRoom = roomMap.has(roomOption.roomName);

    if(isFoundRoom===true) // เท่ากับ 3 อัน หมายถึงต้องเป็น true ที่เป็น boolean ไม่ใช่สตริงที่เป็น 'true' หรือ 0 == '0' เป็นจริงแต่ 0 === '0' เป็นเท็จ 
    {
        //FindRoom
        var callbackMsg = {
            eventName: "CreateRoom",
            status: false,
        }
        var toJsonStr = JSON.stringify(callbackMsg);
        ws.send(toJsonStr);
    }
    else
    {
        //CreateRoom
        let roomName = roomOption.roomName;
        roomMap.set(roomName,{
            roomOption:roomOption,
            wsList: new Map()
        });

        roomMap.get(roomName).wsList.set(ws,{});

        var callbackMsg = {
            eventName: "CreateRoom",
            status: true,
            data: JSON.stringify(roomOption),
        }
        var toJsonStr = JSON.stringify(callbackMsg);
        ws.send(toJsonStr);
    }

    { //โค้ดเก่า
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
}

function JoinRoom(ws,roomOption){
    let roomName = roomOption.roomName;
    let isFoundRoom = roomMap.has(roomName);
    let callbackMsg = {
        eventName: "JoinRoom",
        status: false,
    }

    if(isFoundRoom === false){
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
            callbackMsg.status = true;
            callbackMsg.roomOption = JSON.stringify(roomMap.get(roomName).roomOption);
        }
    }
    ws.send(JSON.stringify(eventData));

    {
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
}

function LeaveRoom(ws,callback){
    for(let roomKey of roomMap.keys()){
        if(roomMap.get(roomKey).wsList.has(ws)){
            callback(roomMap.get(roomKey).wsList.delete(ws),roomKey);
            //callback = true >> return true
            return;
        }
    }

    callback(false,""); //return false
    return;
    //old version
    {
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
}

function RequestUIDObject(ws){
    //Library สำหรับ generate uid >>> node uuid
    //ติดตั้ง พิมพ์ในเทอร์มินอลว่า npm install uuid
    //อิมพอร์ตไลบรารี่ พิมพ์ที่ต้นโค้ดว่า const uuid = require('uuid');
    let uid = uuid.v1();
    var callbackMsg  = {
        eventName: "RequestUIDObject",
        data:uid
    }
    ws.send(JSON.stringify(callbackMsg));
}

function ReplicateData(ws, roomName, data){
    roomMap.get(roomName).wsList.set(ws, {
        replicateData:data
    });

    /*
        {
            replicateData:data
        }
    */
}

setInterval(Boardcast, 1000); //ให้เรียกทำ Boardcast ทุกๆ 1 วินาที