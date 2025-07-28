// setup_sharding.js
print("Adding shard1ReplSet...");
sh.addShard("shard1ReplSet/shard1a:27017,shard1b:27017");
print("Shard 1 added successfully");

print("Adding shard2ReplSet...");
sh.addShard("shard2ReplSet/shard2a:27017,shard2b:27017");
print("Shard 2 added successfully");

// Enable sharding on DB and collection
print("Enabling sharding on database testdb...");
sh.enableSharding("testdb");
print("Sharding collection testdb.testcoll...");
sh.shardCollection("testdb.testcoll", { _id: 1 });
print("Sharding setup complete");