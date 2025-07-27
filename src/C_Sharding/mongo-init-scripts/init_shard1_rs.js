// init_shard1_rs.js
var config = {
    "_id": "shard1ReplSet",
    "members": [
        { "_id": 0, "host": "shard1a:27017" },
        { "_id": 1, "host": "shard1b:27017" }
    ]
};

try {
    rs.status();
    print("Shard 1 replica set 'shard1ReplSet' already initialized or primary.");
} catch (e) {
    if (e.message.includes("no replset config has been received")) {
        print("Initializing Shard 1 replica set 'shard1ReplSet'...");
        rs.initiate(config);
        print("Shard 1 replica set initiated.");
    } else {
        printjson(e);
    }
}