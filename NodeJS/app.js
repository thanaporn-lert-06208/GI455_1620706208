var websocket = require('ws');
const app = require('express')();
const server = require('http').Server(app);
const sqlite = require('sqlite3').verbose();
// const uuid = require('uuid'); // ไว้ generate ตัว uid ของ object ไม่ให้ซ้ำกัน

const wss = new websocket.Server({server});

// var wss = new websocket.Server({port:5500}, callbackInitServer = ()=>{
//     console.log("Server is running");
// });  // wss = web socket server

let db = new sqlite.Database('./db/bomberThiefDB.db', sqlite.OPEN_CREATE | sqlite.OPEN_READWRITE, (err)=>{
    if(err) throw err;

    console.log("Connect to database.");
});


//var wsList = new Map();

wss.on("connection",(ws)=>{
    console.log("client connected.");
    //wsList.push(ws);
    

    ws.on("message", (data)=>{
        console.log("send from client :"+ data);
        var toJsonObj = JSON.parse(data);

        if(toJsonObj.eventName == "Login"){
            Login(ws, toJsonObj.data);
        }

        else if(toJsonObj.eventName == "LogOut"){
            LogOut(ws, toJsonObj.data);
        }

        else if(toJsonObj.eventName == "Register"){
            Register(ws,toJsonObj.data);
        }

        else{
            console.log("Error EventName");
            let callbackMsg = {
                eventName: toJsonObj.eventName,
                data: "Error",
                data2:"",
            }
            ws.send(JSON.stringify(callbackMsg));
        }

    });

    ws.on("close",()=>{
        console.log("clien disconnected.");
        //wsList = ArrayRemove(wsList,ws);
    });
});

server.listen(process.env.PORT || 5500, ()=>{
    console.log("Server is running at port : "+server.address().port);
});


// function Boardcast(data)
// {
//     for(var i = 0; i<wsList.length;i++)
//     {
//         wsList[i].send(data);
//     }
// }

function Login(ws,data)
{
    // console.log(data);
    var toJsonObj = JSON.parse(data);
    
    var sqpSelect = "SELECT * FROM UserData WHERE UserID='"+toJsonObj.userID+"' AND Password='"+toJsonObj.password+"'";
    
    db.all(sqpSelect,(err,rows)=>{
        if(err){
            console.log(err);
        }
        else{
            if(rows.length>0){
                console.log("Login Success");

                let playerStat = {
                    username: rows[0].Username,
                    statLose: rows[0].Stat_Lose,
                    statWin: rows[0].Stat_Win,
                    statDraw: rows[0].Stat_Draw,
                    highestScore: rows[0].HighestScore
                }

                let callbackMsg={
                    eventName:"Login",
                    data:"Success",
                    data2:JSON.stringify(playerStat),
                }
                ws.send(JSON.stringify(callbackMsg));
            }
            else{
                console.log("Login Failed");
                let callbackMsg={
                    eventName:"Login",
                    data:"Failed",
                    data2:"User Not Found",
                }
                ws.send(JSON.stringify(callbackMsg));
            }
        }
    });
}

function LogOut(ws,data)
{
    // console.log(data);
    var toJsonObj = JSON.parse(data);
    
    var sqpSelect = "SELECT * FROM UserData WHERE UserID='"+toJsonObj.userID+"' AND Password='"+toJsonObj.password+"'";
    
    db.all(sqpSelect,(err,rows)=>{
        if(err){
            console.log(err);
        }
        else{
            if(rows.length>0){
                console.log("LogOut Success");

                let callbackMsg={
                    eventName:"LogOut",
                    data:"Success",
                    data2:"",
                }
                ws.send(JSON.stringify(callbackMsg));
            }
            else{
                console.log("Login Failed");
                let callbackMsg={
                    eventName:"Login",
                    data:"Failed",
                    data2:"User Not Found",
                }
                ws.send(JSON.stringify(callbackMsg));
            }
        }
    });
}

function Register(ws,data)
{
    var toJsonObj = JSON.parse(data);
    
    var sqpSelect = "INSERT INTO UserData (UserID, Password, Username) VALUES ('"+toJsonObj.userID+"','"+toJsonObj.password+"','"+toJsonObj.username+"')";
    db.all(sqpSelect,(err,rows)=>{
        if(err){
            console.log(err);
            var callbackMsg = {
                eventName:"Register",
                data:"Failed",
                data2:"",
            }
            ws.send(JSON.stringify(callbackMsg));
        }
        else{
            console.log("Register Success");
            var callbackMsg = {
                eventName:"Register",
                data:"Success",
                data2:"",
            }
            ws.send(JSON.stringify(callbackMsg));
        }
    });
}





// callbackInitServer = ()=>{
//     console.log("Server is running");
// }

//var wss = new websocket.Server({port:5500}, callbackInitServer);  // wss = web socket server
