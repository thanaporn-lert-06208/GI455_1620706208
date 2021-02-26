const sqlite = require('sqlite3').verbose();

let db = new sqlite.Database('./db/chatDB.db', sqlite.OPEN_CREATE | sqlite.OPEN_READWRITE, (err)=>{
    
    if(err) throw err;     
    //^^^ อย่าใส่ตอนเปิดให้ user ใช้จริง เพราะถ้ามีเออเร่อจะปิดเซิร์ฟเวอร์ทิ้งไปเลย
    //ถ้าเปิดเซิร์ฟอยู่ ให้เอา throw ออก เพราะเป็นตัวที่ทำให้ปิดเซิร์ฟเวอร์

    console.log('Connected to database.');

    var id = "test4444";
    var password = "555555";
    var name = 'test7';

    var sqlSelect = "SELECT * FROM UserData WHERE UserID='"+id+"' AND Password='"+password+"'";
    //var sqlInsert = "INSERT INTO UserData (UserID, Password, Name) VALUES ('test7777','777777','test 7')";
    var sqlInsert = "INSERT INTO UserData (UserID, Password, Name, Money) VALUES ('"+id+"','"+password+"','"+name+"', '0')";
    var sqlUpdate = "UPDATE UserData SET Money='100' WHERE UserID='"+id+"'";

    //db.all("SELECT * FROM UserData WHERE Name='test5' AND UserID='test4444'", (err,rows)=>{
    //db.all("SELECT * FROM UserData WHERE UserID='"+id+"' AND Password='"+password+"'", (err,rows)=>{
    db.all("SELECT Money FROM UserData WHERE UserID='"+id+"'", (err,rows)=>{
        if(err) {
            console(err);
        }
        else{
            if(rows.length > 0){
                var currentMoney = rows[0].Money;
                currentMoney += 100;
            }
        }

        console.log(rows);
    });
});