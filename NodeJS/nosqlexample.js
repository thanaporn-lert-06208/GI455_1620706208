var mongo = require('mongodb').MongoClient;
var url = "mongodb://localhost:27017/";

mongo.connect(url,{useUnifiedTopology: true},(err,result)=>{
    if(err) throw err;

    console.log("Database Connected.");

    var selectDB = result.db("gi455_example");

    //CreateCollection(selectDB);
    //Register(selectDB, "aaaa", "1234", "Yahoo");

    AddMoney(selectDB,"aaaa");

});

var AddMoney = (db,_playerID)=>{
    var moneyAdded = 500;
    var collectionName = "playerData";
    var querry = {playerID:_playerID}
    
    db.collection("playerData").find(querry).toArray((err,result)=>{
        if(err){
            console.log("Something wrong");
        }
        else{
            if(result.length == 0){
                console.log("Can't found playerID : "+_playerID);
            }
            else
            {
                var moneyUpdate = moneyAdded;
                if(result[0].money != undefined){
                    moneyUpdate = result[0].money + moneyAdded;
                    
                }
                var updateData = { $set: {money:moneyUpdate}};
                db.collection(collectionName).updateOne(querry,updateData,(err,result)=>{
                    if(err){
                        console.log("Update money failed");
                    }
                    else{
                        if(result.result.nModified == 1){
                            console.log("AddMoney success");
                        }
                        else{
                            console.log("AddMoney fail");
                        }
                    }
                });
            }
        }
    });
}

var Register = (db, _playerID, _password, _playerName)=>{
    var newData = {
        playerID: _playerID,
        password: _password,
        playerName: _playerName,
    }

    var playerData = "playerData";
    var querry = {playerID:_playerID};

    db.collection(playerData).find(querry).toArray((err,result)=>{
        if(err){
            console.log(err);
        }else{
            if(result.length == 0)
            {
                db.collection("playerData").insertOne(newData,(err,result)=>{
                    if(err)
                    {
                        console.log(err);
                    }
                    else{
                        if(result.result.ok == 1){
                            //console.log(result);
                            console.log("Register success");
                        }
                        else
                        {
                            console.log("Register failed");
                        }
                    }
                });
            }
            else{
                console.log("PlayerID is exist.");
            }
        }
    });

}

//ความหมายเดียวกับการสร้างเมทอด function CreateCollection(){}
var CreateCollection = (db)=>{
    db.createCollection("playerData",(err,res)=>{
        if(err) throw err;
    });
    db.createCollection("Inventory",(err,res)=>{
        if(err) throw err;
    })

    db.createCollection("shopData",(err,res)=>{
        if(err) throw err;
    })
}