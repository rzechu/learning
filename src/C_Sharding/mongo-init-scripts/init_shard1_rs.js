// init_shard1_rs.js
var config = {
  _id: "shard1ReplSet",
  members: [
    { _id: 0, host: "shard1a:27017" },
    { _id: 1, host: "shard1b:27017" }
  ]
};

try {
  rs.status();
  print("Shard1 already initialized.");
} catch (e) {
  if (e.message.includes("no replset config has been received")) {
    print("Initializing shard1 replica set...");
    rs.initiate(config);
  } else {
    printjson(e);
  }
}
