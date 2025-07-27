// init_shard2_rs.js
var config = {
    "_id": "shard2ReplSet",
    "members": [
        { "_id": 0, "host": "shard2a:27017" },
        { "_id": 1, "host": "shard2b:27017" }
    ]
};

try {
    rs.status();
    print("Shard 2 replica set 'shard2ReplSet' already initialized or primary.");
} catch (e) {
    if (e.message.includes("no replset config has been received")) {
        print("Initializing Shard 2 replica set 'shard2ReplSet'...");
        rs.initiate(config);
        print("Shard 2 replica set initiated.");
    } else {
        printjson(e);
    }
}