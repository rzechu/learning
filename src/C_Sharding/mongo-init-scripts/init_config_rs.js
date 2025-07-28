var config = {
  "_id": "cfgReplSet",
  "configsvr": true,
  "members": [
    { "_id": 0, "host": "cfgsvr1:27017" },
    { "_id": 1, "host": "cfgsvr2:27017" },
    { "_id": 2, "host": "cfgsvr3:27017" }
  ]
};

try {
  rs.status();
  print("Config replica set 'cfgReplSet' already initialized or primary.");
} catch (e) {
  if (e.message.includes("no replset config has been received")) {
    print("Initializing Config replica set 'cfgReplSet'...");
    rs.initiate(config);
    print("Config replica set initiated.");
  } else {
    printjson(e);
  }
}